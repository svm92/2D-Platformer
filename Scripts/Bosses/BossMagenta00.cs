using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMagenta00 : Boss {

    public GameObject projectile;

    Vector3 direction = Vector3.right;
    float delayBetweenShots = 1f;
    int nOfProjectilesOnExplode = 3;

    protected override void Awake()
    {
        base.Awake();
        speed = 3f;
        health = 195;
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
        if (health <= 0.25f * maxHealth)
        {
            delayBetweenShots = 0.3f;
            nOfProjectilesOnExplode = 5;
        }
        else if (health <= 0.5f * maxHealth)
        {
            delayBetweenShots = 0.5f;
            nOfProjectilesOnExplode = 5;
        }
        else if (health <= 0.7f * maxHealth)
        {
            delayBetweenShots = 0.75f;
            nOfProjectilesOnExplode = 5;
        }
        else if (health <= 0.9f * maxHealth)
        {
            delayBetweenShots = 0.85f;
        }
    }

    IEnumerator spawnProjectile()
    {
        yield return new WaitForSeconds(delayBetweenShots);

        if (isDead)
            yield break;

        Vector3 spawnPosition = transform.position;
        bool randomSign = Random.Range(0, 2) == 0 ? true : false;
        float randomAngle = (randomSign ? 1 : -1) * ( Random.Range(65, 100) );
        Vector3 projectileDirection = Quaternion.Euler(0, 0, randomAngle) * Vector3.down;
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.initialize(power, 10f, 0);
        ep.setDirection(projectileDirection);
        ep.setScale(1.1f);
        ep.setBounceExplode(nOfProjectilesOnExplode);
        ep.setColor(new Color(0.75f, 0.1f, 0.75f));

        StartCoroutine(spawnProjectile());
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[106] = true;
    }
}
