using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    // values for laying out locations for cards in hand
    [SerializeField]
    Vector3 startingPoint;
    [SerializeField]
    float xInterval;
    [SerializeField]
    float zInterval;
    [SerializeField]
    float screenWidth;
    [SerializeField]
    int numHolderLocations;
    [SerializeField]
    float wideInterval;
    [SerializeField]
    float mediuimInterval;
    [SerializeField]
    float shortInterval;

    // prefabs for displaying hand
    [SerializeField]
    GameObject cardPrefab;
    [SerializeField]
    GameObject cardHolderPrefab;

    // field to properly display cards
    [SerializeField]
    string sortingLayer = "CardSortingLayer";

    // list of all possible places to put your cards
    List<CardHolder> cardLocations;

    // current hand
    public List<int> MyHand;

    // reference to GameManager
    public GameManager MyGm;

    // bool checking if drawing a card is allowed
    [SyncVar]
    public bool CanDraw = true;

    // if oppenet lays down a 7 you are locked into drawing 2 cards
    [SyncVar]
    public bool Locked = false;

    // number given by GM to determin play order
    [SyncVar]
    public int MyInt;

    // player name set at Lobby scene
    [SyncVar(hook = "GetMyName")]
    public string MyName = "nobody";

    // reference to checked card
    public Card CheckedCard;

    // reference to cardHolder
    CardHolder cardHolderRef;

    // UI buttons for Jack suit choice
    public GameObject UISuitPanel;
    public Button[] SuitButtons;

    // UI button for dealing cards
    public Button DealButton;

    // Reference to ScoreBoard
    public HandTracker HandTracker;

    public bool ImReady = false;

    bool recievedMyInt = false;

    // Reference to the draw pile
    GameObject drawPile;

    // Prefab to generate draw card
    public GameObject DrawCardPrefab;

    GameObject nextTurnButton;

    public override void OnStartClient()
    {
        GetMyName(MyName);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (drawPile == null)
        {
            drawPile = GameObject.FindGameObjectWithTag("DrawPile");

            if (drawPile != null)
            {
                GameObject gO = Instantiate(DrawCardPrefab, drawPile.transform.position, Quaternion.identity);
                DrawCard drawCard = gO.GetComponent<DrawCard>();
                drawCard.SetPosition(drawPile.transform.position);

                drawCard.MyPlayer = this;
            }
        }

        if (MyGm == null)
        {
            MyGm = FindObjectOfType<GameManager>();

            if (MyGm == null)
            {
                return;
            }
        }

        if (nextTurnButton == null)
        {
            nextTurnButton = GameObject.Find("NextTurn");

            Button nTButton = nextTurnButton.GetComponent<Button>();

            nTButton.onClick.AddListener(EndMyTurn);
        }

        if (!ImReady)
        {
            LayoutHandArea();
        }
    }


    //*****
    // Functions
    //*****

    // Build hand area
    void LayoutHandArea()
    {
        if (cardLocations == null)
        {
            cardLocations = new List<CardHolder>();
        }

        for (int i = 0; i < numHolderLocations; i++)
        {

            GameObject newCardHolder = Instantiate(cardHolderPrefab, transform.position, Quaternion.identity);

            CardHolder cardHolder = newCardHolder.GetComponent<CardHolder>();

            cardLocations.Add(cardHolder);
            cardHolder.SortingLayer = sortingLayer;
            cardHolder.Occupied = false;
            cardHolder.enabled = false;
        }

        FormatHand();

        ImReady = true;
    }

    // Called to adjust hand placement when adding or subtracting cards
    private void FormatHand()
    {
        //variable holders
        List<CardHolder> occupiedCH = new List<CardHolder>();
        List<CardHolder> emptyCH = new List<CardHolder>();

        // separate occupied from unoccupied CardHolders
        foreach (CardHolder ch in cardLocations)
        {
            if (ch.Occupied)
            {
                occupiedCH.Add(ch);
            }
            else
            {
                emptyCH.Add(ch);
            }
        }

        // clear master card location list so it can be repopulated with the occupied spaces first
        cardLocations.Clear();

        foreach (CardHolder ch in occupiedCH)
        {
            cardLocations.Add(ch);
        }

        foreach (CardHolder ch in emptyCH)
        {
            cardLocations.Add(ch);
        }

        if (MyHand == null)
        {
            MyHand = new List<int>();
        }

        if (MyHand.Count < 11)
        {
            xInterval = wideInterval;
        }
        else if (MyHand.Count >= 11 && MyHand.Count < 16)
        {
            xInterval = mediuimInterval;
        }
        else if (MyHand.Count >= 16 && MyHand.Count < 24)
        {
            xInterval = shortInterval;
        }
        else
        {
            xInterval = shortInterval * .6f;
        }

        for (int i = 0; i < cardLocations.Count; i++)
        {
            float xLocation = (float)(xInterval * (.5 * (MyHand.Count - 1)));

            Vector3 location = new Vector3(-xLocation + i * xInterval, startingPoint.y, startingPoint.z + i * zInterval);

            cardLocations[i].Location = location;
            cardLocations[i].Index = i;

            if (i <= MyHand.Count)
            {
                cardLocations[i].enabled = true;
            }
        }

        GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject gO in allCards)
        {
            Card thisCard = gO.GetComponent<Card>();
            thisCard.ReturnToHand();
        }
    }

    // Remove Card from hand
    public void RemoveCard(int cardValue)
    {
        MyHand.Remove(cardValue);
        CheckedCard = null;
        FormatHand();
    }

    // Called by Server to add card to hand
    public void AddCard(int newCard)
    {
        if (isServer)
        {
            RpcAddCard(newCard);
        }
    }

    [ClientRpc]
    public void RpcAddCard(int newCard)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (cardLocations == null)
        {
            LayoutHandArea();
        }

        bool locationFound = false;

        ////////////////////////////////////////////////////////////////////////// this needs to be fixed can be infinite loop if all locations occupied  ////////////////////////////////////////////////////////
        for (int i = 0; !locationFound; i++)
        {
            if (!cardLocations[i].Occupied)
            {
                cardHolderRef = cardLocations[i];

                locationFound = true;
            }
        }

        GameObject nCard = Instantiate(cardPrefab, cardHolderRef.Location, Quaternion.identity);

        Card card = nCard.GetComponent<Card>();

        card.SetValue(newCard);

        card.SetPosition(cardHolderRef.Location);
        cardHolderRef.Occupied = true;

        card.MyPlayer = this;
        card.MyCardHolder = cardHolderRef;

        card.MySpriteRenderer.sortingLayerName = cardHolderRef.SortingLayer;

        MyHand.Add(card.AssingedValue);

        FormatHand();
    }

    [ClientRpc]
    public void RpcGetNumber(int number)
    {
        MyInt = number;

        recievedMyInt = true;
    }

    // Check if valid play
    public void CheckMyCard(Card discardedCard)
    {
        CheckedCard = discardedCard;

        int value = CheckedCard.AssingedValue;

        if (isServer)
        {
            MyGm.CheckCard(value);
        }
        else
        {
            CmdCheckMyCard(value);
        }
    }

    [Command]
    public void CmdCheckMyCard(int cardValue)
    {
        MyGm.CheckCard(cardValue);
    }

    // Called by Server in response of played card
    public void TakeAction(int actionNumber)
    {
        if (isServer)
        {
            RpcTakeAction(actionNumber);
        }
    }

    [ClientRpc]
    public void RpcTakeAction(int actionNumber)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (actionNumber > 0)
        {
            CheckedCard.ValidCard();

            if (actionNumber == 11)
            {
                GetSuit();
            }
        }
        else
        {
            CheckedCard.InValidCard();
            CheckedCard = null;
        }
    }

    // Assign name to gameobject from lobby screne
    public void GetMyName(string myName)
    {
        gameObject.name = myName;
    }

    // Check if it is my turn
    public bool TurnCheck()
    {
        bool itIsMyTurn = (MyInt == MyGm.PlayerTurn);

        return itIsMyTurn;
    }

    // Draw card
    public void DrawCard()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (TurnCheck() && MyGm.AvailableDrawCount > 0)
        {
            if (isServer)
            {
                MyGm.DrawCard();
            }
            else
            {
                CmdDrawCard();
            }
        }
    }

    [Command]
    void CmdDrawCard()
    {
        MyGm.DrawCard();
    }

    public Vector3 FindLandingSpot()
    {
        Vector3 landingSpot = cardLocations[MyHand.Count].Location;

        GameObject gO = Instantiate(DrawCardPrefab, drawPile.transform.position, Quaternion.identity);
        DrawCard drawCard = gO.GetComponent<DrawCard>();

        drawCard.MyPlayer = this;

        return landingSpot;
    }

    // End turn
    public void EndMyTurn()
    {
        if (Locked || CanDraw || !TurnCheck())
        {
            return;
        }

        if (isServer)
        {
            MyGm.ChangePlayerTurn();
        }
        else
        {
            CmdEndMyTurn();
        }
    }

    [Command]
    void CmdEndMyTurn()
    {
        MyGm.ChangePlayerTurn();
    }

    [ClientRpc]
    public void RpcAllowCanDraw()
    {
        CanDraw = true;
    }

    [ClientRpc]
    public void RpcDenyCanDraw()
    {
        CanDraw = false;
    }

    [ClientRpc]
    public void RpcLockDown()
    {
        Locked = true;
    }

    [ClientRpc]
    public void RpcUnlock()
    {
        Locked = false;
    }

    void GetSuit()
    {
        UISuitPanel.SetActive(true);
    }

    public void SetSuit(int suit)
    {
        UISuitPanel.SetActive(false);

        if (isServer)
        {
            MyGm.SetSuit(suit);
        }
        else
        {
            CmdSetSuit(suit);
        }
    }

    [Command]
    void CmdSetSuit(int suit)
    {
        MyGm.SetSuit(suit);
    }

    public int ReturnNumberofPlayers()
    {
        int numPlayers = MyGm.PlayerCount;

        return numPlayers;
    }

    public int ReturnNumberofCards(int playerNum)
    {
        switch (playerNum)
        {
            case 0:
                return MyGm.Player0CardsLeft;

            case 1:
                return MyGm.Player1CardsLeft;

            case 2:
                return MyGm.Player2CardsLeft;

            case 3:
                return MyGm.Player3CardsLeft;

            default:
                return 999999;
        }
    }

    [ClientRpc]
    public void RpcUpdateScore(int playerNumber, int updatedScore)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        HandTracker.SetScore(playerNumber, updatedScore);
    }

    [ClientRpc]
    public void RpcClearHand()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        MyHand.Clear();

        GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject gO in allCards)
        {
            Destroy(gO);
        }

        foreach (CardHolder ch in cardLocations)
        {
            ch.Occupied = false;
        }

        CanDraw = true;
        Locked = false;
    }

    [ClientRpc]
    public void RpcGetSuit()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        GetSuit();
    }

    [ClientRpc]
    public void RpcStartCountdown()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        StartCoroutine("Wait");
    }

    public void DealCards()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (isServer)
        {
            MyGm.StartRound();
            DealButton.enabled = false;
        }
        else
        {
            CmdDealCards();
        }
    }

    [Command]
    void CmdDealCards()
    {
        DealCards();
    }

    void ActivateDealButton()
    {
        DealButton.enabled = true;
    }

    [ClientRpc]
    public void RpcActivateDealButton()
    {
        if (!isLocalPlayer)
        {
            return;
        }

       ActivateDealButton();
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(5f);
        EndMyTurn();
    }
}
