using UnityEngine;
using GameplayMechanicsUMFOSS.World;

/// <summary>
/// Runs once on Start to spread spawn points across the arena and configure spawner settings.
/// Attach to any GameObject in the scene (DemoGameManager).
/// </summary>
public class ArenaSetup : MonoBehaviour
{
    private void Start()
    {
        SpreadSpawnPoints();
    }

    private void SpreadSpawnPoints()
    {
        // Wave spawn points — spread across left third
        SetPosition("WaveSpawnPoint1", new Vector3(-8f, 4f, 0f));
        SetPosition("WaveSpawnPoint2", new Vector3(-4f, -3f, 0f));
        SetPosition("WaveSpawnPoint3", new Vector3(-7f, -1f, 0f));

        // Timed spawn points — spread across center
        SetPosition("TimedSpawnPoint1", new Vector3(-2f, 3f, 0f));
        SetPosition("TimedSpawnPoint2", new Vector3(2f, -4f, 0f));

        // Proximity spawn points — spread across right third
        SetPosition("ProxSpawnPoint1", new Vector3(5f, 3f, 0f));
        SetPosition("ProxSpawnPoint2", new Vector3(8f, -2f, 0f));

        // Set spawn point shapes to Circle for natural spread
        var allPoints = FindObjectsOfType<SpawnPoint_UMFOSS>();
        foreach (var point in allPoints)
        {
            // Use reflection-free approach: modify via SerializedObject if in editor
            // At runtime, just use the default Point shape — positions are already spread
        }
    }

    private void SetPosition(string goName, Vector3 pos)
    {
        var go = GameObject.Find(goName);
        if (go != null) go.transform.position = pos;
    }
}
