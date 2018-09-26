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
    [SyncVar]
    public int Clockwise = -1;
    [SyncVar]
    public int ReadyToStart = 0;

    PlayerScore Player0Score;
    PlayerScore Player1Score;
    PlayerScore Player2Score;
    PlayerScore Player3Score;

    public int ExpectedPlayerCount;

    public Card DiscardPileCard;

    public SpriteRenderer DrawPileSpriteRenderer;

    public Sprite[] DrawPileSprites;

    public bool Draw2 = false;

    // Remove this
    public Text ButtonText;

    DataCollector dataCollector;

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
    List<PlayerScore> playerScores;

    List<string> savedPlayerNames;
    List<int> savedPlayerScores;

    bool handDealt = false;
    bool firstCard = true;
    bool roundOver = false;

    int playerStart = 0;
    float startTimer;

    [SerializeField]
    float defaultStartTime;

    private void Start()
    {
        if (!isServer)
        {
            return;
        }

        // Find Network manager
        NetworkLobbyManager lobbyManager = FindObjectOfType<NetworkLobbyManager>();
        // Find how many players were in the Lobby
        ExpectedPlayerCount = lobbyManager.numPlayers;

        // Find data collector
        dataCollector = FindObjectOfType<DataCollector>();

        // Initialize player hand reference
        playerHands = new List<int>[4];
        playerScores = new List<PlayerScore>();

        player0Hand = new List<int>();
        playerHands[0] = player0Hand;
        Player0Score = new PlayerScore();
        playerScores.Add(Player0Score);

        player1Hand = new List<int>();
        playerHands[1] = player1Hand;
        Player1Score = new PlayerScore();
        playerScores.Add(Player1Score);

        player2Hand = new List<int>();
        playerHands[2] = player2Hand;
        Player2Score = new PlayerScore();
        playerScores.Add(Player2Score);

        player3Hand = new List<int>();
        playerHands[3] = player3Hand;
        Player3Score = new PlayerScore();
        playerScores.Add(Player3Score);

        // Initialize lists
        playerList = new List<Player>();
        discardPile = new List<int>();
        drawPile = new List<int>();

        // If loading a saved game update scores
        if (dataCollector.LoadSavedGame)
        {
            savedPlayerNames = dataCollector.LoadNames();
            savedPlayerScores = dataCollector.LoadScores();

            for (int i = 0; i < savedPlayerScores.Count; i++)
            {
                playerScores[i].SetScore(savedPlayerScores[i]);
            }
        }

        // Set start timer
        startTimer = defaultStartTime;

    }

    void Update()
    {
        if (!isServer)
        { 
            return;
        }

        if (playerList.Count < ExpectedPlayerCount)
        {
            GameObject[] newPlayers = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject gO in newPlayers)
            {
                Player newPlayerScript = gO.GetComponent<Player>();

                if (!playerList.Contains(newPlayerScript))
                {
                    AddPlayer(newPlayerScript);
                }
            }
        }
        else if (ReadyToStart == 0)
        {
            ReadyToStart = 1;

            foreach (Player player in playerList)
            {
                player.GMReady = 1;
            }

            playerList[PlayerTurn].RpcActivateDealButton();
        }

        if(ReadyToStart == 1 && handDealt)
        {
            // Keep playerXCardsLeft values updated
            switch(playerHands.Length)
            {
                case 2:
                    Player0CardsLeft = player0Hand.Count;
                    Player1CardsLeft = player1Hand.Count;
                    break;
                case 3:
                    Player0CardsLeft = player0Hand.Count;
                    Player1CardsLeft = player1Hand.Count;
                    Player2CardsLeft = player2Hand.Count;
                    break;
                case 4:
                    Player0CardsLeft = player0Hand.Count;
                    Player1CardsLeft = player1Hand.Count;
                    Player2CardsLeft = player2Hand.Count;
                    Player3CardsLeft = player3Hand.Count;
                    break;
                default:
                    break;
            }
        }
    }

    //temp button start
    public void StartRound()
    {
        if (isServer)
        {
            if (handDealt)
            {
                ResetValues();
            }

            DealCards(standardDeck);
            CheckCard(DiscardPileCardValue);
        }
    }

    // Add player to list of players and assign their turn number (MyInt)
    public void AddPlayer(Player player)
    {
        if (!isServer)
        {
            return;
        }

        int nextPlayerNumber = playerList.Count;

        playerList.Add(player);

        player.RpcGetNumber(nextPlayerNumber);

        PlayerCount = playerList.Count;

        player.MyGm = this;

        // If loading a saved game assigned saved names
        if (dataCollector.LoadSavedGame)
        {
            player.MyName = savedPlayerNames[player.MyInt];
        }
    }

    // Linked to SyncVar, updates discharge pile card sprite
    void UpdateDiscardPileCard(int DiscardPileCardValue)
    {
        DiscardPileCard.SetValue(DiscardPileCardValue);
    }

    // Linked to SyncVar, updates if player can draw a card or not
    void UpdateCanDraw(int AvailableDrawCount)
    {
        if (!isServer)
        {
            return;
        }

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
        if (SuitOverride == 0)
        {
            discardPile.Remove(DiscardPileCardValue); 
        }

        drawPile = ShuffleDeck(discardPile);

        DrawPileSpriteRenderer.sprite = DrawPileSprites[0];

        discardPile.Clear();

        if (SuitOverride == 0)
        {
            discardPile.Add(DiscardPileCardValue); 
        }
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
                AddCard(j, card);
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

    // Set's GameState number and returns true if card is a played leagally
    public void CheckCard(int cardValue)
    {
        if (!isServer)
        {
            return;
        }

        GameState = 0;

        if (CheckJack(cardValue))
        {
            if (firstCard)
            {
                firstCard = false;
                GameState = 11;
                AvailableDrawCount = 0;

                playerList[PlayerTurn].RpcGetSuit();

                return;
            }

            GameState = 11;
            AvailableDrawCount = 0;

            DiscardPileCardValue = cardValue;
            discardPile.Add(cardValue);

            RemoveCard(PlayerTurn, cardValue);

            playerList[PlayerTurn].TakeAction(GameState);

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

        // If GameState is > 0 then a valid card has been played and should be removed from the player's hand unless it is the first card after the deal, then return
        if (firstCard)
        {
            firstCard = false;

            if (GameState == 4)
            {
                Clockwise = -1 * Clockwise;
            }

            ChangePlayerTurn();

            return;
        }

        if (GameState>0)
        {
            playerList[PlayerTurn].TakeAction(GameState);

            DiscardPileCardValue = cardValue;
            discardPile.Add(cardValue);

            RemoveCard(PlayerTurn, cardValue);

            if (!roundOver)
            {
                ChangePlayerTurn(); 
            }
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
            Clockwise = -1 * Clockwise;
        }

        return GameState;
    }

    //Actions at end of turn
    public void ChangePlayerTurn()
    {
        if (isServer)
        {
            if (Clockwise == -1)
            {
                // If player played an 8 then skip next player
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
                // If player played an 8 then skip next player
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

            // If player played a 7 then lock down next player until they draw 2 cards
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
            // Return if Player is not allowed to draw any more this round
            // Should not be called since player should not be able to call this function if they have already drawn a card
            if (AvailableDrawCount <= 0)
            {
                return;
            }

            // Select top card from the draw pile
            int card = drawPile[0];
            drawPile.RemoveAt(0);

            if (drawPile.Count <= 1)
            {
                // Change sprite to blank so it when last card is drawn you can see that there are none left
                DrawPileSpriteRenderer.sprite = DrawPileSprites[1];

                // Possibly add this to CoRoutine with an animation
                RefreshDrawPile();
            }

            // Add drawn card to player
            playerList[PlayerTurn].AddCard(card);

            // Add cards to reference to each players hand
            AddCard(PlayerTurn, card);

            // Reduce available draw count number so players can't draw too many cards
            AvailableDrawCount--;

            if (Draw2 && AvailableDrawCount == 0)
            {
                Draw2 = false;
                AvailableDrawCount = 1;
                GameState = 0;
                playerList[PlayerTurn].RpcUnlock();

                ChangePlayerTurn();
            }
            else
            {
                playerList[PlayerTurn].RpcStartCountdown();
            }
        }
    }

    // Called by player after they place a Jack on the discard pile
    // Sets the SuitOverride variable
    public void SetSuit(int suit)
    {
        SuitOverride = suit;
        DiscardPileCardValue = (suit * 100 + 14);
        GameState = 0;
        ChangePlayerTurn();
    }

    // Add card to player hand reference
    void RemoveCard(int index, int cardValue)
    {
        int winCheck = 1;

        switch (index)
        {
            case 0:
                player0Hand.Remove(cardValue);
                winCheck = player0Hand.Count;
                break;
            case 1:
                player1Hand.Remove(cardValue);
                winCheck = player1Hand.Count;
                break;
            case 2:
                player2Hand.Remove(cardValue);
                winCheck = player2Hand.Count;
                break;
            case 3:
                player3Hand.Remove(cardValue);
                winCheck = player3Hand.Count;
                break;
            default:
                break;
        }

        if (winCheck < 1)
        {
            RoundOver();
        }
    }

    // Remove card to player hand reference
    void AddCard(int index, int cardValue)
    {
        switch (index)
        {
            case 0:
                player0Hand.Add(cardValue);
                break;
            case 1:
                player1Hand.Add(cardValue);
                break;
            case 2:
                player2Hand.Add(cardValue);
                break;
            case 3:
                player3Hand.Add(cardValue);
                break;
            default:
                break;
        }
    }

    void RoundOver()
    {
        roundOver = true;

        int multiplyer;

        if (GameState == 11) // If last card is a jack add 2X multipier to scores
        {
            multiplyer = 2;
        }
        else if (GameState == 7) // If last card is a 7 then have next player draw 2 cards before adding up scores
        {
            GameState = 0;
            ChangePlayerTurn();
            AvailableDrawCount = 2;
            DrawCard();
            DrawCard();
            multiplyer = 1;
        }
        else multiplyer = 1;

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerHands[i].Count > 0)
            {
                foreach (int card in playerHands[i])
                {
                    int cardValue = card % 100;

                    if (cardValue == 1)
                    {
                        playerScores[i].AddToScore(multiplyer * 15);
                        foreach (Player player in playerList)
                        {
                            player.RpcUpdateScore(i, playerScores[i].Score);
                        }
                    }
                    else if (cardValue == 11)
                    {
                        playerScores[i].AddToScore(multiplyer * 20);
                        foreach (Player player in playerList)
                        {
                            player.RpcUpdateScore(i, playerScores[i].Score);
                        }
                    }
                    else if (cardValue < 10)
                    {
                        playerScores[i].AddToScore(multiplyer * 5);
                        foreach (Player player in playerList)
                        {
                            player.RpcUpdateScore(i, playerScores[i].Score);
                        }
                    }
                    else
                    {
                        playerScores[i].AddToScore(multiplyer * 10);
                        foreach (Player player in playerList)
                        {
                            player.RpcUpdateScore(i, playerScores[i].Score);
                        }
                    }
                } 
            }
        }

        // Activate Deal Button on player who is next dealer
        if (playerStart < PlayerCount - 1)
        {
            playerStart++;
        }
        else
        {
            playerStart = 0;
        }

        PlayerTurn = playerStart;

        playerList[PlayerTurn].RpcActivateDealButton();
    }

    void ResetValues()
    {
        GameState = 0;

        AvailableDrawCount = 1;

        firstCard = true;

        Draw2 = false;

        Clockwise = -1;

        SuitOverride = 0;

        roundOver = false;

        foreach (List<int> playerHand in playerHands)
        {
            if (playerHand.Count>0)
            {
                playerHand.Clear(); 
            }
        }

        foreach (Player player in playerList)
        {
            player.RpcClearHand();
        }

        startTimer = defaultStartTime;
    }


}