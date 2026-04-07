using UnityEngine;
using GameplayMechanicsUMFOSS.World;

public enum EnemyType { Grunt, Rusher, Tank }

public class DemoEnemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 1f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private int scoreValue = 10;

    [Header("Visual")]
    [SerializeField] private EnemyType enemyType = EnemyType.Grunt;
    [SerializeField] private Color deathColor = Color.green;

    private float currentHealth;
    private SpriteRenderer sr;
    private Transform playerTransform;
    private float flashTimer = 0f;
    private Color originalColor;
    private bool isDead = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;
        flashTimer = 0f;

        var player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Assign sprite from PixelArtGenerator at runtime
        PixelArtGenerator.Init();
        if (sr != null)
        {
            sr.sprite = enemyType switch
            {
                EnemyType.Grunt => PixelArtGenerator.GruntSprite,
                EnemyType.Rusher => PixelArtGenerator.RusherSprite,
                EnemyType.Tank => PixelArtGenerator.TankSprite,
                _ => PixelArtGenerator.GruntSprite
            };
            sr.sortingOrder = 2;
            originalColor = Color.white;
            sr.color = originalColor;
        }
    }

    private void Update()
    {
        if (isDead || ScoreManager.IsGameOver) return;

        // Chase player
        if (playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, playerTransform.position,
                moveSpeed * Time.deltaTime);
        }

        // Flash timer
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && sr != null)
                sr.color = originalColor;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.gameObject.GetComponent<DemoPlayer>();
        if (player != null)
        {
            player.TakeDamage(contactDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Flash white
        if (sr != null)
        {
            sr.color = Color.white;
            flashTimer = 0.1f;
        }

        if (currentHealth <= 0f)
            Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        ScoreManager.AddScore(scoreValue);
        SpawnDeathParticles();

        EventBus.Publish(new OnSpawnedObjectDied
        {
            obj = gameObject,
            waveNumber = 0,
            remainingCount = 0
        });

        gameObject.SetActive(false);
    }

    private void SpawnDeathParticles()
    {
        int count = Random.Range(4, 6);
        for (int i = 0; i < count; i++)
        {
            var particleGO = new GameObject("DeathParticle");
            particleGO.transform.position = transform.position;
            var particle = particleGO.AddComponent<DeathParticle>();
            Vector3 dir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f), 0f);
            particle.Init(deathColor, dir, Random.Range(2f, 5f));
        }
    }
}
