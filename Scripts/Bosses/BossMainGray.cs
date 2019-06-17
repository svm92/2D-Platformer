using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMainGray : FinalBoss {

    protected override void Awake()
    {
        base.Awake();
        speed = 3.5f;
        health = 120;
        power = 1;

        jumpSpeed = 12f;
        rollSpeed = 0.3f;
        rollWarningTime = 0.5f;

        lowerWaitTime = 0.75f;
        higherWaitTime = 3f;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        recheckValues();
    }

    void recheckValues()
    {
        if (health <= 0.37f * maxHealth)
        {
            speed = 4.5f;
            jumpSpeed = 14f;
            rollSpeed = 0.4f;
            rollWarningTime = 0.3f;

            lowerWaitTime = 0.4f;
            higherWaitTime = 1f;
        } else if (health <= 0.75f * maxHealth)
        {
            speed = 4f;
            jumpSpeed = 13f;
            rollSpeed = 0.35f;
            rollWarningTime = 0.4f;

            lowerWaitTime = 0.55f;
            higherWaitTime = 1.75f;
        }
    }

    protected override IEnumerator act()
    {
        isActing = false;
        yield return new WaitForSeconds(Random.Range(lowerWaitTime, higherWaitTime));
        isActing = true;
        int randomAction = Random.Range(0, 3);
        switch (randomAction)
        {
            case 0:
                StartCoroutine(move(2f, 8f));
                break;
            case 1:
                StartCoroutine(jump(5f, 8f));
                break;
            case 2:
                StartCoroutine(rollAround(1f));
                break;
            default:
                break;
        }
    }

}
