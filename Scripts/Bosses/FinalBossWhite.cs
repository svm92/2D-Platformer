using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalBossWhite : FinalBoss {

    public GameObject blackBossObj;
    FinalBossBlack blackBoss;
    Image healthBarFill;

    int nOfActionsAvailable = 8;

    protected override void Awake()
    {
        base.Awake();
        speed = 4f;
        health = 600;
        power = 2;

        jumpSpeed = 12f;
        rollSpeed = 0.3f;
        rollWarningTime = 0.5f;

        lowerWaitTime = 0.75f;
        higherWaitTime = 1.75f;

        possibleColors = new Color[] { sprite.color, Color.red, Color.green, Color.blue };

        blackBoss = blackBossObj.GetComponent<FinalBossBlack>();

        healthBarFill = healthBar.GetComponentsInChildren<Image>()[1];
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(changeSliderColor(Color.white));
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        recheckValues();
    }

    IEnumerator changeSliderColor(Color nextColor)
    {
        Color currentColor = healthBarFill.color;
        float transitionTime = 5f;
        float timer = 0;
        while (timer < transitionTime)
        {
            healthBarFill.color = Color.Lerp(currentColor, nextColor, timer / transitionTime);
            timer += Time.deltaTime;
            yield return null;
        }

        nextColor = (nextColor == Color.white) ? Color.black : Color.white;

        StartCoroutine(changeSliderColor(nextColor));
    }

    void recheckValues()
    {
        if (health <= 0.3f * maxHealth)
        {
            lowerWaitTime = 0.5f;
            higherWaitTime = 1f;
            nOfActionsAvailable = 15;
        } else if (health <= 0.5f * maxHealth)
        {
            lowerWaitTime = 0.65f;
            higherWaitTime = 1.1f;
            nOfActionsAvailable = 10;
        } else if (health <= 0.8f * maxHealth)
        {
            lowerWaitTime = 0.75f;
            higherWaitTime = 1.25f;
            nOfActionsAvailable = 9;
        }
    }

    protected override IEnumerator act()
    {
        isActing = false;
        yield return new WaitForSeconds(Random.Range(lowerWaitTime, higherWaitTime));

        while (blackBoss.isActing)
            yield return null;

        isActing = true;
        blackBoss.isActing = true;

        if (transform.position.x != neutralX || blackBossObj.transform.position.x != blackBoss.neutralX)
        {
            if (Random.Range(0, 2) == 0) // 50% chance of trading places
            {
                float placeHolderX = neutralX;
                neutralX = blackBoss.neutralX;
                blackBoss.neutralX = placeHolderX;
            }

            StartCoroutine(jumpToNeutralPosition());
            StartCoroutine(blackBoss.jumpToNeutralPosition());
            yield break;
        }

        int randomAction = Random.Range(0, nOfActionsAvailable);
        switch (randomAction)
        {
            // Shoot
            case 0:
            case 1:
            case 2:
                doubleShoot();
                break;
            // Color Special
            case 3:
            case 8:
            case 9:
            case 10:
                doubleColorSpecial();
                break;
            // Flame Courtain
            case 4:
            case 11:
                doubleFlameCourtain();
                break;
            // Vertical Flame Courtain
            case 5:
            case 12:
                doubleVerticalFlameCourtain();
                break;
            // Random Cannon
            case 6:
            case 13:
                doubleRandomCannon();
                break;
            // Rotating Cannon
            case 7:
            case 14:
                doubleRotatingCannon();
                break;
            default:
                StartCoroutine(act());
                break;
        }
    }

    void doubleShoot()
    {
        StartCoroutine(lineShoot());
        StartCoroutine(blackBoss.lineShoot());
    }

    void doubleColorSpecial()
    {
        StartCoroutine(colorAttack());
        StartCoroutine(blackBoss.colorAttack());
    }

    void doubleFlameCourtain()
    {
        bool goesFirst = (Random.Range(0, 2) == 0);
        StartCoroutine(flameCourtain(goesFirst));
        StartCoroutine(blackBoss.flameCourtain(!goesFirst));
    }

    void doubleVerticalFlameCourtain()
    {
        bool fakeOut = (Random.Range(0, 2) == 0);
        StartCoroutine(verticalFlameCourtain(fakeOut));
        StartCoroutine(blackBoss.verticalFlameCourtain(!fakeOut));
    }

    void doubleRandomCannon()
    {
        int nOfAttacks = Random.Range(3, 6);
        StartCoroutine(randomCannon(nOfAttacks));
        StartCoroutine(blackBoss.randomCannon(nOfAttacks));
    }

    void doubleRotatingCannon()
    {
        bool upperPosition = (Random.Range(0, 2) == 0);
        float attackTime = Random.Range(9f, 15f);
        StartCoroutine(rotatingCannon(upperPosition, blackBoss, attackTime));
        StartCoroutine(blackBoss.rotatingCannon(!upperPosition, this, attackTime));
    }

    public override void receiveDamage(int damage)
    {
        base.receiveDamage(damage);
        if (blackBoss != null && !blackBoss.isHurting)
            blackBoss.receiveDamage(damage);
    }

}
