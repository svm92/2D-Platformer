using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossYellow00 : Boss {

    public GameObject projectile;

    Vector3 direction = Vector3.right;
    float delayBetweenShots = 2f;
    float lastSpawnX = -100;

    protected override void Awake()
    {
        base.Awake();
        speed = 3f;
        health = 150;
        power = 1;
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(spawnProjectile());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isDead)
        {
            transform.position += direction * speed * Time.deltaTime;
            recheckValues();
        }
    }

    void changeDirection()
    {
        direction = Quaternion.Euler(0, 0, 180) * direction;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (GameCharacter.collidingWithObstacle(collision))
            changeDirection();
    }

    void recheckValues()
    {
        if (health <= 0.2f * maxHealth)
        {
            delayBetweenShots = 1f;
        }
        else if (health <= 0.38f * maxHealth)
        {
            delayBetweenShots = 1.25f;
        }
        else if (health <= 0.6f * maxHealth)
        {
            delayBetweenShots = 1.5f;
        }
        else if (health <= 0.82f * maxHealth)
        {
            delayBetweenShots = 1.75f;
        }
    }

    IEnumerator spawnProjectile()
    {
        yield return new WaitForSeconds(delayBetweenShots);

        if (isDead)
            yield break;

        float randomX = lastSpawnX;
        while (Mathf.Abs(randomX - lastSpawnX) <= 6f)
        {
            randomX = Random.Range(-12, 11);
        }
        float randomY = Random.Range(7, 9);
        Vector3 spawnPosition = new Vector3(randomX, randomY);
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.initialize(power, 3.5f, 8f);
        ep.setDirection(Vector3.down);
        ep.setThroughWalls(true);
        ep.setSpawner();
        ep.setScale(1.2f);
        ep.setColor(Color.yellow);

        StartCoroutine(spawnProjectile());
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[104] = true;
    }
}
