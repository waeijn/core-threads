using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class EnemyAnimationSetupUtility
{
    [MenuItem("Tools/Setup Enemy Animations")]
    public static void SetupAllEnemyAnimations()
    {
        SetupEnemyAnimation(
            spriteTexturePath: "Assets/_Project/Sprites/Characters/Enemies/Basic/coupler.png",
            outputFolder: "Assets/_Project/Animations/Enemies/Basic/Coupler",
            enemyPrefix: "Coupler",
            controllerName: "Coupler_Animator",
            enemyDataPath: "Assets/_Project/Data/Enemies/Coupler.asset"
        );

        SetupEnemyAnimation(
            spriteTexturePath: "Assets/_Project/Sprites/Characters/Enemies/Basic/Corrupted.png",
            outputFolder: "Assets/_Project/Animations/Enemies/Basic/Corrupted",
            enemyPrefix: "Corrupted",
            controllerName: "Corrupted_Animator",
            enemyDataPath: "Assets/_Project/Data/Enemies/Corrupted.asset"
        );

        SetupEnemyAnimation(
            spriteTexturePath: "Assets/_Project/Sprites/Characters/Enemies/Boss/Collector.png",
            outputFolder: "Assets/_Project/Animations/Enemies/Boss/Collector",
            enemyPrefix: "Collector",
            controllerName: "Collector_Animator",
            enemyDataPath: "Assets/_Project/Data/Enemies/Collector.asset"
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Enemy animation setup completed successfully!");
    }

    private static void SetupEnemyAnimation(
        string spriteTexturePath,
        string outputFolder,
        string enemyPrefix,
        string controllerName,
        string enemyDataPath)
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

        AnimationClip idleClip = CreateOrUpdateClip(outputFolder, $"{enemyPrefix}_Idle", idleSprites, isLooping: true);
        AnimationClip attackClip = CreateOrUpdateClip(outputFolder, $"{enemyPrefix}_Attack", attackSprites, isLooping: false);

        // Setup Animator Controller
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

        // Clean up legacy TakeDamage state if present
        AnimatorState takeDamageState = rootStateMachine.states
            .Select(s => s.state)
            .FirstOrDefault(s => s.name == "TakeDamage");
        if (takeDamageState != null)
        {
            rootStateMachine.RemoveState(takeDamageState);
        }

        // Clean up legacy TakeDamage parameter if present
        var takeDamageParam = controller.parameters.FirstOrDefault(p => p.name == "TakeDamage");
        if (takeDamageParam != null)
        {
            controller.RemoveParameter(takeDamageParam);
        }

        EditorUtility.SetDirty(controller);

        EnemyData enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(enemyDataPath);
        if (enemyData != null)
        {
            SerializedObject serializedEnemyData = new SerializedObject(enemyData);
            SerializedProperty animProp = serializedEnemyData.FindProperty("<AnimatorController>k__BackingField");

            if (animProp != null)
            {
                animProp.objectReferenceValue = controller;
                serializedEnemyData.ApplyModifiedProperties();
                EditorUtility.SetDirty(enemyData);
            }
        }
    }

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
