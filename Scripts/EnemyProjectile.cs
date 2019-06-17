using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour {

    int power = 1;
    float speed = 5f;
    float lifespan = 0;
    Vector3 direction = Vector3.right;
    bool throughWalls = false;
    bool isHoming = false;
    bool isHomingInitially = false;
    bool isBouncing = false;
    float rotationSpeed;
    float delayBeforeMoving = 0;
    bool isSpawner = false;
    float spawnerTimer = 0;
    int nOfProjectilesOnExplode = 0;
    int nOfProjectilesOnBounce = 0;

    float timer = 0;
    GameObject player;

    private void Awake()
    {
        rotationSpeed = speed;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(delayThenMove());
    }

    public void initialize(int power, float speed, float lifespan)
    {
        this.power = power;
        this.speed = speed;
        rotationSpeed = speed;
        this.lifespan = lifespan;
    }

    public void setRotationSpeed(float rotationSpeed)
    {
        this.rotationSpeed = rotationSpeed;
    }

    public void setDelay(float delay)
    {
        this.delayBeforeMoving = delay;
    }

    public void setDirection(Vector3 direction)
    {
        this.direction = direction;
    }

    public void setScale(float scale)
    {
        transform.localScale = scale * Vector3.one;
    }

    public void setThroughWalls(bool throughWalls)
    {
        this.throughWalls = throughWalls;
    }

    public void setHoming(bool isHoming)
    {
        this.isHoming = isHoming;
    }

    public void homeInitially()
    {
        isHomingInitially = true;
    }

    public void setBouncing()
    {
        isBouncing = true;
    }

    public void setSpawner()
    {
        isSpawner = true;
    }

    public void setExploding(int n)
    {
        nOfProjectilesOnExplode = n;
    }

    public void setBounceExplode(int n)
    {
        nOfProjectilesOnBounce = n;
    }

    public void setColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
        ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
        settings.startColor = color;
    }

    private void Update()
    {
        if (PlayerController.gamePaused)
            return;

        if (lifespan > 0) // For lifespan = 0, it won't disappear naturally
        {
            timer += Time.deltaTime;
            if (timer >= lifespan)
            {
                if (nOfProjectilesOnExplode > 0)
                    explode();
                else
                    Destroy(gameObject);
            }
        }
        transform.Rotate(new Vector3(0, 0, rotationSpeed));

        if (isSpawner)
        {
            spawnerTimer += Time.deltaTime;
            if (spawnerTimer >= 0.5f)
            {
                spawnerTimer = 0;
                spawnSubprojectiles();
            }
        }
    }

    IEnumerator delayThenMove()
    {
        yield return new WaitForSeconds(delayBeforeMoving);
        if (isHomingInitially)
            direction = (player.transform.position - transform.position).normalized;
        StartCoroutine(advance());
    }

    IEnumerator advance()
    {
        Vector3 oldPosition = transform.position;
        Vector3 newPosition;
        if (isHoming)
            newPosition = Vector3.MoveTowards(transform.position, player.transform.position, 1f);
        else
            newPosition = transform.position + direction;

        float timer = 0;
        float distance = Vector3.Distance(transform.position, newPosition);
        float movementDuration = distance / speed;
        Vector3 currentDirection = direction;
        while (timer <= movementDuration)
        {
            if (currentDirection != direction) // Restart advance
            {
                StartCoroutine(advance());
                yield break;
            }

            transform.position = Vector3.Lerp(oldPosition, newPosition, timer / movementDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(advance());
    }

    void bounce()
    {
        // Bouncing angles on floor (opposite for ceiling)
        // Going right -> -90
        // Going left -> 90

        // Bouncing angles on right wall (opposite for left wall)
        // Going up -> -90
        // Going down -> 90
        Vector3 collisionWall = findCollisionWall();
        if (collisionWall == Vector3.zero)
            return;

        /*if (collisionWall == Vector3.down)
            Debug.Log("down");
        else if (collisionWall == Vector3.up)
            Debug.Log("up");
        else if (collisionWall == Vector3.left)
            Debug.Log("left");
        else if (collisionWall == Vector3.right)
            Debug.Log("right");*/

        int angleSign;
        if (collisionWall.x == 0) // Bouncing on floor or ceiling
        {
            bool goingRight = (direction.x > 0);
            angleSign = goingRight ? -1 : 1;
            if (collisionWall == Vector3.up) // Opposite angle for ceiling
                angleSign = -angleSign;
        } else // Bouncing on left or right wall
        {
            bool goingUp = (direction.y > 0);
            angleSign = goingUp ? -1 : 1;
            if (collisionWall == Vector3.left) // Opposite angle for left wall
                angleSign = -angleSign;
        }

        float angleWithPerpendicular = Vector3.Angle(direction, collisionWall);
        float reflectionAngle = angleWithPerpendicular * 2;
        direction = Quaternion.Euler(0, 0, reflectionAngle * angleSign ) * -direction;
    }

    void spawnSubprojectiles()
    {
        spawnSubprojectile(-90);
        spawnSubprojectile(90);
    }

    void spawnSubprojectile(float angle)
    {
        EnemyProjectile ep = Instantiate(gameObject, transform.position, Quaternion.identity).GetComponent<EnemyProjectile>();
        Vector3 subprojectileDirection = Quaternion.Euler(0, 0, angle) * direction;
        ep.initialize(power, 4f, 5f);
        ep.setDirection(subprojectileDirection);
        ep.setThroughWalls(true);
        ep.setScale(0.5f);
        ep.setColor(Color.yellow);
    }

    void explode()
    {
        float angleOfSeparationBetweenProjectiles = 360 / nOfProjectilesOnExplode;
        for (int i=0; i < nOfProjectilesOnExplode; i++)
        {
            spawnExplodingProjectile(45 + angleOfSeparationBetweenProjectiles * i);
        }
        Destroy(gameObject);
    }

    void spawnExplodingProjectile(float angle)
    {
        EnemyProjectile ep = Instantiate(gameObject, transform.position, Quaternion.identity).GetComponent<EnemyProjectile>();
        Vector3 subprojectileDirection = Quaternion.Euler(0, 0, angle) * direction;
        ep.initialize(power, 9f, 0);
        ep.setDirection(subprojectileDirection);
        ep.setScale(0.5f);
        ep.setColor(new Color(0.2f, 0.6f, 0.7f));
    }

    void bounceSpawn()
    {
        Vector3 subprojectileDirection = -findCollisionWall();

        spawnBounceProjectile(0, subprojectileDirection);
        switch (nOfProjectilesOnBounce)
        {
            case 3:
                spawnBounceProjectile(-45, subprojectileDirection);
                spawnBounceProjectile(45, subprojectileDirection);
                break;
            case 5:
                spawnBounceProjectile(-30, subprojectileDirection);
                spawnBounceProjectile(30, subprojectileDirection);
                spawnBounceProjectile(-60, subprojectileDirection);
                spawnBounceProjectile(60, subprojectileDirection);
                break;
            default:
                break;
        }
        
        Destroy(gameObject);
    }

    void spawnBounceProjectile(float angle, Vector3 subprojectileDirection)
    {
        EnemyProjectile ep = Instantiate(gameObject, transform.position, Quaternion.identity).GetComponent<EnemyProjectile>();
        subprojectileDirection = Quaternion.Euler(0, 0, angle) * subprojectileDirection;
        ep.initialize(power, 8f, 0);
        ep.setDirection(subprojectileDirection);
        ep.setScale(0.5f);
        ep.setColor(new Color(0.75f, 0.15f, 0.75f));
    }

    Vector3 findCollisionWall()
    {
        Vector3[] wallsToCheck = new Vector3[] { Vector3.up, Vector3.down, Vector3.right, Vector3.left };
        foreach (Vector3 wall in wallsToCheck)
            if (checkIfCollidingWithWall(wall))
                return wall;
        return Vector3.zero;
    }

    bool checkIfCollidingWithWall(Vector3 wallDirection)
    {
        /*Vector3 raycastOrigin = transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(raycastOrigin, wallDirection, 0.25f, PlayerController.groundAndObstacleLayer);
        return (hits.Length > 0);*/
        Vector3 firstOrigin = transform.position + Quaternion.Euler(0, 0, -90) * wallDirection + wallDirection * 0.5f;
        Vector3 secondOrigin = transform.position + Quaternion.Euler(0, 0, 90) * wallDirection + wallDirection * 0.5f;
        RaycastHit2D[] firstHits = Physics2D.RaycastAll(firstOrigin, wallDirection, 0.05f, PlayerController.groundAndObstacleLayer);
        RaycastHit2D[] secondHits = Physics2D.RaycastAll(secondOrigin, wallDirection, 0.05f, PlayerController.groundAndObstacleLayer);

        return (firstHits.Length > 0 && secondHits.Length > 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().receiveDamage(power);
            if (!isBouncing)
                Destroy(gameObject);
        }
        else if (GameCharacter.collidingWithObstacle(collision) && !throughWalls)
        {
            if (isBouncing)
                bounce();
            else if (nOfProjectilesOnBounce > 0)
                bounceSpawn();
            else
                Destroy(gameObject);
        }
    }

}
