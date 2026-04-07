using UnityEngine;

public static class PixelArtGenerator
{
    private static bool initialized = false;
    public static Sprite PlayerSprite { get; private set; }
    public static Sprite GruntSprite { get; private set; }
    public static Sprite RusherSprite { get; private set; }
    public static Sprite TankSprite { get; private set; }
    public static Sprite BulletSprite { get; private set; }

    public static void Init()
    {
        if (initialized) return;
        initialized = true;

        PlayerSprite = CreatePlayer();
        GruntSprite = CreateGrunt();
        RusherSprite = CreateRusher();
        TankSprite = CreateTank();
        BulletSprite = CreateBullet();
    }

    public static Sprite CreateDeathParticle(Color color)
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, color); tex.SetPixel(1, 0, color);
        tex.SetPixel(0, 1, color); tex.SetPixel(1, 1, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 8);
    }

    private static Sprite CreatePlayer()
    {
        var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var clear = new Color(0, 0, 0, 0);
        var body = new Color(0.25f, 0.55f, 1f);       // bright blue
        var dark = new Color(0.1f, 0.25f, 0.6f);      // dark blue outline
        var light = new Color(0.5f, 0.75f, 1f);       // highlight
        var visor = new Color(0.1f, 1f, 0.9f);        // cyan visor
        var jet = new Color(1f, 0.5f, 0.1f);          // orange thruster
        var gun = new Color(0.7f, 0.7f, 0.75f);       // gun metal
        var muzzle = new Color(1f, 0.9f, 0.3f);       // muzzle flash

        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                tex.SetPixel(x, y, clear);

        // Spaceship body — pointed nose shape
        // Nose (right side, since ship faces right)
        tex.SetPixel(14, 7, light); tex.SetPixel(14, 8, light);
        tex.SetPixel(13, 6, body); tex.SetPixel(13, 7, body);
        tex.SetPixel(13, 8, body); tex.SetPixel(13, 9, body);

        // Main hull
        for (int x = 5; x < 13; x++)
            for (int y = 5; y < 11; y++)
                tex.SetPixel(x, y, body);

        // Wider center body
        for (int x = 4; x < 13; x++)
            for (int y = 6; y < 10; y++)
                tex.SetPixel(x, y, body);

        // Dark outline
        for (int x = 5; x < 13; x++) { tex.SetPixel(x, 4, dark); tex.SetPixel(x, 11, dark); }
        tex.SetPixel(4, 5, dark); tex.SetPixel(4, 10, dark);
        for (int y = 5; y < 11; y++) tex.SetPixel(3, y, dark);

        // Highlight stripe
        for (int x = 6; x < 13; x++) tex.SetPixel(x, 9, light);

        // Cyan visor/cockpit
        tex.SetPixel(11, 7, visor); tex.SetPixel(11, 8, visor);
        tex.SetPixel(12, 7, visor); tex.SetPixel(12, 8, visor);

        // Wings (top and bottom)
        for (int x = 3; x < 8; x++) { tex.SetPixel(x, 3, dark); tex.SetPixel(x, 12, dark); }
        for (int x = 4; x < 7; x++) { tex.SetPixel(x, 2, dark); tex.SetPixel(x, 13, dark); }
        tex.SetPixel(4, 3, body); tex.SetPixel(5, 3, body); tex.SetPixel(6, 3, body);
        tex.SetPixel(4, 12, body); tex.SetPixel(5, 12, body); tex.SetPixel(6, 12, body);

        // Thruster glow (left/back)
        tex.SetPixel(2, 7, jet); tex.SetPixel(2, 8, jet);
        tex.SetPixel(1, 7, new Color(1f, 0.3f, 0f, 0.7f));
        tex.SetPixel(1, 8, new Color(1f, 0.3f, 0f, 0.7f));

        // Gun barrels (right side)
        tex.SetPixel(15, 5, gun); tex.SetPixel(15, 10, gun);
        tex.SetPixel(14, 5, gun); tex.SetPixel(14, 10, gun);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
    }

    private static Sprite CreateGrunt()
    {
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var clear = new Color(0, 0, 0, 0);
        var body = new Color(0.2f, 0.8f, 0.2f);
        var dark = new Color(0.1f, 0.5f, 0.1f);
        var eye = Color.white;

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                tex.SetPixel(x, y, clear);

        for (int x = 1; x < 7; x++)
            for (int y = 1; y < 6; y++)
                tex.SetPixel(x, y, body);
        for (int x = 2; x < 6; x++) tex.SetPixel(x, 6, body);
        for (int x = 2; x < 6; x++) tex.SetPixel(x, 0, dark);

        tex.SetPixel(2, 4, eye); tex.SetPixel(5, 4, eye);
        tex.SetPixel(2, 3, dark); tex.SetPixel(5, 3, dark);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
    }

    private static Sprite CreateRusher()
    {
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var clear = new Color(0, 0, 0, 0);
        var body = new Color(1f, 0.6f, 0.1f);
        var dark = new Color(0.8f, 0.4f, 0f);
        var eye = Color.white;

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                tex.SetPixel(x, y, clear);

        tex.SetPixel(3, 0, dark); tex.SetPixel(4, 0, dark);
        for (int x = 2; x < 6; x++) tex.SetPixel(x, 1, body);
        for (int x = 1; x < 7; x++) tex.SetPixel(x, 2, body);
        for (int x = 1; x < 7; x++) tex.SetPixel(x, 3, body);
        for (int x = 0; x < 8; x++) tex.SetPixel(x, 4, body);
        for (int x = 0; x < 8; x++) tex.SetPixel(x, 5, body);
        for (int x = 1; x < 7; x++) tex.SetPixel(x, 6, dark);

        tex.SetPixel(2, 4, eye); tex.SetPixel(5, 4, eye);
        tex.SetPixel(2, 3, dark); tex.SetPixel(5, 3, dark);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
    }

    private static Sprite CreateTank()
    {
        var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var clear = new Color(0, 0, 0, 0);
        var body = new Color(0.9f, 0.15f, 0.15f);
        var dark = new Color(0.6f, 0.05f, 0.05f);
        var eye = Color.white;

        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                tex.SetPixel(x, y, clear);

        for (int x = 2; x < 14; x++)
            for (int y = 2; y < 14; y++)
                tex.SetPixel(x, y, body);

        for (int x = 2; x < 14; x++) { tex.SetPixel(x, 2, dark); tex.SetPixel(x, 13, dark); }
        for (int y = 2; y < 14; y++) { tex.SetPixel(2, y, dark); tex.SetPixel(13, y, dark); }

        tex.SetPixel(5, 10, eye); tex.SetPixel(6, 10, eye);
        tex.SetPixel(9, 10, eye); tex.SetPixel(10, 10, eye);
        tex.SetPixel(5, 9, dark); tex.SetPixel(6, 9, dark);
        tex.SetPixel(9, 9, dark); tex.SetPixel(10, 9, dark);

        for (int x = 5; x < 11; x++) tex.SetPixel(x, 5, dark);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
    }

    private static Sprite CreateBullet()
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var outer = new Color(1f, 0.9f, 0.2f);
        var center = Color.white;

        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                tex.SetPixel(x, y, outer);

        tex.SetPixel(1, 1, center); tex.SetPixel(2, 1, center);
        tex.SetPixel(1, 2, center); tex.SetPixel(2, 2, center);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 8);
    }
}
