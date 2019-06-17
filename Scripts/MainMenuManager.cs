using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

    public static string savegamePath;

    GameObject lastSelectedObject;
    GameObject imgCanvas;
    Image img;

    public static GameObject mainTheme;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // C:\Users\[user]\AppData\LocalLow\SamuelVazquez\SketchedChroma
        // Can't be called from constructor
        savegamePath = Application.persistentDataPath + "/colorPlatformerData.dat";

        GameObject[] dontDestroyOnLoadObjects = new GameObject[] { GameObject.FindGameObjectWithTag("Player"),
            GameObject.FindGameObjectWithTag("MainCamera"), GameObject.Find("MusicManager")};

        foreach (GameObject g in dontDestroyOnLoadObjects)
            if (g != null)
                Destroy(g);
        
        img = GameObject.Find("MainMenuImage").GetComponent<Image>();
        imgCanvas = img.transform.parent.gameObject;

        if (mainTheme == null)
        {
            mainTheme = GameObject.FindGameObjectWithTag("MainTheme");
            DontDestroyOnLoad(mainTheme);
        } else
        {
            Destroy(GameObject.FindGameObjectsWithTag("MainTheme")[1]);
        }
    }

    private void Start()
    {
        if (saveFileExists())
        {
            lastSelectedObject = GameObject.Find("ContinueButton");
            lastSelectedObject.GetComponent<Button>().interactable = true;
            lastSelectedObject.transform.GetChild(0).GetComponent<Text>().color = new Color(0, 0, 0, 1);
        } else
        {
            lastSelectedObject = GameObject.Find("StartButton");
        }
        
        lastSelectedObject.GetComponent<Button>().Select();
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            Button lastSelectedButton = lastSelectedObject.GetComponent<Button>();
            lastSelectedButton.Select();
        }

        lastSelectedObject = EventSystem.current.currentSelectedGameObject;
    }

    bool saveFileExists()
    {
        return File.Exists(savegamePath);
    }

    public void beginNewGame()
    {
        StartCoroutine(newGameCoroutine());
    }

    public void continueGame()
    {
        StartCoroutine(continueGameCoroutine());
    }

    IEnumerator newGameCoroutine()
    {
        yield return fadeToWhite();
        PlayerController.isNewGame = true;
        SceneManager.LoadScene("Scene00");
        Destroy(mainTheme);
    }

    IEnumerator continueGameCoroutine()
    {
        yield return fadeToWhite();
        SceneManager.LoadScene("SceneTransition");
        Destroy(mainTheme);
    }

    IEnumerator fadeToWhite()
    {
        DontDestroyOnLoad(imgCanvas);
        yield return fadeToColor(Color.white);
    }

    IEnumerator fadeToBlack()
    {
        yield return fadeToColor(Color.black);
    }

    IEnumerator fadeToColor(Color targetColor)
    {
        Color originalColor = img.color;
        float transitionTime = 0.5f;
        float timer = 0;
        while (timer < transitionTime)
        {
            img.color = Color.Lerp(originalColor, targetColor, timer / transitionTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void openCredits()
    {
        StartCoroutine(creditsCoroutine());
    }

    IEnumerator creditsCoroutine()
    {
        yield return fadeToBlack();
        SceneManager.LoadScene("Credits");
    }

    public void abandonGame()
    {
        Application.Quit();
    }

}
