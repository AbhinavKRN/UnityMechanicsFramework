using UnityEngine;
using GameplayMechanicsUMFOSS.World;

public class DemoUI : MonoBehaviour
{
    [SerializeField] private WaveSpawner_UMFOSS waveSpawner;
    [SerializeField] private TimedSpawner_UMFOSS timedSpawner;
    [SerializeField] private ProximitySpawner_UMFOSS proximitySpawner;

    private DemoPlayer player;
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;
    private GUIStyle scoreStyle;
    private GUIStyle statusStyle;
    private Texture2D healthBarBg;
    private Texture2D healthBarFill;
    private Texture2D overlayTex;
    private bool stylesInitialized = false;

    private void Start()
    {
        player = FindObjectOfType<DemoPlayer>();

        healthBarBg = new Texture2D(1, 1);
        healthBarBg.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 0.8f));
        healthBarBg.Apply();

        healthBarFill = new Texture2D(1, 1);
        healthBarFill.SetPixel(0, 0, new Color(0.9f, 0.1f, 0.1f, 0.9f));
        healthBarFill.Apply();

        overlayTex = new Texture2D(1, 1);
        overlayTex.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        overlayTex.Apply();
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;
        stylesInitialized = true;

        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold, fontSize = 14, normal = { textColor = Color.white }
        };
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12, normal = { textColor = Color.white }
        };
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold, fontSize = 36,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.red }
        };
        scoreStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold, fontSize = 18,
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = Color.yellow }
        };
        statusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10, normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
        };
    }

    private void OnGUI()
    {
        InitStyles();

        DrawHealthBar();
        DrawScore();
        DrawWaveInfo();
        DrawSpawnerStatus();

        if (ScoreManager.IsGameOver)
            DrawGameOver();
    }

    private void DrawHealthBar()
    {
        if (player == null) return;

        float barWidth = 200f;
        float barHeight = 20f;
        float x = 10f;
        float y = 10f;

        GUI.DrawTexture(new Rect(x, y, barWidth, barHeight), healthBarBg);
        float fillWidth = barWidth * ((float)player.CurrentHealth / player.MaxHealth);
        GUI.DrawTexture(new Rect(x, y, fillWidth, barHeight), healthBarFill);
        GUI.Label(new Rect(x + 5, y, barWidth, barHeight),
            $"HP: {player.CurrentHealth}/{player.MaxHealth}", labelStyle);
    }

    private void DrawScore()
    {
        GUI.Label(new Rect(Screen.width - 210, 10, 200, 30),
            $"Score: {ScoreManager.Score}", scoreStyle);
    }

    private void DrawWaveInfo()
    {
        if (waveSpawner == null) return;

        float centerX = Screen.width / 2f - 100f;

        if (waveSpawner.IsWaveActive)
        {
            GUI.Label(new Rect(centerX, 10, 200, 25),
                $"WAVE {waveSpawner.CurrentWave + 1} / {waveSpawner.TotalWaves}", headerStyle);
            GUI.Label(new Rect(centerX, 30, 200, 20),
                $"Enemies Remaining: {waveSpawner.ActiveCount}", labelStyle);
        }
        else if (waveSpawner.CurrentWave + 1 < waveSpawner.TotalWaves)
        {
            // Between waves — show countdown feel
            var waveIncomingStyle = new GUIStyle(headerStyle)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.8f, 0.2f) }
            };
            GUI.Label(new Rect(0, 10, Screen.width, 30),
                $"WAVE {waveSpawner.CurrentWave + 2} INCOMING...", waveIncomingStyle);
        }
        else if (waveSpawner.CurrentWave + 1 >= waveSpawner.TotalWaves && waveSpawner.ActiveCount == 0)
        {
            var clearStyle = new GUIStyle(headerStyle)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.3f, 1f, 0.4f) }
            };
            GUI.Label(new Rect(0, 10, Screen.width, 30),
                "ALL WAVES CLEARED!", clearStyle);
        }
    }

    private void DrawSpawnerStatus()
    {
        float y = Screen.height - 30f;
        string waveStatus = waveSpawner != null && waveSpawner.IsWaveActive ? "Active" : "Idle";
        string timedStatus = timedSpawner != null && timedSpawner.IsSpawning ? "Running" : "Stopped";
        string proxStatus = proximitySpawner != null
            ? (proximitySpawner.HasFired() ? "Fired" : "Armed") : "N/A";

        GUI.Label(new Rect(10, y, 500, 20),
            $"Wave: {waveStatus}  |  Timed: {timedStatus}  |  Proximity: {proxStatus}",
            statusStyle);
    }

    private void DrawGameOver()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTex);

        float centerY = Screen.height / 2f;
        GUI.Label(new Rect(0, centerY - 60, Screen.width, 50), "GAME OVER", titleStyle);

        var finalScoreStyle = new GUIStyle(scoreStyle)
        {
            alignment = TextAnchor.MiddleCenter, fontSize = 24
        };
        GUI.Label(new Rect(0, centerY, Screen.width, 40),
            $"Final Score: {ScoreManager.Score}", finalScoreStyle);

        var restartStyle = new GUIStyle(labelStyle)
        {
            alignment = TextAnchor.MiddleCenter, fontSize = 16
        };
        GUI.Label(new Rect(0, centerY + 50, Screen.width, 30),
            "Press R to Restart", restartStyle);
    }
}
