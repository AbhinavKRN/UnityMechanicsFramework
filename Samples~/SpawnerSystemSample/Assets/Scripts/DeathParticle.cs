using UnityEngine;

public class DeathParticle : MonoBehaviour
{
    private Vector3 velocity;
    private float lifetime = 0.5f;
    private float elapsed = 0f;
    private SpriteRenderer sr;

    public void Init(Color color, Vector3 direction, float speed)
    {
        velocity = direction.normalized * speed;

        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
        }
        sr.sprite = PixelArtGenerator.CreateDeathParticle(color);
        sr.color = color;
        elapsed = 0f;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += velocity * Time.deltaTime;
        float alpha = 1f - (elapsed / lifetime);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
    }
}
