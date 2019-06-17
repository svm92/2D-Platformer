using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCyan01 : Boss {

    public GameObject projectile;

    float delayBetweenShots = 0.2f;
    float rotationSpeed = 1f;
    float accelerationFactor = 1f;
    float haltTime = 0.5f;
    float nOfSecondaryBullets = 0;
    bool isWaiting = true;

    protected override void Awake()
    {
        base.Awake();
        speed = 3f;
        health = 165;
        power = 1;
    }

    protected override void Start()
    {
        base.Start();
        if (Random.Range(0, 2) == 0) // Choose random initial direction
            rotationSpeed = -rotationSpeed;
        transform.rotation = Quaternion.Euler(0, 0, 180);

        StartCoroutine(rotate());
        StartCoroutine(spawnProjectile());
        StartCoroutine(spawnSecondaryBullets());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isDead)
        {
            recheckValues();
        }
    }

    IEnumerator rotate()
    {
        bool isBeginningTheRotation = true;

        // Rotate
        while (isBeginningTheRotation || Mathf.Abs(transform.rotation.eulerAngles.z - 180) > 2)
        {
            transform.Rotate(0, 0, rotationSpeed * accelerationFactor);
            accelerationFactor += 0.04f;

            if (isBeginningTheRotation && Mathf.Abs(transform.rotation.eulerAngles.z - 180) > 2)
            {
                isBeginningTheRotation = false;
                isWaiting = false;
            }
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 0, 180);

        // Stop
        isWaiting = true;
        yield return new WaitForSeconds(haltTime);
        accelerationFactor = 1f;
        if (Random.Range(0, 2) == 0)
            rotationSpeed = -rotationSpeed;

        StartCoroutine(rotate());

    }

    void recheckValues()
    {
        if (health <= 0.15f * maxHealth)
        {
            delayBetweenShots = 0.075f;
            haltTime = 0;
            nOfSecondaryBullets = 6;
        }
        else if (health <= 0.45f * maxHealth)
        {
            delayBetweenShots = 0.1f;
            haltTime = 0.2f;
            nOfSecondaryBullets = 6;
        }
        else if (health <= 0.75f * maxHealth)
        {
            delayBetweenShots = 0.125f;
            haltTime = 0.35f;
            nOfSecondaryBullets = 4;
        } else if (health <= 0.95f * maxHealth)
        {
            delayBetweenShots = 0.15f;
        }
    }

    IEnumerator spawnProjectile()
    {
        yield return new WaitForSeconds(delayBetweenShots);

        if (isDead)
            yield break;

        if (isWaiting)
        {
            StartCoroutine(spawnProjectile());
            yield break;
        }

        Vector3 spawnPosition = transform.position;
        Vector3 projectileDirection = -transform.up;
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.initialize(power, 7f, 0);
        ep.setDirection(projectileDirection);
        ep.setScale(1f);
        ep.setColor(new Color(0f, 0.5f, 0.85f));

        StartCoroutine(spawnProjectile());
    }

    IEnumerator spawnSecondaryBullets()
    {
        yield return new WaitForSeconds(Mathf.Max(delayBetweenShots * 25, 2f));

        if (isDead)
            yield break;

        if (nOfSecondaryBullets > 0)
        {
            float angleBetweenBullets = 360 / nOfSecondaryBullets;
            float initialAngle = Random.Range(0, 360);
            Vector3 initialVector = Quaternion.Euler(0, 0, initialAngle) * Vector3.down;
            for (int i = 0; i < nOfSecondaryBullets; i++)
                spawnSecondaryBullet(i * angleBetweenBullets, initialVector);
        }

        StartCoroutine(spawnSecondaryBullets());
    }

    void spawnSecondaryBullet(float angle, Vector3 initialVector)
    {
        Vector3 spawnPosition = transform.position;
        Vector3 projectileDirection = Quaternion.Euler(0, 0, angle) * initialVector;
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.initialize(power, 7f, 0);
        ep.setDirection(projectileDirection);
        ep.setScale(0.7f);
        ep.setColor(new Color(0.1f, 0.8f, 0.45f));
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[108] = true;
    }
}
