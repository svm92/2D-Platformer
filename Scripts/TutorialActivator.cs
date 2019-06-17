using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialActivator : MonoBehaviour {

    public string tutoText;
    public int requiredGlobalVariable = -1;

    GameObject tutoTextCanvas;
    Text tutoTextContainer;
    GameObject player;
    PlayerController playerController;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        if (playerController.globalVariables[requiredGlobalVariable])
            Destroy(gameObject);

        tutoTextCanvas = player.transform.GetChild(3).gameObject;
        tutoTextContainer = tutoTextCanvas.transform.GetChild(0).GetComponent<Text>();
    }

    IEnumerator fadeTextIn()
    {
        StartCoroutine(fadeText(Color.white, false));
        yield break;
    }

    IEnumerator fadeTextOut()
    {
        StartCoroutine(fadeText(Color.clear, true));
        yield break;
    }

    IEnumerator fadeText(Color destinationColor, bool leaving)
    {
        Color originalColor = tutoTextContainer.color;
        float fadeInTime = 0.5f;
        float timer = 0;
        while (tutoTextContainer.color != destinationColor)
        {
            tutoTextContainer.color = Color.Lerp(originalColor, destinationColor, timer / fadeInTime);
            timer += Time.deltaTime;
            yield return null;
        }

        if (leaving)
        {
            tutoTextCanvas.SetActive(false);
            checkIfDestroyThisArea();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            showTutoText();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            hideTutoText();
    }

    void showTutoText()
    {
        if (!tutorialConditionFulfilled())
            return;

        StopAllCoroutines();
        tutoTextCanvas.SetActive(true);
        StartCoroutine(fadeTextIn());
        tutoTextContainer.text = tutoText;
    }

    void hideTutoText()
    {
        if (!tutorialConditionFulfilled())
            return;

        StopAllCoroutines();
        StartCoroutine(fadeTextOut());
    }

    void checkIfDestroyThisArea()
    {
        bool tutorialCompleted = false;
        switch (requiredGlobalVariable)
        {
            case 120:
                if (player.transform.position.x >= 9.5f)
                    tutorialCompleted = true;
                break;
            case 121:
                if (player.transform.position.x >= -94)
                    tutorialCompleted = true;
                break;
            case 122:
                if (player.transform.position.x >= -78)
                    tutorialCompleted = true;
                break;
        }

        if (tutorialCompleted)
        {
            playerController.globalVariables[requiredGlobalVariable] = true;
            Destroy(gameObject);
        }
    }

    bool tutorialConditionFulfilled() // This must be true for the tutorial to appear
    {
        switch (requiredGlobalVariable)
        {
            case 122:
                return (playerController.unlockedColors[0]);
        }
        return true;
    }

}
