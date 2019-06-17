using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGreen01 : Boss {

    float jumpSpeed = 0.2f;
    float height;

    protected override void Awake()
    {
        base.Awake();
        speed = 10f;
        health = 120;
        power = 1;
    }

    protected override void Start()
    {
        base.Start();
        height = transform.lossyScale.y / 2;
        StartCoroutine(waitThenJump());
    }

    IEnumerator waitThenJump()
    {
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(jumpAround());
    }

    IEnumerator jumpAround()
    {
        float jumpRadius = 0;
        float xDestination = -100;
        bool jumpingRight = true;
        while (xDestination <= -10 || xDestination >= 10)
        {
            jumpRadius = Random.Range(2.5f, 5f);
            jumpingRight = Random.Range(0, 2) == 0 ? true : false;
            if (jumpingRight)
                xDestination = transform.position.x + (jumpRadius * 2);
            else
                xDestination = transform.position.x - (jumpRadius * 2);
        }

        // Jump up
        float jumpHeight = Random.Range(5f, 7f);
        while (transform.position.y < jumpHeight)
        {
            if (isDead)
                yield break;

            transform.position += Vector3.up * jumpSpeed;
            yield return null;
        }

        // Circle jump
        float xPosition = transform.position.x;
        float xRadiusCenter = jumpingRight ? xPosition + jumpRadius : xPosition - jumpRadius;
        float yRadiusCenter = transform.position.y;
        bool jumpIsOver = false;
        while (!jumpIsOver)
        {
            if (isDead)
                yield break;

            xPosition += jumpingRight ? jumpSpeed : -jumpSpeed;
            if ((jumpingRight && xPosition >= xDestination) || (!jumpingRight && xPosition <= xDestination))
            {
                jumpIsOver = true;
            } else
            {
                // Circular movement [ x^2 + y^2 = R^2 ]
                float yPosition = yRadiusCenter + Mathf.Sqrt(Mathf.Pow(jumpRadius, 2) - Mathf.Pow(xPosition - xRadiusCenter, 2));
                transform.position = new Vector3(xPosition, yPosition);
            }

            yield return null;
        }

        // Fall down
        float groundLevel = -1.5f + height; // Ground is at y=-1.5, then add the sprite's height
        while (transform.position.y > groundLevel)
        {
            if (isDead)
                yield break;

            transform.position += Vector3.down * jumpSpeed;
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, groundLevel);

        float randomDelay = 3f * Random.Range(0.85f, 1f) * Mathf.Max(health/maxHealth, 0.25f);
        yield return new WaitForSeconds(randomDelay);
        recheckJumpSpeed();
        StartCoroutine(jumpAround());
    }

    void recheckJumpSpeed()
    {
        if (health <= 0.3f * maxHealth)
            jumpSpeed = 0.55f;
        else if (health <= 0.5f * maxHealth)
            jumpSpeed = 0.4f;
        else if (health <= 0.7f * maxHealth)
            jumpSpeed = 0.3f;
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[101] = true;
    }
}
