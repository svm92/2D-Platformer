using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdieRed : Enemy {

    Vector3 direction = Vector3.down;
    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float haltTime = 2f;

    bool isDashing = false;
    float travelLength;
    float originalY;
    int groundLayer;

    Animator ac;

    protected override void Awake()
    {
        base.Awake();
        speed = moveSpeed;
        health = 4;
        power = 1;

        ac = GetComponent<Animator>();

        int groundLayerBit = LayerMask.NameToLayer("Ground");
        groundLayer = 1 << groundLayerBit;

        originalY = transform.position.y;
        travelLength = calculateDistanceToCeiling();
    }

    protected override void Start()
    {
        base.Start();
        takeFlight();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isActive && !isDead && isDashing)
            dash();
    }

    void dash()
    {
        transform.position += direction * speed * Time.deltaTime;
        if (transform.position.y >= originalY + travelLength)
        {
            transform.position = new Vector3(transform.position.x, originalY + travelLength);
            isDashing = false;
            StartCoroutine(halt());
        } else if (transform.position.y <= originalY) {
            transform.position = new Vector3(transform.position.x, originalY);
            isDashing = false;
            StartCoroutine(halt());
        }
    }

    IEnumerator halt()
    {
        ac.SetBool("isMoving", false);
        transform.Rotate(Vector3.back, -90);
        yield return new WaitForSeconds(haltTime);
        takeFlight();
    }

    void takeFlight()
    {
        direction = Quaternion.Euler(0, 0, 180) * direction;
        ac.SetBool("isMoving", true);
        transform.Rotate(Vector3.back, -90);
        isDashing = true;
    }

    float calculateDistanceToCeiling()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.up, 200, groundLayer);
        return hit.distance - (GetComponent<CircleCollider2D>().radius/2);
    }

    protected override Color flashingColor(Color color)
    {
        return Color.cyan;
    }

    protected override void kill()
    {
        base.kill();
        ac.SetBool("isMoving", false);
        transform.rotation = Quaternion.identity;
    }
}
