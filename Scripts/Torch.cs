using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour {

    [HideInInspector] public bool on = false;
    public Color initialColor = Color.white;

    SpriteRenderer sprite;
    Light torchLight;
    Collider2D lightCollider;
    Animator anim;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        torchLight = GetComponentInChildren<Light>();
        lightCollider = transform.GetChild(0).GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        if (initialColor != Color.white)
            illuminate(initialColor);
    }

    public void illuminate(Color color)
    {
        on = true;
        anim.SetTrigger("lightOn");
        sprite.color = color;
        torchLight.enabled = true;
        torchLight.color = color;
        lightCollider.enabled = true;
    }
}
