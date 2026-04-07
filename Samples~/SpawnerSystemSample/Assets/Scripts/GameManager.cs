using UnityEngine;
using UnityEngine.SceneManagement;
using GameplayMechanicsUMFOSS.World;

public class DemoGameManager : MonoBehaviour
{
    private WaveSpawner_UMFOSS waveSpawner;

    private void Start()
    {
        ScoreManager.Reset();

        // Auto-start wave spawner
        waveSpawner = FindObjectOfType<WaveSpawner_UMFOSS>();
        if (waveSpawner != null)
            waveSpawner.StartWaves();

        // Subscribe to wave clear bonus
        EventBus.Subscribe<OnWaveCleared>(OnWaveCleared);

        // Create arena visuals
        CreateArena();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnWaveCleared>(OnWaveCleared);
    }

    private void OnWaveCleared(OnWaveCleared e)
    {
        ScoreManager.AddScore(100); // Wave clear bonus
    }

    private void Update()
    {
        if (ScoreManager.IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            EventBus.Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void CreateArena()
    {
        float arenaW = 20f;
        float arenaH = 12f;

        // Starfield background — random stars
        CreateStarfield(arenaW, arenaH, 120);

        // Nebula glow spots — subtler for darker feel
        CreateNebula(new Vector3(-5f, 2f, 2f), new Color(0.06f, 0.02f, 0.12f, 0.25f), 4f);
        CreateNebula(new Vector3(4f, -1f, 2f), new Color(0.02f, 0.05f, 0.12f, 0.2f), 3.5f);
        CreateNebula(new Vector3(7f, 3f, 2f), new Color(0.08f, 0.01f, 0.04f, 0.2f), 3f);

        // Arena border — glowing energy walls
        var borderColor = new Color(0.3f, 0.5f, 1f, 0.8f);
        float bw = 0.06f;
        CreateLine("BorderTop", new Vector3(-arenaW / 2f, arenaH / 2f, 0.5f),
            new Vector3(arenaW / 2f, arenaH / 2f, 0.5f), borderColor, bw);
        CreateLine("BorderBottom", new Vector3(-arenaW / 2f, -arenaH / 2f, 0.5f),
            new Vector3(arenaW / 2f, -arenaH / 2f, 0.5f), borderColor, bw);
        CreateLine("BorderLeft", new Vector3(-arenaW / 2f, -arenaH / 2f, 0.5f),
            new Vector3(-arenaW / 2f, arenaH / 2f, 0.5f), borderColor, bw);
        CreateLine("BorderRight", new Vector3(arenaW / 2f, -arenaH / 2f, 0.5f),
            new Vector3(arenaW / 2f, arenaH / 2f, 0.5f), borderColor, bw);

        // Zone divider lines (subtle)
        var divColor = new Color(0.2f, 0.3f, 0.6f, 0.15f);
        CreateLine("Div1", new Vector3(-3f, -arenaH / 2f, 0.8f),
            new Vector3(-3f, arenaH / 2f, 0.8f), divColor, 0.03f);
        CreateLine("Div2", new Vector3(3f, -arenaH / 2f, 0.8f),
            new Vector3(3f, arenaH / 2f, 0.8f), divColor, 0.03f);

        // Zone labels
        CreateZoneLabel("WAVE ZONE", new Vector3(-6f, 5.5f, 0f), new Color(0.4f, 0.4f, 1f, 0.25f));
        CreateZoneLabel("TIMED ZONE", new Vector3(0f, 5.5f, 0f), new Color(0.3f, 1f, 0.5f, 0.25f));
        CreateZoneLabel("PROXIMITY", new Vector3(6f, 5.5f, 0f), new Color(1f, 0.4f, 0.3f, 0.25f));
    }

    private void CreateStarfield(float w, float h, int count)
    {
        var starParent = new GameObject("Starfield");

        for (int i = 0; i < count; i++)
        {
            var starGO = new GameObject($"Star_{i}");
            starGO.transform.parent = starParent.transform;
            starGO.transform.position = new Vector3(
                Random.Range(-w / 2f, w / 2f),
                Random.Range(-h / 2f, h / 2f), 2f);

            var sr = starGO.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -10;

            // Vary star sizes and brightness
            float brightness = Random.Range(0.15f, 0.7f);
            float size = Random.Range(0.02f, 0.06f);

            // Some stars have a slight color tint
            float r = brightness * Random.Range(0.8f, 1f);
            float g = brightness * Random.Range(0.8f, 1f);
            float b = brightness * Random.Range(0.9f, 1f);

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var col = new Color(r, g, b);
            tex.SetPixel(0, 0, col); tex.SetPixel(1, 0, col);
            tex.SetPixel(0, 1, col); tex.SetPixel(1, 1, col);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 16);
            starGO.transform.localScale = Vector3.one * size * 10f;
        }
    }

    private void CreateNebula(Vector3 pos, Color color, float radius)
    {
        var go = new GameObject("Nebula");
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -9;

        // Create soft circular gradient
        int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                float alpha = Mathf.Clamp01(1f - dist) * color.a;
                alpha *= alpha; // softer falloff
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / (radius * 2f));
    }

    private void CreateLine(string name, Vector3 start, Vector3 end, Color color, float width)
    {
        var go = new GameObject(name);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = -1;
    }

    private void CreateZoneLabel(string text, Vector3 pos, Color color)
    {
        var go = new GameObject($"Label_{text}");
        go.transform.position = pos;
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 24;
        tm.characterSize = 0.15f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = color;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sortingOrder = -1;
    }
}
