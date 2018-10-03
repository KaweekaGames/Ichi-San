using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DataCollector : MonoBehaviour
{
    public bool LoadSavedGame = false;

    List<int> playerScores;

    List<string> playerNames;

    int playerCount;
    
    
    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        string uniqueID = SystemInfo.deviceUniqueIdentifier;

    }

    public void LoadLobby()
    {
        SceneManager.LoadScene(1);
    }

    public void SetPlayers(int playerCount, List<int> scores, List<string> names)
    {
        for (int i = 0; i < scores.Count; i++)
        {
            PlayerPrefs.SetInt("score" + i.ToString(), scores[i]);
        }

        for (int j = 0; j < names.Count; j++)
        {
            PlayerPrefs.SetString("name" + j.ToString(), names[j]);
        }

        PlayerPrefs.SetInt("PlayerCount", playerCount);
    }

    public List<string> LoadNames()
    {
        playerNames = new List<string>();

        for (int i = 0; i < playerCount; i++)
        {
            playerNames.Add(PlayerPrefs.GetString("name" + i.ToString()));
        }

        return playerNames;
    }

    public List<int> LoadScores()
    {
        playerScores = new List<int>();

        for (int i = 0; i < playerCount; i++)
        {
            playerScores.Add(PlayerPrefs.GetInt("score" + i.ToString()));
        }

        return playerScores;
    }
}
