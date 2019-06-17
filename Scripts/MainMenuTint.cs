using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuTint : MonoBehaviour {

    GameObject imgCanvas;
    Image img;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (this != null)
        {
            img = GetComponent<Image>();
            imgCanvas = img.transform.parent.gameObject;
            StartCoroutine(fadeToClear());
        }
    }

    IEnumerator fadeToClear()
    {
        yield return fadeToColor(Color.clear);
        Destroy(imgCanvas);
    }

    IEnumerator fadeToColor(Color targetColor)
    {
        Color originalColor = img.color;
        float transitionTime = 2.5f;
        float timer = 0;
        while (timer < transitionTime)
        {
            if (PlayerController.gamePaused)
            {
                img.color = Color.clear;
                yield return null;
                continue;
            }

            img.color = Color.Lerp(originalColor, targetColor, timer / transitionTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

}
