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

    public Player MyPlayer;

    [SerializeField]
    float openTimer = 2f;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (openTimer >0)
        {
            openTimer -= Time.deltaTime; 
        }


        if (!initialized && openTimer <= 0)
        {
            numberOfPlayers = MyPlayer.ReturnNumberofPlayers();
            players.Add(MyPlayer);

            foreach (GameObject gO in PlayerPanel)
            {
                gO.SetActive(false);
            }

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
            if (numberOfPlayers ==2)
            {
                if (MyPlayer.MyInt == 0)
                {
                    PlayerPanel[0].SetActive(true);
                    PlayerName[0].text = players[1].MyName;
                    int handCount = players[1].ReturnNumberofCards(1);
                    PlayerCards[0].text = handCount.ToString();
                }
                else
                {
                    PlayerPanel[0].SetActive(true);
                    PlayerName[0].text = players[0].MyName;
                    int handCount = players[0].ReturnNumberofCards(0);
                    PlayerCards[0].text = handCount.ToString();
                }
            }
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
}
