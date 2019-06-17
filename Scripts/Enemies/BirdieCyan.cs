using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdieCyan : Enemy {

    public GameObject projectile;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float cautionDistance = 10f;
    [SerializeField] float shootSpeed = 3f;
    [SerializeField] float timeBetweenShooting = 4f;
    [SerializeField] float bulletLifespan = 4f;
    [SerializeField] bool bulletsThroughWalls = false;
    [SerializeField] int nOfBullets = 4;

    float radius;
    CompositeCollider2D gridCollider;

    protected override void Awake()
    {
        base.Awake();
        speed = moveSpeed;
        health = 10;
        power = 1;
    }

    protected override void Start()
    {
        base.Start();
        radius = GetComponent<CircleCollider2D>().radius;
        gridCollider = GameObject.Find("Grid").GetComponentInChildren<CompositeCollider2D>();
        InvokeRepeating("spawnProjectileBarrage", 2f, timeBetweenShooting);
        StartCoroutine(moveAway());
    }

    IEnumerator moveAway()
    {
        if (isDead)
            yield break;

        lookAtPlayer();

        if (!isActive || Vector3.Distance(transform.position, player.transform.position) >= cautionDistance)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(moveAway());
            yield break;
        }

        Vector3 oldPosition = transform.position;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, player.transform.position, -0.1f);

        if (destinationIsWall(newPosition))
        {
            yield return new WaitForSeconds(0.25f);
            StartCoroutine(moveAway());
            yield break;
        }

        float timer = 0;
        float distance = Vector3.Distance(transform.position, newPosition);
        float movementDuration = distance / speed;
        while (timer <= movementDuration)
        {
            transform.position = Vector3.Lerp(oldPosition, newPosition, (1 / movementDuration) * timer);
            timer += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(moveAway());
    }

    void lookAtPlayer()
    {
        if (player.transform.position.x >= transform.position.x) // Player is right
            sprite.flipX = false;
        else
            sprite.flipX = true;
    }

    bool destinationIsWall(Vector3 newPosition)
    {
        Vector3[] pointsToCheck = new Vector3[] { newPosition + Vector3.up * radius, newPosition + Vector3.down * radius,
        newPosition + Vector3.left * radius, newPosition + Vector3.right * radius};

        foreach (Vector3 point in pointsToCheck)
        {
            if (gridCollider.OverlapPoint(point))
                return true;
        }
        return false;
    }

    void spawnProjectileBarrage()
    {
        if (playerIsTooFar() || isDead)
            return;

        float angleBetweenBullets = 360 / nOfBullets;

        int rnd = Random.Range(0, 2);
        if (rnd == 0)
        {
            for (int i=0; i < nOfBullets; i++)
                spawnProjectile(angleBetweenBullets * i);
        } else
        {
            for (int i = 0; i < nOfBullets; i++)
                spawnProjectile((angleBetweenBullets / 2) + angleBetweenBullets * i);
        }
    }

    void spawnProjectile(float angle)
    {
        Vector3 spawnPosition = transform.position;
        Vector3 shootDirection = (player.transform.position - transform.position).normalized; // Towards player
        shootDirection = Quaternion.Euler(0, 0, angle) * shootDirection;
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.initialize(power, shootSpeed, bulletLifespan);
        ep.setDirection(shootDirection);
        ep.setScale(0.75f);
        ep.setColor(Color.cyan);
        if (bulletsThroughWalls)
            ep.setThroughWalls(true);
    }

    bool playerIsTooFar()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float shootDistance = shootSpeed * bulletLifespan;
        // Player is considered too far if more than 15% away from the max distance a bullet travels
        return (distanceToPlayer >= (shootDistance * 1.15f));
    }
}
