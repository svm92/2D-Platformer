using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdieBlue : Enemy {

    [SerializeField] Vector3 direction = Vector3.left;
    [SerializeField] float moveSpeed = 3.5f;

    protected override void Awake()
    {
        base.Awake();
        speed = moveSpeed;
        health = 4;
        power = 1;

        if (direction == Vector3.left)
            sprite.flipX = true;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isActive && !isDead)
            transform.position += direction * speed * Time.deltaTime;
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
