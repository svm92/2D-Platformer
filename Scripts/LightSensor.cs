using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightSensor : MonoBehaviour {

    public GameObject associatedMechanism;

    public Sprite unlitSprite;
    public Sprite litSprite;

    float colliderRadius;
    Color requiredColor;

    SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        colliderRadius = GetComponent<CircleCollider2D>().radius;
        requiredColor = associatedMechanism.GetComponent<SpriteRenderer>().color;
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = requiredColor;
    }

    private void Update()
    {
        if (sprite.color == requiredColor && sprite.sprite == litSprite)
            associatedMechanism.GetComponent<Lightning>().activate();
        else
            associatedMechanism.GetComponent<Lightning>().deactivate();
    }

    public void illuminate(Color color)
    {
        sprite.sprite = litSprite;
        sprite.color = color;
    }

    void illuminate(List<Color> listOfColors)
    {
        sprite.sprite = litSprite;

        Color mixedColor = mixColors(listOfColors);

        if (mixedColor == Color.white && requiredColor != Color.white)
            darken();
        else
            sprite.color = mixedColor;
    }

    Color mixColors(List<Color> listOfColors)
    {
        // C+M+Y = Black
        if (listOfColors.Contains(Color.cyan) && listOfColors.Contains(Color.magenta) && 
            listOfColors.Contains(new Color(1, 1, 0)))
            return Color.black;
        
        listOfColors = listOfColors.Distinct().ToList(); // Remove duplicates
        Color mixedColor = Color.clear;
        foreach (Color c in listOfColors)
                mixedColor += c;
        float maxColorValue = Mathf.Max(mixedColor.r, mixedColor.g, mixedColor.b);
        mixedColor.r = mixedColor.r >= maxColorValue ? 1 : 0;
        mixedColor.g = mixedColor.g >= maxColorValue ? 1 : 0;
        mixedColor.b = mixedColor.b >= maxColorValue ? 1 : 0;
        mixedColor.a = 1;
        return mixedColor;
    }

    void darken()
    {
        sprite.sprite = unlitSprite;
        sprite.color = Color.white;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        checkLightSources();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        checkLightSources();
    }

    void checkLightSources()
    {
        List<Color> listOfColors = new List<Color>();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, colliderRadius);

        foreach (Collider2D collider in colliders)
        {
            // If player's spotlight colliding
            if (collider.name == "SpotlightCollider")
            {
                listOfColors.Add(collider.transform.parent.GetComponent<PlayerController>().playerColor);
                continue;
            }

            Torch torch = collider.GetComponentInParent<Torch>(); // If torch
            if (torch != null && torch.on)
                listOfColors.Add(torch.GetComponent<SpriteRenderer>().color);
        }

        if (listOfColors.Count > 0)
            illuminate(listOfColors);
        else
            darken();
    }

}
