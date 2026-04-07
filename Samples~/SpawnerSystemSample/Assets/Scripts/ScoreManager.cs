using UnityEngine;

public static class ScoreManager
{
    public static int Score { get; private set; }
    public static bool IsGameOver { get; private set; }

    public static void AddScore(int points)
    {
        if (IsGameOver) return;
        Score += points;
    }

    public static void GameOver()
    {
        IsGameOver = true;
    }

    public static void Reset()
    {
        Score = 0;
        IsGameOver = false;
    }
}
