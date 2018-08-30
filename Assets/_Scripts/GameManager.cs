using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;   

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    public int PlayerTurn = 0;
    [SyncVar]
    public int PlayerCount = 0;
    [SyncVar(hook = "UpdateDiscardPileCard")]
    public int DiscardPileCardValue;
    [SyncVar]
    public int SuitOverride;
    [SyncVar]
    public int Player0CardsLeft = 0;
    [SyncVar]
    public int Player1CardsLeft = 0;
    [SyncVar]
    public int Player2CardsLeft = 0;
    [SyncVar]
    public int Player3CardsLeft = 0;
    [SyncVar]
    public int GameState = 0;
    [SyncVar(hook = "UpdateCanDraw")]
    public int AvailableDrawCount = 1;

    public int ExpectedPlayerCount;

    public Card DiscardPileCard;

    public SpriteRenderer DrawPileSpriteRenderer;

    public Sprite[] DrawPileSprites;

    public List<string> PlayerNames;

    public bool Clockwise = true;

    public bool Draw2 = false;

    public Text ButtonText;

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

            ButtonText.text = playerList.Count.ToString();
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

        PlayerCount = playerList.Count;

        string newName = player.name;

        PlayerNames.Add(newName);

        int playerNamesLength = PlayerNames.Count;

        player.MyGm = this;
    }

    // Linked to SyncVar, updates discharge pile card sprite
    void UpdateDiscardPileCard(int DiscardPileCardValue)
    {
        DiscardPileCard.SetValue(DiscardPileCardValue);
    }

    // Linked to SyncVar, updates if player can draw a card or not
    void UpdateCanDraw(int AvailableDrawCount)
    {
        if (AvailableDrawCount <= 0)
        {
            foreach (Player player in playerList)
            {
                player.RpcDenyCanDraw();
            }
        }
        else
        {
            foreach (Player player in playerList)
            {
                player.RpcAllowCanDraw();
            }
        }
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
            for (int j = 0; j < PlayerCount; j++)
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

        GameState = 0;

        if (CheckJack(cardValue))
        {
            GameState = 11;

            playerList[PlayerTurn].TakeAction(GameState);

            DiscardPileCardValue = cardValue;
            discardPile.Add(cardValue);

            switch (PlayerTurn)
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
            GameState = CheckSpecial(cardValue);
        }

        if (CheckRank(cardValue))
        {
            GameState = CheckSpecial(cardValue);
        }

        if (GameState>0)
        {
            playerList[PlayerTurn].TakeAction(GameState);

            DiscardPileCardValue = cardValue;
            discardPile.Add(cardValue);

            switch (PlayerTurn)
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
            playerList[PlayerTurn].TakeAction(GameState);
        }
    }

    // Function returns true if card suit matches discard pile card suit
    private bool CheckSuit(int cardValue)
    {
        bool matchedSuit = false;

        int discardPileCardSuit;

        int cardSuit = cardValue / 100;

        if (SuitOverride == 0)
        {
            discardPileCardSuit = DiscardPileCardValue / 100; 
        }
        else
        {
            discardPileCardSuit = SuitOverride;
        }

        if(cardSuit == discardPileCardSuit)
        {
            matchedSuit = true;
            SuitOverride = 0;
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
        GameState = 1;

        if ((cardValue % 100) == 7)
        {
            GameState = 7;
        }
        else if ((cardValue % 100) == 8)
        {
            GameState = 8;
        }
        else if ((cardValue % 100) == 4 && PlayerCount > 2)
        {
            GameState = 4;
            Clockwise = !Clockwise;
        }

        return GameState;
    }

    //Actions at end of turn
    public void ChangePlayerTurn()
    {
        if (isServer)
        {

            if (Clockwise)
            {
                if (GameState!=8)
                {
                    if (PlayerTurn < PlayerCount - 1)
                    {
                        PlayerTurn++;
                    }
                    else PlayerTurn = 0;  
                }
                else
                {
                    if (PlayerTurn < PlayerCount - 2)
                    {
                        PlayerTurn = PlayerTurn + 2;
                    }
                    else if (PlayerTurn == PlayerCount - 2)
                    {
                        PlayerTurn = 0;
                    }
                    else if (PlayerTurn == PlayerCount - 1)
                    {
                        PlayerTurn = 1;
                    }
                }
            }
            else
            {
                if (GameState!=8)
                {
                    if (PlayerTurn > 0)
                    {
                        PlayerTurn--;
                    }
                    else PlayerTurn = PlayerCount - 1; 
                }
                else
                {
                    if (PlayerTurn > 1)
                    {
                        PlayerTurn = PlayerTurn - 2;
                    }
                    else if (PlayerTurn == 1)
                    {
                        PlayerTurn = PlayerCount - 1;
                    }
                    else if (PlayerTurn == 0)
                    {
                        PlayerTurn = PlayerCount - 2;
                    }
                }
            }

            if (GameState == 7)
            {
                playerList[PlayerTurn].RpcLockDown();
                AvailableDrawCount = 2;
                Draw2 = true;
            }
            else
            {
                GameState = 0;
                AvailableDrawCount = 1;
                playerList[PlayerTurn].RpcAllowCanDraw();
            }
        }
    }

    //Called when player draws a card
    public void DrawCard()
    {
        if (isServer)
        {
            if (AvailableDrawCount <= 0)
            {
                return;
            }

            int rNum = Random.Range(0, drawPile.Count);

            int card = drawPile[rNum];

            drawPile.RemoveAt(rNum);

            if (drawPile.Count <= 1)
            {
                DrawPileSpriteRenderer.sprite = DrawPileSprites[1];
            }

            // set player turn so only that player can recive this card
            playerList[PlayerTurn].AddCard(card);

            // Add cards to reference to each players hand
            switch (PlayerTurn)
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

            AvailableDrawCount--;

            if (Draw2 && AvailableDrawCount == 0)
            {
                Draw2 = false;
                AvailableDrawCount = 1;
                GameState = 0;
                playerList[PlayerTurn].RpcUnlock();

                ChangePlayerTurn();
            }
        }
    }

    public void ChooseSuit()
    {
        

    }
}