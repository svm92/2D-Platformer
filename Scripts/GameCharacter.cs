using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class GameCharacter : MonoBehaviour {

    [HideInInspector] public int health;
    [HideInInspector] public int power;
    protected float speed;

    protected float flashingSpeed = 7f;
    [HideInInspector] public bool isHurting = false;
    protected bool isFlashing = false;

    protected SpriteRenderer sprite;

    protected virtual void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {

    }

    public static bool collidingWithObstacle(Collider2D collision)
    {
        return collision.CompareTag("Ground") || collision.CompareTag("Lightning");
    }

    protected virtual IEnumerator flashForDamage()
    {
        isFlashing = true;

        Color originalColor = sprite.color;
        Color destinationColor = flashingColor(originalColor);
        float elapsedTime = 0;
        while (sprite.color != destinationColor) // Flash red (enemy) or inverted color (player/boss)
        {
            sprite.color = Color.Lerp(sprite.color, destinationColor, flashingSpeed * elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        while (sprite.color != originalColor) // Return to normal
        {
            sprite.color = Color.Lerp(sprite.color, originalColor, flashingSpeed * elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isFlashing = false;
    }

    protected virtual Color flashingColor(Color color)
    {
        return Color.red;
    }

    protected virtual Color getLighterColor(Color color)
    {
        float lighterR = (color.r == 1) ? 1 : 0.2f;
        float lighterG = (color.g == 1) ? 1 : 0.2f;
        float lighterB = (color.b == 1) ? 1 : 0.2f;
        return new Color(lighterR, lighterG, lighterB);
    }

    protected abstract void kill();

}
