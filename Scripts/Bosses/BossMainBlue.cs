using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainBlue : FinalBoss {

    int nOfActionsAvailable = 6;

    protected override void Awake()
    {
        base.Awake();
        speed = 3.5f;
        health = 150;
        power = 2;

        jumpSpeed = 12f;
        rollSpeed = 0.3f;
        rollWarningTime = 0.5f;

        lowerWaitTime = 0.75f;
        higherWaitTime = 1.5f;

        nOfProjectiles = 3;

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
            speed = 4.5f;
            jumpSpeed = 14f;
            rollSpeed = 0.4f;
            rollWarningTime = 0.3f;

            lowerWaitTime = 0.3f;
            higherWaitTime = 1f;
            beforeShootWaitTime = 0.4f;
            afterShootWaitTime = 0.4f;

            nOfProjectiles = 7;
            nOfActionsAvailable = 9;
        } else if (health <= 0.66f * maxHealth)
        {
            speed = 4f;
            jumpSpeed = 13f;
            rollSpeed = 0.35f;
            rollWarningTime = 0.4f;

            lowerWaitTime = 0.65f;
            higherWaitTime = 1.25f;
            beforeShootWaitTime = 0.45f;
            afterShootWaitTime = 0.45f;

            nOfProjectiles = 5;
            nOfActionsAvailable = 7;
        }
    }

    protected override void resetFlame()
    {
        base.resetFlame();
        alterFlameLifetime(0.4f, 0.5f);
        alterFlameAngle(0);
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
                StartCoroutine(rollAround(1f, 4));
                break;
            // Jump up & shoot
            case 3:
            case 6:
            case 7:
                StartCoroutine(jumpUpAndShoot());
                break;
            // Jump roll & shoot
            case 4:
            case 8:
                StartCoroutine(jump(6.5f, 8f, true));
                break;
            // Jump roll
            case 5:
                StartCoroutine(jump(4.5f, 6f, true, 8f, 0.6f));
                break;
            default:
                StartCoroutine(act());
                break;
        }
    }

}
