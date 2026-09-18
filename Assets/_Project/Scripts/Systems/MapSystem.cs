using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Orchestrates the procedural Network Map:
/// - Generates the DAG on first visit, reloads it on return from combat.
/// - Instantiates MapNodeView and MapConnectorView objects inside NodesContainer.
/// - Handles node selection, path locking, and scene transitions.
/// </summary>
public class MapSystem : MonoBehaviour
{
    // ── Inspector References ───────────────────────────────────────────────

    [Header("Config & Data")]
    [SerializeField] private MapConfig mapConfig;
    [SerializeField] private HeroData  heroData;

    [Header("Prefabs")]
    [SerializeField] private MapNodeView      nodeViewPrefab;
    [SerializeField] private MapConnectorView connectorPrefab;

    [Header("Scene UI")]
    [SerializeField] private RectTransform nodesContainer;
    [SerializeField] private ScrollRect    mapScrollRect;
    [SerializeField] private TMP_Text      actTitleText;
    [SerializeField] private TMP_Text      playerHPText;

    [Header("Node Interaction UI")]
    [SerializeField] private ChoicePanelUI   nodeChoicePanel;
    [SerializeField] private CardSelectionUI nodeCardSelection;

    // ── Runtime ────────────────────────────────────────────────────────────

    private List<List<MapNodeRuntime>>  _map;
    private List<MapNodeView>           _nodeViews     = new();
    private List<MapConnectorView>      _connectors    = new();

    // ── Unity Lifecycle ────────────────────────────────────────────────────

    private void Start()
    {
        // Disable redundant camera when MapScene is loaded additively over GameScene
        if (SceneManager.GetSceneByName("GameScene").isLoaded)
        {
            var roots = gameObject.scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                if (root.CompareTag("MainCamera") || root.name == "Main Camera")
                {
                    root.SetActive(false);
                }
            }
        }

        // Initialize hero data on first ever load
        if (!GameState.IsInitialized)
        {
            GameState.HeroData     = heroData;
            GameState.PlayerMaxHP  = heroData.Health;
            GameState.PlayerCurrentHP = heroData.Health;
            GameState.InitializeDeck(heroData.Deck);
            GameState.IsInitialized   = true;
        }

        CreateBackButton();

        // Generate or reload map
        if (GameState.GeneratedMap == null)
            _map = MapGenerator.GenerateMap(GameState.CurrentAct, mapConfig);
        else
            _map = GameState.GeneratedMap;

        BuildMapUI();
        RefreshAllNodes();
        UpdateHUD();
        StartCoroutine(CenterMapAfterLayout());
    }

    private void CreateBackButton()
    {
        // Don't show "Back to Menu" if we are just peeking at the map during combat
        if (SceneManager.GetSceneByName("GameScene").isLoaded) return;

        if (nodesContainer == null) return;
        var canvas = nodesContainer.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("BackButton");
        go.transform.SetParent(canvas.transform, false);
        go.transform.SetAsLastSibling(); // Ensure it renders on top and catches clicks first
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(40, -80); // Lowered from -20 to -80
        rect.sizeDelta = new Vector2(220, 60); // Enlarged from 160x45

        var img = go.AddComponent<Image>();
        // Make it a bright, distinct red so it stands out from the dark tech background
        img.color = new Color(0.8f, 0.2f, 0.2f, 1f); 

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            GameState.GeneratedMap = null;
            GameState.IsInitialized = false;
            SceneManager.LoadScene("MainMenu");
        });

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = "< BACK TO MENU";
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 24; // Increased font size
        text.fontStyle = FontStyles.Bold;
    }

    // ── Map UI Building ────────────────────────────────────────────────────

    private void BuildMapUI()
    {
        // Clear old views
        foreach (var v in _nodeViews)    if (v) Destroy(v.gameObject);
        foreach (var c in _connectors)   if (c) Destroy(c.gameObject);
        _nodeViews.Clear();
        _connectors.Clear();

        int totalLayers = _map.Count;
        float layerH    = mapConfig.layerHeight > 0 ? mapConfig.layerHeight : 160f;
        float treeH     = (totalLayers - 1) * layerH;

        // Get viewport height safely from scrollRect
        float viewportH = 1010f; // Default fallback (1080 - 70 topbar)
        if (mapScrollRect != null && mapScrollRect.viewport != null)
        {
            float vh = mapScrollRect.viewport.rect.height;
            if (vh > 500f) viewportH = vh;
        }

        float minPadding = 120f;
        float neededHeight = treeH + minPadding * 2f;

        float contentH;
        float startY;

        if (neededHeight <= viewportH)
        {
            // Map fits entirely within visible viewport — match container height to viewport height.
            // This prevents ScrollRect from forcing content to align to the top.
            contentH = viewportH;
            startY = (viewportH - treeH) * 0.5f;

            if (mapScrollRect != null)
            {
                mapScrollRect.vertical = false;
            }
        }
        else
        {
            // Map is taller than viewport (Act 2 / 3) — allow vertical scrolling.
            contentH = neededHeight;
            startY = minPadding;

            if (mapScrollRect != null)
            {
                mapScrollRect.vertical = true;
            }
        }

        nodesContainer.sizeDelta = new Vector2(nodesContainer.sizeDelta.x, contentH);
        nodesContainer.anchoredPosition = Vector2.zero;

        // Position nodes bottom-to-top (Layer 0 at bottom)
        for (int l = 0; l < _map.Count; l++)
        {
            var layer = _map[l];
            int count = layer.Count;
            float yPos = startY + l * layerH;

            for (int n = 0; n < count; n++)
            {
                var node = layer[n];

                // Centre columns horizontally
                float totalWidth = (count - 1) * mapConfig.columnWidth;
                float xPos = n * mapConfig.columnWidth - totalWidth * 0.5f;

                var view = Instantiate(nodeViewPrefab, nodesContainer);
                var viewRT = view.GetComponent<RectTransform>();
                viewRT.anchorMin = new Vector2(0.5f, 0f);
                viewRT.anchorMax = new Vector2(0.5f, 0f);
                viewRT.pivot     = new Vector2(0.5f, 0.5f);
                viewRT.anchoredPosition = new Vector2(xPos, yPos);
                view.Setup(node, mapConfig, OnNodeClicked);

                node.ViewLocalPos = new Vector2(xPos, yPos); // Cache for connectors
                _nodeViews.Add(view);
            }
        }

        // Build connectors (drawn under nodes — instantiate before nodes so they sit behind)
        for (int l = 0; l < _map.Count - 1; l++)
        {
            foreach (var parent in _map[l])
            {
                foreach (var child in parent.Children)
                {
                    bool active = !parent.IsLocked && !child.IsLocked;
                    var conn = Instantiate(connectorPrefab, nodesContainer);
                    var connRT = conn.GetComponent<RectTransform>();
                    connRT.anchorMin = new Vector2(0.5f, 0f);
                    connRT.anchorMax = new Vector2(0.5f, 0f);
                    connRT.pivot     = new Vector2(0.5f, 0.5f);
                    conn.transform.SetAsFirstSibling();
                    conn.Setup(parent.ViewLocalPos, child.ViewLocalPos, mapConfig, active);
                    _connectors.Add(conn);
                }
            }
        }
    }

    // ── State Refresh ──────────────────────────────────────────────────────

    private void RefreshAllNodes()
    {
        foreach (var view in _nodeViews)
            view?.RefreshVisuals();

        // Refresh connector colors too
        int connIdx = 0;
        for (int l = 0; l < _map.Count - 1; l++)
        {
            foreach (var parent in _map[l])
            {
                foreach (var child in parent.Children)
                {
                    if (connIdx < _connectors.Count)
                    {
                        bool active = !parent.IsLocked && !child.IsLocked;
                        _connectors[connIdx].RefreshColor(mapConfig, active);
                        connIdx++;
                    }
                }
            }
        }
    }

    private void UpdateHUD()
    {
        if (actTitleText != null)
        {
            actTitleText.text = GameState.CurrentAct switch
            {
                1 => "Act I (Floppy Sector)",
                2 => "Act II (System Ram)",
                3 => "Act III (CPU)",
                _ => "Network Map"
            };
        }

        if (playerHPText != null)
        {
            int cur = GameState.PlayerCurrentHP > 0 ? GameState.PlayerCurrentHP : GameState.PlayerMaxHP;
            playerHPText.text = $"SYS HEALTH : {cur} / {GameState.PlayerMaxHP}";
        }
    }

    private IEnumerator CenterMapAfterLayout()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        // If map is larger than viewport, scroll to bottom (Layer 0) on start
        float totalLayers = _map.Count;
        float layerH    = mapConfig.layerHeight > 0 ? mapConfig.layerHeight : 160f;
        float treeH     = (totalLayers - 1) * layerH;
        float minPadding = 120f;

        if (treeH + minPadding * 2f > 1010f && mapScrollRect != null)
        {
            mapScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // Keep for external calls if needed
    private void ScrollToBottom()
    {
        if (mapScrollRect != null)
            mapScrollRect.verticalNormalizedPosition = 0f;
    }

    // ── Node Selection ─────────────────────────────────────────────────────

    private void OnNodeClicked(MapNodeView view)
    {
        // View-Only mode when GameScene is loaded — do not allow node selection mid-fight
        if (SceneManager.GetSceneByName("GameScene").isLoaded)
        {
            Debug.Log("[MapSystem] Map is in View-Only mode during active combat.");
            return;
        }

        var node = view.RuntimeNode;
        if (node == null || !node.IsUnlocked || node.IsVisited || node.IsLocked) return;

        // Lock all siblings (other nodes on the same layer that were unlocked)
        foreach (var sibling in _map[node.Layer])
        {
            if (sibling != node && !sibling.IsVisited)
            {
                sibling.IsLocked   = true;
                sibling.IsUnlocked = false;

                // Recursively lock descendants of locked siblings
                LockDescendants(sibling);
            }
        }

        GameState.SelectedNode = node;

        // Non-combat nodes: show interaction UI
        if (node.Type == NodeType.Rest)
        {
            HandleRestNode();
            return;
        }
        if (node.Type == NodeType.Treasure)
        {
            HandleTreasureNode();
            return;
        }

        // Combat / Boss → load GameScene
        SceneManager.LoadScene("GameScene");
    }

    // ── Rest Node ──────────────────────────────────────────────────────────

    private void HandleRestNode()
    {
        if (nodeChoicePanel == null)
        {
            CompleteNodeAndRefresh();
            return;
        }

        bool canPurge = GameState.CanPurge();

        nodeChoicePanel.Show(
            "REST SITE",
            "SYSTEM RESTORE\n<size=60%>Heal 30% Max HP</size>",
            canPurge ? "GARBAGE COLLECTION\n<size=60%>Purge 1 Card</size>" : "GARBAGE COLLECTION\n<size=60%>(MIN DECK)</size>",
            OnRestHeal,
            OnRestPurge,
            () => CompleteNodeAndRefresh(), // Skip
            optionBEnabled: canPurge
        );
    }

    private void OnRestHeal()
    {
        int currentHP = GameState.PlayerCurrentHP > 0 ? GameState.PlayerCurrentHP : GameState.PlayerMaxHP;
        int healAmount = Mathf.CeilToInt(GameState.PlayerMaxHP * 0.3f);
        int newHP = Mathf.Min(currentHP + healAmount, GameState.PlayerMaxHP);
        GameState.PlayerCurrentHP = newHP;
        Debug.Log($"[MapSystem] Healed {healAmount} HP. Now at {newHP}/{GameState.PlayerMaxHP}");
        CompleteNodeAndRefresh();
    }

    private void OnRestPurge()
    {
        if (nodeCardSelection == null)
        {
            CompleteNodeAndRefresh();
            return;
        }

        List<CardData> cards = GameState.GetDistinctCards();
        nodeCardSelection.Show(cards, "SELECT A CARD TO PURGE", (card) =>
        {
            GameState.PurgeCard(card);
            CompleteNodeAndRefresh();
        });
    }

    // ── Treasure Node ──────────────────────────────────────────────────────

    private void HandleTreasureNode()
    {
        if (nodeChoicePanel == null)
        {
            CompleteNodeAndRefresh();
            return;
        }

        bool canPurge = GameState.CanPurge();

        nodeChoicePanel.Show(
            "DATA CACHE",
            "TARGETED DUPLICATION\n<size=60%>Duplicate 1 non-Repair Card</size>",
            canPurge ? "SYSTEM CLEANSE\n<size=60%>Purge 1 Card</size>" : "SYSTEM CLEANSE\n<size=60%>(MIN DECK)</size>",
            OnTreasureDuplicate,
            OnTreasurePurge,
            () => CompleteNodeAndRefresh(), // Skip
            optionBEnabled: canPurge
        );
    }

    private void OnTreasureDuplicate()
    {
        if (nodeCardSelection == null)
        {
            CompleteNodeAndRefresh();
            return;
        }

        // Filter out Repair (.patch) cards
        List<CardData> cards = GameState.GetDistinctCards()
            .Where(c => !c.IsExhaust).ToList(); // Repair cards use Exhaust flag
        nodeCardSelection.Show(cards, "SELECT A CARD TO DUPLICATE", (card) =>
        {
            GameState.DuplicateCard(card);
            CompleteNodeAndRefresh();
        });
    }

    private void OnTreasurePurge()
    {
        if (nodeCardSelection == null)
        {
            CompleteNodeAndRefresh();
            return;
        }

        List<CardData> cards = GameState.GetDistinctCards();
        nodeCardSelection.Show(cards, "SELECT A CARD TO PURGE", (card) =>
        {
            GameState.PurgeCard(card);
            CompleteNodeAndRefresh();
        });
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private void CompleteNodeAndRefresh()
    {
        GameState.CompleteSelectedNode(GameState.PlayerCurrentHP > 0
            ? GameState.PlayerCurrentHP
            : GameState.PlayerMaxHP);
        BuildMapUI();
        RefreshAllNodes();
        UpdateHUD();
    }

    private void LockDescendants(MapNodeRuntime node)
    {
        foreach (var child in node.Children)
        {
            // Only lock if not already reachable from another unlocked parent
            bool hasUnlockedParent = false;
            foreach (var p in child.Parents)
            {
                if (!p.IsLocked && p != node) { hasUnlockedParent = true; break; }
            }

            if (!hasUnlockedParent && !child.IsVisited)
            {
                child.IsLocked   = true;
                child.IsUnlocked = false;
                LockDescendants(child);
            }
        }
    }
}
