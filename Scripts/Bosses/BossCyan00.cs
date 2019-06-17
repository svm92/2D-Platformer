using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCyan00 : Boss {

    public GameObject projectile;

    Vector3 direction = Vector3.right;
    float delayBetweenShots = 1.5f;
    int nOfProjectilesOnExplode = 4;

    protected override void Awake()
    {
        base.Awake();
        speed = 3f;
        health = 180;
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
        if (health <= 0.16f * maxHealth)
        {
            delayBetweenShots = 0.75f;
            nOfProjectilesOnExplode = 12;
        }
        else if (health <= 0.45f * maxHealth)
        {
            delayBetweenShots = 0.75f;
            nOfProjectilesOnExplode = 10;
        }
        else if (health <= 0.6f * maxHealth)
        {
            delayBetweenShots = 1f;
            nOfProjectilesOnExplode = 8;
        }
        else if (health <= 0.85f * maxHealth)
        {
            delayBetweenShots = 1.25f;
            nOfProjectilesOnExplode = 6;
        }
    }

    IEnumerator spawnProjectile()
    {
        yield return new WaitForSeconds(delayBetweenShots);

        if (isDead)
            yield break;

        Vector3 spawnPosition = transform.position;
        Vector3 projectileDirection = Quaternion.Euler(0, 0, Random.Range(-100, 100)) * Vector3.down;
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.initialize(power, 4f, 1f);
        ep.setDirection(projectileDirection);
        ep.setThroughWalls(true);
        ep.setScale(1.2f);
        ep.setExploding(nOfProjectilesOnExplode);
        ep.setColor(new Color(0.1f, 0.8f, 0.75f));

        StartCoroutine(spawnProjectile());
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[105] = true;
    }
}
