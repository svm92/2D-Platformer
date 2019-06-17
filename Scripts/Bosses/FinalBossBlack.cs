using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossBlack : FinalBoss {

    public GameObject whiteBossObj;
    FinalBossWhite whiteBoss;

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
        higherWaitTime = 1.5f;

        bulletSpeed = 7.5f;

        possibleColors = new Color[] { sprite.color, Color.cyan, Color.magenta, new Color(1, 1, 0) };

        whiteBoss = whiteBossObj.GetComponent<FinalBossWhite>();

        healthBar = whiteBoss.healthBar;
    }

    protected override IEnumerator act()
    {
        isActing = false;
        yield break;
    }

    protected override Color getLighterColor(Color color)
    {
        if (color == bossColor)
            return Color.black;

        return base.getLighterColor(color);
    }

    protected override void shootCyan()
    {
        nOfProjectiles = 4;
        base.shootCyan();
        nOfProjectiles = 0;
    }

    public override void receiveDamage(int damage)
    {
        base.receiveDamage(damage);
        if (whiteBoss != null && !whiteBoss.isHurting)
            whiteBoss.receiveDamage(damage);
    }

}
