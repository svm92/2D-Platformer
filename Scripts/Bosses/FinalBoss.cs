using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : BossMain {

    protected Color[] possibleColors;
    [HideInInspector] public float neutralX;

    static int partneredRotationDirection = 1;
    static float partneredRotationSpeed = 1f;

    ParticleSystem.MainModule main;
    ParticleSystem.ShapeModule shp;

    protected override void Awake()
    {
        base.Awake();
        neutralX = transform.position.x;
        main = ps.main;
        shp = ps.shape;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isHurting && sprite.color != bossColor)
            sprite.color = bossColor;
    }

    void changeColor(int colorIndex)
    {
        Color randomColor = possibleColors[colorIndex];
        sprite.color = randomColor;

        bossColor = randomColor;
        Color lighterBossColor = getLighterColor(bossColor);
        ParticleSystem.MainModule settings = ps.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(bossColor, lighterBossColor);

        spotlight.color = bossColor;

        fireballColor = bossColor * 0.85f;
    }

    void changeColorAtRandom()
    {
        int randomColorIndex = Random.Range(1, possibleColors.Length); // 1~3
        changeColor(randomColorIndex);
    }

    void recoverOriginalColor()
    {
        changeColor(0);
    }

    public IEnumerator jumpToNeutralPosition()
    {
        float travelTime = 1f;
        float travelDistance = Mathf.Abs(transform.position.x - neutralX);
        float timer = 0;

        if (travelDistance <= 5) // If close, move there
        {
            Vector3 currentPosition = transform.position;
            Vector3 destination = new Vector3(neutralX, transform.position.y);
            
            while (timer < travelTime)
            {
                transform.position = Vector3.Lerp(currentPosition, destination, timer / travelTime);
                timer += Time.deltaTime;
                yield return null;
            }
        } else // If far, jump there
        {
            float currentX = transform.position.x;
            float circleRadius = travelDistance / 2f;
            float xDisplacement = (neutralX > currentX) ? currentX + circleRadius : currentX - circleRadius;

            while (timer < travelTime)
            {
                float newX = Mathf.Lerp(currentX, neutralX, timer / travelTime);
                float newY = Mathf.Sqrt(Mathf.Pow(circleRadius, 2) - Mathf.Pow(newX - xDisplacement, 2));
                transform.position = new Vector3(newX, newY);
                timer += Time.deltaTime;
                yield return null;
            }
        }
        transform.position = new Vector3(neutralX, -0.5f);

        StartCoroutine(act());
    }

    public IEnumerator lineShoot()
    {
        ps.Stop();

        changeColorAtRandom();

        // Move up
        Vector3 currentPosition = transform.position;
        Vector3 destination = currentPosition + (Vector3.up * Random.Range(0.75f, 2f));
        Quaternion targetRotation = (transform.position.x > 0) ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, -90);
        float timer = 0;
        while (timer < beforeShootWaitTime)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / beforeShootWaitTime);
            transform.rotation = Quaternion.Lerp(Quaternion.identity, targetRotation, timer / beforeShootWaitTime);
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(beforeShootWaitTime);

        shoot();

        yield return new WaitForSeconds(afterShootWaitTime);

        recoverOriginalColor();
        ps.Play();

        // Go down
        Quaternion currentRotation = transform.rotation;
        currentPosition = transform.position;
        destination = new Vector3(transform.position.x, -0.5f);
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / afterShootWaitTime);
            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / afterShootWaitTime);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        StartCoroutine(act());
    }

    public IEnumerator colorAttack()
    {
        changeColorAtRandom();

        yield return new WaitForSeconds(beforeShootWaitTime);

        // All attacks must last exacly 15 seconds
        if (bossColor == Color.red)
            StartCoroutine(redColorAttack());
        else if (bossColor == Color.green)
            StartCoroutine(greenColorAttack());
        else if (bossColor == Color.blue)
            StartCoroutine(blueColorAttack());
        else if (bossColor == Color.cyan)
            StartCoroutine(cyanColorAttack());
        else if (bossColor == Color.magenta)
            StartCoroutine(magentaColorAttack());
        else if (bossColor == new Color(1, 1, 0))
            StartCoroutine(yellowColorAttack());
    }

    IEnumerator redColorAttack()
    {
        float firstMoveTime = 1.5f;
        float attackDuration = 12f;
        float undoRotationTime = 0.5f;
        float lastMoveTime = 1f;

        ps.Stop();

        Vector3 originalPosition = transform.position;
        Vector3 destination = new Vector3(0, 5);
        float timer = 0;

        // Move to center of screen
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / firstMoveTime);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = destination;

        // Alter flame
        prepareRedFlameCannon();
        ps.Play();

        // Rotate (attack)
        int rotationDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
        timer = 0;
        while (timer < attackDuration)
        {
            if (isDead)
                yield break;

            transform.Rotate(0, 0, rollSpeed * 5 * rotationDirection);
            timer += Time.deltaTime;
            yield return null;
        }

        // End rotation
        ps.Stop();
        Quaternion originalRotation = transform.rotation;
        timer = 0;
        while (transform.rotation != Quaternion.identity)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(originalRotation, Quaternion.identity, timer / undoRotationTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Fall
        originalPosition = transform.position;
        destination = new Vector3(transform.position.x, -0.5f);
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / lastMoveTime);
            timer += Time.deltaTime;
            yield return null;
        }

        resetFlame();
        recoverOriginalColor();
        ps.Play();
        StartCoroutine(act());
    }

    IEnumerator greenColorAttack()
    {
        float preparationDuration = 1f;
        float attackDuration = 12f;
        float undoRotationTime = 1f;
        float fallDuration = 1f;

        float timer = 0;
        int rotationDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
        // Pre attack warning
        float preAttackDistance = 1.5f;
        Vector3 originalPosition = transform.position;
        Vector3 destination = new Vector3(transform.position.x, preAttackDistance);
        while (timer < preparationDuration)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / preparationDuration);
            transform.Rotate(0, 0, speed * rotationDirection);
            timer += Time.deltaTime;
            yield return null;
        }

        // Rotate around
        Vector3 attackDirection = Quaternion.Euler(0, 0, Random.Range(90, 270)) * Vector3.right;
        timer = 0;
        while (timer < attackDuration)
        {
            if (isDead)
                yield break;

            transform.position += attackDirection * 0.1f;
            transform.Rotate(0, 0, speed * rotationDirection * 0.5f);
            timer += Time.deltaTime;

            Vector3 collisionWall = findCollisionWall();
            if (collisionWall != Vector3.zero)
            {
                int angleSign;
                if (collisionWall.x == 0) // Bouncing on floor or ceiling
                {
                    bool goingRight = (attackDirection.x > 0);
                    angleSign = goingRight ? -1 : 1;
                    if (collisionWall == Vector3.up) // Opposite angle for ceiling
                        angleSign = -angleSign;
                }
                else // Bouncing on left or right wall
                {
                    bool goingUp = (attackDirection.y > 0);
                    angleSign = goingUp ? -1 : 1;
                    if (collisionWall == Vector3.left) // Opposite angle for left wall
                        angleSign = -angleSign;
                }

                float angleWithPerpendicular = Vector3.Angle(attackDirection, collisionWall);
                float reflectionAngle = angleWithPerpendicular * 2;
                attackDirection = Quaternion.Euler(0, 0, reflectionAngle * angleSign) * -attackDirection;
            }

            yield return null;
        }

        ps.Stop();

        // Stop
        Quaternion currentRotation = transform.rotation;
        timer = 0;
        while (timer < undoRotationTime)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / undoRotationTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Fall
        originalPosition = transform.position;
        destination = new Vector3(transform.position.x, -0.5f);
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / fallDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        recoverOriginalColor();
        ps.Play();
        StartCoroutine(act());
    }

    void spawnBlueFireballCircle()
    {
        Vector3 spawnPosition = transform.position + transform.up * 0.5f + transform.right * 0.2f;
        int nOfBullets = 16;
        float separationBetweenBullets = 360 / nOfBullets;
        for (int i=0; i < nOfBullets; i++)
        {
            Fireball fireballObj = shootBlueBullet(separationBetweenBullets * i, spawnPosition);
            fireballObj.setSpeed(6.5f);
        }
    }

    IEnumerator blueColorAttack()
    {
        float firstMoveTime = 1f;
        float afterMoveWaitTime = 0.5f;
        float attackDuration = 12f;
        float undoRotationTime = 0.5f;
        float lastMoveTime = 1f;

        Vector3 originalPosition = transform.position;
        Vector3 destination = new Vector3(0, 5);
        float timer = 0;

        // Move to center of screen
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / firstMoveTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = destination;

        yield return new WaitForSeconds(afterMoveWaitTime);

        // Rotate (attack)
        int rotationDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
        float betweenBulletSpawnTime = attackDuration / 3.2f; // Shoot ~3 times
        float bulletSpawnTimer = 0;
        timer = 0;
        while (timer < attackDuration)
        {
            if (isDead)
                yield break;

            transform.Rotate(0, 0, rollSpeed * 10 * rotationDirection);
            timer += Time.deltaTime;
            bulletSpawnTimer += Time.deltaTime;

            if (bulletSpawnTimer >= betweenBulletSpawnTime)
            {
                bulletSpawnTimer = 0;
                spawnBlueFireballCircle();
            }

            yield return null;
        }

        // End rotation
        Quaternion originalRotation = transform.rotation;
        timer = 0;
        while (transform.rotation != Quaternion.identity)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(originalRotation, Quaternion.identity, timer / undoRotationTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Fall
        originalPosition = transform.position;
        destination = new Vector3(transform.position.x, -0.5f);
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / lastMoveTime);
            timer += Time.deltaTime;
            yield return null;
        }

        recoverOriginalColor();
        StartCoroutine(act());
    }

    IEnumerator cyanColorAttack()
    {
        float firstMoveTime = 2f;
        float waitAfterFirstMove = 1.5f;
        float sparkRaiseDuration = 1.5f;
        float sparkExpansionDuration = 2.5f;
        float attackDuration = 2.5f;
        float sparkShrinkingDuration = 2f;
        float sparkLowerDuration = 2f;
        float afterAttackDuration = 1f;

        ps.Stop();

        // Prepare spark
        alterFlameSize(0.2f, 0.5f);
        alterFlameLifetime(0.25f);
        alterFlameSpeed(3f, 3f);
        alterFlamesOverTime(50);
        alterFlameAngle(180);
        shp.position = new Vector3(0, 1, 0);
        shp.rotation = Vector3.zero;
        main.startColor = Color.cyan;

        // Move to center
        Vector3 currentPosition = transform.position;
        float closestBorder = (transform.position.x > 0) ? 8f : -8f;
        Vector3 destination = new Vector3(closestBorder, currentPosition.y);
        float timer = 0;
        while (timer < firstMoveTime)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / firstMoveTime);
            timer += Time.deltaTime;
            yield return null;
        }

        ps.Play();
        yield return new WaitForSeconds(waitAfterFirstMove);

        // Move spark up
        timer = 0;
        while (timer < sparkRaiseDuration)
        {
            if (isDead)
                yield break;

            shp.position = new Vector3(0, Mathf.Lerp(1, 2.5f, timer / sparkRaiseDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        shp.position = new Vector3(0, 2.5f);

        alterFlameLifetime(10f);
        alterFlameSpeed(3.5f, 3.5f);
        alterFlameSize(1f);
        alterFlamesOverTime(2);

        // Initiate attack
        timer = 0;
        while (timer < sparkExpansionDuration)
        {
            if (isDead)
                yield break;

            alterFlameLifetime(Mathf.Lerp(0.25f, 10f, timer / sparkExpansionDuration));
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(attackDuration);

        // End attack
        timer = 0;
        while (timer < sparkShrinkingDuration)
        {
            if (isDead)
                yield break;

            alterFlameLifetime(Mathf.Lerp(10f, 0.25f, timer / sparkShrinkingDuration));
            timer += Time.deltaTime;
            yield return null;
        }

        // Prepare small spark
        alterFlameSize(0.2f, 0.5f);
        alterFlameLifetime(0.25f);
        alterFlameSpeed(3f, 3f);
        alterFlamesOverTime(50);

        // Move spark down
        timer = 0;
        while (timer < sparkLowerDuration)
        {
            if (isDead)
                yield break;

            shp.position = new Vector3(0, Mathf.Lerp(2.5f, 1, timer / sparkLowerDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        shp.position = new Vector3(0, 1);

        alterFlameAngle(15);
        shp.position = new Vector3(0, 0.5f, 0);
        shp.rotation = new Vector3(180, 180, -105);
        resetFlame();
        yield return new WaitForSeconds(afterAttackDuration);

        recoverOriginalColor();
        StartCoroutine(act());
    }

    IEnumerator magentaSingleCycle(float travelTime, float preAttackWarningTime, float attackTime)
    {
        ps.Stop();

        // Prepare warning flame
        prepareWarningFlame();

        float randomArc = (Random.Range(0, 2) == 0) ? 0 : 45;
        shp.rotation = new Vector3(0, 0, randomArc);

        // Move to random destination
        float randomX = Random.Range(-8, 8);
        float randomY = Random.Range(-1, 8);
        Vector3 currentPosition = transform.position;
        Vector3 destination = new Vector3(randomX, randomY);
        float timer = 0;
        while (timer < travelTime)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / travelTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Pre-attack warning
        ps.Play();
        yield return new WaitForSeconds(preAttackWarningTime);

        // Attack
        prepareAttackFlame();
        yield return new WaitForSeconds(attackTime);

        // End attack
        ps.Stop();
    }

    IEnumerator magentaColorAttack()
    {
        float travelTime = 2f; // x3
        float preAttackWarningTime = 1.5f; // x3
        float attackTime = 1f; // x3
        float fallTime = 1.5f;

        // Specific magenta flame properties
        alterFlamesOverTime(250);
        main.scalingMode = ParticleSystemScalingMode.Shape;
        shp.radiusThickness = 0;
        shp.arc = 360;
        shp.arcSpread = 0.25f;
        shp.position = Vector3.zero;

        yield return StartCoroutine(magentaSingleCycle(travelTime, preAttackWarningTime, attackTime));
        yield return StartCoroutine(magentaSingleCycle(travelTime, preAttackWarningTime, attackTime));
        yield return StartCoroutine(magentaSingleCycle(travelTime, preAttackWarningTime, attackTime));

        // Fall
        Vector3 currentPosition = transform.position;
        Vector3 destination = new Vector3(transform.position.x, -0.5f);
        float timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / fallTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Reset flame values
        main.scalingMode = ParticleSystemScalingMode.Local;
        shp.radius = 0.2f;
        shp.radiusThickness = 1;
        shp.arc = 30;
        shp.arcSpread = 0;
        shp.position = Vector3.up * 0.5f;
        shp.rotation = new Vector3(180, 180, -105);
        resetFlame();

        recoverOriginalColor();
        ps.Play();
        StartCoroutine(act());
    }

    IEnumerator moveIntoPositionAndShootYellow(float movementTime, float afterShootWaitTime)
    {
        float oldBulletSpeed = bulletSpeed;
        bulletSpeed = 10f;

        float randomY = 3;
        float randomAngle = 90;
        while ((Mathf.Abs(randomY - 3) < 2 && randomAngle == 90) || // If shooting straight and block ahead
            (Mathf.Abs(randomY - 3) > 2 && randomAngle == 45) || // If shooting upwards near ceiling/floor (to avoid block)
            (Mathf.Abs(randomY - 3) > 2 && randomAngle == 135)) // If shooting downwards near ceiling/floor
        {
            randomY = Random.Range(-0.5f, 8.5f);
            randomAngle = 45 * Random.Range(1, 4);
        }

        // Move into position
        Vector3 currentPosition = transform.position;
        float closestBorder = (currentPosition.x > 0) ? 8.5f : -8.5f;
        
        Vector3 destination = new Vector3(closestBorder, randomY);
        Quaternion currentRotation = transform.rotation;
        float angleSign = (currentPosition.x > 0) ? 1 : -1;
        Quaternion destinationRotation = Quaternion.Euler(0, 0, randomAngle * angleSign);
        float timer = 0;
        while (timer < movementTime)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / movementTime);
            transform.rotation = Quaternion.Lerp(currentRotation, destinationRotation, timer / movementTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = destinationRotation;

        shootYellow(true, transform.up);

        yield return new WaitForSeconds(afterShootWaitTime);

        bulletSpeed = oldBulletSpeed;
    }

    IEnumerator yellowColorAttack()
    {
        float beforeAttackWaitTime = 1f;
        float movementTime = 1f; // x3
        float afterShootWaitTime = 3f; // x3
        float afterAttackWaitTime = 1f;
        float fallTime = 1f;
        
        // Wait with flame on
        yield return new WaitForSeconds(beforeAttackWaitTime);
        ps.Stop();

        // Move & shoot n times
        yield return moveIntoPositionAndShootYellow(movementTime, afterShootWaitTime);
        yield return moveIntoPositionAndShootYellow(movementTime, afterShootWaitTime);
        yield return moveIntoPositionAndShootYellow(movementTime, afterShootWaitTime);

        // Turn flame on again
        ps.Play();
        yield return new WaitForSeconds(afterAttackWaitTime);

        // Fall
        Vector3 currentPosition = transform.position;
        Vector3 destination = new Vector3(transform.position.x, -0.5f);
        Quaternion currentRotation = transform.rotation;
        float timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / fallTime);
            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / fallTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        recoverOriginalColor();
        StartCoroutine(act());
    }

    public IEnumerator flameCourtain(bool goesFirst)
    {
        // Move to position
        Vector3 currentPosition = transform.position;
        float targetX = (currentPosition.x > 0) ? 10.5f : -10.5f;
        float targetY = (goesFirst) ? 10.5f : -2.5f;
        Vector3 destination = new Vector3(targetX, targetY);
        float angleSign = (currentPosition.x > 0) ? 1 : -1;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 90 * angleSign);
        float movementTime = 1.5f;
        float timer = 0;
        while (timer < movementTime)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / movementTime);
            transform.rotation = Quaternion.Lerp(Quaternion.identity, targetRotation, timer / movementTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;

        // Stop flames
        ps.Stop();
        prepareBWFlameCannon();
        yield return new WaitForSeconds(0.5f);

        float firstAttackTime = 3f;
        if (goesFirst)
        {
            ps.Play();

            currentPosition = transform.position;
            destination = new Vector3(currentPosition.x, 1);
            timer = 0;
            while (timer < firstAttackTime)
            {
                if (isDead)
                    yield break;

                transform.position = Vector3.Lerp(currentPosition, destination, timer / firstAttackTime);
                timer += Time.deltaTime;
                yield return null;
            }

            ps.Stop();
        } else
        {
            yield return new WaitForSeconds(firstAttackTime);
        }

        float secondAttackTime = 2f;
        if (!goesFirst)
        {
            ps.Play();

            currentPosition = transform.position;
            destination = new Vector3(currentPosition.x, 7);
            timer = 0;
            while (timer < secondAttackTime)
            {
                if (isDead)
                    yield break;

                transform.position = Vector3.Lerp(currentPosition, destination, timer / secondAttackTime);
                timer += Time.deltaTime;
                yield return null;
            }

            ps.Stop();
        }
        else
        {
            yield return new WaitForSeconds(secondAttackTime);
        }

        yield return new WaitForSeconds(0.5f);

        resetFlame();
        ps.Play();

        // Return to neutral
        currentPosition = transform.position;
        destination = new Vector3(neutralX, -0.5f);
        Quaternion currentRotation = transform.rotation;
        movementTime = 1f;
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / movementTime);
            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / movementTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        StartCoroutine(act());
    }

    public IEnumerator verticalFlameCourtain(bool fakeOut)
    {
        // Move to position
        Vector3 currentPosition = transform.position;
        float targetX = (currentPosition.x > 0) ? 10.5f : -10.5f;
        Vector3 destination = new Vector3(targetX, 11);
        float angleSign = (currentPosition.x > 0) ? 1 : -1;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 180 * angleSign);
        float movementTime = 1.5f;
        float timer = 0;
        while (timer < movementTime)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / movementTime);
            transform.rotation = Quaternion.Lerp(Quaternion.identity, targetRotation, timer / movementTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;

        yield return new WaitForSeconds(0.5f);

        // Advance
        prepareBWFlameCannon();

        currentPosition = transform.position;
        float destinationX = (currentPosition.x > 0) ? -1.5f : 1.5f;
        destination = new Vector3(destinationX, currentPosition.y);
        float attackTime = 2.5f;
        timer = 0;
        while (timer < attackTime)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / attackTime);
            timer += Time.deltaTime;

            if (fakeOut && Mathf.Abs(transform.position.x) < 3) // Stop the flame of one of them
                ps.Stop();

            yield return null;
        }

        ps.Stop();
        yield return new WaitForSeconds(0.5f);

        resetFlame();
        ps.Play();

        // Swap neutrals
        neutralX = -neutralX;

        // Return to neutral
        currentPosition = transform.position;
        destination = new Vector3(neutralX, -0.5f);
        Quaternion currentRotation = transform.rotation;
        movementTime = 1f;
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / movementTime);
            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / movementTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        StartCoroutine(act());
    }

    public IEnumerator randomCannon(int nOfAttacks)
    {
        for (int i=0; i < nOfAttacks; i++)
        {
            if (i == 0)
                yield return shootRandomCannon(1.25f); // Greater warning time for first attack
            else
                yield return shootRandomCannon(0.75f);
        }

        // Return to neutral
        Vector3 currentPosition = transform.position;
        Vector3 destination = new Vector3(neutralX, -0.5f);
        Quaternion currentRotation = transform.rotation;
        float movementTime = 1f;
        float timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / movementTime);
            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / movementTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        StartCoroutine(act());
    }

    IEnumerator shootRandomCannon(float preAttackWarningTime)
    {
        ps.Stop();

        // Move to position
        Vector3 currentPosition = transform.position;
        float targetX = (currentPosition.x > 0) ? 10.5f : -10.5f;
        float targetY = Random.Range(-1f, 9f);
        Vector3 destination = new Vector3(targetX, targetY);
        Quaternion currentRotation = transform.rotation;
        float randomAngle = 90 + (30 * Random.Range(-1, 2)); // 60, 90, 120
        float angleSign = (currentPosition.x > 0) ? 1 : -1;
        Quaternion targetRotation = Quaternion.Euler(0, 0, randomAngle * angleSign);
        float movementTime = 1f;
        float timer = 0;
        while (timer < movementTime)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / movementTime);
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, timer / movementTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;

        // Pre attack warning
        ps.Play();
        yield return new WaitForSeconds(preAttackWarningTime);

        // Cannon
        prepareBWFlameCannon();
        yield return new WaitForSeconds(0.75f);
        resetFlame();
    }

    public IEnumerator rotatingCannon(bool upperPosition, FinalBoss partner, float attackTime)
    {
        // Move to position
        Vector3 currentPosition = transform.position;
        float targetY = upperPosition ? 5f : 4f;
        Vector3 destination = new Vector3(0, targetY);
        float angleSign = (currentPosition.x > 0) ? 1 : -1;
        float rotationAngle = upperPosition ? 0 : 180;
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotationAngle * angleSign);
        float movementTime = 1.5f;
        float timer = 0;
        while (timer < movementTime)
        {
            transform.position = Vector3.Lerp(currentPosition, destination, timer / movementTime);
            transform.rotation = Quaternion.Lerp(Quaternion.identity, targetRotation, timer / movementTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;

        // Initiate cannon
        yield return new WaitForSeconds(0.75f);
        prepareBWFlameCannon(10f);
        yield return new WaitForSeconds(0.75f);

        // Rotate around
        partneredRotationSpeed = 1f;
        float rotationRadius = transform.lossyScale.y / 2f;
        if (upperPosition) // Make the upper one choose a common direction
            partneredRotationDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
        timer = 0;
        while (timer < attackTime)
        {
            if (isDead)
                yield break;

            partneredRotationSpeed = Mathf.Min(partneredRotationSpeed + 0.02f, 4.5f);
            transform.Rotate(0, 0, rollSpeed * partneredRotationSpeed * partneredRotationDirection);
            timer += Time.deltaTime;

            float currentX = rotationRadius * transform.up.x;
            float currentY = rotationRadius * transform.up.y + 4.5f;
            transform.position = new Vector3(currentX, currentY);

            if (upperPosition && Random.Range(0, 500) == 0) // Chance of having the upper one change the common direction
            {
                partneredRotationDirection = -partneredRotationDirection;
                partneredRotationSpeed = 1f;
            }  

            yield return null;
        }

        resetFlame();
        yield return new WaitForSeconds(0.5f);
        ps.Stop();

        // Return to neutral
        currentPosition = transform.position;
        destination = new Vector3(neutralX, -0.5f);
        Quaternion currentRotation = transform.rotation;
        movementTime = 1f;
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, destination, timer / movementTime);
            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / movementTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        ps.Play();
        StartCoroutine(act());
    }

    void prepareBWFlameCannon()
    {
        prepareBWFlameCannon(100f);
    }

    void prepareBWFlameCannon(float flameSpeed)
    {
        alterFlameSize(1f);
        alterFlameSpeed(flameSpeed, flameSpeed); // Affects curvature, higher means more lineal
        alterFlameLifetime(25 / flameSpeed);
        alterFlamesOverTime((int)(flameSpeed * 2));
        alterFlameAngle(0);
    }

    void prepareRedFlameCannon()
    {
        alterFlameSize(1.5f);
        alterFlameSpeed(8f, 8f);
        alterFlameLifetime(3f);
        alterFlamesOverTime(200);
        alterFlameAngle(5);
    }

    void prepareWarningFlame()
    {
        alterFlameLifetime(0.1f);
        alterFlameSpeed(5, 5);
        alterFlameSize(1);
        shp.radius = 0.5f;
    }

    void prepareAttackFlame()
    {
        alterFlameLifetime(2);
        alterFlameSpeed(15, 15);
        alterFlameSize(1.5f);
        shp.radius = 1f;
    }

}
