using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainRed : FinalBoss {

    int nOfActionsAvailable = 7;

    protected override void Awake()
    {
        base.Awake();
        speed = 2.5f;
        health = 165;
        power = 2;

        jumpSpeed = 10f;
        rollSpeed = 0.25f;
        rollWarningTime = 0.5f;

        lowerWaitTime = 0.75f;
        higherWaitTime = 1.75f;

        changeLifeAccordingToOtherDefeatedBosses();
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
            speed = 3.5f;
            jumpSpeed = 12f;
            rollSpeed = 0.3f;
            rollWarningTime = 0.4f;

            lowerWaitTime = 0.3f;
            higherWaitTime = 1.1f;
            beforeShootWaitTime = 0.4f;
            afterShootWaitTime = 0.4f;
            
            nOfActionsAvailable = 12;
        } else if (health <= 0.66f * maxHealth)
        {
            speed = 3f;
            jumpSpeed = 11f;
            rollSpeed = 0.275f;
            rollWarningTime = 0.45f;

            lowerWaitTime = 0.65f;
            higherWaitTime = 1.35f;
            beforeShootWaitTime = 0.45f;
            afterShootWaitTime = 0.45f;

            nOfActionsAvailable = 9;
        }
    }

    protected override void resetFlame()
    {
        base.resetFlame();
        alterFlameSize(1.5f);
        alterFlameSpeed(5, 7);
        alterFlamesOverTime(15);
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
                StartCoroutine(move(2f, 6f));
                break;
            // Jump
            case 1:
                StartCoroutine(jump(5f, 6.5f));
                break;
            // Roll
            case 2:
                StartCoroutine(rollAround(1f, 3f));
                break;
            // Jump up & shoot
            case 3:
            case 11:
                StartCoroutine(jumpUpAndShoot());
                break;
            // Jump roll & shoot
            case 4:
                StartCoroutine(jump(6.5f, 7f, true));
                break;
            // Jump roll
            case 5:
                StartCoroutine(jump(4f, 6f, true, 7f, 0.6f));
                break;
            // Flame rotation
            case 7:
            case 8:
            case 9:
                StartCoroutine(jumpToCenterOfScreen());
                break;
            // Floor is lava
            case 6:
            case 10:
                StartCoroutine(floorIsLava());
                break;
            default:
                StartCoroutine(act());
                break;
        }
    }

    IEnumerator jumpToCenterOfScreen()
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

        // Alter flame
        alterFlameSize(1f);
        alterFlameLifetime(3f);
        alterFlamesOverTime(200);
        alterFlameAngle(0);
        ps.Play();

        // Rotate (attack)
        float attackDuration = Random.Range(4f, 12f);
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
        float rotationTime = 0.5f;
        timer = 0;
        while (transform.rotation != Quaternion.identity)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(originalRotation, Quaternion.identity, timer / rotationTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Fall
        originalPosition = transform.position;
        destination = new Vector3(transform.position.x, 0);
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
        
        resetFlame();
        ps.Play();
        StartCoroutine(act());
    }

    IEnumerator floorIsLava()
    {
        ps.Stop();

        Quaternion destinationRotation = Quaternion.Euler(0, 0, 180);
        float rotationTime = rollWarningTime * 2;
        float timer = 0;
        // Turn upsidedown
        while (timer < rotationTime)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(Quaternion.identity, destinationRotation, timer / rotationTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = destinationRotation;

        // Throw fire
        alterFlameSpeed(20f, 20f);
        alterFlameLifetime(1f, 1f);
        alterFlameAngle(90);
        alterFlamesOverTime(100);
        ParticleSystem.ShapeModule shp = ps.shape;
        shp.arcMode = ParticleSystemShapeMultiModeValue.BurstSpread;
        ParticleSystem.TriggerModule tg = ps.trigger;
        tg.enabled = true;
        ps.Play();
        yield return new WaitForSeconds(Random.Range(1f, 1.5f));

        // Turn upside
        ps.Stop();
        Quaternion originalRotation = transform.rotation;
        rotationTime = rollWarningTime;
        timer = 0;
        while (timer < rotationTime)
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(originalRotation, Quaternion.identity, timer / rotationTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        resetFlame();
        shp.arcMode = ParticleSystemShapeMultiModeValue.Random;
        tg.enabled = false;
        ps.Play();
        StartCoroutine(act());
    }

}
