using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class ScoreKeeper : NetworkBehaviour
{
    public TextMeshProUGUI[] PlayerName;
    public TextMeshProUGUI[] PlayerScore;

    public void SetScore(int index, int score)
    {
        PlayerScore[index].text = score.ToString();
    }

    public void SetName(int index, string name)
    {
        PlayerName[index].text = name;
    }
}
