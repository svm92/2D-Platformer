using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossYellow01 : Boss {

    public GameObject projectile;
    int nOfProjectiles = 2;
    bool[] spawnedProjectileN = new bool[5];
    List<GameObject> listOfProjectiles = new List<GameObject>();

    Vector3 direction = Vector3.right;

    protected override void Awake()
    {
        base.Awake();
        speed = 4f;
        health = 175;
        power = 2;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isDead)
        {
            transform.position += direction * speed * Time.deltaTime;
            recheckValues();

            removeOutOfScreenProjectiles();
            if (notEnoughProjectilesOnScreen())
                spawnProjectile();
        }
    }

    void changeDirection()
    {
        direction = Quaternion.Euler(0, 0, 180) * direction;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (GameCharacter.collidingWithObstacle(collision))
            changeDirection();
    }

    void recheckValues()
    {
        if (health <= 0.17f * maxHealth && !spawnedProjectileN[4])
        {
            nOfProjectiles += 3;
            spawnedProjectileN[4] = true;
        }
        else if (health <= 0.33f * maxHealth && !spawnedProjectileN[3])
        {
            nOfProjectiles += 2;
            spawnedProjectileN[3] = true;
        }
        else if (health <= 0.49f * maxHealth && !spawnedProjectileN[2])
        {
            nOfProjectiles += 2;
            spawnedProjectileN[2] = true;
        }
        else if (health <= 0.66f * maxHealth && !spawnedProjectileN[1])
        {
            nOfProjectiles += 2;
            spawnedProjectileN[1] = true;
        }
        else if (health <= 0.83f * maxHealth && !spawnedProjectileN[0])
        {
            nOfProjectiles += 1;
            spawnedProjectileN[0] = true;
        }
    }

    void spawnProjectile()
    {
        float randomAngle = Random.Range(-110, 110); 
        Vector3 spawnPosition = transform.position;
        GameObject epObject = Instantiate(projectile, spawnPosition, Quaternion.identity);
        listOfProjectiles.Add(epObject);
        EnemyProjectile ep = epObject.GetComponent<EnemyProjectile>();
        ep.initialize(power, 10f, 0);
        ep.setDirection(Quaternion.Euler(0, 0, randomAngle) * Vector3.up);
        ep.setBouncing();
        ep.setColor(new Color(0.9f, 0.65f, 0.1f));
    }

    void removeOutOfScreenProjectiles()
    {
        List<int> listOfProjectilesToRemove = new List<int>();
        // Find projectiles out of screen
        for (int i=0; i < listOfProjectiles.Count; i++)
        {
            GameObject p = listOfProjectiles[i];
            if (Mathf.Abs(p.transform.position.x) > 16 || p.transform.position.y < -5 || p.transform.position.y > 14)
                listOfProjectilesToRemove.Add(i);
        }

        // Remove those projectiles
        foreach (int i in listOfProjectilesToRemove)
        {
            GameObject p = listOfProjectiles[i];
            listOfProjectiles.Remove(p);
            Destroy(p);
        }
    }

    bool notEnoughProjectilesOnScreen()
    {
        return (listOfProjectiles.Count < nOfProjectiles);
    }

    protected override void kill()
    {
        base.kill();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().globalVariables[116] = true;
    }
}
