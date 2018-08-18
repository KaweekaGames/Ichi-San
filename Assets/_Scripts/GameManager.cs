using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    public int playerTurn = 0;
    [SyncVar]
    public int playerCount = 0;
    [SyncVar]
    public int DiscardPileCard;
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

    public Player MyLocalPlayer;

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

    private void Start()
    {
        if (player0Hand == null)
        {
            player0Hand = new List<int>();
        }

        if (player1Hand == null)
        {
            player1Hand = new List<int>();
        }

        if (player2Hand == null)
        {
            player2Hand = new List<int>();
        }

        if (player3Hand == null)
        {
            player3Hand = new List<int>();
        }

        if (playerList == null)
        {
            playerList = new List<Player>();
        }

        if (discardPile == null)
        {
            discardPile = new List<int>();
        }
    }

    void Update()
    {
        if (!isServer)
        {
            // TODO local actions

        }

        if (drawPile != null && drawPile.Count < 1)
        {
            RefreshDrawPile();
        }

        //Temp
        if (playerList != null && playerList.Count >0 && !handDealt)
        {
            DealCards(standardDeck);
        }
    }

    //// Add local player
    //public void AddLocalPlayer(Player localPlayer)
    //{
    //    MyLocalPlayer = localPlayer;
    //}

    // Add player to list of players and assign their turn number (MyInt)
    public void AddPlayer(NetworkIdentity id)
    {
        if (!isServer)
        {
            return;
        }

        Player newPlayer = id.gameObject.GetComponent<Player>();

        int nextPlayerNumber = playerList.Count;

        playerList.Add(newPlayer);

        newPlayer.RpcGetNumber(nextPlayerNumber);

        playerCount = playerList.Count;

        string newName = newPlayer.name;

        playerNames.Add(newName);

        int playerNamesLength = playerNames.Count;

        //lastPlayerAdded = playerNames[playerNamesLength - 1];
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
        discardPile.RemoveAt(discardPile.Count - 1);

        drawPile = ShuffleDeck(discardPile);
    }

    // Function deals cards to players to start round
    private void DealCards(List<int> cardDeck)
    {
        // Generate draw pile from a fresh deck
        drawPile = ShuffleDeck(cardDeck);

        // Deal 7 cards to each player
        for (int i = 0; i < 22; i++)
        {
            // Switch from player to player after each card dealt from deck
            for (int j = 0; j < playerCount; j++)
            {
                int rNum = Random.Range(0, drawPile.Count);

                int card = drawPile[rNum];

                drawPile.RemoveAt(rNum);

                // set player turn so only that player can recive this card
                playerList[j].RpcAddCard(card);

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

        DiscardPileCard = drawPile[drawPile.Count - 1];

        discardPile.Add(DiscardPileCard);

        drawPile.RemoveAt(drawPile.Count - 1);

        handDealt = true;
    }


    // *****
    // These functions are called when a player attempts to play a card
    // *****

    // Function returns true if card is a played leagally
    private bool CheckCard(int cardValue)
    {
        bool acceptedCard = false;

        if (CheckJack(cardValue))
        {
            // TODO jack function

            acceptedCard = true;

            return acceptedCard;
        }

        if (CheckSuit(cardValue))
        {
            acceptedCard = true;

            CheckSpecial(cardValue);
        }

        if (CheckRank(cardValue))
        {
            acceptedCard = true;

            CheckSpecial(cardValue);
        }

        return acceptedCard;
    }

    // Function returns true if card suit matches discard pile card suit
    private bool CheckSuit(int cardValue)
    {
        bool matchedSuit = false;

        int cardSuit = cardValue / 100;

        int discardPileCardSuit = DiscardPileCard / 100;

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

        int discardPileCardRank = DiscardPileCard % 100;

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

        isJack = ((cardValue % 100) == 12);

        return isJack;
    }

    // Function to see if a 7 or 8 has been played
    // If true calls for special actions to be performed 
    private void CheckSpecial(int cardValue)
    {
        if ((cardValue % 100) == 7)
        {
            //TODO enter draw 2 state
        }
        else if ((cardValue % 100) == 8)
        {
            //TODO skip turn state
        }
        else return;
    }
}
