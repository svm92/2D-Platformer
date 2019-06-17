using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGreen00 : Boss {

    public GameObject projectile;

    Vector3 direction = Vector3.right;
    float bulletSpeed = 10f;
    float delayBetweenShots = 1f;

    protected override void Awake()
    {
        base.Awake();
        speed = 10f;
        health = 120;
        power = 1;
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(spawnProjectiles());
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
        recheckDelayBetweenShots();
        recheckBulletSpeed();
    }

    void recheckDelayBetweenShots()
    {
        if (health <= 0.37f * maxHealth)
            delayBetweenShots = 0.7f;
        else if (health <= 0.85f * maxHealth)
            delayBetweenShots = 0.8f;
    }

    void recheckBulletSpeed()
    {
        if (health <= 0.3f * maxHealth)
            bulletSpeed = 15f;
        else if (health <= 0.62f * maxHealth)
            bulletSpeed = 12.5f;
    }

    IEnumerator spawnProjectiles()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            spawnProjectile();
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    void spawnProjectile()
    {
        if (isDead)
            return;

        float distanceToPlayer = 0;
        float randomX = 0;
        float randomY = 0;
        while (distanceToPlayer <= 3.5f || distanceToPlayer >= 10f) // Don't spawn too close or far from the player
        {
            randomX = Random.Range(-10, 10);
            randomY = Random.Range(1, 6);
            distanceToPlayer = Vector3.Distance(player.transform.position, new Vector3(randomX, randomY));
        }

        Vector3 spawnPosition = new Vector3(randomX, randomY);
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.homeInitially();
        ep.initialize(power, bulletSpeed, 0);
        ep.setDelay(0.5f);
        ep.setScale(0.75f);
        ep.setColor(new Color(0.15f, 0.8f, 0.15f));
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[19] = true;
    }
}
