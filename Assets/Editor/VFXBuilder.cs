using UnityEngine;
using UnityEditor;

public class VFXBuilder : Editor
{
    [MenuItem("Tools/Build Digital VFX Prefabs")]
    public static void BuildVFX()
    {
        string dir = "Assets/_Project/Prefabs/VFX";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        // We will use the default particle material (which is a blurred circle),
        // or a sprite if we can find one. Let's look for a square sprite to look digital.
        Material defaultMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
        Material squareMat = new Material(Shader.Find("Particles/Standard Unlit"));
        // Create a basic square material in the folder
        AssetDatabase.CreateAsset(squareMat, dir + "/DigitalSquareMat.mat");

        CreateHitVFX(dir);
        CreateBlockVFX(dir);
        CreateHealVFX(dir);
        CreateBuffVFX(dir);

        AssetDatabase.SaveAssets();
        Debug.Log("Digital VFX Prefabs Built successfully!");
    }

    private static void CreateHitVFX(string dir)
    {
        GameObject go = new GameObject("HitVFX");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = Color.red;
        main.playOnAwake = true;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15, 25) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = AssetDatabase.LoadAssetAtPath<Material>(dir + "/DigitalSquareMat.mat");
        renderer.sortingOrder = 100; // Render on top

        go.AddComponent<AutoDestroyVFX>();

        PrefabUtility.SaveAsPrefabAsset(go, dir + "/HitVFX.prefab");
        DestroyImmediate(go);
    }

    private static void CreateBlockVFX(string dir)
    {
        GameObject go = new GameObject("BlockVFX");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startColor = new Color(0.2f, 0.6f, 1f, 1f); // Blue
        main.playOnAwake = true;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = AssetDatabase.LoadAssetAtPath<Material>(dir + "/DigitalSquareMat.mat");
        renderer.sortingOrder = 100;

        go.AddComponent<AutoDestroyVFX>();

        PrefabUtility.SaveAsPrefabAsset(go, dir + "/BlockVFX.prefab");
        DestroyImmediate(go);
    }

    private static void CreateHealVFX(string dir)
    {
        GameObject go = new GameObject("HealVFX");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startColor = Color.green;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(2f, 4f); // Float upwards

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = AssetDatabase.LoadAssetAtPath<Material>(dir + "/DigitalSquareMat.mat");
        renderer.sortingOrder = 100;

        go.AddComponent<AutoDestroyVFX>();

        PrefabUtility.SaveAsPrefabAsset(go, dir + "/HealVFX.prefab");
        DestroyImmediate(go);
    }

    private static void CreateBuffVFX(string dir)
    {
        GameObject go = new GameObject("BuffVFX");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startColor = new Color(1f, 0.8f, 0.2f, 1f); // Gold/Yellow
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(1f, 3f); // Float upwards

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = AssetDatabase.LoadAssetAtPath<Material>(dir + "/DigitalSquareMat.mat");
        renderer.sortingOrder = 100;

        go.AddComponent<AutoDestroyVFX>();

        PrefabUtility.SaveAsPrefabAsset(go, dir + "/BuffVFX.prefab");
        DestroyImmediate(go);
    }
}
