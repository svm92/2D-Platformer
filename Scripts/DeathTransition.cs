using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathTransition : MonoBehaviour {

    Image img;

    private void Start()
    {
        img = GetComponent<Image>();
        StartCoroutine(startDeathTransition());
    }

    IEnumerator startDeathTransition()
    {
        Color originalColor = img.color;
        Color targetColor = Color.white;
        float transitionTime = 2.5f;
        float timer = 0;
        while (timer < transitionTime)
        {
            img.color = Color.Lerp(originalColor, targetColor, timer / transitionTime);
            timer += Time.deltaTime;
            yield return null;
        }
        
        SceneManager.LoadScene("MainMenu");
    }

}
