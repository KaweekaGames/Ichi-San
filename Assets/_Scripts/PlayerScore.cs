using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScore
{
    private int score = 0;

    public int Score { get { return score; } }

    public void SetScore(int value)
    {
        score = value;
    }

    public void AddToScore(int value)
    {
        score += value;
    }
}
