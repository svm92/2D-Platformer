using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour {

    static CameraController cameraController;

    Transform player;
    Camera mainCamera;
    float offset = 2f;
    float speed = 6f;

    bool fixedCamera = false;
    float fixedY = 3.5f; // Used for SceneYellow
    public bool dontFollowPlayer = false;
    int nOfProgrammedMovements = 0;

    public bool isShowingMap = false;
    public Texture mapCameraTexture;

    private void Awake()
    {
        if (cameraController == null)
        {
            DontDestroyOnLoad(gameObject);
            cameraController = this;
        }
        else if (cameraController != this)
        {
            Destroy(gameObject);
            return;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = GetComponent<Camera>();
        cameraController.jumpToPlayerPosition();
    }

    private void OnPostRender()
    {
        if (isShowingMap)
            Mapper.printMap(player.GetComponent<PlayerController>(), mapCameraTexture);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fix camera only on "SceneYellow"
        fixedCamera = (scene.name == "SceneYellow");
    }

    public void jumpToPlayerPosition()
    {
        float targetY = !fixedCamera ? player.position.y + offset : fixedY;
        transform.position = new Vector3(player.position.x, targetY, -10);
    }

    private void LateUpdate()
    {
        if (dontFollowPlayer)
            return;

        float targetY = !fixedCamera ? player.position.y + offset : fixedY;
        Vector3 targetPosition = new Vector3( player.position.x, targetY, -10 );
        transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
    }

    public void changeSize(float n)
    {
        mainCamera.orthographicSize = n;
    }

    public void resetSize()
    {
        changeSize(6);
    }

    public void focusCameraOnPoint(float x, float y, float cameraSize, float transitionTime)
    {
        dontFollowPlayer = true;
        nOfProgrammedMovements++;
        StartCoroutine(moveCameraToPoint(x, y, cameraSize, transitionTime, false));
    }

    IEnumerator moveCameraToPoint(float x, float y, float cameraSize, float transitionTime, bool resumeFollowingPlayer)
    {
        yield return new WaitForSeconds(0.1f);

        float timer = 0;
        Vector3 originalPosition = transform.position;
        Vector3 destination = new Vector3(x, y, transform.position.z);
        float currentCameraSize = mainCamera.orthographicSize;

        while (transform.position != destination)
        {
            transform.position = Vector3.Lerp(originalPosition, destination, timer/transitionTime);
            mainCamera.orthographicSize = Mathf.Lerp(currentCameraSize, cameraSize, timer / transitionTime);
            timer += Time.deltaTime;

            if (nOfProgrammedMovements == 2)
            {
                nOfProgrammedMovements--;
                yield break;
            }
            yield return null;
        }

        nOfProgrammedMovements--;
        if (resumeFollowingPlayer)
            dontFollowPlayer = false;
    }

    public void focusCameraOnPlayer(float transitionTime)
    {
        nOfProgrammedMovements++;
        float x = player.transform.position.x;
        float y = player.transform.position.y;
        StartCoroutine(moveCameraToPoint(x, y, 6, transitionTime, true));
    }

    public IEnumerator changeSizeGradually(float size, float transitionTime)
    {
        nOfProgrammedMovements++;

        yield return new WaitForSeconds(0.1f);

        float timer = 0;
        float currentCameraSize = mainCamera.orthographicSize;
        float oldFixedY = fixedY;
        float newFixedY = getFixedY(size);

        while (mainCamera.orthographicSize != size)
        {
            mainCamera.orthographicSize = Mathf.Lerp(currentCameraSize, size, timer / transitionTime);
            fixedY = Mathf.Lerp(oldFixedY, newFixedY, timer / transitionTime);
            timer += Time.deltaTime;

            if (nOfProgrammedMovements == 2)
            {
                nOfProgrammedMovements--;
                yield break;
            }
            yield return null;
        }

        fixedY = newFixedY;

        nOfProgrammedMovements--;
    }

    float getFixedY(float ortographicSize)
    {
        if (ortographicSize == 6)
            return 3.5f;
        if (ortographicSize == 9)
            return 7f;
        if (ortographicSize == 10.5f)
            return 18.5f;

        return 3.5f;
    }

}
