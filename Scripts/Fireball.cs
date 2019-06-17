using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour {

    Vector3 direction;
    Color color;
    int power = 2;
    float speed = 10f;
    float lifespan = 1f;
    float timer = 0;

    bool greenAbility = false;
    bool cyanAbility = false;
    int nOfCyanBullets = 4;
    bool magentaAbility = false;
    bool yellowAbility = false;
    float yellowTimer = 0;
    bool yellowReplicate = false;

    bool isEnemyFireball = false;
    bool isThroughWalls = false;

    public void initialize(Vector3 direction, Color color)
    {
        this.direction = direction;
        float angle = Vector3.SignedAngle(Vector3.left, direction, Vector3.back); // Sprite is looking left
        transform.Rotate(Vector3.back, angle);

        GetComponent<SpriteRenderer>().color = color;
        this.color = GetComponent<SpriteRenderer>().color;
    }

    public void setPower(int power)
    {
        this.power = power;
    }

    public void setSpeed(float speed)
    {
        this.speed = speed;
    }

    public void setLifespan(float lifespan)
    {
        this.lifespan = lifespan;
    }

    public void setByEnemy()
    {
        isEnemyFireball = true;
    }

    public void setThroughWalls()
    {
        isThroughWalls = true;
    }

    private void Update()
    {
        if (PlayerController.gamePaused)
            return;

        timer += Time.deltaTime;
        if (timer > lifespan && lifespan > 0)
        {
            if (cyanAbility)
                cyanDivide();
            vanish();
        }

        if (yellowAbility)
        {
            yellowTimer += Time.deltaTime;
            checkIfSpawnYellow();
        }  

        Vector3 destination = transform.position + direction;
        transform.position = Vector3.Lerp(transform.position, destination, speed * Time.deltaTime);
    }

    public void addGreenAbility()
    {
        greenAbility = true;
    }

    public void addCyanAbility()
    {
        cyanAbility = true;
    }

    public void addCyanAbility(int nOfDivisions)
    {
        addCyanAbility();
        nOfCyanBullets = nOfDivisions;
    }

    void cyanDivide()
    {
        float separationBetweenBullets = 360 / nOfCyanBullets;
        for (int i=0; i < nOfCyanBullets; i++)
        {
            float angle = 45 + separationBetweenBullets * i;
            spawnCyanFireball(Quaternion.Euler(0, 0, angle) * direction);
        }
    }

    void spawnCyanFireball(Vector3 spawnDirection)
    {
        Vector3 spawnPosition = transform.position + direction;
        Fireball fireballObj = Instantiate(gameObject, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(spawnDirection, color);
        fireballObj.setSpeed(10f);
        fireballObj.setLifespan(1f);
        fireballObj.setPower(power);

        if (isEnemyFireball)
        {
            fireballObj.setByEnemy();
            fireballObj.setLifespan(3f);
        }
    }

    public void addMagentaAbility()
    {
        magentaAbility = true;
    }

    void spawnMagenta()
    {
        spawnMagentaFireball(0, 11);
        spawnMagentaFireball(-2.5f, 9);
        spawnMagentaFireball(2.5f, 9);
        spawnMagentaFireball(-5f, 7);
        spawnMagentaFireball(5f, 7);
        vanish();
    }

    void spawnMagentaFireball(float xDisplacement, float speed)
    {
        Vector3 spawnDisplacement = Quaternion.Euler(0, 0, -90) * direction;
        Vector3 spawnPosition = transform.position + (spawnDisplacement * xDisplacement);
        if (!possibleToSpawnMagentaFireballIn(transform.position, spawnPosition))
            return;

        Fireball fireballObj = Instantiate(gameObject, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(-direction, color);
        fireballObj.setSpeed(speed);
        fireballObj.setLifespan(5f);
        fireballObj.setPower(power);
        fireballObj.transform.localScale = 0.9f * Vector3.one;

        if (isEnemyFireball)
        {
            fireballObj.setByEnemy();
        }
        
        // Code snippet to solve a bug causing fireballs to spawn with components disabled
        if (!fireballObj.GetComponent<Animator>().enabled)
            fireballObj.GetComponent<Animator>().enabled = true;
        if (!fireballObj.GetComponent<CapsuleCollider2D>().enabled)
            fireballObj.GetComponent<CapsuleCollider2D>().enabled = true;
        if (!fireballObj.GetComponent<Fireball>().enabled)
            fireballObj.GetComponent<Fireball>().enabled = true;
    }

    bool possibleToSpawnMagentaFireballIn(Vector3 originalPosition, Vector3 spawnPosition)
    {
        // Check if it's a wall or there's one close
        RaycastHit2D[] hits = Physics2D.RaycastAll(spawnPosition, Vector3.down, 1.5f, PlayerController.groundAndObstacleLayer);
        if (hits.Length > 0)
            return false;

        // If not, check if there's a wall in the middle (between the original fireball and the one to spawn)
        float distanceBetweenFireballs = Mathf.Abs(originalPosition.x - spawnPosition.x);
        if (distanceBetweenFireballs == 0)
            return true;

        Vector3 directionToCheck = (spawnPosition.x > originalPosition.x) ? Vector3.right : Vector3.left;

        hits = Physics2D.RaycastAll(originalPosition, directionToCheck, 
            distanceBetweenFireballs, PlayerController.groundAndObstacleLayer);
        return (hits.Length == 0);
    }

    public void addYellowAbility()
    {
        yellowAbility = true;
    }

    public void addYellowAbility(bool replicate)
    {
        addYellowAbility();
        yellowReplicate = replicate;
    }

    void checkIfSpawnYellow()
    {
        if (yellowTimer >= 0.5f)
        {
            spawnYellow();
            yellowTimer = 0;
        }
    }

    void spawnYellow()
    {
        spawnYellowFireball(Quaternion.Euler(0, 0, -90) * direction);
        spawnYellowFireball(Quaternion.Euler(0, 0, 90) * direction);
    }

    void spawnYellowFireball(Vector3 spawnDirection)
    {
        Vector3 spawnPosition = transform.position + direction;
        Fireball fireballObj = Instantiate(gameObject, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(spawnDirection, color);
        fireballObj.setPower(power);

        if (!isEnemyFireball)
        {
            fireballObj.setSpeed(4f);
            fireballObj.setLifespan(1.25f);
        }
        else
        {
            fireballObj.setByEnemy();
            fireballObj.setSpeed(5f);
            fireballObj.setLifespan(0);
            if (yellowReplicate)
                fireballObj.addYellowAbility();
        }
    }

    void vanish()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") && !isThroughWalls)
        {
            if (magentaAbility)
                spawnMagenta();
            else
                vanish();
        }
        else if (collision.CompareTag("Torch"))
        {
            Color color = GetComponent<SpriteRenderer>().color;
            collision.GetComponent<Torch>().illuminate(color);
            vanish();
        }
        else if (collision.CompareTag("Enemy") && !isEnemyFireball)
        {
            collision.GetComponent<Enemy>().receiveDamage(power);
            if (!greenAbility)
                vanish();
        }
        else if (isEnemyFireball && collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().receiveDamage(power);
            vanish();
        }  
    }

}
