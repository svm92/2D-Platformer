using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdieYellow : Enemy {

    [SerializeField] Vector3 direction = Vector3.left;
    [SerializeField] float pathVerticalWidth = 2f;
    [SerializeField] float moveSpeed = 3.5f;

    bool flyingUp = true;
    float originalY;

    protected override void Awake()
    {
        base.Awake();
        speed = moveSpeed;
        health = 5;
        power = 1;

        originalY = transform.position.y;
        if (direction == Vector3.left)
            sprite.flipX = true;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isActive && !isDead)
            advance();
    }

    void advance()
    {
        transform.position += direction * speed * Time.deltaTime;
        int moveUp = flyingUp ? 1 : -1;
        transform.position += Vector3.up * moveUp * speed * Time.deltaTime;

        if (transform.position.y <= originalY)
        {
            flyingUp = true;
            transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
        }
        else if (transform.position.y >= originalY + pathVerticalWidth)
        {
            flyingUp = false;
            transform.position = new Vector3(transform.position.x, originalY + pathVerticalWidth, transform.position.z);
        }
    }

    void changeDirection()
    {
        direction = Quaternion.Euler(0, 0, 180) * direction;
        GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (GameCharacter.collidingWithObstacle(collision))
            changeDirection();
    }

}
