using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Credits : MonoBehaviour {

    float speed = 0.3f * Screen.height / 327;
    float yScreenBorder; // Once the credits pass this point, they end
    bool creditsEnded = false;

    Image img;

    private void Awake()
    {
        // The text's transform.position is given in pixels (as Screen points, measured from its center)
        
        float textHalfHeight = GetComponent<RectTransform>().rect.height / 2;
        // Canvas has a height of 900 pixels
        float textHalfHeightInScreens = textHalfHeight / 900;
        float textHalfHeightInPixels = textHalfHeightInScreens * Screen.height;
        // That's roughly 2000 / 900 -> 2.22 screens for the text -> 2.22 * 327 = 726 pixels for the text
        yScreenBorder = Screen.height + textHalfHeightInPixels;

        GetComponent<Text>().text = "<size=90>-CREDITS-</size>\n\n" +
            "<size=70>-PROGRAMMING AND DESIGN-</size>\n" +
            "<size=60>Samuel Vázquez</size>\n\n" +
            "<size=70>-GRAPHICS-</size>\n" +
            "<size=52>All graphical assets were created by the\nOpenGameArt.org community," +
            "and belong\nto their respective authors\n\n</size>" +
            "<size=60>Game Developer Studio [CC-BY 3.0]</size>\n" +
            "<size=45>https://opengameart.org/content/high-res-fire-ball</size>\n\n" +
            "<size=60>MoikMellah [CC0]</size>\n" +
            "<size=45>https://opengameart.org/content/4-color-dungeon-bricks-16x16</size>\n\n" +
            "<size=60>Cagil Ozdemirag (Parriah) [CC0]</size>\n" +
            "<size=42>https://opengameart.org/content/2d-platformer-side-scroller-stone-fence-street-lamp</size>\n\n" +
            "<size=60>StumpyStrust [CC0]</size>\n" +
            "<size=45>https://opengameart.org/content/ui-orbs</size>\n\n" +
            "<size=60>ztn [CC0]</size>\n" +
            "<size=45>https://opengameart.org/content/collection-of-rune-stones-seamless-tiles</size>\n\n" +
            "<size=60>EVIL_ENT [CC0]</size>\n" +
            "<size=45>https://opengameart.org/content/lightning-sprite-texture</size>\n\n" +
            "<size=65>Mantis [CC0]</size>\n" +
            "<size=45>https://opengameart.org/content/update-animated-birds-character-sheet</size>\n\n" +
            "<size=65>mart [CC0]</size>\n" +
            "<size=45>https://opengameart.org/content/heart-2</size>\n\n" +
            "<size=60>Evilence [CC0]</size>\n" +
            "<size=45>https://opengameart.org/content/magical-orbs-pack</size>\n\n\n" +
            "<size=52>All menu icons were retrieved from https://game-icons.net/\n Credit to:</size>\n" +
            "<size=60>Lorc</size>\n" +
            "<size=60>sbed</size>\n" +
            "<size=60>Delapouite</size>\n\n\n" +
            "<size=70>-MUSIC-</size>\n" +
            "<size=50>All music generated via https://www.fakemusicgenerator.com/,\n using cgMusic:</size>\n" +
            "<size=40>http://codeminion.com/blogs/maciek/2008/05/cgmusic-computers-create-music/</size>";

        if (MainMenuManager.mainTheme == null)
        {
            MainMenuManager.mainTheme = GameObject.FindGameObjectWithTag("MainTheme");
            DontDestroyOnLoad(MainMenuManager.mainTheme);
        }
        else
        {
            Destroy(GameObject.FindGameObjectsWithTag("MainTheme")[1]);
        }
    }

    private void Start()
    {
        img = GameObject.Find("CreditsImage").GetComponent<Image>();
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") || Input.GetButton("Fire2"))
            transform.position += Vector3.up * speed * 20;
        else
            transform.position += Vector3.up * speed;

        if (transform.position.y > yScreenBorder && !creditsEnded)
        {
            creditsEnded = true;
            StartCoroutine(endCredits());
        }
    }

    IEnumerator endCredits()
    {
        Color originalColor = img.color;
        Color targetColor = Color.white;
        float transitionTime = 3.5f;
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
