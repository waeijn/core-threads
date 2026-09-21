using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SpriteSheetImporter : Editor
{
    [MenuItem("Tools/Force Slice All Enemy Sprites")]
    public static void SliceAllSprites()
    {
        string rootDir = "Assets/_Project/Sprites/Characters/Enemies";
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { rootDir });

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                // Force basic settings
                importer.spritePixelsPerUnit = 64;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.isReadable = true;

                // Force 64x64 Grid Slicing
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    int width = tex.width;
                    int height = tex.height;
                    int cellWidth = 64;
                    int cellHeight = 64;

                    int cols = width / cellWidth;
                    int rows = height / cellHeight;

                    List<SpriteMetaData> metaDataList = new List<SpriteMetaData>();

                    int frameIndex = 0;
                    // Slicing from top-left to bottom-right (typical reading order)
                    for (int y = rows - 1; y >= 0; y--)
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            SpriteMetaData smd = new SpriteMetaData();
                            smd.pivot = new Vector2(0.5f, 0.5f);
                            smd.alignment = 0; // Center
                            smd.name = $"{tex.name}_{frameIndex}";
                            smd.rect = new Rect(x * cellWidth, y * cellHeight, cellWidth, cellHeight);
                            metaDataList.Add(smd);
                            frameIndex++;
                        }
                    }

                    importer.spritesheet = metaDataList.ToArray();
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    
                    Debug.Log($"[SpriteSheetImporter] Sliced {path} into {frameIndex} frames.");
                    count++;
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SpriteSheetImporter] Sliced {count} sprite sheets successfully!");
    }
}
