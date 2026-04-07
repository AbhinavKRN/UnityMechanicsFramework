using UnityEngine;

public class DemoBullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 2f;

    private Vector3 direction;
    private float elapsed;
    private SpriteRenderer sr;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;
        elapsed = 0f;

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
        }
        sr.sprite = PixelArtGenerator.BulletSprite;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        if (ScoreManager.IsGameOver) return;

        elapsed += Time.deltaTime;
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<DemoEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}
