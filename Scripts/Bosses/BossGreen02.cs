using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGreen02 : Boss {

    public GameObject projectile;
    int lastSkippedProjectile = 0;
    float bulletSpeed = 3f;
    float delayBetweenShots = 2.5f;

    Vector3 direction = Vector3.right;

    protected override void Awake()
    {
        base.Awake();
        speed = 11f;
        health = 165;
        power = 2;
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
        if (health <= 0.2f * maxHealth)
        {
            bulletSpeed = 4.25f;
            delayBetweenShots = 1.4f;
        }
        else if (health <= 0.4f * maxHealth)
        {
            bulletSpeed = 3.75f;
            delayBetweenShots = 1.75f;
        }
        else if (health <= 0.6f * maxHealth)
        {
            bulletSpeed = 3.5f;
            delayBetweenShots = 2f;
        }
        else if (health <= 0.8f * maxHealth)
        {
            bulletSpeed = 3.25f;
            delayBetweenShots = 2.25f;
        }
    }

    IEnumerator spawnProjectiles()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            spawnProjectileBarrage();
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    void spawnProjectileBarrage()
    {
        int projectileToSkip = lastSkippedProjectile;
        while (projectileToSkip == lastSkippedProjectile)
        {
            projectileToSkip = Random.Range(-11, 11); // -11 to 10
            if (Mathf.Abs(projectileToSkip - lastSkippedProjectile) > 8) // If too far, repeat
                projectileToSkip = lastSkippedProjectile;
        }

        lastSkippedProjectile = projectileToSkip;

        for (int i = -11; i <= 11; i++)
        {
            if (i == projectileToSkip || i == projectileToSkip + 1)
                continue;
            spawnProjectile(i);
        }
    }

    void spawnProjectile(float spawnX)
    {
        if (isDead)
            return;

        Vector3 spawnPosition = new Vector3(spawnX, 9);
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.initialize(power, bulletSpeed, 0);
        ep.setDirection(Vector3.down);
        ep.setColor(new Color(0.15f, 0.8f, 0.15f));
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[119] = true;
    }
}
