using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class EnemyAnimationSetupUtility
{
    // ── Sprite root & naming conventions ────────────────────────────────────
    private static readonly string SpritesRoot = "Assets/_Project/Sprites/Characters/Enemies";
    private static readonly string AnimationsRoot = "Assets/_Project/Animations/Enemies";
    private static readonly string DataRoot = "Assets/_Project/Data/Enemies";

    // Map from sprite sheet filename (without extension) to clean enemy name
    private static readonly Dictionary<string, string> NameOverrides = new()
    {
        { "coupler",          "Coupler"    },
        { "Corrupted",        "Corrupted"  },
        { "Collector",        "Collector"  },
        { "leech-Sheet",      "Leech"      },
        { "sniffer-Sheet",    "Sniffer"    },
        { "bad_sector-Sheet", "BadSector"  },
        { "inject-Sheet",     "Inject"     },
        { "bloat-Sheet",      "Bloat"      },
        { "overclock-Sheet",  "Overclock"  },
        { "spooler-Sheet",    "Spooler"    },
        { "surge-Sheet",      "Surge"      },
        { "kernel-Sheet",     "Kernel"     },
    };

    // Placeholder stats per act tier (HP, ATK, BLK, BUFF)
    private static readonly Dictionary<string, int[]> EnemyStats = new()
    {
        // Act 1 — already exist, won't be overwritten
        { "Coupler",   new[] { 25, 6, 5, 2 } },
        { "Corrupted", new[] { 30, 8, 7, 3 } },
        { "Collector", new[] { 60, 10, 8, 4 } },
        // Act 2 Standard
        { "Leech",     new[] { 35, 8, 8, 3 } },
        { "Sniffer",   new[] { 40, 10, 6, 2 } },
        { "BadSector", new[] { 55, 12, 12, 4 } },
        // Act 2 Boss
        { "Inject",    new[] { 100, 14, 12, 5 } },
        // Act 3 Standard
        { "Bloat",     new[] { 55, 14, 12, 4 } },
        { "Overclock", new[] { 60, 18, 10, 3 } },
        { "Spooler",   new[] { 50, 12, 15, 5 } },
        { "Surge",     new[] { 65, 16, 14, 4 } },
        // Act 3 Boss
        { "Kernel",    new[] { 150, 20, 18, 6 } },
    };

    // ── Main Entry Point ───────────────────────────────────────────────────

    [MenuItem("Tools/Setup Enemy Animations")]
    public static void SetupAllEnemyAnimations()
    {
        int processed = 0;

        // Scan Act 1, Act 2, Act 3
        for (int act = 1; act <= 3; act++)
        {
            string actFolder = $"{SpritesRoot}/Act {act}";
            if (!AssetDatabase.IsValidFolder(actFolder)) continue;

            // Scan Basic and Boss subfolders
            foreach (string role in new[] { "Basic", "Boss" })
            {
                string roleFolder = $"{actFolder}/{role}";
                if (!AssetDatabase.IsValidFolder(roleFolder)) continue;

                // Find all .png sprite sheets
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { roleFolder });
                foreach (string guid in guids)
                {
                    string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!texturePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;

                    string fileNameNoExt = Path.GetFileNameWithoutExtension(texturePath);
                    string cleanName = NameOverrides.ContainsKey(fileNameNoExt)
                        ? NameOverrides[fileNameNoExt]
                        : CleanFileName(fileNameNoExt);

                    string animOutputFolder = $"{AnimationsRoot}/Act {act}/{role}/{cleanName}";
                    string dataFolder = $"{DataRoot}/Act {act}";
                    string enemyDataPath = $"{dataFolder}/{cleanName}.asset";

                    Debug.Log($"[AnimSetup] Processing: {cleanName} (Act {act} {role}) from {texturePath}");

                    // Setup animation clips + animator controller
                    SetupEnemyAnimation(
                        spriteTexturePath: texturePath,
                        outputFolder: animOutputFolder,
                        enemyPrefix: cleanName,
                        controllerName: $"{cleanName}_Animator",
                        enemyDataPath: enemyDataPath,
                        dataFolder: dataFolder,
                        cleanName: cleanName
                    );

                    processed++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[AnimSetup] ✓ Enemy animation setup completed! Processed {processed} enemies.");
    }

    // ── Core Setup ─────────────────────────────────────────────────────────

    private static void SetupEnemyAnimation(
        string spriteTexturePath,
        string outputFolder,
        string enemyPrefix,
        string controllerName,
        string enemyDataPath,
        string dataFolder,
        string cleanName)
    {
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            CreateFolderRecursively(outputFolder);
        }

        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spriteTexturePath);
        Sprite[] sprites = assets.OfType<Sprite>()
            .OrderBy(s => ExtractFrameNumber(s.name))
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError($"No sprites found at {spriteTexturePath}");
            return;
        }

        // Split frames: 8 frames for Idle, remaining frames for Attack
        Sprite[] idleSprites = sprites.Take(8).ToArray();
        Sprite[] attackSprites = sprites.Skip(8).ToArray();

        if (attackSprites.Length == 0)
        {
            Debug.LogWarning($"[AnimSetup] {cleanName}: No attack frames found (only {sprites.Length} total frames). Using last 2 idle frames as attack.");
            attackSprites = sprites.Skip(Mathf.Max(0, sprites.Length - 2)).ToArray();
        }

        AnimationClip idleClip = CreateOrUpdateClip(outputFolder, $"{enemyPrefix}_Idle", idleSprites, isLooping: true);
        AnimationClip attackClip = CreateOrUpdateClip(outputFolder, $"{enemyPrefix}_Attack", attackSprites, isLooping: false);

        // ── Animator Controller ────────────────────────────────────────────
        string controllerPath = $"{outputFolder}/{controllerName}.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        if (controller.layers.Length == 0)
        {
            controller.AddLayer("Base Layer");
        }

        // Ensure "Attack" Trigger parameter exists
        if (!controller.parameters.Any(p => p.name == "Attack"))
        {
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        }

        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // Idle State
        AnimatorState idleState = rootStateMachine.states
            .Select(s => s.state)
            .FirstOrDefault(s => s.name == "Idle");

        if (idleState == null)
        {
            idleState = rootStateMachine.AddState("Idle");
        }
        idleState.motion = idleClip;
        rootStateMachine.defaultState = idleState;

        // Attack State
        AnimatorState attackState = rootStateMachine.states
            .Select(s => s.state)
            .FirstOrDefault(s => s.name == "Attack");

        if (attackState == null)
        {
            attackState = rootStateMachine.AddState("Attack");
        }
        attackState.motion = attackClip;

        // Idle -> Attack transition on "Attack" trigger
        AnimatorStateTransition idleToAttack = idleState.transitions
            .FirstOrDefault(t => t.destinationState == attackState);
        if (idleToAttack == null)
        {
            idleToAttack = idleState.AddTransition(attackState);
        }
        idleToAttack.hasExitTime = false;
        idleToAttack.duration = 0f;
        idleToAttack.conditions = new AnimatorCondition[0];
        idleToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");

        // Attack -> Idle transition when attack clip finishes
        AnimatorStateTransition attackToIdle = attackState.transitions
            .FirstOrDefault(t => t.destinationState == idleState);
        if (attackToIdle == null)
        {
            attackToIdle = attackState.AddTransition(idleState);
        }
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 1f;
        attackToIdle.duration = 0f;

        // Clean up legacy states/params
        AnimatorState takeDamageState = rootStateMachine.states
            .Select(s => s.state)
            .FirstOrDefault(s => s.name == "TakeDamage");
        if (takeDamageState != null) rootStateMachine.RemoveState(takeDamageState);

        var takeDamageParam = controller.parameters.FirstOrDefault(p => p.name == "TakeDamage");
        if (takeDamageParam != null) controller.RemoveParameter(takeDamageParam);

        EditorUtility.SetDirty(controller);

        // ── EnemyData Asset ────────────────────────────────────────────────
        EnemyData enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(enemyDataPath);

        if (enemyData == null)
        {
            // Auto-create a new EnemyData ScriptableObject
            if (!AssetDatabase.IsValidFolder(dataFolder))
            {
                CreateFolderRecursively(dataFolder);
            }

            enemyData = ScriptableObject.CreateInstance<EnemyData>();
            AssetDatabase.CreateAsset(enemyData, enemyDataPath);
            Debug.Log($"[AnimSetup] Created new EnemyData: {enemyDataPath}");
        }

        // Wire up the animator controller and set placeholder stats
        SerializedObject so = new SerializedObject(enemyData);

        // AnimatorController
        SerializedProperty animProp = so.FindProperty("<AnimatorController>k__BackingField");
        if (animProp != null) animProp.objectReferenceValue = controller;

        // Image (first sprite from sheet)
        SerializedProperty imageProp = so.FindProperty("<Image>k__BackingField");
        if (imageProp != null && sprites.Length > 0) imageProp.objectReferenceValue = sprites[0];

        // Only set stats if they are currently zero (don't overwrite manually tuned values)
        SerializedProperty healthProp = so.FindProperty("<Health>k__BackingField");
        if (healthProp != null && healthProp.intValue == 0 && EnemyStats.ContainsKey(cleanName))
        {
            int[] stats = EnemyStats[cleanName];
            healthProp.intValue = stats[0];

            var atkProp = so.FindProperty("<AttackPower>k__BackingField");
            if (atkProp != null) atkProp.intValue = stats[1];

            var blkProp = so.FindProperty("<BlockPower>k__BackingField");
            if (blkProp != null) blkProp.intValue = stats[2];

            var buffProp = so.FindProperty("<BuffAmount>k__BackingField");
            if (buffProp != null) buffProp.intValue = stats[3];

            // Default MovePool if empty
            var movePoolProp = so.FindProperty("<MovePool>k__BackingField");
            if (movePoolProp != null && movePoolProp.arraySize == 0)
            {
                movePoolProp.arraySize = 3;

                // Attack (weight 5)
                var move0 = movePoolProp.GetArrayElementAtIndex(0);
                move0.FindPropertyRelative("Intent").enumValueIndex = 0; // Attack
                move0.FindPropertyRelative("Weight").intValue = 5;
                move0.FindPropertyRelative("OverrideValue").intValue = 0;

                // Defend (weight 3)
                var move1 = movePoolProp.GetArrayElementAtIndex(1);
                move1.FindPropertyRelative("Intent").enumValueIndex = 1; // Defend
                move1.FindPropertyRelative("Weight").intValue = 3;
                move1.FindPropertyRelative("OverrideValue").intValue = 0;

                // Buff (weight 1)
                var move2 = movePoolProp.GetArrayElementAtIndex(2);
                move2.FindPropertyRelative("Intent").enumValueIndex = 3; // Buff
                move2.FindPropertyRelative("Weight").intValue = 1;
                move2.FindPropertyRelative("OverrideValue").intValue = 0;
            }

            Debug.Log($"[AnimSetup] Set placeholder stats for {cleanName}: HP={stats[0]} ATK={stats[1]} BLK={stats[2]} BUFF={stats[3]}");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(enemyData);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static AnimationClip CreateOrUpdateClip(string outputFolder, string clipName, Sprite[] frameSprites, bool isLooping)
    {
        string clipPath = $"{outputFolder}/{clipName}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        bool isNewClip = false;

        if (clip == null)
        {
            clip = new AnimationClip();
            isNewClip = true;
        }

        clip.frameRate = 12f;

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = isLooping;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        float frameDuration = 1f / 12f;
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frameSprites.Length];
        for (int i = 0; i < frameSprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameDuration,
                value = frameSprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        if (isNewClip)
        {
            AssetDatabase.CreateAsset(clip, clipPath);
        }
        else
        {
            EditorUtility.SetDirty(clip);
        }

        return clip;
    }

    private static int ExtractFrameNumber(string name)
    {
        Match match = Regex.Match(name, @"_(\d+)$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int result))
        {
            return result;
        }
        return 0;
    }

    /// <summary>
    /// Converts a file name like "bad_sector-Sheet" to "BadSector".
    /// </summary>
    private static string CleanFileName(string fileName)
    {
        // Remove "-Sheet" suffix
        fileName = fileName.Replace("-Sheet", "").Replace("-sheet", "");
        // PascalCase: split on _ and -, capitalize first letter of each part
        string[] parts = fileName.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", parts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));
    }

    private static void CreateFolderRecursively(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string currentPath = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string nextPath = currentPath + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, parts[i]);
            }
            currentPath = nextPath;
        }
    }
}
