using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class FullEnemySetup
{
    static FullEnemySetup()
    {
        EditorApplication.delayCall += Run;
    }

    private struct EnemyDef
    {
        public string name;
        public string spriteSheet;
        public int hp, atk, blk, buff;
        public float scaleXY;
        public string tier; // weak, normal, boss
    }

    private static void Run()
    {
        EnemyDef[] act2 = new EnemyDef[]
        {
            new EnemyDef { name="BadSector", spriteSheet="bad_sector-Sheet", hp=28, atk=6, blk=4, buff=2, scaleXY=3f, tier="weak" },
            new EnemyDef { name="Inject", spriteSheet="inject-Sheet", hp=30, atk=7, blk=3, buff=2, scaleXY=3f, tier="weak" },
            new EnemyDef { name="Leech", spriteSheet="leech-Sheet", hp=35, atk=8, blk=5, buff=3, scaleXY=3.5f, tier="normal" },
            new EnemyDef { name="Spooler", spriteSheet="spooler-Sheet", hp=38, atk=9, blk=6, buff=3, scaleXY=3.5f, tier="normal" },
            new EnemyDef { name="Surge", spriteSheet="surge-Sheet", hp=40, atk=10, blk=5, buff=3, scaleXY=4f, tier="normal" },
            new EnemyDef { name="Bloat", spriteSheet="bloat-Sheet", hp=42, atk=9, blk=7, buff=4, scaleXY=4f, tier="normal" },
            new EnemyDef { name="Kernel", spriteSheet="kernel-Sheet", hp=55, atk=11, blk=8, buff=4, scaleXY=5f, tier="boss" },
            new EnemyDef { name="Sniffer", spriteSheet="sniffer-Sheet", hp=65, atk=12, blk=8, buff=4, scaleXY=5.5f, tier="boss" },
        };

        // Step 1: Create EnemyData assets
        foreach (EnemyDef def in act2)
        {
            string path = "Assets/_Project/Data/Enemies/" + def.name + ".asset";
            EnemyData ed = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (ed == null)
            {
                ed = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(ed, path);
            }

            SerializedObject so = new SerializedObject(ed);
            so.FindProperty("<Health>k__BackingField").intValue = def.hp;
            so.FindProperty("<AttackPower>k__BackingField").intValue = def.atk;
            so.FindProperty("<BlockPower>k__BackingField").intValue = def.blk;
            so.FindProperty("<BuffAmount>k__BackingField").intValue = def.buff;
            so.FindProperty("<Scale>k__BackingField").vector3Value = new Vector3(def.scaleXY, def.scaleXY, 1f);

            SerializedProperty pool = so.FindProperty("<MovePool>k__BackingField");
            pool.ClearArray();
            AddMove(pool, 0, 0, 4, 0);
            AddMove(pool, 1, 1, 2, 0);
            AddMove(pool, 2, 2, 2, 0);
            AddMove(pool, 3, 3, 1, 0);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(ed);
        }

        AssetDatabase.SaveAssets();

        // Step 2: Setup animations and assign sprites
        foreach (EnemyDef def in act2)
        {
            string spritePath = "Assets/_Project/Sprites/Characters/Enemies/Act2/" + def.spriteSheet + ".png";
            string outputFolder = "Assets/_Project/Animations/Enemies/Act2/" + def.name;
            string controllerName = def.name + "_Animator";
            string enemyDataPath = "Assets/_Project/Data/Enemies/" + def.name + ".asset";

            SetupAnimation(spritePath, outputFolder, def.name, controllerName, enemyDataPath);

            // Assign first sprite as Image
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            Sprite[] sprites = allAssets.OfType<Sprite>().OrderBy(s => ExtractFrame(s.name)).ToArray();
            if (sprites.Length > 0)
            {
                EnemyData ed = AssetDatabase.LoadAssetAtPath<EnemyData>(enemyDataPath);
                if (ed != null)
                {
                    SerializedObject so = new SerializedObject(ed);
                    so.FindProperty("<Image>k__BackingField").objectReferenceValue = sprites[0];
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(ed);
                }
            }
        }

        // Step 3: Wire MapConfig
        MapConfig cfg = AssetDatabase.LoadAssetAtPath<MapConfig>("Assets/_Project/Data/Maps/MapConfig.asset");
        if (cfg == null)
        {
            Debug.LogError("Antigravity: MapConfig not found!");
            return;
        }

        cfg.act1WeakEnemies = new List<EnemyData>();
        cfg.act1NormalEnemies = new List<EnemyData>();
        cfg.act1BossEnemies = new List<EnemyData>();
        cfg.act2WeakEnemies = new List<EnemyData>();
        cfg.act2NormalEnemies = new List<EnemyData>();
        cfg.act2BossEnemies = new List<EnemyData>();
        cfg.act3WeakEnemies = new List<EnemyData>();
        cfg.act3NormalEnemies = new List<EnemyData>();
        cfg.act3BossEnemies = new List<EnemyData>();

        // Act 1
        AddIfNotNull(cfg.act1WeakEnemies, "Assets/_Project/Data/Enemies/Coupler.asset");
        AddIfNotNull(cfg.act1NormalEnemies, "Assets/_Project/Data/Enemies/Corrupted.asset");
        AddIfNotNull(cfg.act1BossEnemies, "Assets/_Project/Data/Enemies/Collector.asset");

        // Act 2
        foreach (EnemyDef def in act2)
        {
            string path = "Assets/_Project/Data/Enemies/" + def.name + ".asset";
            EnemyData ed = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (ed == null) continue;
            if (def.tier == "weak") cfg.act2WeakEnemies.Add(ed);
            else if (def.tier == "normal") cfg.act2NormalEnemies.Add(ed);
            else if (def.tier == "boss") cfg.act2BossEnemies.Add(ed);
        }

        // Act 3: reuse Act 2 normals as weak, Act 2 bosses as normal, Collector as placeholder boss
        cfg.act3WeakEnemies = new List<EnemyData>(cfg.act2NormalEnemies);
        cfg.act3NormalEnemies = new List<EnemyData>(cfg.act2BossEnemies);
        AddIfNotNull(cfg.act3BossEnemies, "Assets/_Project/Data/Enemies/Collector.asset");

        EditorUtility.SetDirty(cfg);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Antigravity: SUCCESS! 8 Act 2 enemies created, animations set up, MapConfig wired.");
    }

    private static void AddMove(SerializedProperty pool, int idx, int intent, int weight, int overrideVal)
    {
        pool.InsertArrayElementAtIndex(idx);
        SerializedProperty m = pool.GetArrayElementAtIndex(idx);
        m.FindPropertyRelative("Intent").intValue = intent;
        m.FindPropertyRelative("Weight").intValue = weight;
        m.FindPropertyRelative("OverrideValue").intValue = overrideVal;
    }

    private static void AddIfNotNull(List<EnemyData> list, string path)
    {
        EnemyData ed = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
        if (ed != null) list.Add(ed);
    }

    private static void SetupAnimation(string spritePath, string outFolder, string prefix, string ctrlName, string edPath)
    {
        if (!AssetDatabase.IsValidFolder(outFolder))
            CreateFolders(outFolder);

        Object[] all = AssetDatabase.LoadAllAssetsAtPath(spritePath);
        Sprite[] sprites = all.OfType<Sprite>().OrderBy(s => ExtractFrame(s.name)).ToArray();
        if (sprites.Length == 0) { Debug.LogWarning("No sprites: " + spritePath); return; }

        Sprite[] idle = sprites.Take(8).ToArray();
        Sprite[] attack = sprites.Skip(8).ToArray();
        if (attack.Length == 0) attack = idle;

        AnimationClip idleClip = MakeClip(outFolder, prefix + "_Idle", idle, true);
        AnimationClip atkClip = MakeClip(outFolder, prefix + "_Attack", attack, false);

        string ctrlPath = outFolder + "/" + ctrlName + ".controller";
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
        if (ctrl == null) ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        if (ctrl.layers.Length == 0) ctrl.AddLayer("Base Layer");
        if (!ctrl.parameters.Any(p => p.name == "Attack"))
            ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;

        AnimatorState idleSt = sm.states.Select(s => s.state).FirstOrDefault(s => s.name == "Idle");
        if (idleSt == null) idleSt = sm.AddState("Idle");
        idleSt.motion = idleClip;
        sm.defaultState = idleSt;

        AnimatorState atkSt = sm.states.Select(s => s.state).FirstOrDefault(s => s.name == "Attack");
        if (atkSt == null) atkSt = sm.AddState("Attack");
        atkSt.motion = atkClip;

        AnimatorStateTransition t1 = idleSt.transitions.FirstOrDefault(t => t.destinationState == atkSt);
        if (t1 == null) t1 = idleSt.AddTransition(atkSt);
        t1.hasExitTime = false; t1.duration = 0f;
        t1.conditions = new AnimatorCondition[0];
        t1.AddCondition(AnimatorConditionMode.If, 0, "Attack");

        AnimatorStateTransition t2 = atkSt.transitions.FirstOrDefault(t => t.destinationState == idleSt);
        if (t2 == null) t2 = atkSt.AddTransition(idleSt);
        t2.hasExitTime = true; t2.exitTime = 1f; t2.duration = 0f;

        EditorUtility.SetDirty(ctrl);

        EnemyData ed = AssetDatabase.LoadAssetAtPath<EnemyData>(edPath);
        if (ed != null)
        {
            SerializedObject so = new SerializedObject(ed);
            SerializedProperty ap = so.FindProperty("<AnimatorController>k__BackingField");
            if (ap != null) { ap.objectReferenceValue = ctrl; so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(ed); }
        }
    }

    private static AnimationClip MakeClip(string folder, string name, Sprite[] frames, bool loop)
    {
        string path = folder + "/" + name + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        bool isNew = clip == null;
        if (isNew) clip = new AnimationClip();
        clip.frameRate = 12f;
        AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, s);

        EditorCurveBinding b = new EditorCurveBinding();
        b.type = typeof(SpriteRenderer);
        b.path = "";
        b.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] kf = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            kf[i] = new ObjectReferenceKeyframe();
            kf[i].time = i / 12f;
            kf[i].value = frames[i];
        }
        AnimationUtility.SetObjectReferenceCurve(clip, b, kf);

        if (isNew) AssetDatabase.CreateAsset(clip, path);
        else EditorUtility.SetDirty(clip);
        return clip;
    }

    private static int ExtractFrame(string name)
    {
        Match m = Regex.Match(name, @"_(\d+)$");
        if (m.Success && int.TryParse(m.Groups[1].Value, out int r)) return r;
        return 0;
    }

    private static void CreateFolders(string path)
    {
        string[] parts = path.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }
}
