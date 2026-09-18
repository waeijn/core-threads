using UnityEditor;
using UnityEngine;
using System.Linq;

[InitializeOnLoad]
public class CheckEnemySizes
{
    static CheckEnemySizes()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        string[] names = { "BadSector", "Inject", "Leech", "Spooler", "Surge", "Bloat", "Kernel", "Sniffer" };
        foreach (string name in names)
        {
            string assetPath = $"Assets/_Project/Data/Enemies/{name}.asset";
            EnemyData ed = AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath);
            if (ed != null && ed.Image != null)
            {
                Debug.Log($"Antigravity: {name} - Native Size: {ed.Image.rect.width}x{ed.Image.rect.height} - Current Scale: {ed.Scale}");
            }
        }
    }
}
