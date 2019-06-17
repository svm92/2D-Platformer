using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : GameCharacter {

    static PlayerController playerController;

    public GameObject fireball;

    [HideInInspector] public int maxHealth = 3;
    [HideInInspector] public int healthContainers = 0;

    float jumpSpeed = 12.5f;
    float fallingSpeed = 0.05f;
    float gravityPull = 0;
    float verticalMovement = 0;
    float shootDelay = 0.33f;
    float mercyTime = 0.2f;
    float radius;
    float spotlightRadius = 60f;

    [HideInInspector] public float gameTimer = 0;
    float shootTimer = 0;
    [HideInInspector] public int nOfEnemiesKilled = 0;
    [HideInInspector] public int totalDamageReceived = 0;

    bool isRolling = false;
    bool isJumping = false;
    bool isDoubleJumping = false;
    bool isWallJumping = false;
    bool isTouchingGround = false;
    bool isTouchingRightWall = false;
    bool isTouchingLeftWall = false;
    bool isTouchingUpperWall = false;
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool allowMovement = true;
    [HideInInspector] public bool frozenInPlace = false;

    public static bool gamePaused = false;
    GameObject gameMenu;

    public Color playerColor = Color.white;
    Color[] adjacentColors = new Color[3];
    public bool[] unlockedColors = new bool[6]; // Red, green, blue, cyan, magenta, yellow
    Color playerYellow = new Color(1f, 1f, 0f);
    
    static int groundLayer;
    static int obstacleLayer;
    public static int playerLayer;
    public static int groundAndObstacleLayer;
    public static int groundAndPlayerLayer;

    Animator anim;
    ParticleSystem ps;
    Light spotlight;
    HUD HUD;

    public static bool isNewGame = false;
    public bool[] globalVariables = new bool[1000];
    [HideInInspector] Dictionary<string, int> globalVariableTranslator = new Dictionary<string, int>
    { { "BossBlue00", 0 }, { "MirrorAbility", 1 }, { "BossRed00", 2 }, { "RollUnlocked", 3},
        { "ShootR", 4 }, { "ShootG", 5 }, { "ShootB", 6 }, { "ShootC", 7 }, { "ShootM", 8 }, { "ShootY", 9 },
        { "BossBlue01", 10 }, { "DoubleJumpUnlocked", 11 }, { "TeleporterR", 12 }, { "TeleporterG", 13 },
        { "TeleporterB", 14 }, { "TeleporterC", 15 }, { "TeleporterM", 16 }, { "TeleporterY", 17 },
        { "BossRed01", 18 }, { "BossGreen00", 19 }, { "WallJumpUnlocked", 100 }, { "BossGreen01", 101 },
        { "BossGray00", 102 }, { "LongRoll", 103 }, {"BossYellow00", 104}, { "BossCyan00", 105 },
        { "BossMagenta00", 106 }, { "BossMagenta01", 107 }, { "BossCyan01", 108 }, { "MainBossR", 109 },
        { "MainBossG", 110}, { "MainBossB", 111 }, { "MainBossC", 112 }, { "MainBossM", 113 }, { "MainBossY", 114 },
        { "FinalLightning", 115 }, { "BossYellow01", 116 }, { "PowerUp1", 117 }, { "PowerUp2", 118 },
        { "BossGreen02", 119 }, { "TutoJump", 120 }, { "TutoColorSwap1", 121}, { "TutoColorSwap2", 122}
        // Health containers: #20~#46   -   Max health = 12 (3 when game starts) (27 containers, up to #46)
        // Platforms: #50~#90
        // Map areas: #150~
        };

    /*
     * Sprites
     * Heart 0, Power 4, Shot 5, Skill 7
     * - Gray
     * Skill 1, Heart 3
     * - Red
     * Skill 2, Heart 3
     * - Green
     * Skill 3, Heart 3
     * - Blue
     * Skill 1, Heart 5
     * - Cyan
     * Skill 2, Heart 6
     * - Magenta
     * Skill 2, Heart 4
     * - Yellow
     * Skill 2, Heart 3
     * */

        // Check places to put camera changers?

    protected override void Awake()
    {
        if (playerController == null) // If no player, set this as player
        {
            DontDestroyOnLoad(gameObject);
            playerController = this;
        }
        else if (playerController != this) // If a different player already exists, destroy this
        {
            Destroy(gameObject);
            return;
        }

        if (MainMenuManager.savegamePath == null)
            MainMenuManager.savegamePath = Application.persistentDataPath + "/colorPlatformerData.dat";

        if (isNewGame)
        {
            isNewGame = false;
        } else
        {
            loadGame(); // Is skipped if no savefile
        }

        base.Awake();

        speed = 7f;
        health = maxHealth;
        power = 3;

        anim = GetComponent<Animator>();
        ps = GetComponent<ParticleSystem>();
        spotlight = GetComponentInChildren<Light>();
        HUD = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUD>();
        radius = GetComponent<CircleCollider2D>().radius;

        gameMenu = GameObject.FindGameObjectWithTag("MainCamera").transform.GetChild(1).gameObject;

        shootTimer = shootDelay; // Allow shooting as soon as the games starts
        int groundLayerBit = LayerMask.NameToLayer("Ground");
        groundLayer = 1 << groundLayerBit; // Left shift (to turn, say, 8 into 10000000)
        int obstacleLayerBit = LayerMask.NameToLayer("Obstacle");
        obstacleLayer = 1 << obstacleLayerBit;
        int playerLayerBit = LayerMask.NameToLayer("Player");
        playerLayer = 1 << playerLayerBit;
        groundAndObstacleLayer = groundLayer + obstacleLayer;
        groundAndPlayerLayer = groundLayer + playerLayer;

        if (playerColor == Color.white)
            ps.Stop();
        spotlight.spotAngle = spotlightRadius;

        // Check power ups
        if (globalVariables[globalVariableTranslator["PowerUp1"]])
            power++;
        if (globalVariables[globalVariableTranslator["PowerUp2"]])
            power++;
    }

    protected override void Start() // applyColor cannot be called in Awake (HUD not ready yet)
    {
        applyColor();
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.N)) // Simulate a framedrop
            System.Threading.Thread.Sleep(1000);*/

        if (!gamePaused)
            gameTimer += Time.deltaTime;

        if (isDead)
            return;

        if (Input.GetButtonDown("Menu"))
            pauseGame();

        if (frozenInPlace || gamePaused)
            return;

        shootTimer += Time.deltaTime; // Shooting timer

        // Left / Right movement
        isTouchingLeftWall = checkIfTouchingWall(Vector3.left);
        isTouchingRightWall = checkIfTouchingWall(Vector3.right);

        if (Input.GetAxisRaw("Horizontal") > 0 && allowMovement)
            move(Vector3.right);
        else if (Input.GetAxisRaw("Horizontal") < 0 && allowMovement)
            move(Vector3.left);

        applyHorizontalCorrection();

        // Jumping / Falling
        verticalMovement = 0;
        isTouchingGround = checkIfTouchingWall(Vector3.down);
        isTouchingUpperWall = checkIfTouchingWall(Vector3.up);

        if (Input.GetButtonDown("Jump") && allowMovement)
        {
            if (!isTouchingGround && !isWallJumping && (isTouchingLeftWall || isTouchingRightWall)
                && globalVariables[globalVariableTranslator["WallJumpUnlocked"]])
                wallJump();
            else if (!isTouchingGround && !isDoubleJumping && globalVariables[globalVariableTranslator["DoubleJumpUnlocked"]])
                doubleJump();
            else if (isTouchingGround)
                jump();
        }

        applyGravity();
        if (Time.deltaTime <= 0.05f) // Except for framedrops (to avoid bugs)
            transform.position += Vector3.up * verticalMovement * Time.deltaTime;

        if (verticalMovement == 0)
            applyVerticalCorrection();

        // Roll
        if (Input.GetButtonDown("Fire1") && globalVariables[globalVariableTranslator["RollUnlocked"]]
            && !isRolling && allowMovement)
            StartCoroutine(rollAround());

        // Shooting
        if (Input.GetButtonDown("Fire2") && canShoot() && !isTouchingUpperWall && allowMovement)
            shoot();

        // Color shifting
        if (Input.GetButtonDown("ShiftColorLeft") && allowMovement)
            shiftColor(0);
        else if (Input.GetButtonDown("ShiftColorRight") && allowMovement)
            shiftColor(1);
        else if (Input.GetButtonDown("ChangeColorMode") && allowMovement)
            shiftColor(2);

    }

    /**private void FixedUpdate() // Debug for test purposes only
    {
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

    void move(Vector3 direction)
    {
        anim.SetBool("facingLeft", direction == Vector3.left);
        bool isTouchingWall = direction == Vector3.left ? isTouchingLeftWall : isTouchingRightWall;
        /*if (isTouchingWall && checkForStairsInFront(direction))
        {
            transform.position += direction * speed/100 * Time.deltaTime + Vector3.up;
        } 
        else*/
        if (!isTouchingWall)
            transform.position += direction * speed * Time.deltaTime;
    }

    void jump()
    {
        isJumping = true;
        isTouchingGround = false;
    }

    void doubleJump()
    {
        isJumping = true;
        isDoubleJumping = true;
        gravityPull = 0;
    }

    void wallJump()
    {
        isJumping = true;
        isWallJumping = true;
        gravityPull = 0;
    }

    public void stopJump()
    {
        gravityPull = 0;
        isJumping = false;
        isDoubleJumping = false;
        isWallJumping = false;
    }

    void applyGravity()
    {
        float averageTimeDelta = 0.01656f;
        if (Time.deltaTime <= 0.05f) // Except for framedrops (to avoid bugs)
            gravityPull -= Physics.gravity.y * Time.deltaTime / averageTimeDelta;

        if (gravityPull > 600)
            gravityPull = 600;

        if (isTouchingGround)
        {
            stopJump();
        }

        if (isJumping)
        {
            verticalMovement += jumpSpeed;
        }

        if (isTouchingUpperWall && verticalMovement > 0)
        {
            verticalMovement = 0;
            gravityPull = -Physics.gravity.y;
            isJumping = false;
        }

        verticalMovement -= gravityPull * fallingSpeed;
    }

    bool checkIfTouchingWall(Vector3 direction)
    {
        float height = transform.lossyScale.y;

        Vector3 characterSide = transform.position + direction * height * 0.5f;
        Vector3 firstCorner = Quaternion.Euler(0, 0, -90) * direction;
        Vector3 secondCorner = Quaternion.Euler(0, 0, 90) * direction;
        Vector3 firstRaycastOrigin = characterSide + firstCorner * height * 0.4f;
        Vector3 secondRaycastOrigin = characterSide + secondCorner * height * 0.4f;
        float distance;
        if (direction.x != 0 && Time.deltaTime >= 0.04f) // Horizontal movement with framedrops
                distance = speed * Time.deltaTime; // Horizontal displacement
        else
            distance = height * 0.1f;

        return Physics2D.Raycast(firstRaycastOrigin, direction, distance, groundAndObstacleLayer) ||
            Physics2D.Raycast(secondRaycastOrigin, direction, distance, groundAndObstacleLayer);
    }
    
    /*bool checkForStairsInFront(Vector3 direction) // Assumes touching wall in that direction, used for stairs
    {
        if (!checkIfTouchingWall(Vector3.down)) // If not on floor, cannot climb stairs
            return false;

        float height = transform.lossyScale.y;

        Vector3 characterSide = transform.position + direction * height * 0.5f;
        Vector3 raycastOrigin = characterSide + Vector3.up * height * 0.501f; // Just right above the character
        float distance = height * 0.1f;

        return !Physics2D.Raycast(raycastOrigin, direction, distance, groundAndObstacleLayer);
    }*/

    void applyHorizontalCorrection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, obstacleLayer);
        if (hit != null)
            pushBack(hit.gameObject);
    }

    // Push the player back if stuck in lightning
    void pushBack(GameObject obstacle)
    {
        float obstacleWidth = obstacle.transform.lossyScale.x;
        bool obstacleToTheLeft = (obstacle.transform.position.x - transform.position.x) >= 0;
        float offset = obstacleWidth - Mathf.Abs(obstacle.transform.position.x - transform.position.x);
        if (offset > 0)
            if (obstacleToTheLeft)
                transform.position += Vector3.left * (offset + radius); // Add player radius to break collision
            else if (!obstacleToTheLeft)
                transform.position += Vector3.right * (offset + radius);
    }

    // Push the player upwards if stuck in floor
    void applyVerticalCorrection()
    {
        float width = transform.lossyScale.x;
        float height = transform.lossyScale.y;

        Vector3 characterSide = transform.position + Vector3.up * height * 0.35f;
        Vector3 firstCorner = Quaternion.Euler(0, 0, -90) * Vector3.up;
        Vector3 secondCorner = Quaternion.Euler(0, 0, 90) * Vector3.up;
        Vector3 firstRaycastOrigin = characterSide + firstCorner * width * 0.4f;
        Vector3 secondRaycastOrigin = characterSide + secondCorner * width * 0.4f;
        float depth = height * 1.5f;
        float leftOffset = float.MinValue;
        float rightOffset = float.MinValue;

        RaycastHit2D[] hits = Physics2D.RaycastAll(firstRaycastOrigin, Vector3.down, depth, groundLayer);
        foreach (RaycastHit2D hit in hits)
        {
            float possibleOffset = (height * 0.5f) - (transform.position.y - hit.point.y);
            if (possibleOffset > leftOffset)
                leftOffset = possibleOffset;
        }

        hits = Physics2D.RaycastAll(secondRaycastOrigin, Vector3.down, depth, groundLayer);
        foreach (RaycastHit2D hit in hits)
        {
            float possibleOffset = (height * 0.5f) - (transform.position.y - hit.point.y);
            if (possibleOffset > rightOffset)
                rightOffset = possibleOffset;
        }

        // Take the smallest of the two offsets (unless they are float.MinValue)
        float offset = (leftOffset == float.MinValue) ? rightOffset : 
            (rightOffset == float.MinValue) ? leftOffset : Mathf.Min(leftOffset, rightOffset);

        if (Math.Abs(offset) > 0.000001f && Math.Abs(offset) <= 1f)
            transform.position += Vector3.up * offset;
    }

    IEnumerator rollAround()
    {
        isRolling = true;
        if (globalVariables[globalVariableTranslator["LongRoll"]])
        {
            alterFlameSize(1.35f, 1.5f);
            alterFlameSpeed(8.5f, 10f);
        }
        else
        {
            alterFlameSize(0.8f, 1f);
        } 
        bool facingLeft = anim.GetBool("facingLeft");
        float speed = 0.3f;
        float timer = 0;
        Quaternion firstRotation = facingLeft ? Quaternion.Euler(0, 0, -180) : Quaternion.Euler(0, 0, 180);
        Quaternion secondRotation = facingLeft ? Quaternion.Euler(0, 0, -360) : Quaternion.Euler(0, 0, 360);

        while (timer / speed < 1f) // First half-rotation
        {
            transform.rotation = Quaternion.Lerp(Quaternion.identity, firstRotation, timer / speed);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        while (timer / speed < 1f) // Second half-rotation
        {
            transform.rotation = Quaternion.Lerp(firstRotation, secondRotation, timer / speed);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity; // Resolve decimal approximation errors
        normalizeFlame();
        isRolling = false;
    }

    void alterFlameSize(float min, float max)
    {
        ParticleSystem.MainModule main = ps.main;
        main.startSize = new ParticleSystem.MinMaxCurve(min, max);
    }

    void alterFlameSpeed(float min, float max)
    {
        ParticleSystem.MainModule main = ps.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(min, max);
    }

    void normalizeFlame()
    {
        alterFlameSize(0.2f, 0.5f);
        alterFlameSpeed(4f, 6f);
    }

    void shoot()
    {
        switch (getColorIndex(playerColor))
        {
            case 0:
                shootRed();
                break;
            case 1:
                shootGreen();
                break;
            case 2:
                shootBlue();
                break;
            case 3:
                shootCyan();
                break;
            case 4:
                shootMagenta();
                break;
            case 5:
                shootYellow();
                break;
            default:
                break;
        }
    }

    void shootRed() // Big Shot
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f + Vector3.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(Vector3.up, playerColor);
        fireballObj.transform.localScale = 1.5f * Vector3.one;
        fireballObj.setPower(power);
        fireballObj.setSpeed(7f);
        fireballObj.setLifespan(1.2f);
    }

    void shootGreen() // Pierce shot
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f + Vector3.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(Vector3.up, playerColor);
        fireballObj.transform.localScale = 0.95f * Vector3.one;
        fireballObj.setPower(power - 1);
        fireballObj.setSpeed(15f);
        fireballObj.addGreenAbility();
    }

    void shootBlue() // x3 Shot
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f + Vector3.right * 0.2f;
        // Center fireball
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(Vector3.up, playerColor);
        fireballObj.setPower(power - 1);

        // Left fireball
        fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        Vector3 upLeft = Quaternion.Euler(0, 0, -45) * Vector3.up;
        fireballObj.initialize(upLeft, playerColor);
        fireballObj.setPower(power - 1);

        // Right fireball
        fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        Vector3 upRight = Quaternion.Euler(0, 0, 45) * Vector3.up;
        fireballObj.initialize(upRight, playerColor);
        fireballObj.setPower(power - 1);
    }

    void shootCyan() // Dividing Shot
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f + Vector3.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(Vector3.up, playerColor);
        fireballObj.setPower(power - 1);
        fireballObj.setSpeed(5f);
        fireballObj.setLifespan(0.75f);
        fireballObj.addCyanAbility();
    }

    void shootMagenta()
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f + Vector3.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(Vector3.up, playerColor);
        fireballObj.setPower(power - 1);
        fireballObj.setSpeed(10f);
        fireballObj.setLifespan(5f);
        fireballObj.transform.localScale = 1.15f * Vector3.one;
        fireballObj.addMagentaAbility();
    }

    void shootYellow() // Spawning shot
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f + Vector3.right * 0.2f;
        Fireball fireballObj = Instantiate(fireball, spawnPosition, Quaternion.identity).GetComponent<Fireball>();
        fireballObj.initialize(Vector3.up, playerColor);
        fireballObj.setPower(power - 1);
        fireballObj.setSpeed(4f);
        fireballObj.setLifespan(1.1f);
        fireballObj.addYellowAbility();
    }

    bool canShoot()
    {
        if (isRolling || !colorShotUnlocked())
            return false;
        if (shootTimer >= shootDelay)
        {
            shootTimer = 0;
            return true;
        }
        return false;
    }

    bool colorShotUnlocked()
    {
        // Variables 4~9 correspond to unlocked color shots
        int colorIndex = getColorIndex(playerColor);
        if (colorIndex < 0)
            return false;
        return globalVariables[4 + colorIndex];
    }

    private void OnParticleCollision(GameObject other)
    {
        // Ignore messages from other particle colliders
        int nOfHits = ParticlePhysicsExtensions.GetCollisionEvents(ps, other, new List<ParticleCollisionEvent>());
        if (nOfHits == 0)
            return;

        if (other.CompareTag("Torch"))
            other.GetComponent<Torch>().illuminate(playerColor);
        else if (other.CompareTag("Enemy"))
            other.GetComponent<Enemy>().receiveDamage(power);      
    }

    public void receiveDamage(int damage)
    {
        if (isHurting || frozenInPlace)
            return;

        isHurting = true;
        health -= damage;
        totalDamageReceived += damage;
        StartCoroutine(flashForDamage());
        HUD.updateHearts();

        if (health <= 0)
            kill();
    }

    protected override void kill()
    {
        isDead = true;
        ps.Stop();
        GameObject.Find("MusicManager").GetComponent<AudioSource>().Stop();
        StartCoroutine(hopAndDie());
    }

    IEnumerator hopAndDie()
    {
        sprite.flipY = true;
        Vector3 destination = transform.position + Vector3.up * 2f;
        float elapsedTime = 0;
        float dyingTime = 0.5f;
        while (elapsedTime <= dyingTime) // Jump upwards
        {
            transform.position = Vector3.Lerp(transform.position, destination,  elapsedTime / dyingTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        destination = transform.position + Vector3.down * 2f;
        while (elapsedTime <= dyingTime) // Fall and disappear
        {
            transform.position = Vector3.Lerp(transform.position, destination, elapsedTime / dyingTime);
            sprite.color = Color.Lerp(sprite.color, Color.clear, elapsedTime / dyingTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        endGame();
    }

    protected override IEnumerator flashForDamage()
    {
        yield return base.flashForDamage();
        StartCoroutine(blink());
        yield break;
    }

    protected override Color flashingColor(Color color)
    {
        return invertColor(color);
    }

    public static Color invertColor(Color color)
    {
        float r = color.r > 0.5 ? 0 : 1;
        float g = color.g > 0.5 ? 0 : 1;
        float b = color.b > 0.5 ? 0 : 1;
        return new Color(r, g, b);
    }

    IEnumerator blink()
    {
        float elapsedTime = 0;
        bool isBlinking = false;

        while (elapsedTime < mercyTime)
        {
            if (isDead)
                yield break;

            elapsedTime += Time.deltaTime;
            sprite.color = isBlinking ? playerColor : Color.clear;
            isBlinking = !isBlinking;
            yield return new WaitForSeconds(0.2f);
        }

        sprite.color = playerColor;
        isHurting = false;
    }

    void shiftColor(int pos)
    {
        if (adjacentColors[pos] == Color.clear)
            return;

        playerColor = adjacentColors[pos];
        applyColor();
    }

    void applyColor()
    {
        sprite.color = playerColor;
        Color lighterPlayerColor = getLighterColor(playerColor);
        ParticleSystem.MainModule settings = ps.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(playerColor, lighterPlayerColor);
        spotlight.color = playerColor;
        decideAdjacentColors();
        HUD.updateHearts();
    }

    void decideAdjacentColors()
    {
        if (playerColor == Color.red)
            changeAdjacentColors(Color.blue, Color.green, Color.cyan);
        else if (playerColor == Color.green)
            changeAdjacentColors(Color.red, Color.blue, Color.magenta);
        else if (playerColor == Color.blue)
            changeAdjacentColors(Color.green, Color.red, playerYellow);
        else if (playerColor == Color.cyan)
            changeAdjacentColors(playerYellow, Color.magenta, Color.red);
        else if (playerColor == Color.magenta)
            changeAdjacentColors(Color.cyan, playerYellow, Color.green);
        else if (playerColor == playerYellow)
            changeAdjacentColors(Color.magenta, Color.cyan, Color.blue);
        else if (playerColor == Color.white)
            changeAdjacentColors(Color.clear, Color.clear, Color.clear);
    }

    void changeAdjacentColors(Color leftColor, Color rightColor, Color inverseColor)
    {
        changeAdjacentColor(leftColor, 0);
        changeAdjacentColor(rightColor, 1);
        changeAdjacentColor(inverseColor, 2);
    }

    void changeAdjacentColor(Color color, int pos)
    {
        int colorIndex = getColorIndex(color);
        if (colorIndex == -1 || !unlockedColors[colorIndex]) // If that color is locked, make it invisible
            color = Color.clear;

        adjacentColors[pos] = color;
        HUD.updateColor(color, pos);
    }

    public int getColorIndex(Color color)
    {
        if (color == Color.red)
            return 0;
        if (color == Color.green)
            return 1;
        if (color == Color.blue)
            return 2;
        if (color == Color.cyan)
            return 3;
        if (color == Color.magenta)
            return 4;
        if (color == playerYellow)
            return 5;

        return -1; // Invalid color
    }

    public void unlockColor(Color color)
    {
        bool firstColor = (playerColor == Color.white);
        bool firstCMYColor = (!unlockedColors[3] && !unlockedColors[4] && !unlockedColors[5] &&
            (color == Color.cyan || color == Color.magenta || color == playerYellow));

        int colorIndex = getColorIndex(color);
        unlockedColors[colorIndex] = true;
        playerColor = color;
        applyColor();

        if (firstColor)
            unlockFire();

        if (firstCMYColor)
            StartCoroutine(showInverseColorTutorial());
    }

    void unlockFire()
    {
        ps.Play();
        StartCoroutine(changeSpotlightRadius(120));
    }

    IEnumerator changeSpotlightRadius(int newRadius)
    {
        spotlightRadius = newRadius;
        while (spotlight.spotAngle < newRadius)
        {
            spotlight.spotAngle = Mathf.Lerp(spotlight.spotAngle, newRadius, 0.75f * Time.deltaTime);
            yield return null;
        }  

        spotlight.spotAngle = newRadius; // To ensure no decimal errors from Lerp
    }

    IEnumerator showInverseColorTutorial()
    {
        GameObject tutoTextCanvas = transform.GetChild(3).gameObject;
        Text tutoTextContainer = tutoTextCanvas.transform.GetChild(0).GetComponent<Text>();
        tutoTextCanvas.SetActive(true);
        tutoTextContainer.text = "(W) - Swap color palette";

        // Fade in
        float fadeInTime = 0.5f;
        float timer = 0;
        while (tutoTextContainer.color != Color.white)
        {
            tutoTextContainer.color = Color.Lerp(Color.clear, Color.white, timer / fadeInTime);
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        // Fade out
        timer = 0;
        while (tutoTextContainer.color != Color.clear)
        {
            tutoTextContainer.color = Color.Lerp(Color.white, Color.clear, timer / fadeInTime);
            timer += Time.deltaTime;
            yield return null;
        }

        tutoTextCanvas.SetActive(false);
    }

    public void heal()
    {
        health = maxHealth;
        HUD.updateHearts();
    }

    public void addHealthContainer()
    {
        healthContainers++;
        if (healthContainers >= 3)
        {
            healthContainers = 0;
            maxHealth++;
            heal();
        }
    }

    public void freezeMovement()
    {
        frozenInPlace = true;
        verticalMovement = 0;
    }

    public void pauseGame()
    {
        if (gamePaused)
        {
            gamePaused = false;
            Time.timeScale = 1;
            gameMenu.GetComponent<GameMenu>().hideMap();
            gameMenu.GetComponent<GameMenu>().resetQuitButton();
            gameMenu.SetActive(false);
        }
        else
        {
            gamePaused = true;
            Time.timeScale = 0;
            gameMenu.SetActive(true);
        }
    }

    void endGame()
    {
        SceneManager.LoadScene("DeathTransition", LoadSceneMode.Additive);
    }

    public void saveGame(GameObject savepoint)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(MainMenuManager.savegamePath);

        String sceneName = SceneManager.GetActiveScene().name;
        Vector3 spawnPosition = savepoint.transform.position + Vector3.down * 0.5f;
        Savegame save = new Savegame(sceneName, spawnPosition, maxHealth, healthContainers, playerColor, unlockedColors, 
            spotlightRadius, globalVariables, gameTimer, nOfEnemiesKilled, totalDamageReceived);

        bf.Serialize(file, save);
        file.Close();
    }

    void loadGame()
    {
        if (File.Exists(MainMenuManager.savegamePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(MainMenuManager.savegamePath, FileMode.Open);
            Savegame save = (Savegame) bf.Deserialize(file);
            file.Close();

            applySavegameDataToPlayer(save);
        }
    }

    void applySavegameDataToPlayer(Savegame save)
    {
        if (SceneManager.GetActiveScene().name != save.sceneName)
            SceneManager.LoadScene(save.sceneName);

        transform.position = new Vector3(save.pos[0], save.pos[1], 0);
        maxHealth = save.maxHealth;
        healthContainers = save.healthContainers;
        playerColor = new Color(save.playerColor[0], save.playerColor[1], save.playerColor[2]);
        unlockedColors = save.unlockedColors;
        spotlightRadius = save.spotlightRadius;
        globalVariables = save.globalVariables;
        gameTimer = save.gameTimer;
        nOfEnemiesKilled = save.nOfEnemiesKilled;
        totalDamageReceived = save.totalDamageReceived;
    }

}

[Serializable]
class Savegame
{
    public String sceneName;
    public float[] pos = new float[2]; // Vectors cannot be serialized, so divide into 'x' and 'y'
    public int maxHealth;
    public int healthContainers;
    public float[] playerColor = new float[3]; // "Color" cannot be serialized, so divided it into r, g, b
    public bool[] unlockedColors = new bool[6];
    public float spotlightRadius;
    public bool[] globalVariables;
    public float gameTimer;
    public int nOfEnemiesKilled;
    public int totalDamageReceived;

    public Savegame(String sceneName, Vector3 pos, int maxHealth, int healthContainers,
        Color playerColor, bool[] unlockedColors, float spotlightRadius, bool[] globalVariables,
        float gameTimer, int nOfEnemiesKilled, int totalDamageReceived)
    {
        this.sceneName = sceneName;
        this.pos[0] = pos.x;
        this.pos[1] = pos.y;
        this.maxHealth = maxHealth;
        this.healthContainers = healthContainers;
        this.playerColor[0] = playerColor.r;
        this.playerColor[1] = playerColor.g;
        this.playerColor[2] = playerColor.b;
        this.unlockedColors = unlockedColors;
        this.spotlightRadius = spotlightRadius;
        this.globalVariables = globalVariables;
        this.gameTimer = gameTimer;
        this.nOfEnemiesKilled = nOfEnemiesKilled;
        this.totalDamageReceived = totalDamageReceived;
    }
}
