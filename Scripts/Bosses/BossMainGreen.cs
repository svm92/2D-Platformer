using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainGreen : FinalBoss {
    
    int nOfActionsAvailable = 6;

    protected override void Awake()
    {
        base.Awake();
        speed = 5f;
        health = 150;
        power = 2;

        jumpSpeed = 14f;
        rollSpeed = 0.4f;
        rollWarningTime = 0.4f;

        lowerWaitTime = 0.5f;
        higherWaitTime = 1f;
        
        changeLifeAccordingToOtherDefeatedBosses();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        recheckValues();
    }

    protected virtual void recheckValues()
    {
        if (health <= 0.33f * maxHealth)
        {
            speed = 7f;
            jumpSpeed = 16f;
            rollSpeed = 0.5f;
            rollWarningTime = 0.3f;

            lowerWaitTime = 0.15f;
            higherWaitTime = 0.5f;
            beforeShootWaitTime = 0.3f;
            afterShootWaitTime = 0.2f;

            nOfActionsAvailable = 9;
        } else if (health <= 0.66f * maxHealth)
        {
            speed = 6f;
            jumpSpeed = 15f;
            rollSpeed = 0.45f;
            rollWarningTime = 0.35f;

            lowerWaitTime = 0.35f;
            higherWaitTime = 0.75f;
            beforeShootWaitTime = 0.4f;
            afterShootWaitTime = 0.4f;

            nOfActionsAvailable = 7;
        }
    }
    

    protected override void resetFlame()
    {
        base.resetFlame();
        alterFlameSpeed(7f, 7f);
        alterFlameSize(0.15f, 0.3f);
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
                StartCoroutine(move(4f, 15f));
                break;
            // Jump
            case 1:
                StartCoroutine(jump(6f, 12f));
                break;
            // Roll
            case 2:
                StartCoroutine(rollAround(0.75f, 4));
                break;
            // Jump up & shoot
            case 3:
                StartCoroutine(jumpUpAndShoot());
                break;
            // Jump roll & shoot
            case 4:
                StartCoroutine(jump(6.5f, 8.5f, true));
                break;
            // Jump roll
            case 5:
            case 7:
                StartCoroutine(jump(5f, 6.5f, true, 8f, 0.45f));
                break;
            // Bouncing roll
            case 6:
            case 8:
                StartCoroutine(bouncingRoll());
                break;
            default:
                StartCoroutine(act());
                break;
        }
    }

    IEnumerator bouncingRoll()
    {
        ps.Stop();

        yield return new WaitForSeconds(0.2f);

        float timer = 0;
        int rotationDirection = (Random.Range(0, 2) == 0) ? 1 : -1;
        // Pre attack warning
        float preAttackDistance = 1.5f;
        Vector3 originalPosition = transform.position;
        Vector3 destination = new Vector3(transform.position.x, preAttackDistance);
        while (timer < (beforeShootWaitTime * 2))
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / (beforeShootWaitTime * 2));
            transform.Rotate(0, 0, speed * rotationDirection);
            timer += Time.deltaTime;
            yield return null;
        }

        ps.Play();

        // Rotate around
        Vector3 attackDirection = Quaternion.Euler(0, 0, Random.Range(90, 270)) * Vector3.right;
        float attackDuration = Random.Range(7f, 10f);
        timer = 0;
        while (timer < attackDuration)
        {
            if (isDead)
                yield break;

            transform.position += attackDirection * 0.15f;
            transform.Rotate(0, 0, speed * rotationDirection);
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

                //attackDirection = Quaternion.Euler(0, 0, 90) * attackDirection;
            }

            yield return null;
        }

        ps.Stop();

        // Stop
        Quaternion currentRotation = transform.rotation;
        timer = 0;
        while (timer < afterShootWaitTime)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.identity, timer / afterShootWaitTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Fall
        originalPosition = transform.position;
        destination = new Vector3(transform.position.x, -0.25f);
        float travelTime = Vector3.Distance(originalPosition, destination) / (jumpSpeed * 2);
        timer = 0;
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / travelTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        ps.Play();
        StartCoroutine(act());
    }

    /*bool collidingWithWall()
    {
        return (Physics2D.OverlapCircle(transform.position, radius, PlayerController.groundAndObstacleLayer) != null);
    }*/

}
