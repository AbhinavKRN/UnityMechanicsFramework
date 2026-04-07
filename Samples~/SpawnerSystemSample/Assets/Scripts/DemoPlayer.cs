using UnityEngine;

public class DemoPlayer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 0.2f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private float fireCooldown = 0f;
    private float invincibilityTimer = 0f;
    private float flashTimer = 0f;
    private SpriteRenderer sr;
    private Color originalColor;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
    }

    private void Start()
    {
        PixelArtGenerator.Init();
        sr.sprite = PixelArtGenerator.PlayerSprite;
        sr.sortingOrder = 3;
        originalColor = Color.white;
        sr.color = originalColor;
        currentHealth = maxHealth;
        gameObject.tag = "Player";
    }

    private void Update()
    {
        if (ScoreManager.IsGameOver) return;

        HandleMovement();
        HandleAiming();
        HandleShooting();
        HandleTimers();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = new Vector3(h, v, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Clamp to arena bounds
        float clampX = 9f;
        float clampY = 5f;
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -clampX, clampX),
            Mathf.Clamp(transform.position.y, -clampY, clampY),
            0f);
    }

    private void HandleAiming()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3 dir = (mouseWorld - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void HandleShooting()
    {
        fireCooldown -= Time.deltaTime;

        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            fireCooldown = fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3 dir = (mouseWorld - transform.position).normalized;

        var bulletGO = new GameObject("Bullet");
        bulletGO.transform.position = transform.position + dir * 0.5f;
        bulletGO.layer = gameObject.layer;

        var sr2 = bulletGO.AddComponent<SpriteRenderer>();
        sr2.sprite = PixelArtGenerator.BulletSprite;
        sr2.sortingOrder = 4;

        var col = bulletGO.AddComponent<CircleCollider2D>();
        col.radius = 0.1f;
        col.isTrigger = true;

        var rb = bulletGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var bullet = bulletGO.AddComponent<DemoBullet>();
        bullet.Init(dir);
    }

    private void HandleTimers()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
                sr.color = originalColor;
        }
    }

    public void TakeDamage(int damage)
    {
        if (invincibilityTimer > 0f || ScoreManager.IsGameOver) return;

        currentHealth -= damage;
        invincibilityTimer = 0.5f;

        // Flash red
        sr.color = Color.red;
        flashTimer = 0.2f;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            ScoreManager.GameOver();
        }
    }
}
