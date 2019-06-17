using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainCyan : FinalBoss {

    int nOfActionsAvailable = 9;

    protected override void Awake()
    {
        base.Awake();
        speed = 3.5f;
        health = 150;
        power = 2;

        jumpSpeed = 12f;
        rollSpeed = 0.3f;
        rollWarningTime = 0.5f;
        bulletSpeed = 5f;

        lowerWaitTime = 0.75f;
        higherWaitTime = 1.25f;

        nOfProjectiles = 4;

        changeLifeAccordingToOtherDefeatedBosses();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        recheckValues();
    }

    void recheckValues()
    {
        if (health <= 0.25f * maxHealth)
        {
            speed = 4.5f;
            jumpSpeed = 13f;
            rollSpeed = 0.4f;
            rollWarningTime = 0.4f;

            lowerWaitTime = 0.6f;
            higherWaitTime = 1f;
            beforeShootWaitTime = 0.4f;
            afterShootWaitTime = 0.4f;

            nOfProjectiles = 16;
            nOfActionsAvailable = 16;
        } else if (health <= 0.65f * maxHealth)
        {
            speed = 4f;
            jumpSpeed = 12.5f;
            rollSpeed = 0.35f;
            rollWarningTime = 0.45f;

            lowerWaitTime = 0.75f;
            higherWaitTime = 1.1f;
            beforeShootWaitTime = 0.45f;
            afterShootWaitTime = 0.45f;

            nOfProjectiles = 8;
            nOfActionsAvailable = 12;
        }
    }

    protected override void resetFlame()
    {
        base.resetFlame();
        alterFlameLifetime(0.25f);
        alterFlameSpeed(3f, 3f);
        alterFlamesOverTime(50);
    }

    protected override IEnumerator act()
    {
        isActing = false;
        yield return new WaitForSeconds(Random.Range(lowerWaitTime, higherWaitTime));
        isActing = true;
        int randomAction = Random.Range(0, nOfActionsAvailable);
        switch (randomAction)
        {
            // Move
            case 0:
                StartCoroutine(move(2f, 8f));
                break;
            // Jump
            case 1:
                StartCoroutine(jump(5f, 8f));
                break;
            // Roll
            case 2:
                StartCoroutine(rollAround(0.8f, 3));
                break;
            // Jump up & shoot
            case 3:
            case 12:
                StartCoroutine(jumpUpAndShoot());
                break;
            // Jump roll & shoot
            case 4:
            case 13:
                StartCoroutine(jump(6.5f, 8f, true));
                break;
            // Jump roll
            case 5:
                StartCoroutine(jump(4f, 6f, true, 8f, 0.6f));
                break;
            // Normal shoot
            case 6:
            case 10:
                ps.Stop();
                yield return new WaitForSeconds(beforeShootWaitTime);
                shoot();
                yield return new WaitForSeconds(afterShootWaitTime);
                ps.Play();
                StartCoroutine(act());
                break;
            // Spark Explosion
            case 7:
                StartCoroutine(sparkExplosion());
                break;
            // Remote spark
            case 8:
            case 11:
            case 14:
                StartCoroutine(remoteSpark());
                break;
            // Rotating spark
            case 9:
            case 15:
                StartCoroutine(rotatingSpark());
                break;
            default:
                StartCoroutine(act());
                break;
        }
    }

    IEnumerator sparkExplosion()
    {
        float timer = 0;
        float attackStepDuration = beforeShootWaitTime * 2.5f;
        ParticleSystem.ShapeModule shp = ps.shape;

        // Move spark up
        while (timer < attackStepDuration)
        {
            if (isDead)
                yield break;

            shp.position = new Vector3(0, Mathf.Lerp(1, 2.5f, timer / attackStepDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        shp.position = new Vector3(0, 2.5f);

        alterFlameLifetime(10f);
        alterFlameSpeed(2f, 2f);
        alterFlameSize(0.3f);
        alterFlamesOverTime((int)Mathf.Floor(rollSpeed * 100f));
        // Initiate attack
        attackStepDuration = Random.Range(2f, 3f);
        timer = 0;
        while (timer < attackStepDuration)
        {
            if (isDead)
                yield break;

            alterFlameLifetime(Mathf.Lerp(0.25f, 10f, timer / attackStepDuration));
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(Random.Range(2f, 5f));

        // End attack
        attackStepDuration = Random.Range(1.5f, 2f);
        timer = 0;
        while (timer < attackStepDuration)
        {
            if (isDead)
                yield break;

            alterFlameLifetime(Mathf.Lerp(10f, 0.25f, timer / attackStepDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        resetFlame();

        // Move spark down
        attackStepDuration = afterShootWaitTime * 2;
        timer = 0;
        while (timer < attackStepDuration)
        {
            if (isDead)
                yield break;

            shp.position = new Vector3(0, Mathf.Lerp(2.5f, 1, timer / attackStepDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        shp.position = new Vector3(0, 1);

        
        yield return new WaitForSeconds(afterShootWaitTime);
        StartCoroutine(act());
    }

    IEnumerator remoteSpark()
    {
        bool facingLeft = sprite.flipX;
        Quaternion preAttackRotation = facingLeft ? Quaternion.Euler(0, 0, -45) : Quaternion.Euler(0, 0, 45);
        Quaternion attackRotation = facingLeft ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, -90);
        float stepDuration = rollWarningTime * 2;
        float timer = 0;

        // Warning
        while (timer < stepDuration)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(Quaternion.identity, preAttackRotation, timer / stepDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        // Roll head forward
        while (timer < rollSpeed)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(preAttackRotation, attackRotation, timer / rollSpeed);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        // Shoot spark
        ParticleSystem.ShapeModule shp = ps.shape;
        float attackDistance = Random.Range(6f, 12f);
        stepDuration = attackDistance / jumpSpeed;
        while (timer < stepDuration)
        {
            if (isDead)
                yield break;

            shp.position = new Vector3(0, Mathf.Lerp(1, attackDistance, timer / stepDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        yield return new WaitForSeconds(afterShootWaitTime * 0.5f);

        // Retrieve spark + Return to normal
        stepDuration = afterShootWaitTime * 2;
        while (timer < stepDuration)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(attackRotation, Quaternion.identity, timer / stepDuration);
            shp.position = new Vector3(0, Mathf.Lerp(attackDistance, 1, timer / stepDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        transform.rotation = Quaternion.identity;
        shp.position = new Vector3(0, 1);
        StartCoroutine(act());
    }

    IEnumerator rotatingSpark()
    {
        ps.Stop();

        Vector3 originalPosition = transform.position;
        Vector3 destination = new Vector3(0, 5);
        float timer = 0;
        float travelTime = Vector3.Distance(originalPosition, destination) / jumpSpeed;

        // Move to center of screen
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / travelTime);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = destination;

        ps.Play();

        // Rotate (attack)
        ParticleSystem.ShapeModule shp = ps.shape;
        int rotationDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
        int movementDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
        float attackTime = Random.Range(5f, 10f);
        float sparkPreparationTime = Random.Range(1f, 2.5f);
        float nextSparkPosition = Random.Range(3f, 4.5f);
        bool sparkReachedDestination = false;
        float sparkTimer = 0;
        timer = 0;
        while (timer < attackTime)
        {
            if (isDead)
                yield break;

            // Rotate
            transform.Rotate(0, 0, rollSpeed * 15 * rotationDirection);
            timer += Time.deltaTime;

            // Move
            transform.position = new Vector3(transform.position.x + (movementDirection * (speed/50)), transform.position.y);
            if (transform.position.x <= -11 || transform.position.x >= 11) // If reaching wall
                movementDirection = -movementDirection;

            // Move spark to limit
            if (!sparkReachedDestination)
            {
                shp.position = new Vector3(0, Mathf.Lerp(1, nextSparkPosition, sparkTimer / sparkPreparationTime));
                sparkTimer += Time.deltaTime;

                if (sparkTimer >= sparkPreparationTime)
                    sparkReachedDestination = true;
            }
            
            yield return null;
        }

        // Rotate to normal
        Quaternion currentRotation = transform.rotation;
        float currentSparkPosition = shp.position.y;
        travelTime = afterShootWaitTime * 1.5f;
        timer = 0;
        while (timer < travelTime)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / travelTime);
            shp.position = new Vector3(0, Mathf.Lerp(currentSparkPosition, 1, timer / travelTime));
            timer += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        shp.position = new Vector3(0, 1);

        // Fall
        originalPosition = transform.position;
        destination = new Vector3(transform.position.x, -0.25f);
        travelTime = Vector3.Distance(originalPosition, destination) / (jumpSpeed * 2);
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / travelTime);
            timer += Time.deltaTime;
            yield return null;
        }
        
        StartCoroutine(act());
    }

}
