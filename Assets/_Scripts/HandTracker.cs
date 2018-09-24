using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class HandTracker : NetworkBehaviour
{
    public GameObject[] PlayerPanel;
    public TextMeshProUGUI[] PlayerName;
    public TextMeshProUGUI[] PlayerCards;

    public GameObject ScoreBoardPanel;
    public TextMeshProUGUI[] PlayerScoreBoardNames;
    public TextMeshProUGUI[] PlayerScores;

    GameObject playerHandArea;
    SpriteRenderer pHASpriteRenderer;

    public Player MyPlayer;

    Image panelImage0;
    Image panelImage1;
    Image panelImage2;
    Image panelImage3;

    [SerializeField]
    Color[] PlayerColors;

    int numberOfPlayers;
    List<Player> players;
    List<Player> tempList;
    bool sorted = false;
    bool initialized = false;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        players = new List<Player>();

        ScoreBoardPanel.SetActive(false);

        // Get Panel Sprites/Image reference for color change
        panelImage0 = PlayerPanel[0].GetComponent<Image>();
        panelImage1 = PlayerPanel[1].GetComponent<Image>();
        panelImage2 = PlayerPanel[2].GetComponent<Image>();
        panelImage3 = PlayerPanel[3].GetComponent<Image>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (!initialized && MyPlayer.GMReady == 1)
        {
            numberOfPlayers = MyPlayer.ReturnNumberofPlayers();
            ActivateUIObjects();
            players.Add(MyPlayer);
            PlayerScoreBoardNames[MyPlayer.MyInt].text = MyPlayer.MyName;

            playerHandArea = GameObject.FindGameObjectWithTag("HandArea");
            pHASpriteRenderer = playerHandArea.GetComponent<SpriteRenderer>();
            pHASpriteRenderer.color = PlayerColors[MyPlayer.MyInt];

            initialized = true;
        }

        if (initialized && players.Count < numberOfPlayers)
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject gameOb in gameObjects)
            {
                Player playerScript = gameOb.GetComponent<Player>();
                if (!players.Contains(playerScript))
                {
                    players.Add(playerScript);

                    int playerNum = playerScript.MyInt;
                    PlayerScoreBoardNames[playerNum].text = playerScript.MyName;
                }
            }
        }

        if (initialized && players.Count == numberOfPlayers && !sorted)
        {
            SortPlayers();

            sorted = true;
        }

        if (sorted && MyPlayer.MyHand.Count > 0)
        {
            UpdateHandCounts();
        }
    }

    void SortPlayers()
    {
        tempList = new List<Player>();

        for (int i = 0; i < players.Count - 1; i++)
        {
            foreach (Player plyr in players)
            {
                if (plyr.MyInt == i)
                {
                    tempList.Add(plyr);
                }
            }
        }

        players = tempList;
    }

    void UpdateHandCounts()
    {
        if (numberOfPlayers == 2)
        {
            if (MyPlayer.MyInt == 0)
            {
                PlayerName[0].text = players[1].MyName;
                int handCount = players[0].ReturnNumberofCards(1);
                PlayerCards[0].text = handCount.ToString();
                PlayerCards[0].color = PlayerColors[1];
                panelImage0.color = PlayerColors[1];
            }
            else
            {
                PlayerName[0].text = players[0].MyName;
                int handCount = players[1].ReturnNumberofCards(0);
                PlayerCards[0].text = handCount.ToString();
                PlayerCards[0].color = PlayerColors[0];
                panelImage0.color = PlayerColors[0];
            }
        }
        else if (numberOfPlayers == 3)
        {
            if (MyPlayer.MyInt == 0)
            {
                PlayerName[1].text = players[1].MyName;
                int handCount = players[0].ReturnNumberofCards(1);
                PlayerCards[1].text = handCount.ToString();
                PlayerCards[1].color = PlayerColors[1];
                panelImage1.color = PlayerColors[1];

                PlayerName[2].text = players[2].MyName;
                int handCount2 = players[0].ReturnNumberofCards(2);
                PlayerCards[2].text = handCount2.ToString();
                PlayerCards[2].color = PlayerColors[2];
                panelImage2.color = PlayerColors[2];
            }
            else if (MyPlayer.MyInt == 1)
            {
                PlayerName[1].text = players[2].MyName;
                int handCount = players[1].ReturnNumberofCards(2);
                PlayerCards[1].text = handCount.ToString();
                PlayerCards[1].color = PlayerColors[2];
                panelImage1.color = PlayerColors[2];

                PlayerName[2].text = players[0].MyName;
                int handCount2 = players[1].ReturnNumberofCards(0);
                PlayerCards[2].text = handCount2.ToString();
                PlayerCards[2].color = PlayerColors[0];
                panelImage2.color = PlayerColors[0];
            }
            else if (MyPlayer.MyInt == 2)
            {
                PlayerName[1].text = players[0].MyName;
                int handCount = players[2].ReturnNumberofCards(0);
                PlayerCards[1].text = handCount.ToString();
                PlayerCards[1].color = PlayerColors[0];
                panelImage1.color = PlayerColors[0];

                PlayerName[2].text = players[1].MyName;
                int handCount2 = players[2].ReturnNumberofCards(1);
                PlayerCards[2].text = handCount2.ToString();
                PlayerCards[2].color = PlayerColors[1];
                panelImage2.color = PlayerColors[1];
            }
        }
        else if (numberOfPlayers == 4)
        {
            if (MyPlayer.MyInt == 0)
            {
                PlayerName[0].text = players[2].MyName;
                int handCount = players[0].ReturnNumberofCards(2);
                PlayerCards[0].text = handCount.ToString();
                PlayerCards[0].color = PlayerColors[2];
                panelImage0.color = PlayerColors[2];

                PlayerName[1].text = players[1].MyName;
                int handCount1 = players[0].ReturnNumberofCards(1);
                PlayerCards[1].text = handCount1.ToString();
                PlayerCards[1].color = PlayerColors[1];
                panelImage1.color = PlayerColors[1];

                PlayerName[2].text = players[3].MyName;
                int handCount2 = players[0].ReturnNumberofCards(3);
                PlayerCards[2].text = handCount2.ToString();
                PlayerCards[2].color = PlayerColors[3];
                panelImage2.color = PlayerColors[3];
            }
            else if (MyPlayer.MyInt == 1)
            {
                PlayerName[0].text = players[3].MyName;
                int handCount = players[1].ReturnNumberofCards(3);
                PlayerCards[0].text = handCount.ToString();
                PlayerCards[0].color = PlayerColors[3];
                panelImage0.color = PlayerColors[3];

                PlayerName[1].text = players[2].MyName;
                int handCount1 = players[1].ReturnNumberofCards(2);
                PlayerCards[1].text = handCount1.ToString();
                PlayerCards[1].color = PlayerColors[2];
                panelImage1.color = PlayerColors[2];

                PlayerName[2].text = players[0].MyName;
                int handCount2 = players[1].ReturnNumberofCards(0);
                PlayerCards[2].text = handCount2.ToString();
                PlayerCards[2].color = PlayerColors[0];
                panelImage2.color = PlayerColors[0];
            }
            else if (MyPlayer.MyInt == 2)
            {
                PlayerName[0].text = players[0].MyName;
                int handCount = players[2].ReturnNumberofCards(0);
                PlayerCards[0].text = handCount.ToString();
                PlayerCards[0].color = PlayerColors[0];
                panelImage0.color = PlayerColors[0];

                PlayerName[1].text = players[3].MyName;
                int handCount1 = players[2].ReturnNumberofCards(3);
                PlayerCards[1].text = handCount1.ToString();
                PlayerCards[1].color = PlayerColors[3];
                panelImage1.color = PlayerColors[3];

                PlayerName[2].text = players[1].MyName;
                int handCount2 = players[2].ReturnNumberofCards(1);
                PlayerCards[2].text = handCount2.ToString();
                PlayerCards[2].color = PlayerColors[1];
                panelImage2.color = PlayerColors[1];
            }
            else if (MyPlayer.MyInt == 3)
            {
                PlayerName[0].text = players[1].MyName;
                int handCount = players[3].ReturnNumberofCards(1);
                PlayerCards[0].text = handCount.ToString();
                PlayerCards[0].color = PlayerColors[1];
                panelImage0.color = PlayerColors[1];

                PlayerName[1].text = players[0].MyName;
                int handCount1 = players[3].ReturnNumberofCards(0);
                PlayerCards[1].text = handCount1.ToString();
                PlayerCards[1].color = PlayerColors[0];
                panelImage1.color = PlayerColors[0];

                PlayerName[2].text = players[2].MyName;
                int handCount2 = players[3].ReturnNumberofCards(2);
                PlayerCards[2].text = handCount2.ToString();
                PlayerCards[2].color = PlayerColors[2];
                panelImage2.color = PlayerColors[2];
            }
        }
    }

    void ActivateUIObjects()
    {
        switch (numberOfPlayers)
        {
            case 2:
                PlayerPanel[0].SetActive(true);
                PlayerScoreBoardNames[0].enabled = true;
                PlayerScoreBoardNames[1].enabled = true;
                PlayerScores[0].enabled = true;
                PlayerScores[1].enabled = true;
                break;
            case 3:
                PlayerPanel[1].SetActive(true);
                PlayerPanel[2].SetActive(true);
                PlayerScoreBoardNames[0].enabled = true;
                PlayerScoreBoardNames[1].enabled = true;
                PlayerScoreBoardNames[2].enabled = true;
                PlayerScores[0].enabled = true;
                PlayerScores[1].enabled = true;
                PlayerScores[2].enabled = true;
                break;
            case 4:
                PlayerPanel[0].SetActive(true);
                PlayerPanel[1].SetActive(true);
                PlayerPanel[2].SetActive(true);
                PlayerScoreBoardNames[0].enabled = true;
                PlayerScoreBoardNames[1].enabled = true;
                PlayerScoreBoardNames[2].enabled = true;
                PlayerScoreBoardNames[3].enabled = true;
                PlayerScores[0].enabled = true;
                PlayerScores[1].enabled = true;
                PlayerScores[2].enabled = true;
                PlayerScores[3].enabled = true;
                break;
            default:
                break;
        }
    }

    public void SetScore(int index, int score)
    {
        int updatedScore = score;
        PlayerScores[index].text = score.ToString();
    }
}
