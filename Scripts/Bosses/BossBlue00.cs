using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBlue00 : Boss {

    public GameObject projectile;

    Vector3 direction = Vector3.right;

    protected override void Awake()
    {
        base.Awake();
        speed = 3f;
        health = 75;
        power = 1;
    }

    protected override void Start()
    {
        base.Start();
        InvokeRepeating("spawnProjectile", 1.5f, 0.75f);
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
        int rnd = Random.Range(0, 2); // 0 appears from left, 1 appears from right
        float randomX = (rnd == 0) ? -11 : 11;
        float randomY = Random.Range(0, 2.5f);
        float randomSpeed = Random.Range(5f, 9f);
        Vector3 spawnPosition = new Vector3(randomX, randomY);
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        if (rnd == 0)
            ep.setDirection(Vector3.right);
        else
            ep.setDirection(Vector3.left);
        ep.initialize(power, randomSpeed, 0);
        ep.setColor(Color.blue);
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[0] = true;
    }
}
