using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;

    public bool PlayerCanInteract()
    {
        if (GameOverSystem.Instance != null && GameOverSystem.Instance.IsGameOver) return false;
        if (!ActionSystem.Instance.IsPerforming) return true;
        else return false;
    }

    public bool PlayerCanHover()
    {
        if (GameOverSystem.Instance != null && GameOverSystem.Instance.IsGameOver) return false;
        if (PlayerIsDragging) return false;
        return true;
    }
}
