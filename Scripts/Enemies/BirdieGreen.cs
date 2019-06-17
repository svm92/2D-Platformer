using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdieGreen : Enemy {

    public GameObject projectile;
    [SerializeField] float shootSpeed = 5f;
    [SerializeField] float timeBetweenShooting = 1.5f;
    [SerializeField] float bulletLifespan = 3f;

    protected override void Awake()
    {
        base.Awake();
        health = 7;
        power = 1;
    }

    protected override void Start()
    {
        base.Start();
        InvokeRepeating("spawnProjectile", 2f, timeBetweenShooting);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isActive && !isDead)
            lookAtPlayer();
    }

    void lookAtPlayer()
    {
        if (player.transform.position.x >= transform.position.x) // Player is right
            sprite.flipX = false;
        else
            sprite.flipX = true;
    }

    void spawnProjectile()
    {
        if (playerIsTooFar() || isDead)
            return;
        Vector3 spawnPosition = transform.position;
        Vector3 shootDirection = (player.transform.position - transform.position).normalized; // Towards player
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.setDirection(shootDirection);
        ep.initialize(power, shootSpeed, bulletLifespan);
        ep.setScale(0.75f);
        ep.setColor(Color.green);
    }

    bool playerIsTooFar()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float shootDistance = shootSpeed * bulletLifespan;
        // Player is considered too far if more than 15% away from the max distance a bullet travels
        return (distanceToPlayer >= (shootDistance * 1.15f));
    }
}
