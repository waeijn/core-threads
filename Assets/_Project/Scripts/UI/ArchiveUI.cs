using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ARCHIVE — Main Menu compendium showing all Cards (SCRIPTS) and Enemies (THREATS).
/// Loaded from Resources and ActEnemyRoster assets.
/// </summary>
public class ArchiveUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button backgroundOverlay;
    [SerializeField] private Button mainCloseButton; // Added close button

    [Header("Tabs")]
    [SerializeField] private Button scriptsTabButton;
    [SerializeField] private Button threatsTabButton;
    [SerializeField] private GameObject scriptsContent;
    [SerializeField] private GameObject threatsContent;

    [Header("Card Detail Popup")]
    [SerializeField] private GameObject cardDetailPanel;
    [SerializeField] private Image cardDetailImage;
    [SerializeField] private TMP_Text cardDetailTitle;
    [SerializeField] private TMP_Text cardDetailDescription;
    [SerializeField] private TMP_Text cardDetailStats;
    [SerializeField] private Button cardDetailCloseButton;

    [Header("Enemy Detail Popup")]
    [SerializeField] private GameObject enemyDetailPanel;
    [SerializeField] private Image enemyDetailImage;
    [SerializeField] private TMP_Text enemyDetailName;
    [SerializeField] private TMP_Text enemyDetailStats;
    [SerializeField] private TMP_Text enemyDetailMoves;
    [SerializeField] private Button enemyDetailCloseButton;

    [Header("Card Grid Prefab")]
    [SerializeField] private GameObject cardEntryPrefab; // Will be created by editor script

    [Header("Enemy Roster Data")]
    [SerializeField] private ActEnemyRoster act1Roster;
    [SerializeField] private ActEnemyRoster act2Roster;
    [SerializeField] private ActEnemyRoster act3Roster;

    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset mainFont;

    // Animation
    private CanvasGroup _cg;
    private RectTransform _panelRT;
    private Coroutine _anim;
    private float openDuration = 0.20f;
    private float closeDuration = 0.15f;

    public bool IsOpen { get; private set; }

    private enum Tab { Scripts, Threats }
    private Tab currentTab = Tab.Scripts;
    private List<GameObject> _spawnedCards = new();
    private List<GameObject> _spawnedEnemies = new();

    private void Awake()
    {
        if (panel != null)
        {
            _cg = panel.GetComponent<CanvasGroup>();
            if (_cg == null) _cg = panel.AddComponent<CanvasGroup>();
            _panelRT = panel.GetComponent<RectTransform>();
        }

        if (backgroundOverlay != null)
            backgroundOverlay.onClick.AddListener(() => { AudioSystem.Instance?.PlayButtonClick(); Hide(); });
            
        if (mainCloseButton != null)
        {
            mainCloseButton.onClick.AddListener(() => { AudioSystem.Instance?.PlayButtonClick(); Hide(); });
            AddHoverSound(mainCloseButton);
        }

        if (scriptsTabButton != null)
        {
            scriptsTabButton.onClick.AddListener(() => { AudioSystem.Instance?.PlayButtonClick(); SwitchTab(Tab.Scripts); });
            AddHoverSound(scriptsTabButton);
        }
        if (threatsTabButton != null)
        {
            threatsTabButton.onClick.AddListener(() => { AudioSystem.Instance?.PlayButtonClick(); SwitchTab(Tab.Threats); });
            AddHoverSound(threatsTabButton);
        }

        if (cardDetailCloseButton != null)
        {
            cardDetailCloseButton.onClick.AddListener(() => { AudioSystem.Instance?.PlayButtonClick(); CloseCardDetail(); });
            AddHoverSound(cardDetailCloseButton);
        }
        if (enemyDetailCloseButton != null)
        {
            enemyDetailCloseButton.onClick.AddListener(() => { AudioSystem.Instance?.PlayButtonClick(); CloseEnemyDetail(); });
            AddHoverSound(enemyDetailCloseButton);
        }

        gameObject.SetActive(false);
    }

    public void Show()
    {
        IsOpen = true;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        if (panel != null) panel.SetActive(true);

        CloseCardDetail();
        CloseEnemyDetail();
        SwitchTab(Tab.Scripts);

        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimOpen());
    }

    public void Hide()
    {
        IsOpen = false;
        CloseCardDetail();
        CloseEnemyDetail();

        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimClose());
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            // Close detail popups first, then the whole panel
            if (cardDetailPanel != null && cardDetailPanel.activeSelf)
            {
                AudioSystem.Instance?.PlayButtonClick();
                CloseCardDetail();
            }
            else if (enemyDetailPanel != null && enemyDetailPanel.activeSelf)
            {
                AudioSystem.Instance?.PlayButtonClick();
                CloseEnemyDetail();
            }
            else
            {
                AudioSystem.Instance?.PlayButtonClick();
                Hide();
            }
        }
    }

    // ── Tab Switching ──────────────────────────────────────────────────────

    private void SwitchTab(Tab tab)
    {
        currentTab = tab;

        if (scriptsContent != null) scriptsContent.SetActive(tab == Tab.Scripts);
        if (threatsContent != null) threatsContent.SetActive(tab == Tab.Threats);

        // Update tab button visuals
        UpdateTabVisuals();

        if (tab == Tab.Scripts)
            PopulateCards();
        else
            PopulateEnemies();
    }

    private void UpdateTabVisuals()
    {
        Color activeColor = new Color(0.2f, 0.8f, 1f); // Cyan
        Color inactiveColor = new Color(0.5f, 0.5f, 0.5f);

        if (scriptsTabButton != null)
        {
            var txt = scriptsTabButton.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.color = currentTab == Tab.Scripts ? activeColor : inactiveColor;
        }
        if (threatsTabButton != null)
        {
            var txt = threatsTabButton.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.color = currentTab == Tab.Threats ? activeColor : inactiveColor;
        }
    }

    // ── Cards (SCRIPTS) ────────────────────────────────────────────────────

    private void PopulateCards()
    {
        ClearSpawned(_spawnedCards);
        if (scriptsContent == null) return;

        CardData[] allCards = Resources.LoadAll<CardData>("Cards");

        // Group by tier
        var act1 = allCards.Where(c => c.Tier == CardActTier.Act1).OrderBy(c => c.Role).ToList();
        var act2 = allCards.Where(c => c.Tier == CardActTier.Act2).OrderBy(c => c.Role).ToList();
        var act3 = allCards.Where(c => c.Tier == CardActTier.Act3).OrderBy(c => c.Role).ToList();

        Transform container = scriptsContent.transform;
        CreateActCardSection(container, "// ACT 1 — BASIC PROTOCOLS", act1);
        CreateActCardSection(container, "// ACT 2 — ADVANCED MODULES", act2);
        CreateActCardSection(container, "// ACT 3 — CLASSIFIED PAYLOADS", act3);
    }

    private void CreateActCardSection(Transform parent, string header, List<CardData> cards)
    {
        // Header
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(parent, false);
        TextMeshProUGUI headerTmp = headerObj.AddComponent<TextMeshProUGUI>();
        headerTmp.text = header;
        headerTmp.fontSize = 24;
        headerTmp.color = new Color(0.2f, 0.8f, 1f); // Cyan
        headerTmp.alignment = TextAlignmentOptions.Left;
        if (mainFont != null) headerTmp.font = mainFont;
        LayoutElement headerLe = headerObj.AddComponent<LayoutElement>();
        headerLe.minHeight = 40;
        headerLe.preferredWidth = 1100;
        _spawnedCards.Add(headerObj);

        // Card row
        GameObject rowObj = new GameObject("CardRow");
        rowObj.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 15;
        hlg.childAlignment = TextAnchor.UpperLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        LayoutElement rowLe = rowObj.AddComponent<LayoutElement>();
        rowLe.minHeight = 200;
        _spawnedCards.Add(rowObj);

        foreach (var card in cards)
        {
            GameObject entryObj = new GameObject(card.name);
            entryObj.transform.SetParent(rowObj.transform, false);

            Image img = entryObj.AddComponent<Image>();
            if (card.Image != null)
            {
                img.sprite = card.Image;
                img.preserveAspect = true;
            }
            else
            {
                img.color = new Color(0.2f, 0.2f, 0.2f);
            }

            LayoutElement le = entryObj.AddComponent<LayoutElement>();
            le.minWidth = 130;
            le.minHeight = 182;

            Button btn = entryObj.AddComponent<Button>();
            CardData captured = card;
            btn.onClick.AddListener(() =>
            {
                AudioSystem.Instance?.PlayButtonClick();
                ShowCardDetail(captured);
            });
            AddHoverSound(btn);

            _spawnedCards.Add(entryObj);
        }
    }

    // ── Enemies (THREATS) ──────────────────────────────────────────────────

    private void PopulateEnemies()
    {
        ClearSpawned(_spawnedEnemies);
        if (threatsContent == null) return;

        Transform container = threatsContent.transform;

        if (act1Roster != null) CreateActEnemySection(container, "// ACT 1 — LOW-LEVEL THREATS", act1Roster);
        if (act2Roster != null) CreateActEnemySection(container, "// ACT 2 — MID-LEVEL THREATS", act2Roster);
        if (act3Roster != null) CreateActEnemySection(container, "// ACT 3 — CRITICAL THREATS", act3Roster);
    }

    private void CreateActEnemySection(Transform parent, string header, ActEnemyRoster roster)
    {
        // Header
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(parent, false);
        TextMeshProUGUI headerTmp = headerObj.AddComponent<TextMeshProUGUI>();
        headerTmp.text = header;
        headerTmp.fontSize = 24;
        headerTmp.color = new Color(1f, 0.3f, 0.3f); // Reddish
        headerTmp.alignment = TextAlignmentOptions.Left;
        if (mainFont != null) headerTmp.font = mainFont;
        LayoutElement headerLe = headerObj.AddComponent<LayoutElement>();
        headerLe.minHeight = 40;
        headerLe.preferredWidth = 1100;
        _spawnedEnemies.Add(headerObj);

        // Standard enemies
        if (roster.StandardEnemies != null)
        {
            foreach (var enemy in roster.StandardEnemies)
            {
                if (enemy != null) CreateEnemyEntry(parent, enemy, false);
            }
        }

        // Boss
        if (roster.BossEnemy != null)
        {
            CreateEnemyEntry(parent, roster.BossEnemy, true);
        }
    }

    private void CreateEnemyEntry(Transform parent, EnemyData enemy, bool isBoss)
    {
        GameObject entryObj = new GameObject(enemy.name);
        entryObj.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = entryObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(10, 10, 5, 5);

        // Background
        Image bg = entryObj.AddComponent<Image>();
        bg.color = isBoss ? new Color(0.3f, 0.1f, 0.1f, 0.8f) : new Color(0.1f, 0.1f, 0.15f, 0.8f);

        LayoutElement entryLe = entryObj.AddComponent<LayoutElement>();
        entryLe.minHeight = 80;
        entryLe.preferredWidth = 1100;

        // Sprite
        GameObject spriteObj = new GameObject("Sprite");
        spriteObj.transform.SetParent(entryObj.transform, false);
        Image spriteImg = spriteObj.AddComponent<Image>();
        if (enemy.Image != null)
        {
            spriteImg.sprite = enemy.Image;
            spriteImg.preserveAspect = true;
        }
        LayoutElement spriteLe = spriteObj.AddComponent<LayoutElement>();
        spriteLe.minWidth = 60;
        spriteLe.minHeight = 60;

        // Name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
        string bossTag = isBoss ? " <color=#FF4444>[BOSS]</color>" : "";
        nameTmp.text = $"{enemy.name.ToUpper()}{bossTag}";
        nameTmp.fontSize = 28;
        nameTmp.color = Color.white;
        nameTmp.alignment = TextAlignmentOptions.Left;
        if (mainFont != null) nameTmp.font = mainFont;
        LayoutElement nameLe = nameObj.AddComponent<LayoutElement>();
        nameLe.minWidth = 300;
        nameLe.minHeight = 60;

        // Stats
        GameObject statsObj = new GameObject("Stats");
        statsObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI statsTmp = statsObj.AddComponent<TextMeshProUGUI>();
        statsTmp.text = $"<color=#FF6666>HP: {enemy.Health}</color>  <color=#FF4444>ATK: {enemy.AttackPower}</color>  <color=#4488FF>BLK: {enemy.BlockPower}</color>  <color=#44FF44>BUFF: {enemy.BuffAmount}</color>";
        statsTmp.fontSize = 22;
        statsTmp.alignment = TextAlignmentOptions.Left;
        if (mainFont != null) statsTmp.font = mainFont;
        LayoutElement statsLe = statsObj.AddComponent<LayoutElement>();
        statsLe.minWidth = 500;
        statsLe.minHeight = 60;

        // Click handler for detail view
        Button btn = entryObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None; // Keep custom background
        EnemyData captured = enemy;
        bool capturedIsBoss = isBoss;
        btn.onClick.AddListener(() =>
        {
            AudioSystem.Instance?.PlayButtonClick();
            ShowEnemyDetail(captured, capturedIsBoss);
        });
        AddHoverSound(btn);

        _spawnedEnemies.Add(entryObj);
    }

    // ── Card Detail Popup ──────────────────────────────────────────────────

    private void ShowCardDetail(CardData card)
    {
        if (cardDetailPanel == null) return;
        cardDetailPanel.SetActive(true);
        cardDetailPanel.transform.SetAsLastSibling();

        if (cardDetailImage != null && card.Image != null)
        {
            cardDetailImage.sprite = card.Image;
            cardDetailImage.preserveAspect = true;
        }

        if (cardDetailTitle != null)
            cardDetailTitle.text = card.GetFormattedTitle();

        if (cardDetailDescription != null)
            cardDetailDescription.text = card.Description ?? "No description available.";

        if (cardDetailStats != null)
        {
            string exhaust = card.IsExhaust ? "  |  <color=#FF8800>EXHAUST</color>" : "";
            string role = card.Role.ToString().ToUpper();
            cardDetailStats.text = $"<color=#00CCFF>COST: {card.Mana}</color>  |  <color=#AAAAAA>{role}</color>  |  <color=#888888>TIER: ACT {(int)card.Tier}</color>{exhaust}";
        }
    }

    private void CloseCardDetail()
    {
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
    }

    // ── Enemy Detail Popup ─────────────────────────────────────────────────

    private void ShowEnemyDetail(EnemyData enemy, bool isBoss)
    {
        if (enemyDetailPanel == null) return;
        enemyDetailPanel.SetActive(true);
        enemyDetailPanel.transform.SetAsLastSibling();

        if (enemyDetailImage != null && enemy.Image != null)
        {
            enemyDetailImage.sprite = enemy.Image;
            enemyDetailImage.preserveAspect = true;
        }

        if (enemyDetailName != null)
        {
            string bossTag = isBoss ? " <color=#FF4444>[BOSS]</color>" : "";
            enemyDetailName.text = $"{enemy.name.ToUpper()}{bossTag}";
        }

        if (enemyDetailStats != null)
        {
            enemyDetailStats.text = $"<color=#FF6666>HP: {enemy.Health}</color>\n<color=#FF4444>ATK: {enemy.AttackPower}</color>\n<color=#4488FF>BLK: {enemy.BlockPower}</color>\n<color=#44FF44>BUFF: {enemy.BuffAmount}</color>";
        }

        if (enemyDetailMoves != null)
        {
            string moveInfo = "";

            // FSM States
            if (enemy.FSMStates != null && enemy.FSMStates.Count > 0)
            {
                moveInfo += "<color=#FFAA00>// BEHAVIOR STATES</color>\n";
                foreach (var state in enemy.FSMStates)
                {
                    moveInfo += $"\n<color=#CCCCCC>{state.StateName}</color> (HP {state.MinHPPercent}%-{state.MaxHPPercent}%):\n";
                    foreach (var move in state.Moves)
                    {
                        string val = move.OverrideValue > 0 ? $" [{move.OverrideValue}]" : "";
                        moveInfo += $"  • {move.Intent}{val} (Weight: {move.Weight})\n";
                    }
                }
            }

            // Default Move Pool
            if (enemy.MovePool != null && enemy.MovePool.Count > 0)
            {
                moveInfo += "\n<color=#FFAA00>// DEFAULT MOVE POOL</color>\n";
                foreach (var move in enemy.MovePool)
                {
                    string val = move.OverrideValue > 0 ? $" [{move.OverrideValue}]" : "";
                    moveInfo += $"  • {move.Intent}{val} (Weight: {move.Weight})\n";
                }
            }

            if (string.IsNullOrEmpty(moveInfo))
                moveInfo = "<color=#888888>No move data configured.</color>";

            enemyDetailMoves.text = moveInfo;
        }
    }

    private void CloseEnemyDetail()
    {
        if (enemyDetailPanel != null) enemyDetailPanel.SetActive(false);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private void ClearSpawned(List<GameObject> list)
    {
        foreach (var go in list)
            if (go != null) Destroy(go);
        list.Clear();
    }

    private void AddHoverSound(Button button)
    {
        if (button == null) return;
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>() ?? button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.RemoveAll(e => e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerEnter || e.eventID == UnityEngine.EventSystems.EventTriggerType.PointerExit);

        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { AudioSystem.Instance?.PlayButtonHover(); });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { AudioSystem.Instance?.StopHoverSFX(); });
        trigger.triggers.Add(exitEntry);
    }

    // ── Animation ──────────────────────────────────────────────────────────

    private IEnumerator AnimOpen()
    {
        if (_cg == null || _panelRT == null) yield break;
        _panelRT.localScale = Vector3.one * 0.85f;
        _cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            _panelRT.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, t);
            _cg.alpha = t;
            yield return null;
        }
        _panelRT.localScale = Vector3.one;
        _cg.alpha = 1f;
    }

    private IEnumerator AnimClose()
    {
        if (_cg == null || _panelRT == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        float elapsed = 0f;
        Vector3 startScale = _panelRT.localScale;
        float startAlpha = _cg.alpha;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / closeDuration);
            _panelRT.localScale = Vector3.Lerp(startScale, Vector3.one * 0.85f, t);
            _cg.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
