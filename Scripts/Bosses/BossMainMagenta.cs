using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainMagenta : FinalBoss {

    int nOfActionsAvailable = 6;
    bool firstAction = true;

    ParticleSystem.MainModule main;
    ParticleSystem.ShapeModule shp;

    Vector3[] possibleLocations = new Vector3[] { new Vector3(-7, 8), new Vector3(6, 8),
        new Vector3(-7, 21), new Vector3(6, 21), new Vector3(-10.5f, 25), new Vector3(-3.5f, 25), new Vector3(2.5f, 25),
        new Vector3(9.5f, 25), new Vector3(-10, 15), new Vector3(-4, 15), new Vector3(3, 15), new Vector3(9, 15),
        new Vector3(-10.5f, 12), new Vector3(-3.5f, 12), new Vector3(2.5f, 12), new Vector3(9.5f, 12),
        new Vector3(-10, 2), new Vector3(-4, 2), new Vector3(3, 2), new Vector3(9, 2)};

    Vector3[] roomCores = new Vector3[] { new Vector3(-6.5f, 19.5f), new Vector3(6.5f, 19.5f),
        new Vector3(-6.5f, 6.5f), new Vector3(6.5f, 6.5f) };

    protected override void Awake()
    {
        base.Awake();
        speed = 3.5f;
        health = 175;
        power = 2;

        jumpSpeed = 12f;
        rollSpeed = 0.3f;
        rollWarningTime = 0.5f;

        lowerWaitTime = 0.85f;
        higherWaitTime = 1.5f;

        changeLifeAccordingToOtherDefeatedBosses();

        main = ps.main;
        shp = ps.shape;

        // Adjust positions according to sprite size (displace x +0.5 and y +0.75)
        for (int i = 0; i < possibleLocations.Length; i++)
            possibleLocations[i] = new Vector3(possibleLocations[i].x + 0.5f, possibleLocations[i].y + 0.75f);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        recheckValues();
    }

    void recheckValues()
    {
        if (health <= 0.33f * maxHealth)
        {
            speed = 4.5f;
            jumpSpeed = 14f;
            rollSpeed = 0.4f;
            rollWarningTime = 0.3f;

            lowerWaitTime = 0.5f;
            higherWaitTime = 1f;
            beforeShootWaitTime = 0.4f;
            afterShootWaitTime = 0.4f;
            
            nOfActionsAvailable = 12;
        } else if (health <= 0.66f * maxHealth)
        {
            speed = 4f;
            jumpSpeed = 13f;
            rollSpeed = 0.35f;
            rollWarningTime = 0.4f;

            lowerWaitTime = 0.75f;
            higherWaitTime = 1.25f;
            beforeShootWaitTime = 0.45f;
            afterShootWaitTime = 0.45f;
            
            nOfActionsAvailable = 9;
        }
    }

    protected override void resetFlame()
    {
        base.resetFlame();
        alterFlameLifetime(2);
        alterFlameSpeed(0.75f, 0.75f);
        main.scalingMode = ParticleSystemScalingMode.Local;
        shp.radius = 0.2f;
        shp.radiusThickness = 1;
        shp.arc = 30;
        shp.arcSpread = 0;
        shp.position = Vector3.up * 0.5f;
        shp.rotation = new Vector3(180, 180, -105);
    }

    protected override IEnumerator act()
    {
        isActing = false;
        yield return new WaitForSeconds(Random.Range(lowerWaitTime, higherWaitTime));
        isActing = true;

        if (firstAction)
        {
            firstAction = false;
            StartCoroutine(jumpToNewPosition());
            yield break;
        }

        int randomAction = Random.Range(0, nOfActionsAvailable);
        switch (randomAction)
        {
            // Roll
            case 0:
                StartCoroutine(rollAround(1f, 4));
                break;
            // Shoot
            case 1:
                shoot();
                yield return new WaitForSeconds(afterShootWaitTime);
                StartCoroutine(act());
                break;
            // Jump
            case 2:
            case 6:
            case 9:
            case 10:
                StartCoroutine(jumpToNewPosition());
                break;
            // Wall flame cross
            case 3:
                if (transform.position.y == 12.75f || transform.position.y == 25.75f)
                    StartCoroutine(jumpToNewPosition());
                else
                    StartCoroutine(wallFlameCross(false));
                break;
            // Wall flame cross rotating
            case 4:
            case 7:
                if (transform.position.y == 12.75f || transform.position.y == 25.75f)
                    StartCoroutine(jumpToNewPosition());
                else
                    StartCoroutine(wallFlameCross(true));
                break;
            // Flame cross
            case 5:
                StartCoroutine(flameCross(false));
                break;
            // Flame cross rotation
            case 8:
            case 11:
                if (transform.position.y == 25.75f)
                    StartCoroutine(jumpToNewPosition());
                else
                    StartCoroutine(flameCross(true));
                break;
            default:
                StartCoroutine(act());
                break;
        }
    }

    IEnumerator jumpToNewPosition()
    {
        Vector3 destination = transform.position;
        while (destination == transform.position)
        {
            int rnd = Random.Range(0, possibleLocations.Length);
            destination = possibleLocations[rnd];

            if (Vector3.Distance(transform.position, destination) > 15) // Max jump distance
                destination = transform.position; // Repeat
        }

        if (destination.x == transform.position.x)
        {
            StartCoroutine(verticalJump(destination));
        }
        else if (destination.y == transform.position.y)
        {
            float distance = Vector3.Distance(transform.position, destination) / 2f;
            // 1 left, 2 right
            int jumpDirection = (destination.x > transform.position.x) ? 2 : 1;
            StartCoroutine(jump(distance, distance + 0.0001f, true, jumpSpeed * 0.5f, 0, false, jumpDirection));
        }
        else
        {
            StartCoroutine(ellipseJumpTo(destination));
        }
        yield break;
    }

    IEnumerator verticalJump(Vector3 destination)
    {
        bool jumpingUp = (destination.y > transform.position.y) ;
        Vector3 currentPosition = transform.position;
        Vector3 firstDestination = (jumpingUp) ? destination + (Vector3.up * 3f) : currentPosition + (Vector3.up * 3f);
        float firstTravelTime = Vector3.Distance(currentPosition, firstDestination) / (jumpSpeed * 0.5f);
        float secondTravelTime = Vector3.Distance(firstDestination, destination) / (jumpSpeed * 0.5f);

        float rotationTimer = 0;
        float halfTravelTime = (firstTravelTime + secondTravelTime) / 2;
        bool firstHalfOfJump = true;
        bool jumpingRight = (destination.x > currentPosition.x);
        float angleSign = jumpingRight ? 1 : -1;
        Quaternion firstHalfRotation = Quaternion.Euler(0, 0, 180 * angleSign);
        Quaternion secondHalfRotation = Quaternion.Euler(0, 0, 360 * angleSign);

        float timer = 0;

        while (transform.position != firstDestination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(currentPosition, firstDestination, timer / firstTravelTime);
            timer += Time.deltaTime;

            // Rotation
            if (firstHalfOfJump)
                transform.rotation = Quaternion.Lerp(Quaternion.identity, firstHalfRotation, rotationTimer / halfTravelTime);
            else
                transform.rotation = Quaternion.Lerp(firstHalfRotation, secondHalfRotation, rotationTimer / halfTravelTime);

            rotationTimer += Time.deltaTime;

            // Check if we are on first or second half of jump
            if (rotationTimer >= halfTravelTime)
            {
                firstHalfOfJump = false;
                rotationTimer = 0;
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            yield return null;
        }

        
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(firstDestination, destination, timer / secondTravelTime);
            timer += Time.deltaTime;

            // Rotation
            if (firstHalfOfJump)
                transform.rotation = Quaternion.Lerp(Quaternion.identity, firstHalfRotation, rotationTimer / halfTravelTime);
            else
                transform.rotation = Quaternion.Lerp(firstHalfRotation, secondHalfRotation, rotationTimer / halfTravelTime);

            rotationTimer += Time.deltaTime;

            // Check if we are on first or second half of jump
            if (rotationTimer >= halfTravelTime)
            {
                firstHalfOfJump = false;
                rotationTimer = 0;
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            yield return null;
        }

        transform.rotation = Quaternion.identity;
        StartCoroutine(act());
    }

    IEnumerator ellipseJumpTo(Vector3 destination)
    {
        Vector3 currentPosition = transform.position;
        float travelTime = (Vector3.Distance(currentPosition, destination)) / (jumpSpeed * 0.5f);
        float halfTravelTime = travelTime / 2;
        bool trajectoryCompleted = false;

        bool firstHalfOfJump = true;
        bool jumpingRight = (destination.x > currentPosition.x);
        float angleSign = jumpingRight ? 1 : -1;
        Quaternion firstHalfRotation = Quaternion.Euler(0, 0, 180 * angleSign);
        Quaternion secondHalfRotation = Quaternion.Euler(0, 0, 360 * angleSign);

        Vector3 ellipseCenter;
        if (currentPosition.y >= destination.y)
            ellipseCenter = new Vector3(currentPosition.x, destination.y);
        else
            ellipseCenter = new Vector3(destination.x, currentPosition.y);

        float xDisplacement = ellipseCenter.x;
        float yDisplacement = ellipseCenter.y;

        float ellipseParameterA = Mathf.Abs(destination.x - currentPosition.x);
        float ellipseParameterB = Mathf.Abs(destination.y - currentPosition.y);

        /*Debug.DrawLine(ellipseCenter + Vector3.left, ellipseCenter + Vector3.right, Color.cyan, 5f);
        Debug.DrawLine(ellipseCenter + Vector3.down, ellipseCenter + Vector3.up, Color.cyan, 5f);*/
        /*Debug.Log("From " + currentPosition + " to " + destination);
        Debug.Log(ellipseParameterA + ", " + ellipseParameterB);*/

        float timer = 0;
        float rotationTimer = 0;
        
        while (!trajectoryCompleted)
        {
            if (isDead)
                yield break;

            float xPos = Mathf.Lerp(currentPosition.x, destination.x, timer / travelTime);
            timer += Time.deltaTime;

            // Check negative root
            if (1 - Mathf.Pow((xPos - xDisplacement) / ellipseParameterA, 2) < 0) // End of trajectory
            {
                trajectoryCompleted = true;
            } else
            {
                // Ellipse formula: ((x - x0)/a)^2 + ((y - y0)/b)^2 = 1
                float yPos = yDisplacement + Mathf.Sqrt(1 - Mathf.Pow((xPos - xDisplacement) / ellipseParameterA, 2))
                    * ellipseParameterB;
                //Debug.Log(xPos + " , " + yPos);
                transform.position = new Vector3(xPos, yPos);
            }

            // Rotation
            if (firstHalfOfJump)
                transform.rotation = Quaternion.Lerp(Quaternion.identity, firstHalfRotation, rotationTimer / halfTravelTime);
            else
                transform.rotation = Quaternion.Lerp(firstHalfRotation, secondHalfRotation, rotationTimer / halfTravelTime);

            rotationTimer += Time.deltaTime;

            // Check if we are on first or second half of jump
            if (rotationTimer >= halfTravelTime)
            {
                firstHalfOfJump = false;
                rotationTimer = 0;
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            // Check if end of trajectory
            if (transform.position.x == destination.x)
                trajectoryCompleted = true;
            
            yield return null;
        }

        transform.position = destination;
        transform.rotation = Quaternion.identity;
        StartCoroutine(act());
    }

    IEnumerator wallFlameCross(bool rotatingAttack)
    {
        // Stop
        ps.Stop();
        yield return new WaitForSeconds(beforeShootWaitTime * 3.5f);

        // Find closest room core
        Vector3 closestCore = roomCores[0];
        float closestDistance = 10000;
        foreach (Vector3 core in roomCores)
        {
            float distanceToCore = Vector3.Distance(transform.position, core);
            if (distanceToCore < closestDistance)
            {
                closestCore = core;
                closestDistance = distanceToCore;
            }
        }
        Vector3 coreDisplacement = closestCore - transform.position;

        // Prepare flame
        alterFlameLifetime(0.1f);
        alterFlameSpeed(5, 5);
        alterFlameSize(1);
        main.scalingMode = ParticleSystemScalingMode.Shape;
        shp.radius = 1f;
        shp.radiusThickness = 0;
        shp.arc = 360;
        shp.position = coreDisplacement;

        if (rotatingAttack)
        {
            shp.arcSpread = 1f;
            float[] possibleStartingAngles = new float[] { 0, 90, 180, 270 };
            shp.rotation = new Vector3(0, 0, possibleStartingAngles[Random.Range(0, possibleStartingAngles.Length)]);
        } else
        {
            alterFlamesOverTime(200);
            shp.arcSpread = 0.25f;
            shp.rotation = new Vector3(0, 0, 0);
        }

        // Pre-attack warning
        ps.Play();
        yield return new WaitForSeconds(beforeShootWaitTime * 2f);

        // Attack
        alterFlameLifetime(2);
        alterFlameSpeed(15, 15);
        alterFlameSize(1.5f);

        if (!rotatingAttack)
            yield return new WaitForSeconds(2f);
        else
        {
            yield return new WaitForSeconds(beforeShootWaitTime * 0.5f);
            int rotationDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
            float attackDuration = Random.Range(4f, 8f);
            float timer = 0;
            while (timer < attackDuration)
            {
                if (isDead)
                    yield break;

                float newRotation = shp.rotation.z + (jumpSpeed * 0.09f * rotationDirection);
                shp.rotation = new Vector3(0, 0, newRotation);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        // End attack
        ps.Stop();
        resetFlame();

        yield return new WaitForSeconds(afterShootWaitTime * 2);
        ps.Play();
        StartCoroutine(act());
    }

    IEnumerator flameCross(bool rotatingAttack)
    {
        float randomArc = (Random.Range(0, 2) == 0) ? 0 : 45;

        // Stop
        ps.Stop();
        yield return new WaitForSeconds(beforeShootWaitTime * 2.5f);

        // Prepare flame
        alterFlameLifetime(0.1f);
        alterFlameSpeed(5, 5);
        alterFlameSize(1);
        alterFlamesOverTime(200);
        main.scalingMode = ParticleSystemScalingMode.Shape;
        shp.radius = 0.5f;
        shp.radiusThickness = 0;
        shp.arc = 360;
        shp.arcSpread = 0.25f;
        shp.position = Vector3.zero;
        shp.rotation = new Vector3(0, 0, randomArc);

        // Pre-attack warning
        ps.Play();
        yield return new WaitForSeconds(beforeShootWaitTime * 2f);

        // Attack
        alterFlameLifetime(2);
        alterFlameSpeed(15, 15);
        alterFlameSize(1.5f);
        shp.radius = 1f;

        if (!rotatingAttack)
            yield return new WaitForSeconds(2f);
        else
        {
            yield return new WaitForSeconds(beforeShootWaitTime);
            int rotationDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
            float attackDuration = Random.Range(5f, 12f);
            float timer = 0;

            // Rotating attack
            while (timer < attackDuration)
            {
                if (isDead)
                    yield break;

                transform.Rotate(0, 0, jumpSpeed * 0.04f * rotationDirection);
                timer += Time.deltaTime;
                yield return null;
            }
        }
        
        // End attack
        ps.Stop();
        resetFlame();

        if (rotatingAttack)
        {
            Quaternion currentRotation = transform.rotation;
            float timer = 0;
            while (timer < afterShootWaitTime)
            {
                transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / afterShootWaitTime);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.rotation = Quaternion.identity;
        }

        yield return new WaitForSeconds(afterShootWaitTime * 2);
        ps.Play();
        StartCoroutine(act());
    }

}
