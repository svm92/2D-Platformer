using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraChanger : MonoBehaviour {

    public float x;
    public float y;
    public float ortographicSize = 13;
    public float transitionTime = 2f;
    public bool changeOnlyOrtographicSize = false;
    public float ortographicSizeOnExit = 0;

    CameraController mainCamera;

    private void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        ortographicSizeOnExit = (ortographicSizeOnExit == 0) ? 6 : ortographicSizeOnExit;
    }

    public void changeCamera()
    {
        if (changeOnlyOrtographicSize)
            StartCoroutine(mainCamera.changeSizeGradually(ortographicSize, transitionTime));
        else
            mainCamera.focusCameraOnPoint(x, y, ortographicSize, transitionTime);
    }

    public void restoreCamera()
    {
        if (changeOnlyOrtographicSize)
            StartCoroutine(mainCamera.changeSizeGradually(ortographicSizeOnExit, transitionTime / 2));
        else
            mainCamera.focusCameraOnPlayer(transitionTime / 4);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            changeCamera();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            restoreCamera();
    }

}
