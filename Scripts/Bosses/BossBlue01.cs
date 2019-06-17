using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBlue01 : Boss {

    public GameObject projectile;

    Vector3 direction = Vector3.right;
    int nOfBullets = 1;

    protected override void Awake()
    {
        base.Awake();
        speed = 5;
        health = 90;
        power = 1;
    }

    protected override void Start()
    {
        base.Start();
        InvokeRepeating("spawnProjectileBarrage", 1.5f, 2f);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isDead)
        {
            transform.position += direction * speed * Time.deltaTime;
            recheckNOfBullets();
            recheckSpeed();
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

    void recheckNOfBullets()
    {
        if (health <= 0.46f * maxHealth)
            nOfBullets = 5;
        else if (health <= 0.9f * maxHealth)
            nOfBullets = 3;
    }

    void recheckSpeed()
    {
        if (health <= 0.16f * maxHealth)
            speed = 12.5f;
        else if (health <= 0.33f * maxHealth)
            speed = 11f;
        else if (health <= 0.56f * maxHealth)
            speed = 10f;
        else if (health <= 0.76f * maxHealth)
            speed = 7f;
    }

    void spawnProjectileBarrage()
    {
        spawnProjectile(0);
        switch (nOfBullets)
        {
            case 3:
                spawnProjectile(-45);
                spawnProjectile(45);
                break;
            case 5:
                spawnProjectile(-60);
                spawnProjectile(60);
                spawnProjectile(-30);
                spawnProjectile(30);
                break;
            default:
                break;
        }
    }

    void spawnProjectile(float n)
    {
        if (isDead)
            return;
        Vector3 spawnPosition = transform.position + Vector3.down;
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        Vector3 towardsPlayer = (player.transform.position - transform.position).normalized;
        Vector3 attackDirection = Quaternion.Euler(0, 0, n) * towardsPlayer;
        ep.setDirection(attackDirection);
        ep.initialize(power, 10f, 0);
        ep.setScale(1.2f);
        ep.setColor(new Color(0.2f, 0.2f, 0.8f));
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[10] = true;
    }
}
