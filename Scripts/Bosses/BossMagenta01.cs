using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMagenta01 : Boss {

    public GameObject projectile;

    float delayBetweenShots = 1.25f;
    int jumpChance = 30; // 1 in n chance of jumping every 0.5 seconds
    int tackleChance = 30; // 1 in n chance of tackling every 0.5 seconds
    float jumpSpeed = 0.25f;
    float fallSpeed = 25f;
    float riseSpeed = 10f;
    float tackleWarningTime = 1f;
    float timer = 0;
    bool isJumping = false;
    bool isTackling = false;

    Vector3[] projectileSpawnPoints = new Vector3[] 
        {new Vector3(-22, 15), new Vector3(22, 15), new Vector3(-19, 27), new Vector3(19, 27)};

    Vector3[] destinationPoints = new Vector3[] {new Vector3(-10.5f, 29), new Vector3(11.5f, 29)};

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
        transform.position = destinationPoints[ Random.Range(0, destinationPoints.Length) ];
        StartCoroutine(spawnProjectile());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isDead)
        {
            timer += Time.deltaTime;
            if (timer >= 0.5f)
            {
                timer = 0;
                if (Random.Range(0, 2) == 0)
                    tryJumping();
                else
                    tryTackling();
            }
            recheckValues();
        }
    }

    void recheckValues()
    {
        if (health <= 0.2f * maxHealth)
        {
            delayBetweenShots = 0.55f;
            tackleWarningTime = 0.6f;
            jumpChance = 5;
            tackleChance = 7;
        }
        else if (health <= 0.4f * maxHealth)
        {
            delayBetweenShots = 0.7f;
            tackleWarningTime = 0.8f;
            jumpChance = 8;
            tackleChance = 10;
        }
        else if (health <= 0.6f * maxHealth)
        {
            delayBetweenShots = 0.85f;
            jumpChance = 10;
            tackleChance = 15;
        }
        else if (health <= 0.8f * maxHealth)
        {
            delayBetweenShots = 1f;
            jumpChance = 15;
        }
    }

    void tryJumping()
    {
        if (isJumping || isTackling)
            return;

        if (Random.Range(0, jumpChance) == 0)
            doubleJump();
    }

    void doubleJump()
    {
        timer = -10f; // Miniminum time the boss has to wait before being able to act again
        isJumping = true;
        Vector3 jumpDirection = (transform.position.x < 0) ? Vector3.right : Vector3.left;
        StartCoroutine(jump(jumpDirection, true));
    }

    IEnumerator jump(Vector3 direction, bool isFirstJump)
    {
        const float distanceToJump = 11;
        const float jumpRadius = distanceToJump / 2;
        float yCircleCenter = transform.position.y;
        float xCircleCenter = transform.position.x + (direction == Vector3.right ? jumpRadius : -jumpRadius);
        bool jumpIsOver = false;

        while (!jumpIsOver)
        {
            if (isDead)
                yield break;

            float xPosition = transform.position.x + (direction == Vector3.right ? jumpSpeed : -jumpSpeed);

            if (Mathf.Pow(jumpRadius, 2) - Mathf.Pow(xPosition - xCircleCenter, 2) < 0) // End of jump
                jumpIsOver = true;
            else {
                // Circle formula: (x-x0)^2 + (y-y0)^2 = R=^2 -> y = y0 + sqrt[ R^2 - (x-x0)^2 ]
                float yPosition = yCircleCenter + 
                    Mathf.Sqrt(Mathf.Pow(jumpRadius, 2) - Mathf.Pow(xPosition - xCircleCenter, 2));
                transform.position = new Vector3(xPosition, yPosition);
                yield return null;
            }
        }

        float xDestination = xCircleCenter + (direction == Vector3.right ? jumpRadius : -jumpRadius);
        transform.position = new Vector3(xDestination, yCircleCenter);

        if (isFirstJump)
            StartCoroutine(jump(direction, false));
        else
            isJumping = false;
    }

    void tryTackling()
    {
        if (isJumping || isTackling)
            return;

        if (Random.Range(0, tackleChance) == 0)
            beginTackle();
    }

    void beginTackle()
    {
        timer = -5f; // Miniminum time the boss has to wait before being able to act again
        isTackling = true;
        StartCoroutine(tackle());
    }

    IEnumerator tackle()
    {
        const float fallDistance = 27;
        const float warningDistance = 2;
        Vector3 originalPosition = transform.position;
        Vector3 destination = transform.position + Vector3.down * fallDistance;
        Vector3 warningPosition = transform.position + Vector3.up * warningDistance;
        float fallTimer = 0;
        float travelTime = fallDistance / fallSpeed;

        // Pre-attack warning
        while (transform.position != warningPosition)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, warningPosition, fallTimer / tackleWarningTime);
            fallTimer += Time.deltaTime;
            yield return null;
        }

        fallTimer = 0;
        // Fall
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(warningPosition, destination, fallTimer / travelTime);
            fallTimer += Time.deltaTime;
            yield return null;
        }

        fallTimer = 0;
        travelTime = fallDistance / riseSpeed;
        // Rise
        while (transform.position != originalPosition)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(destination, originalPosition, fallTimer / travelTime);
            fallTimer += Time.deltaTime;
            yield return null;
        }

        isTackling = false;
    }

    IEnumerator spawnProjectile()
    {
        yield return new WaitForSeconds(delayBetweenShots);

        if (isDead)
            yield break;

        Vector3 spawnPosition = projectileSpawnPoints[ Random.Range(0, projectileSpawnPoints.Length) ];
        EnemyProjectile ep = Instantiate(projectile, spawnPosition, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.initialize(power, 7f, 25f);
        ep.homeInitially();
        ep.setThroughWalls(true);
        ep.setScale(0.75f);
        ep.setColor(new Color(0.75f, 0.1f, 0.75f));

        StartCoroutine(spawnProjectile());
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[107] = true;
    }
}
