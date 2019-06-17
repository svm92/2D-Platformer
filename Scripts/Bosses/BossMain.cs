using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMain : Boss {

    [HideInInspector] public bool isActing = false;
    protected float jumpSpeed;
    protected float rollSpeed;
    protected float lowerWaitTime;
    protected float higherWaitTime;
    protected float rollWarningTime;
    protected int nOfProjectiles = 0;

    protected float beforeShootWaitTime = 0.5f;
    protected float afterShootWaitTime = 0.5f;

    protected float bulletSpeed = 3f;

    protected ParticleSystem ps;
    protected Light spotlight;
    protected Color bossColor;
    public GameObject fireball;
    protected Color fireballColor;

    protected override void Awake()
    {
        base.Awake();

        ps = GetComponent<ParticleSystem>();

        bossColor = sprite.color;
        Color lighterBossColor = getLighterColor(bossColor);
        ParticleSystem.MainModule settings = ps.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(bossColor, lighterBossColor);

        spotlight = GetComponentInChildren<Light>();
        spotlight.color = bossColor;

        fireballColor = bossColor * 0.85f;
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(act());
    }

    /*protected override void FixedUpdate() // For debug test only
    {
        base.FixedUpdate();
        ParticleSystem.Particle[] parts = new ParticleSystem.Particle[1000];
        ps.GetParticles(parts);
        foreach (ParticleSystem.Particle p in parts)
        {
            float size = p.GetCurrentSize(ps);
            size /= 2;
            Vector3 partPos = p.position;
            Debug.DrawLine(new Vector3(partPos.x - size, partPos.y), new Vector3(partPos.x + size, partPos.y));
            Debug.DrawLine(new Vector3(partPos.x, partPos.y - size), new Vector3(partPos.x, partPos.y + size));
        }
    }*/

    private void Update()
    {
        if (!isActing && !isDead)
            lookAtPlayer();
    }

    protected void changeLifeAccordingToOtherDefeatedBosses()
    {
        // Mainbosses are 109~114
        for (int i = 109; i <= 114; i++)
        {
            if (player.GetComponent<PlayerController>().globalVariables[i])
                health += 12;
        }
    }

    void lookAtPlayer()
    {
        sprite.flipX = player.transform.position.x > transform.position.x ? false : true;
    }

    protected virtual IEnumerator act()
    {
        yield return null;
    }

    protected IEnumerator move(float minDistance, float maxDistance)
    {
        alterFlameSize(ps.main.startSize.constantMax * 0.6f);
        alterFlameLifetime(ps.main.startLifetime.constantMax * 0.65f);

        float distanceToTravel = 0;
        float xDestination = -100;
        bool movingRight = false;
        int counterOfTries = 0;
        while (xDestination < -11.5 || xDestination > 10.5)
        {
            distanceToTravel = Random.Range(minDistance, maxDistance);
            movingRight = Random.Range(0, 2) == 0 ? true : false;
            xDestination = transform.position.x + (distanceToTravel * (movingRight ? 1 : -1));
            counterOfTries++;
            if (counterOfTries >= 1000)
            {
                resetFlame();
                StartCoroutine(act());
                yield break;
            }
        }

        sprite.flipX = movingRight ? false : true;

        Vector3 originalPosition = transform.position;
        Vector3 destination = new Vector3(xDestination, transform.position.y);
        float travelTime = distanceToTravel / speed;
        float timer = 0;
        while (transform.position.x != xDestination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer / travelTime);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = destination; // Correct decimal errors
        resetFlame();
        StartCoroutine(act());
    }

    protected IEnumerator jump(float minDistance, float maxDistance)
    {
        StartCoroutine(jump(minDistance, maxDistance, false, 0, 0, false, 0));
        yield break;
    }

    protected IEnumerator jump(float minDistance, float maxDistance, bool shootMidway)
    {
        StartCoroutine(jump(minDistance, maxDistance, true, 0, 0, shootMidway, 0));
        yield break;
    }

    protected IEnumerator jump(float minDistance, float maxDistance, bool rotatingJump, float newJumpSpeed, float flameLifetime)
    {
        StartCoroutine(jump(minDistance, maxDistance, rotatingJump, newJumpSpeed, flameLifetime, false, 0));
        yield break;
    }

    protected IEnumerator jump(float minDistance, float maxDistance, bool rotatingJump, float newJumpSpeed, 
        float flameLifetime, bool shootMidway, int forceJump)
        // ForceJump -> 0 random, 1 left, 2 right
    {
        float jumpSpeed = (newJumpSpeed != 0 ) ? newJumpSpeed : this.jumpSpeed;
        if (flameLifetime != 0)
            alterFlameLifetime(flameLifetime);

        float jumpRadius = 0;
        float xDestination = -100;
        bool jumpingRight = true;
        int counterOfTries = 0;
        while (xDestination < -11.5 || xDestination > 10.5)
        {
            jumpRadius = Random.Range(minDistance, maxDistance);

            if (forceJump == 1)
                jumpingRight = false;
            else if (forceJump == 2)
                jumpingRight = true;
            else
                jumpingRight = Random.Range(0, 2) == 0 ? true : false;

            if (jumpingRight)
                xDestination = transform.position.x + (jumpRadius * 2);
            else
                xDestination = transform.position.x - (jumpRadius * 2);

            counterOfTries++;
            if (counterOfTries >= 1000)
            {
                resetFlame();
                StartCoroutine(act());
                yield break;
            }
        }

        float xPosition = transform.position.x;
        float xRadiusCenter = jumpingRight ? xPosition + jumpRadius : xPosition - jumpRadius;
        float yRadiusCenter = transform.position.y;

        // Roll/Shoot variables
        float angleSign = jumpingRight ? 1 : -1;
        Quaternion firstHalfRotation = Quaternion.Euler(0, 0, 180 * angleSign);
        Quaternion secondHalfRotation = Quaternion.Euler(0, 0, 360 * angleSign);
        float halfTravelTime = jumpRadius / jumpSpeed;
        float rotationTimer = 0;
        bool jumpIsOver = false;
        bool firstHalfOfJump = true;

        while (!jumpIsOver)
        {
            if (isDead)
                yield break;

            xPosition += (jumpingRight ? jumpSpeed : -jumpSpeed) * Time.deltaTime;

            // Check if we are on first or second half of jump
            if (firstHalfOfJump &&
                ((jumpingRight && xPosition >= xRadiusCenter) || (!jumpingRight && xPosition <= xRadiusCenter)))
            {
                firstHalfOfJump = false;
                rotationTimer = 0;
                if (rotatingJump)
                    transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            // Jump is over
            if ((jumpingRight && xPosition >= xDestination) || (!jumpingRight && xPosition <= xDestination))
            {
                jumpIsOver = true;
            // Center of jump
            } else if (shootMidway &&
                ((jumpingRight && xPosition >= xRadiusCenter) || (!jumpingRight && xPosition <= xRadiusCenter)))
            {
                yield return new WaitForSeconds(beforeShootWaitTime);
                shoot();
                yield return new WaitForSeconds(afterShootWaitTime);
                shootMidway = false; // Don't shoot again
            }
            // Continue jump
            else
            {
                // Circular movement [ x^2 + y^2 = R^2 ]
                float yPosition = yRadiusCenter + Mathf.Sqrt(Mathf.Pow(jumpRadius, 2) - Mathf.Pow(xPosition - xRadiusCenter, 2));
                transform.position = new Vector3(xPosition, yPosition);

                if (rotatingJump)
                {
                    if (firstHalfOfJump)
                        transform.rotation = Quaternion.Lerp(Quaternion.identity, firstHalfRotation, rotationTimer / halfTravelTime);
                    else
                        transform.rotation = Quaternion.Lerp(firstHalfRotation, secondHalfRotation, rotationTimer / halfTravelTime);
                        
                    rotationTimer += Time.deltaTime;
                }
            }

            yield return null;
        }

        transform.position = new Vector3(xDestination, yRadiusCenter);
        transform.rotation = Quaternion.identity;
        resetFlame();
        StartCoroutine(act());
    }

    protected IEnumerator rollAround(float flameLifetime)
    {
        StartCoroutine(rollAround(flameLifetime, 0));
        yield break;
    }

    protected IEnumerator rollAround(float flameLifetime, float flameSpeed)
    {
        alterFlameLifetime(flameLifetime);
        if (flameSpeed > 0)
            alterFlameSpeed(flameSpeed, flameSpeed);
        bool facingLeft = sprite.flipX;
        float timer = 0;
        Quaternion preAttackRotation = facingLeft ? Quaternion.Euler(0, 0, -45) : Quaternion.Euler(0, 0, 45);
        Quaternion firstRotation = facingLeft ? Quaternion.Euler(0, 0, -180) : Quaternion.Euler(0, 0, 180);
        Quaternion secondRotation = facingLeft ? Quaternion.Euler(0, 0, -360) : Quaternion.Euler(0, 0, 360);

        while (timer < rollWarningTime) // Pre-attack warning
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(Quaternion.identity, preAttackRotation, timer / rollWarningTime);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        while (timer / rollSpeed < 1f) // Return to normal
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(preAttackRotation, Quaternion.identity, timer / rollSpeed);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        while (timer / rollSpeed < 1f) // First half-rotation
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(Quaternion.identity, firstRotation, timer / rollSpeed);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        while (timer / rollSpeed < 1f) // Second half-rotation
        {
            if (isDead)
                yield break;

            transform.rotation = Quaternion.Lerp(firstRotation, secondRotation, timer / rollSpeed);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity; // Resolve decimal approximation errors
        resetFlame();
        StartCoroutine(act());
    }

    void changeSpotlightSize(float size) // 120 is default
    {
        spotlight.spotAngle = size;
    }

    protected void alterFlameSize(float n)
    {
        ParticleSystem.MainModule main = ps.main;
        main.startSize = n;
    }

    protected void alterFlameSize(float min, float max)
    {
        ParticleSystem.MainModule main = ps.main;
        main.startSize = new ParticleSystem.MinMaxCurve(min, max);
    }

    protected void alterFlameSpeed(float min, float max)
    {
        ParticleSystem.MainModule main = ps.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(min, max);
    }

    protected void alterFlameLifetime(float n)
    {
        ParticleSystem.MainModule main = ps.main;
        main.startLifetime = n;
    }

    protected void alterFlameLifetime(float min, float max)
    {
        ParticleSystem.MainModule main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(min, max);
    }

    protected void alterFlamesOverTime(int n)
    {
        ParticleSystem.EmissionModule em = ps.emission;
        em.rateOverTime = n;
    }

    protected void alterFlameAngle(int n)
    {
        ParticleSystem.ShapeModule shp = ps.shape;
        shp.arc = n * 2;
        shp.rotation = new Vector3(shp.rotation.x, shp.rotation.y, -90 - n);
    }

    protected virtual void resetFlame()
    {
        alterFlameSize(0.2f, 0.5f);
        alterFlameSpeed(4, 6);
        alterFlameLifetime(0.25f, 0.4f);
        alterFlamesOverTime(100);
        if (ps.shape.arc != 360)
            alterFlameAngle(15);
    }

    protected IEnumerator jumpUpAndShoot()
    {
        StartCoroutine(jumpUpAndShoot(135));
        yield break; ;
    }

    protected IEnumerator jumpUpAndShoot(float angle)
    {
        ps.Stop();

        float jumpHeight = Random.Range(4f, 6.5f);
        float jumpTime = Random.Range(3f, 6f) / jumpSpeed;
        Vector3 originalPosition = transform.position;
        Vector3 destination = originalPosition + Vector3.up * jumpHeight;
        float angleSign = (transform.position.x > player.transform.position.x) ? 1 : -1;
        Quaternion finalRotation = Quaternion.Euler(0, 0, angle * angleSign);
        float timer = 0;
        
        // Jump up
        while (transform.position != destination)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(originalPosition, destination, timer/jumpTime);
            transform.rotation = Quaternion.Lerp(Quaternion.identity, finalRotation, timer/jumpTime);
            timer += Time.deltaTime;

            yield return null;
        }

        // Shoot
        yield return new WaitForSeconds(beforeShootWaitTime);
        shoot();
        yield return new WaitForSeconds(afterShootWaitTime);
        ps.Play();

        timer = 0;
        // Fall down
        while (transform.position != originalPosition)
        {
            if (isDead)
                yield break;

            transform.position = Vector3.Lerp(destination, originalPosition, timer / (jumpTime * 0.5f));
            transform.rotation = Quaternion.Lerp(finalRotation, Quaternion.identity, timer / (jumpTime * 0.5f));
            timer += Time.deltaTime;

            yield return null;
        }
        
        StartCoroutine(act());
    }

    protected void shoot()
    {
        if (bossColor == Color.red)
            shootRed();
        else if (bossColor == Color.green || bossColor == new Color(0, 0.5f, 0))
            shootGreen();
        else if (bossColor == Color.blue)
            shootBlue();
        else if (bossColor == Color.cyan)
            shootCyan();
        else if (bossColor == Color.magenta)
            shootMagenta();
        else if (bossColor == new Color(1, 1, 0))
            shootYellow();
    }

    void shootRed()
    {
        Vector3 spawnPosition = transform.position + transform.up * 0.5f + transform.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(transform.up, fireballColor);
        fireballObj.transform.localScale = 2.5f * Vector3.one;
        fireballObj.setByEnemy();
        fireballObj.setPower(3);
        fireballObj.setSpeed(7f);
        fireballObj.setLifespan(1.2f);
        fireballObj.setThroughWalls();
    }

    void shootGreen()
    {
        Vector3 spawnPosition = transform.position + transform.up * 0.5f + transform.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(transform.up, fireballColor);
        fireballObj.transform.localScale = 0.95f * Vector3.one;
        fireballObj.setByEnemy();
        fireballObj.setSpeed(20f);
    }

    void shootBlue()
    {
        Vector3 spawnPosition = transform.position + transform.up * 0.5f + transform.right * 0.2f;
        shootBlueBullet(0, spawnPosition);
        shootBlueBullet(-45, spawnPosition);
        shootBlueBullet(45, spawnPosition);
        if (nOfProjectiles >= 5)
        {
            shootBlueBullet(-22.5f, spawnPosition);
            shootBlueBullet(22.5f, spawnPosition);
        }
        if (nOfProjectiles >= 7)
        {
            shootBlueBullet(-67.5f, spawnPosition);
            shootBlueBullet(67.5f, spawnPosition);
        }
    }

    protected Fireball shootBlueBullet(float angle, Vector3 spawnPosition)
    {
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        Vector3 fireballDirection = Quaternion.Euler(0, 0, angle) * transform.up;
        fireballObj.initialize(fireballDirection, fireballColor);
        fireballObj.setByEnemy();
        fireballObj.setLifespan(2f);
        return fireballObj;
    }

    protected virtual void shootCyan()
    {
        Vector3 spawnPosition = transform.position + transform.up * 0.5f + transform.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(transform.up, fireballColor);
        fireballObj.setByEnemy();
        fireballObj.setSpeed(bulletSpeed);
        fireballObj.setLifespan(0.75f);
        fireballObj.addCyanAbility(nOfProjectiles);
    }

    void shootMagenta()
    {
        Vector3 spawnPosition = transform.position + transform.up * 0.5f + transform.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(transform.up, fireballColor);
        fireballObj.setByEnemy();
        fireballObj.setSpeed(10f);
        fireballObj.setLifespan(5f);
        fireballObj.transform.localScale = 1.15f * Vector3.one;
        fireballObj.addMagentaAbility();
    }

    void shootYellow()
    {
        if (nOfProjectiles <= 1)
            shootYellow(false, transform.up);
        else if (nOfProjectiles == 2)
        {
            shootYellow(false, Quaternion.Euler(0, 0, -45) * transform.up);
            shootYellow(false, Quaternion.Euler(0, 0, 45) * transform.up);
        }

        nOfProjectiles = 1;
    }

    protected void shootYellow(bool replicate, Vector3 bulletDirection)
    {
        Vector3 spawnPosition = transform.position + transform.up * 0.5f + transform.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(bulletDirection, fireballColor);
        fireballObj.setByEnemy();
        fireballObj.setSpeed(bulletSpeed);
        fireballObj.setLifespan(0);
        fireballObj.addYellowAbility(replicate);
    }

    protected Vector3 findCollisionWall()
    {
        Vector3[] wallsToCheck = new Vector3[] { Vector3.up, Vector3.down, Vector3.right, Vector3.left };
        foreach (Vector3 wall in wallsToCheck)
            if (checkIfCollidingWithWall(wall))
                return wall;
        return Vector3.zero;
    }

    bool checkIfCollidingWithWall(Vector3 wallDirection)
    {
        float halfWidth = transform.lossyScale.x / 2f;
        Vector3 firstOrigin = transform.position + Quaternion.Euler(0, 0, -90) * wallDirection + wallDirection * halfWidth;
        Vector3 secondOrigin = transform.position + Quaternion.Euler(0, 0, 90) * wallDirection + wallDirection * halfWidth;
        RaycastHit2D[] firstHits = Physics2D.RaycastAll(firstOrigin, wallDirection, 0.05f, PlayerController.groundAndObstacleLayer);
        RaycastHit2D[] secondHits = Physics2D.RaycastAll(secondOrigin, wallDirection, 0.05f, PlayerController.groundAndObstacleLayer);

        return (firstHits.Length > 0 && secondHits.Length > 0);
    }

    private void OnParticleCollision(GameObject other)
    {
        // Ignore messages from other particle colliders
        int nOfHits = ParticlePhysicsExtensions.GetCollisionEvents(ps, other, new List<ParticleCollisionEvent>());
        if (nOfHits == 0)
            return;

        if (other.CompareTag("Player"))
            other.GetComponent<PlayerController>().receiveDamage(power);
    }

    protected override void kill()
    {
        base.kill();
        ps.Stop();
        player.GetComponent<PlayerController>().heal();
    }
}
