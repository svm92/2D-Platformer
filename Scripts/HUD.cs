using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

    public GameObject heart;

    PlayerController player;
    Image[] colorImages = new Image[3];

    GameObject[] heartArray;
    float separationBetweenHearts;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        colorImages[0] = transform.GetComponentsInChildren<Image>()[0];
        colorImages[1] = transform.GetComponentsInChildren<Image>()[1];
        colorImages[2] = transform.GetComponentsInChildren<Image>()[2];
        separationBetweenHearts = Screen.width * 0.065f;
    }

    private void Start()
    {
        updateHearts();
    }

    public void updateHearts()
    {
        deleteHearts();
        int maxHealth = player.maxHealth;
        heartArray = new GameObject[maxHealth];
        for (int i = 0; i < maxHealth; i++)
            paintHeart(i);
    }

    void paintHeart(int i)
    {
        float offsetX = Screen.width * 0.01f;
        float offsetY = Screen.height * 0.03f;
        Vector3 heartPosition = new Vector3(offsetX + separationBetweenHearts * i, offsetY);
        heartPosition = GetComponentInParent<Camera>().ScreenToWorldPoint(heartPosition);

        GameObject heartObject = Instantiate(heart, heartPosition, Quaternion.identity);
        Transform heartTr = heartObject.transform;
        heartTr.SetParent(gameObject.transform); // Set as child of the HUD
        heartTr.localScale = Vector3.one;
        heartTr.localPosition = new Vector3(heartTr.localPosition.x, heartTr.localPosition.y, 0);

        heartArray[i] = heartObject;

        if (player.health <= i)
            darkenHeart(i);
        else
            lightenHeart(i);
    }

    void lightenHeart(int i)
    {
        GameObject heartObject = heartArray[i];
        heartObject.GetComponent<Image>().color = player.playerColor;
    }

    void darkenHeart(int i)
    {
        GameObject heartObject = heartArray[i];
        heartObject.GetComponent<Image>().color = Color.black;
    }

    void deleteHearts()
    {
        if (heartArray == null)
            return;

        foreach (GameObject h in heartArray)
            Destroy(h);
    }

    public void updateColor(Color color, int pos)
    {
        colorImages[pos].color = color;
    }

}
