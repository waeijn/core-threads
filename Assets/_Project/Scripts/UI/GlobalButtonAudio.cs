using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GlobalButtonAudio : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(ContinuousButtonHookup());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopAllCoroutines();
    }

    private System.Collections.IEnumerator ContinuousButtonHookup()
    {
        while (true)
        {
            CheckAudioListener();
            HookupAllButtons();
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void CheckAudioListener()
    {
        var listeners = FindObjectsOfType<AudioListener>();
        var myListener = GetComponent<AudioListener>();

        if (listeners.Length == 0 && myListener == null)
        {
            gameObject.AddComponent<AudioListener>();
        }
        else if (listeners.Length > 1 && myListener != null)
        {
            myListener.enabled = false;
        }
        else if (listeners.Length == 1 && myListener != null)
        {
            myListener.enabled = true;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAudioListener();
        HookupAllButtons();
    }

    private void HookupAllButtons()
    {
        var buttons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (var btn in buttons)
        {
            if (btn.gameObject.scene.name == null) continue;
            HookupButton(btn);
        }
    }

    private void HookupButton(Button btn)
    {
        if (btn.gameObject.GetComponent<ButtonAudioHooked>() != null) return;
        btn.gameObject.AddComponent<ButtonAudioHooked>();

        btn.onClick.AddListener(() => {
            if (AudioSystem.Instance != null) AudioSystem.Instance.PlayButtonClick();
        });
    }
}

public class ButtonAudioHooked : MonoBehaviour { }
