using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : Enemy {

    protected float maxHealth;
    [HideInInspector] public Slider healthBar;

    protected override void Awake()
    {
        base.Awake();
        healthBar = GetComponentInChildren<Slider>();
        detachHealthBarCanvas();
    }

    protected override void Start()
    {
        maxHealth = health;
        healthBar.maxValue = health;
        healthBar.value = health;
        healthBar.GetComponentsInChildren<Image>()[1].color = sprite.color; // Fill color
        healthBar.interactable = false;
    }

    void detachHealthBarCanvas() // Needed so that it doesn't inherit rotations, etc from boss
    {
        if (GetComponentInChildren<Canvas>() != null)
            GetComponentInChildren<Canvas>().transform.SetParent(null);
    }

    protected override Color flashingColor(Color color)
    {
        if (color == Color.magenta)
            return new Color(0.2f, 0.6f, 0.2f); // Avoid quick green-magenta flashes

        return PlayerController.invertColor(GetComponent<SpriteRenderer>().color);
    }

    protected override void kill()
    {
        base.kill();
        foreach (GameObject projectile in GameObject.FindGameObjectsWithTag("EnemyProjectile"))
            Destroy(projectile);
        GameObject.FindGameObjectWithTag("Teleporter").GetComponent<Collider2D>().enabled = true;
    }

    public override void receiveDamage(int damage)
    {
        base.receiveDamage(damage);
        healthBar.value = health;
    }

}
