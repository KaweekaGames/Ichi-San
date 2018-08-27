using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;   

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    public int playerTurn = 0;
    [SyncVar]
    public int playerCount = 0;
    [SyncVar(hook = "UpdateDiscardPileCard")]
    public int DiscardPileCardValue;
    [SyncVar]
    public int SuitOverride;
    [SyncVar]
    public int player0CardsLeft = 0;
    [SyncVar]
    public int player1CardsLeft = 0;
    [SyncVar]
    public int player2CardsLeft = 0;
    [SyncVar]
    public int player3CardsLeft = 0;

    public int ExpectedPlayerCount;

    public Card DiscardPileCard;

    public SpriteRenderer DrawPileSpriteRenderer;

    public Sprite[] DrawPileSprites;

    public List<string> playerNames;

    List<Player> playerList;

    List<Player> localPlayerList;

    [SerializeField]
    List<int> standardDeck;

    List<int> discardPile;

    List<int> drawPile;

    List<int> player0Hand;
    List<int> player1Hand;
    List<int> player2Hand;
    List<int> player3Hand;

    List<int>[] playerHands;

    bool handDealt = false;

    public Text buttonText;

    private void Start()
    {
        if (!isServer)
        {
            return;
        }

        NetworkLobbyManager lobbyManager = FindObjectOfType<NetworkLobbyManager>();

        ExpectedPlayerCount = lobbyManager.numPlayers;

        // Initialize lists
        player0Hand = new List<int>();
        player1Hand = new List<int>();
        player2Hand = new List<int>();
        player3Hand = new List<int>();

        // Initialize lists
        playerList = new List<Player>();
        discardPile = new List<int>();
        drawPile = new List<int>();

    }

    void Update()
    {
        if (!isServer)
        { 
            return;
        }

        //if (handDealt && drawPile != null && drawPile.Count < 1)
        //{
        //    RefreshDrawPile();
        //}

        if (playerList.Count < ExpectedPlayerCount)
        {
            GameObject[] newPlayers = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject gO in newPlayers)
            {
                Player newPlayerScript = gO.GetComponent<Player>();

                if (!playerList.Contains(newPlayerScript))
                {
                    AddPlayer(gO);
                }
            }

            buttonText.text = playerList.Count.ToString();
        }
    }

    //temp button start
    public void startRound()
    {
        if (isServer)
        {
            DealCards(standardDeck); 
        }
    }

    // Add player to list of players and assign their turn number (MyInt)
    public void AddPlayer(GameObject newPlayer)
    {
        if (!isServer)
        {
            return;
        }

        Player player = newPlayer.GetComponent<Player>();

        int nextPlayerNumber = playerList.Count;

        playerList.Add(player);

        player.RpcGetNumber(nextPlayerNumber);

        playerCount = playerList.Count;

        string newName = player.name;

        playerNames.Add(newName);

        int playerNamesLength = playerNames.Count;

        player.MyGm = this;
    }

    // Linked to Syncar, updates discharge pile card sprite
    void UpdateDiscardPileCard(int DiscardPileCardValue)
    {
        DiscardPileCard.SetValue(DiscardPileCardValue);
    }


    // *****
    // Deck Management Functions
    // *****

    // Function randomizes deck
    private List<int> ShuffleDeck(List<int> cardDeck)
    {
        List<int> tmpDeck = new List<int>();

        List<int> shuffledDeck = new List<int>();

        tmpDeck = cardDeck;

        for (int i = 0; i < 2; i++)
        {
            shuffledDeck.Clear();

            while (tmpDeck.Count > 0)
            {
                int rNum = Random.Range(0, tmpDeck.Count);

                shuffledDeck.Add(tmpDeck[rNum]);

                tmpDeck.RemoveAt(rNum);

            }

            //tmpDeck = shuffledDeck;

            foreach (int num in shuffledDeck)
            {
                tmpDeck.Add(num);
            }
        }

        return shuffledDeck;
    }

    // Function replinishes draw pile if it runs out
    private void RefreshDrawPile()
    {
        discardPile.Remove(DiscardPileCardValue);

        drawPile = ShuffleDeck(discardPile);

        DrawPileSpriteRenderer.sprite = DrawPileSprites[0];

        discardPile.Clear();

        discardPile.Add(DiscardPileCardValue);
    }

    // Function deals cards to players to start round
    private void DealCards(List<int> cardDeck)
    {
        

        // Generate draw pile from a fresh deck
        drawPile = ShuffleDeck(cardDeck);

        // Deal 7 cards to each player
        for (int i = 0; i < 7; i++)
        {
            // Switch from player to player after each card dealt from deck
            for (int j = 0; j < playerCount; j++)
            {
                int rNum = Random.Range(0, drawPile.Count);

                int card = drawPile[rNum];

                drawPile.RemoveAt(rNum);

                // set player turn so only that player can recive this card
                playerList[j].AddCard(card);

                // Add cards to reference to each players hand
                switch (j)
                {
                    case 0:
                        player0Hand.Add(card);
                        break;
                    case 1:
                        player1Hand.Add(card);
                        break;
                    case 2:
                        player2Hand.Add(card);
                        break;
                    case 3:
                        player3Hand.Add(card);
                        break;
                    default:
                        break;
                }
            }
        }

        DiscardPileCardValue = drawPile[drawPile.Count - 1];

        discardPile.Add(DiscardPileCardValue);

        drawPile.RemoveAt(drawPile.Count - 1);

        handDealt = true;
    }


    // *****
    // These functions are called when a player attempts to play a card
    // *****

    // Function returns true if card is a played leagally
    public void CheckCard(int cardValue)
    {
        if (!isServer)
        {
            return;
        }

        int actionNumber = 0;

        if (CheckJack(cardValue))
        {
            actionNumber = 11;

            playerList[playerTurn].TakeAction(actionNumber);

            DiscardPileCardValue = cardValue;
            discardPile.Add(cardValue);

            switch (playerTurn)
            {
                case 0:
                    player0Hand.Remove(cardValue);
                    break;
                case 1:
                    player1Hand.Remove(cardValue);
                    break;
                case 2:
                    player2Hand.Remove(cardValue);
                    break;
                case 3:
                    player3Hand.Remove(cardValue);
                    break;
                default:
                    break;
            }

            ChangePlayerTurn();

            return;
        }

        if (CheckSuit(cardValue))
        {
            actionNumber = CheckSpecial(cardValue);
        }

        if (CheckRank(cardValue))
        {
            actionNumber = CheckSpecial(cardValue);
        }

        if (actionNumber>0)
        {
            playerList[playerTurn].TakeAction(actionNumber);

            DiscardPileCardValue = cardValue;
            discardPile.Add(cardValue);

            switch (playerTurn)
            {
                case 0:
                    player0Hand.Remove(cardValue);
                    break;
                case 1:
                    player1Hand.Remove(cardValue);
                    break;
                case 2:
                    player2Hand.Remove(cardValue);
                    break;
                case 3:
                    player3Hand.Remove(cardValue);
                    break;
                default:
                    break;
            }

            ChangePlayerTurn();
        }
        else
        {
            playerList[playerTurn].TakeAction(actionNumber);
        }
    }

    // Function returns true if card suit matches discard pile card suit
    private bool CheckSuit(int cardValue)
    {
        bool matchedSuit = false;

        int cardSuit = cardValue / 100;

        int discardPileCardSuit = DiscardPileCardValue / 100;

        if(cardSuit == discardPileCardSuit)
        {
            matchedSuit = true;
        }

        return matchedSuit;
    }

    // Function returns true if card rank matches discard pile card rank
    private bool CheckRank(int cardValue)
    {
        bool matchedRank = false;

        int cardRank = cardValue % 100;

        int discardPileCardRank = DiscardPileCardValue % 100;

        if (cardRank == discardPileCardRank)
        {
            matchedRank = true;
        }

        return matchedRank;
    }

    // Function returns true if Jack has been plade
    private bool CheckJack(int cardValue)
    {
        bool isJack = false;

        isJack = ((cardValue % 100) == 11);

        return isJack;
    }

    // Function to see if a 7 or 8 has been played
    // If true calls for special actions to be performed 
    private int CheckSpecial(int cardValue)
    {
        int actionNumber = 1;

        if ((cardValue % 100) == 7)
        {
            actionNumber = 7;
        }
        else if ((cardValue % 100) == 8)
        {
            actionNumber = 8;
        }

        return actionNumber;
    }

    ////temp
    public void ChangePlayerTurn()
    {
        if (isServer)
        {
            if (playerTurn < playerList.Count - 1)
            {
                playerTurn++;
            }
            else playerTurn = 0;
        }
    }

    public void DrawCard()
    {
        if (isServer)
        {
            int rNum = Random.Range(0, drawPile.Count);

            int card = drawPile[rNum];

            drawPile.RemoveAt(rNum);

            if (drawPile.Count <= 1)
            {
                DrawPileSpriteRenderer.sprite = DrawPileSprites[1];
            }

            // set player turn so only that player can recive this card
            playerList[playerTurn].AddCard(card);

            // Add cards to reference to each players hand
            switch (playerTurn)
            {
                case 0:
                    player0Hand.Add(card);
                    break;
                case 1:
                    player1Hand.Add(card);
                    break;
                case 2:
                    player2Hand.Add(card);
                    break;
                case 3:
                    player3Hand.Add(card);
                    break;
                default:
                    break; 
            }
        }
    }
}