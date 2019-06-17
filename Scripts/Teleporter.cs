using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Teleporter : MonoBehaviour {

    public string targetScene;
    public Vector3 spawnPosition;
    public bool slowTransition = false;
    public bool moveToCenterOfTeleporter = false;
    public bool freezePosition = false;
    public bool isMirror = false;
    public bool isMainBossPortal = false;
    public Color fadeoutColor = Color.black;
    public int associatedGlobalVariable = -1; // If this var is true, destroy teleporter
    public int requiredGlobalVariable = -1; // If this var is false, destroy teleporter
    public int activatesGlobalVariable = -1; // Set this var true when entering teleporter
    public int associatedMapArea = -1;
    public float timeUntilTeleport = 0f;
    Sprite openMirror;
    public Sprite closedMirror;
    public Color grandTorchColor = Color.white;

    bool isActive = true;
    bool playerArrivedFromHere = false;
    GameObject player;
    CameraController cam;
    Image image;
    ParticleSystem ps; // Can be null

    //AsyncOperation asyncLoad;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        image = GetComponentInChildren<Image>();
        ps = GetComponent<ParticleSystem>();

        if (isMirror && !player.GetComponent<PlayerController>().globalVariables[1]) // If mirrors not unlocked
        {
            openMirror = GetComponent<SpriteRenderer>().sprite;
            GetComponent<SpriteRenderer>().sprite = closedMirror;
            ps.Stop();
            isActive = false;
        }

        if (associatedGlobalVariable >= 0 && 
            player.GetComponent<PlayerController>().globalVariables[associatedGlobalVariable])
            Destroy(gameObject);
        else if (requiredGlobalVariable >= 0 &&
            !player.GetComponent<PlayerController>().globalVariables[requiredGlobalVariable])
            Destroy(gameObject);
            
        if (ps != null && !isMainBossPortal && fadeoutColor.a != 0)
        {
            ParticleSystem.MainModule settings = ps.main;
            settings.startColor = fadeoutColor;
        }

        if (fadeoutColor.a == 0)
            fadeoutColor = Color.gray;
    }

    private void Update()
    {
        if (isMirror)
            transform.Rotate(Vector3.back, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!isActive)
                return;

            if (Time.timeSinceLevelLoad < 0.25f)
                playerArrivedFromHere = true;

            if (!playerArrivedFromHere || Time.timeSinceLevelLoad > 2.5f) // Except right after entering a level
            {
                if (moveToCenterOfTeleporter)
                    StartCoroutine(movePlayerToCenter());
                else if (freezePosition)
                    player.GetComponent<PlayerController>().freezeMovement();

                if (!slowTransition)
                    StartCoroutine(fadeOutScreen(0.5f));
                else
                    StartCoroutine(fadeOutScreen(2f));
            }
        }   
    }

    public IEnumerator fadeOutScreen(float transitionTime)
    {
        //StartCoroutine(loadSceneAsync());

        yield return new WaitForSeconds(timeUntilTeleport);
        float timer = 0;
        while (timer < transitionTime)
        {
            if (PlayerController.gamePaused)
            {
                image.color = Color.clear;
                yield return null;
                continue;
            }

            timer += Time.deltaTime;
            image.color = Color.Lerp(Color.clear, fadeoutColor, (1 / transitionTime) * timer);
            yield return null;
        }

        StartCoroutine(loadScene());
    }

    /*IEnumerator loadSceneAsync()
    {
        Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
        asyncLoad = SceneManager.LoadSceneAsync(targetScene);
        asyncLoad.allowSceneActivation = false;
        yield break;
    }*/
    
    IEnumerator loadScene()
    {
        image.color = fadeoutColor;

        if (activatesGlobalVariable >= 0)
            player.GetComponent<PlayerController>().globalVariables[activatesGlobalVariable] = true;

        GetComponent<Collider2D>().enabled = false;
        player.transform.SetParent(null); // In case the player is boarding a platform
        DontDestroyOnLoad(player); // In case the player is boarding a platform
        DontDestroyOnLoad(gameObject);

        SceneManager.LoadScene(targetScene);
        /*asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }*/

        player.transform.position = spawnPosition;
        player.GetComponent<PlayerController>().stopJump();

        cam.dontFollowPlayer = false;
        cam.resetSize();
        cam.jumpToPlayerPosition();

        if (grandTorchColor != Color.white)
            StartCoroutine(lightUp());
        else
            player.GetComponent<PlayerController>().frozenInPlace = false;

        float transitionTime = 2f;
        float timer = 0;
        while (timer < transitionTime)
        {
            if (PlayerController.gamePaused)
            {
                image.color = Color.clear;
                yield return null;
                continue;
            }

            timer += Time.deltaTime;
            image.color = Color.Lerp(fadeoutColor, Color.clear, (1 / transitionTime) * timer);
            yield return null;
        }

        if (grandTorchColor == Color.white)
            Destroy(gameObject);
    }

    IEnumerator movePlayerToCenter()
    {
        player.GetComponent<PlayerController>().freezeMovement();
        Vector3 playerOriginalPosition = player.transform.position;
        float transitionTime = 1f;
        float timer = 0;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            player.transform.position = Vector3.Lerp(playerOriginalPosition, transform.position, (1 / transitionTime) * timer);
            yield return null;
        }
    }

    public void unlockMirror()
    {
        GetComponent<SpriteRenderer>().sprite = openMirror;
        ps.Play();
        isActive = true;
    }

    IEnumerator lightUp()
    {
        while (SceneManager.GetActiveScene().name != "Scene00") // Wait until scene loads
            yield return null;

        foreach (GrandTorch gt in GameObject.Find("GrandTorches").GetComponentsInChildren<GrandTorch>())
        {
            if (gt.requiredColor == grandTorchColor)
            {
                StartCoroutine(gt.showCutscene());
                break;
            }
        }

        Destroy(gameObject);
    }

}
