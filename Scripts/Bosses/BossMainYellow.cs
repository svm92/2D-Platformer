using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainYellow : FinalBoss {

    int nOfActionsAvailable = 6;

    protected override void Awake()
    {
        base.Awake();
        speed = 3f;
        health = 150;
        power = 2;

        jumpSpeed = 11f;
        rollSpeed = 0.25f;
        rollWarningTime = 0.55f;

        lowerWaitTime = 0.75f;
        higherWaitTime = 1.5f;
        
        nOfProjectiles = 1;

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
            speed = 4f;
            jumpSpeed = 13f;
            rollSpeed = 0.35f;
            rollWarningTime = 0.4f;

            lowerWaitTime = 0.6f;
            higherWaitTime = 1f;
            beforeShootWaitTime = 0.45f;
            afterShootWaitTime = 0.45f;
            
            nOfActionsAvailable = 11;
        } else if (health <= 0.66f * maxHealth)
        {
            speed = 3.5f;
            jumpSpeed = 12f;
            rollSpeed = 0.3f;
            rollWarningTime = 0.5f;

            lowerWaitTime = 0.75f;
            higherWaitTime = 1.25f;
            beforeShootWaitTime = 0.475f;
            afterShootWaitTime = 0.475f;
            
            nOfActionsAvailable = 9;
        }
    }

    protected override void resetFlame()
    {
        base.resetFlame();
        alterFlameAngle(25);
        ParticleSystem.ShapeModule shp = ps.shape;
        shp.arcMode = ParticleSystemShapeMultiModeValue.BurstSpread;
    }

    protected override IEnumerator act()
    {
        bulletSpeed = 3f;
        isActing = false;
        yield return new WaitForSeconds(Random.Range(lowerWaitTime, higherWaitTime));
        isActing = true;
        int randomAction = Random.Range(0, nOfActionsAvailable);
        switch (randomAction)
        {
            // Move
            case 0:
                StartCoroutine(move(2f, 7f));
                break;
            // Jump
            case 1:
                StartCoroutine(jump(5f, 8f));
                break;
            // Roll
            case 2:
                StartCoroutine(rollAround(0.8f, 3));
                break;
            // Jump roll & shoot
            case 3:
            case 8:
                StartCoroutine(jump(6.5f, 8f, true));
                break;
            // Jump roll
            case 4:
                StartCoroutine(jump(4f, 6.5f, true, 8f, 0.55f));
                break;
            // Jump up & shoot vertically
            case 5:
            case 9:
                bulletSpeed = 5f;
                StartCoroutine(jumpUpAndShoot(90));
                break;
            // Shoot without moving
            case 6:
                ps.Stop();
                yield return new WaitForSeconds(beforeShootWaitTime * 2);
                bulletSpeed = 5f;
                shootYellow(true, transform.up);
                bulletSpeed = 3f;
                yield return new WaitForSeconds(afterShootWaitTime * Random.Range(3f, 5f));
                ps.Play();
                StartCoroutine(act());
                break;
            // Jump roll and shoot multiple
            case 7:
            case 10:
                nOfProjectiles = 2;
                bulletSpeed = 7f;
                StartCoroutine(jump(6.5f, 8f, true));
                break;
            default:
                StartCoroutine(act());
                break;
        }
    }

}
