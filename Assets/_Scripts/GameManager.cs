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
}
