using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorCube : MonoBehaviour {

    public Color color = Color.white;

    private void Start()
    {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        int colorIndex = player.getColorIndex(color);
        if (player.unlockedColors[colorIndex]) // If the color is already unlocked, destroy the cube
            Destroy(gameObject);

        GetComponent<SpriteRenderer>().color = color;
        GetComponentInChildren<Light>().color = color;
        ParticleSystem ps = GetComponent<ParticleSystem>();
        Color darkerColor = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f);
        ParticleSystem.MainModule settings = ps.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(color, darkerColor);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().unlockColor(color);
            Destroy(gameObject);
        }
    }

}
