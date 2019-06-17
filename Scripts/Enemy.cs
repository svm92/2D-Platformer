using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : GameCharacter {

    protected float dyingSpeed = 3f;
    protected bool isDead = false;
    protected bool isActive = false;

    protected GameObject player;

    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    protected virtual void FixedUpdate()
    {
        if (PlayerController.gamePaused)
        {
            isActive = false;
            return;
        }  

        if (Vector3.Distance(transform.position, player.transform.position) >= 100f)
            isActive = false;
        else
            isActive = true;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            collision.GetComponent<PlayerController>().receiveDamage(power);
    }

    public virtual void receiveDamage(int damage)
    {
        if (isHurting || isFlashing || isDead)
            return;

        isHurting = true;
        health -= damage;
        StartCoroutine(flashForDamage());

        if (health <= 0)
            kill();
    }

    protected override void kill()
    {
        isDead = true;
        GetComponent<Collider2D>().enabled = false;
        player.GetComponent<PlayerController>().nOfEnemiesKilled++;
        StartCoroutine(hopAndDie());
    }

    protected override IEnumerator flashForDamage()
    {
        yield return base.flashForDamage();
        isHurting = false;
        yield break;
    }

    protected IEnumerator hopAndDie()
    {
        sprite.flipY = true;
        Vector3 destination = transform.position + Vector3.up * 2f;
        float elapsedTime = 0;
        while (transform.position != destination) // Jump upwards
        {
            transform.position = Vector3.Lerp(transform.position, destination, dyingSpeed * elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        elapsedTime = 0;
        destination = transform.position + Vector3.down * 2f;
        while (dyingSpeed * elapsedTime < 1) // Fall and disappear
        {
            transform.position = Vector3.Lerp(transform.position, destination, dyingSpeed * elapsedTime);
            sprite.color = Color.Lerp(sprite.color, Color.clear, dyingSpeed * elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

}
