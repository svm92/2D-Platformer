using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandTorch : MonoBehaviour {

    SpriteRenderer sprite;
    Light torchLight;
    Animator anim;

    public Color requiredColor = Color.white;
    GameObject player;
    PlayerController playerController;
    Teleporter teleporter;
    CameraController mainCamera;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        torchLight = GetComponentInChildren<Light>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        teleporter = GameObject.Find("TeleporterFirst").GetComponent<Teleporter>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        decideIfLightUp();
    }

    void decideIfLightUp()
    {
        if (playerController.globalVariables[getGlobalVariable()])
            illuminate();
    }

    void illuminate()
    {
        anim.SetTrigger("lightOn");
        sprite.color = requiredColor;
        torchLight.enabled = true;
        torchLight.color = requiredColor;
    }

    public IEnumerator showCutscene()
    {
        StartCoroutine(cutscene());
        yield break;
    }

    IEnumerator cutscene() // This one can't be public (WaitForSeconds doesn't work)
    {
        mainCamera.dontFollowPlayer = true;
        player.transform.position = new Vector3(-100, -100);
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
        mainCamera.transform.position = new Vector3(105, 24, mainCamera.transform.position.z);
        mainCamera.changeSize(13);

        yield return new WaitForSeconds(1.5f);
        illuminate();
        playerController.globalVariables[getGlobalVariable()] = true;

        if (allTorchesLit())
        {
            yield return new WaitForSeconds(2f);
            playerController.globalVariables[115] = true; // Final Lightning variable
            mainCamera.focusCameraOnPoint(132, 24.5f, 14.5f, 2f);
            yield return new WaitForSeconds(3.5f);
            GameObject finalLightning = GameObject.Find("FinalLightning");
            Destroy(finalLightning);
            yield return new WaitForSeconds(2f);
        } else
        {
            yield return new WaitForSeconds(3f);
        }

        chooseDestination();
        StartCoroutine(teleporter.fadeOutScreen(2f));
    }

    void chooseDestination()
    {
        if (requiredColor.r == 0.6f && requiredColor.g == 0.6f && requiredColor.b == 0.6f)
        {
            teleporter.targetScene = "Scene00";
            teleporter.spawnPosition = new Vector3(79, 11, 0);
        }
        else if (requiredColor == Color.red)
        {
            teleporter.targetScene = "SceneRed";
            teleporter.spawnPosition = new Vector3(-11, 249, 0);
        }
        else if (requiredColor == Color.green)
        {
            teleporter.targetScene = "SceneGreen";
            teleporter.spawnPosition = new Vector3(-15, 40, 0);
        }
        else if (requiredColor == Color.blue)
        {
            teleporter.targetScene = "SceneBlue";
            teleporter.spawnPosition = new Vector3(-169, -110, 0);
        }
        else if (requiredColor == Color.cyan)
        {
            teleporter.targetScene = "SceneCyan";
            teleporter.spawnPosition = new Vector3(93, -2, 0);
        }
        else if (requiredColor == Color.magenta)
        {
            teleporter.targetScene = "SceneMagenta";
            teleporter.spawnPosition = new Vector3(187, 198, 0);
        }
        else if (requiredColor == new Color(1, 1, 0))
        {
            teleporter.targetScene = "SceneYellow";
            teleporter.spawnPosition = new Vector3(4, 11, 0);
        }
    }

    int getGlobalVariable()
    {
        if (requiredColor.r == 0.6f && requiredColor.g == 0.6f && requiredColor.b == 0.6f)
            return 102;
        else if (requiredColor == Color.red)
            return 109;
        else if (requiredColor == Color.green)
            return 110;
        else if (requiredColor == Color.blue)
            return 111;
        else if (requiredColor == Color.cyan)
            return 112;
        else if (requiredColor == Color.magenta)
            return 113;
        else if (requiredColor == new Color(1, 1, 0))
            return 114;

        return 102;
    }

    bool allTorchesLit()
    {
        if (!playerController.globalVariables[102]) // Check gray boss
            return false;

        for (int i = 109; i <= 114; i++) // Check color bosses
            if (!playerController.globalVariables[i])
                return false;

        return true;
    }

}
