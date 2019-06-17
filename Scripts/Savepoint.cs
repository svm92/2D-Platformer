using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Savepoint : MonoBehaviour {

    ParticleSystem ps;
    SpriteRenderer sprite;

    public int associatedMapArea = -1;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(changeColor());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Time.timeSinceLevelLoad > 1f) // Except right after loading a game
            {
                ps.Play();
                PlayerController player = collision.GetComponent<PlayerController>();
                player.heal();
                player.saveGame(gameObject);
            }
        } 
    }

    IEnumerator changeColor()
    {
        sprite.color = Color.red;
        Color[] shiftingColors = new Color[] { Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow, Color.red };
        while (true)
            foreach (Color c in shiftingColors)
                yield return StartCoroutine(changeToColor(c));
    }

    IEnumerator changeToColor(Color newColor)
    {
        float timer = 0;
        float colorTransitionDuration = .75f;
        while (timer < colorTransitionDuration)
        {
            sprite.color = Color.Lerp(sprite.color, newColor, (1/colorTransitionDuration + .2f) * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

}
