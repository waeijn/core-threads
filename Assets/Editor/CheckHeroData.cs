using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CheckHeroData
{
    static CheckHeroData()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        string assetPath = "Assets/_Project/Data/Heroes/DefaultHero.asset";
        HeroData heroData = AssetDatabase.LoadAssetAtPath<HeroData>(assetPath);
        if (heroData != null)
        {
            Debug.Log($"Antigravity Audit: Attack={heroData.AttackSound?.name}, Heal={heroData.HealSound?.name}, Buff={heroData.BuffSound?.name}, Discard={heroData.CardDiscardSound?.name}, Draw={heroData.CardDrawSound?.name}");
            
            string[] guids = AssetDatabase.FindAssets("t:CardData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var card = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (card != null && card.PlaySound != null)
                {
                    Debug.Log($"Antigravity Audit: Card '{card.name}' has PlaySound={card.PlaySound.name}");
                }
            }
        }
    }
}
