using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRed01 : Boss {

    public GameObject projectile;

    Vector3 direction = Vector3.right;

    protected override void Awake()
    {
        base.Awake();
        speed = 2.5f;
        health = 150;
        power = 2;
    }

    protected override void Start()
    {
        base.Start();
        InvokeRepeating("spawnProjectile", 0.5f, 2.5f);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isDead)
            transform.position += direction * speed * Time.deltaTime;
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

    void spawnProjectile()
    {
        if (isDead)
            return;

        int movingUp = 0; // 1 yes, 0 no

        if (health <= 0.5f * maxHealth)
            movingUp = Random.Range(0, 2);

        float randomY = movingUp == 1 ? -7 : 12;
        float randomX = Random.Range(-11, 11);
        Vector3 spawnPosition = new Vector3(randomX, randomY);
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();

        if (movingUp == 1)
        {
            ep.setDirection(Vector3.up);
            ep.initialize(power, 3.5f, 5f);
            ep.setScale(7f);
        }
        else
        {
            ep.setDirection(Vector3.down);
            ep.initialize(power, 5.5f, 5f);
            ep.setScale(8f);
        }
        
        ep.setRotationSpeed(1f);
        ep.setThroughWalls(true);
        ep.setColor(new Color(.85f, .1f, .15f));
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[18] = true;
    }

}
