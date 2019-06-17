using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour {

    PlayerController player;
    CameraController cam;

    GameObject firstPanel;
    GameObject secondPanel;
    GameObject lastSelectedObject;
    GameObject menuItems;
    Text nameText;
    Text infoText;
    Text mapNameText;
    Text noMapDataText;
    Text statsMapL;
    Text statsMapG;
    Text statsItemL;
    Text statsItemG;
    Text nOfHeartsText;
    Text playedTimeText;

    int currentScreen = 0;

    Button quitButton;
    Text quitButtonText;
    bool quitWarningDisplayed = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        
        firstPanel = transform.GetChild(0).gameObject;
        secondPanel = transform.GetChild(1).gameObject;
        menuItems = firstPanel.transform.GetChild(3).gameObject;
        nameText = GameObject.Find("NameText").GetComponent<Text>();
        infoText = GameObject.Find("InfoText").GetComponent<Text>();
        quitButton = firstPanel.transform.GetChild(4).GetComponent<Button>();
        quitButtonText = quitButton.transform.GetChild(0).GetComponent<Text>();

        mapNameText = secondPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        noMapDataText = secondPanel.transform.GetChild(1).GetComponent<Text>();
        statsMapL = secondPanel.transform.GetChild(9).GetComponent<Text>();
        statsMapG = secondPanel.transform.GetChild(10).GetComponent<Text>();
        statsItemL = secondPanel.transform.GetChild(11).GetComponent<Text>();
        statsItemG = secondPanel.transform.GetChild(12).GetComponent<Text>();

        nOfHeartsText = GameObject.Find("HeartCompletion").GetComponent<Text>();
        playedTimeText = GameObject.Find("CurrentPlayedTime").GetComponent<Text>();
    }

    private void Start()
    {
        selectFirstItemAvailable(); // Doesn't highlight when called from Awake or OnEnable
    }

    private void OnEnable() // Happens when entering from game (not when swapping menu screens)
    {
        if (currentScreen == 1)
        {
            showhMap();
        }

        // Decide what items to show on first screen
        for (int i = 0; i < menuItems.transform.childCount; i++)
        {
            GameObject item = menuItems.transform.GetChild(i).gameObject;
            MenuItem menuItem = item.GetComponent<MenuItem>();
            int requiredGlobalVar = menuItem.requiredGlobalVariable;
            int alternateRequiredGlobalVar = menuItem.alternateRequiredGlobalVariable;
            int requiredColorVar = menuItem.requiredColorVariable;
            // Enable item in menu only if player has unlocked it
            if ((requiredGlobalVar >= 0 && player.globalVariables[requiredGlobalVar]) ||
                (alternateRequiredGlobalVar >= 0 && player.globalVariables[alternateRequiredGlobalVar]) ||
                (requiredColorVar >= 0 && player.unlockedColors[requiredColorVar]))
                item.SetActive(true);
            else
                item.SetActive(false);
        }

        updateHeartCount();
        updateTimeText();
    }

    private void Update()
    {
        if (currentScreen == 0)
        {
            // Decide currently selected item
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (lastSelectedObject.activeInHierarchy)
                    EventSystem.current.SetSelectedGameObject(lastSelectedObject);
                else
                    selectFirstItemAvailable();
            }
            else
                lastSelectedObject = EventSystem.current.currentSelectedGameObject;

            updateInfoText();
        } else if (currentScreen == 1)
        {
            if (Mapper.noMapData)
                noMapDataText.text = "NO MAP DATA";
        }

        if (Input.GetButtonDown("ShiftColorLeft") || Input.GetButtonDown("ShiftColorRight"))
            changeScreen();
    }

    void changeScreen()
    {
        if (currentScreen == 0) // Change to map panel
        {
            currentScreen = 1;
            firstPanel.SetActive(false);
            secondPanel.SetActive(true);
            showhMap();
        } else if (currentScreen == 1) // Change to items panel
        {
            currentScreen = 0;
            secondPanel.SetActive(false);
            firstPanel.SetActive(true);
            hideMap();
            EventSystem.current.SetSelectedGameObject(null);
            lastSelectedObject.GetComponent<Selectable>().Select();
            resetQuitButton();
        }
    }

    void updateInfoText()
    {
        MenuItem selectedMenuItem = EventSystem.current.currentSelectedGameObject.GetComponent<MenuItem>();
        if (selectedMenuItem != null)
        {
            nameText.text = selectedMenuItem.itemName;
            nameText.color = selectedMenuItem.nameColor;
            infoText.text = selectedMenuItem.description;
        } else
        {
            nameText.text = "";
            nameText.color = Color.black;
            infoText.text = "";
        }
    }

    void selectFirstItemAvailable()
    {
        for (int i=0; i < menuItems.transform.childCount; i++) // Select first available item
        {
            GameObject menuItem = menuItems.transform.GetChild(i).gameObject;
            if (menuItem.activeInHierarchy)
            {
                menuItem.GetComponent<Selectable>().Select();
                return;
            }
        }
        
        // If no items unlocked, pick return button by default
        GameObject.Find("ResumeGameButton").GetComponent<Button>().Select();
    }

    void showhMap()
    {
        noMapDataText.text = "";
        updateStats();
        cam.isShowingMap = true;
    }

    public void hideMap()
    {
        cam.isShowingMap = false;
    }

    void updateStats()
    {
        mapNameText.text = StageInfo.mapName();

        float statsMapLocal = StageInfo.statsMapLocal(player);
        float statsMapGlobal = StageInfo.statsMapGlobal(player);
        float statsItemLocal = StageInfo.statsItemLocal(player);
        float statsItemGlobal = StageInfo.statsItemGlobal(player);

        statsMapL.text = (statsMapLocal >= 0) ? statsMapLocal + "%" : "- %";
        statsMapG.text = (statsMapGlobal >= 0) ? statsMapGlobal + "%" : "- %";
        statsItemL.text = (statsItemLocal >= 0) ? statsItemLocal + "%" : "- %";
        statsItemG.text = (statsItemGlobal >= 0) ? statsItemGlobal + "%" : "- %";

        decideTextColor(statsMapL);
        decideTextColor(statsMapG);
        decideTextColor(statsItemL);
        decideTextColor(statsItemG);
    }

    void decideTextColor(Text textObj)
    {
        if (textObj.text == "100%")
            textObj.color = new Color(0.05f, 0.5f, 0.05f);
        else
            textObj.color = new Color(0.19f, 0.19f, 0.19f);
    }

    void updateHeartCount()
    {
        int nOfHealthContainers = player.healthContainers + (player.maxHealth - 3) * 3;
        nOfHeartsText.text = nOfHealthContainers + "/27";
        if (nOfHealthContainers >= 27)
            nOfHeartsText.color = new Color(0.1f, 0.75f, 0.25f);
    }

    void updateTimeText()
    {
        playedTimeText.text = EndingScore.getTimeText(player.gameTimer);
    }

    public void resumeGame()
    {
        StartCoroutine(resumeGameCoroutine());
    }

    public IEnumerator resumeGameCoroutine()
    {
        yield return null; // Wait a frame to avoid passing the input to the player
        player.pauseGame();
        
    }

    public void quitGame()
    {
        if (quitWarningDisplayed)
        {
            player.isDead = true;
            player.pauseGame();
            SceneManager.LoadScene("DeathTransition", LoadSceneMode.Additive);
        } else
        {
            quitWarningDisplayed = true;
            quitButtonText.text = "Are you sure?";
            quitButtonText.fontSize = 57;
            quitButtonText.color = Color.red;
        }
    }

    public void resetQuitButton()
    {
        quitWarningDisplayed = false;
        quitButtonText.text = "Quit";
        quitButtonText.fontSize = 65;
        quitButtonText.color = Color.black;
    }

}
