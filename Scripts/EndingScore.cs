using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingScore : MonoBehaviour {

    PlayerController player;

    GameObject scoreValues;
    GameObject scoreRanks;
    float completionTime;
    float mapCompletion;
    float itemCompletion;
    int nOfEnemiesKilled;
    int totalDamageReceived;

    bool menuInteractable = false;
    Image img;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        img = transform.GetChild(1).GetComponent<Image>();

        scoreValues = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        scoreRanks = transform.GetChild(0).GetChild(0).GetChild(1).gameObject;
        completionTime = player.gameTimer;
        mapCompletion = StageInfo.statsMapGlobal(player);
        itemCompletion = StageInfo.statsItemGlobal(player);
        nOfEnemiesKilled = player.nOfEnemiesKilled;
        totalDamageReceived = player.totalDamageReceived;

        GameObject[] dontDestroyOnLoadObjects = new GameObject[] { player.gameObject,
            GameObject.FindGameObjectWithTag("MainCamera"), GameObject.Find("MusicManager")};

        foreach (GameObject g in dontDestroyOnLoadObjects)
            if (g != null)
                Destroy(g);
    }

    private void Start()
    {
        updateScoreTexts();
        updateScoreRanks();
        StartCoroutine(waitThreeSeconds());
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && menuInteractable)
        {
            StartCoroutine(goToCredits());
            menuInteractable = false;
        }
    }

    IEnumerator waitThreeSeconds()
    {
        yield return new WaitForSeconds(3);
        menuInteractable = true;
    }

    void updateScoreTexts()
    {
        Text[] scoreTexts = scoreValues.GetComponentsInChildren<Text>();

        scoreTexts[0].text = getTimeText(completionTime);
        scoreTexts[1].text = mapCompletion + "%";
        scoreTexts[2].text = itemCompletion + "%";
        scoreTexts[3].text = nOfEnemiesKilled + "";
        scoreTexts[4].text = totalDamageReceived + "";
    }

    void updateScoreRanks()
    {
        char[] ranks = new char[5];
        Text[] scoreRankTexts = scoreRanks.GetComponentsInChildren<Text>();
        
        ranks[0] = applyLetterRank(scoreRankTexts[0], completionTime, new float[] { 90*60, 120*60, 150*60, 210*60 }, false);
        // Mandatory rooms: roughly 292/374 (78.1 %)
        ranks[1] = applyLetterRank(scoreRankTexts[1], mapCompletion, new float[] { 100, 95, 90, 85 });
        // Mandatory items: roughly 11/40 (27.5%)
        ranks[2] = applyLetterRank(scoreRankTexts[2], itemCompletion, new float[] { 100, 90, 80, 60 });
        ranks[3] = applyLetterRank(scoreRankTexts[3], nOfEnemiesKilled, new float[] { 600, 550, 500, 400 });
        ranks[4] = applyLetterRank(scoreRankTexts[4], totalDamageReceived, new float[] { 120, 135, 160, 190 }, false);

        applyOverallRank(scoreRankTexts[5], ranks);
    }

    char applyLetterRank(Text text, float scoringValue, float[] scoreThresholds)
    {
        return applyLetterRank(text, scoringValue, scoreThresholds, true);
    }

    char applyLetterRank(Text text, float scoringValue, float[] scoreThresholds, bool higherIsBetter)
    {
        char letter = getLetter(scoringValue, scoreThresholds, higherIsBetter);
        text.text = letter + "";

        switch (letter)
        {
            case 'S':
                text.color = Color.yellow;
                break;
            case 'A':
                text.color = Color.red;
                break;
            case 'B':
                text.color = Color.blue;
                break;
            case 'C':
                text.color = Color.green;
                break;
            case 'D':
                text.color = Color.black;
                break;
        }

        return letter;
    }

    char getLetter(float scoringValue, float[] scoreThresholds, bool higherIsBetter)
    {
        char[] possibleRanks = new char[] { 'S', 'A', 'B', 'C' };

        for (int i=0; i < scoreThresholds.Length; i++)
        {
            if (higherIsBetter)
            {
                if (scoringValue >= scoreThresholds[i])
                    return possibleRanks[i];
            } else
            {
                if (scoringValue <= scoreThresholds[i])
                    return possibleRanks[i];
            }  
        }

        return 'D';
    }

    void applyOverallRank(Text overallRankText, char[] ranks)
    {
        int overallScore = 0;
        foreach (char rank in ranks)
        {
            switch (rank)
            {
                case 'S':
                    overallScore += 4;
                    break;
                case 'A':
                    overallScore += 3;
                    break;
                case 'B':
                    overallScore += 2;
                    break;
                case 'C':
                    overallScore += 1;
                    break;
            }
        }

        applyLetterRank(overallRankText, overallScore, new float[] { 20, 15, 10, 5 });
    }

    IEnumerator goToCredits()
    {
        Color originalColor = img.color;
        Color targetColor = Color.black;
        float transitionTime = 2.5f;
        float timer = 0;
        while (timer < transitionTime)
        {
            img.color = Color.Lerp(originalColor, targetColor, timer / transitionTime);
            timer += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene("Credits");
    }

    public static string getTimeText(float playedTime)
    {
        float hours = getHours(playedTime);
        float minutes = getMinutes(playedTime);
        return (hours + ":" + ((minutes <= 9) ? "0" : "") + minutes);
    }

    public static int getHours(float time) // time in seconds
    {
        return (int)Mathf.Floor(time / 3600);
    }

    public static int getMinutes(float time)
    {
        return (int)Mathf.Floor((time / 60) % 60);
    }

}
