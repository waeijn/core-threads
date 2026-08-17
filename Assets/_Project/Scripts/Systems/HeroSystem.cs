using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    [field:SerializeField] public HeroView HeroView { get; private set; }

    public void Setup(HeroData heroData)
    {
        HeroView.Setup(heroData);
    }
}
