using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRed00 : Boss {

    public GameObject projectile;

    Vector3 rotationAxis = Vector3.back;

    protected override void Awake()
    {
        base.Awake();
        speed = 50f;
        health = 105;
        power = 1;
    }

    protected override void Start()
    {
        base.Start();
        InvokeRepeating("spawnProjectile", 2.5f, 0.2f);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isDead)
            transform.Rotate(rotationAxis, speed * speedModifier() * Time.deltaTime);
    }

    float speedModifier()
    {
        if (health >= 0.45f * maxHealth)
            return 0.7f;
        else if (health >= 0.8f * maxHealth)
            return 0.85f;
        else
            return 1f;
    }

    void spawnProjectile()
    {
        if (isDead)
            return;
        Vector3 spawnPosition = transform.position;
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.setDirection(-transform.up);
        ep.initialize(1, 10f, 2.5f);
        ep.setScale(3.5f);
        ep.setThroughWalls(true);
        ep.setColor(new Color(.85f, .1f, .15f));
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[2] = true;
    }

}
