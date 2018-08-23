using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
    List<int> myHand;

    // reference to GameManager
    GameManager myGM;

    // number given by GM to determin play order
    [SyncVar]
    public int MyInt;

    // player name set at Lobby scene
    [SyncVar(hook = "GetMyName")]
    public string myName = "nobody";

    // reference to checked card
    public Card CheckedCard;

    // reference to cardHolder
    CardHolder cardHolderRef;

    bool recievedMyInt = false;



    private void Start()
    {
        GetMyName(myName);

        if (isLocalPlayer)
        {
            cardLocations = new List<CardHolder>();

            LayoutHandArea();
        }

        myHand = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            if (myGM == null)
            {
                myGM = FindObjectOfType<GameManager>();

                if (myGM == null)
                {
                    return;
                }
            }

            if (myHand == null)
            {
                myHand = new List<int>();
            }

            return;
        }

        if (myGM == null)
        {
            myGM = FindObjectOfType<GameManager>();

            if (myGM == null)
            {
                return;
            }
        }

        if (myHand == null)
        {
            myHand = new List<int>();
        }
    }


    //*****
    // Functions
    //*****

    // Build hand area
    void LayoutHandArea()
    {
        for (int i = 0; i < numHolderLocations; i++)
        {
            //Vector3 location = new Vector3(startingPoint.x + i * xInterval, startingPoint.y, startingPoint.z + i * zInterval);
            // GameObject newCardHolder = Instantiate(cardHolderPrefab, location, Quaternion.identity);

            GameObject newCardHolder = Instantiate(cardHolderPrefab, transform.position, Quaternion.identity);

            CardHolder cardHolder = newCardHolder.GetComponent<CardHolder>();
            //cardHolder.Index = i;
            //cardHolder.Location = location;

            cardLocations.Add(cardHolder);
            cardHolder.SortingLayer = sortingLayer;
            cardHolder.Occupied = false;
            cardHolder.enabled = false;
        }

        FormatHand();
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

        if (myHand == null)
        {
            myHand = new List<int>();
        }

        if (myHand.Count < 8)
        {
            xInterval = wideInterval;
        }
        else if (myHand.Count >= 8 && myHand.Count < 16)
        {
            xInterval = mediuimInterval;
        }
        else
        {
            xInterval = shortInterval;
        }

        for (int i = 0; i < cardLocations.Count; i++)
        {
            float xLocation = (float) (xInterval * (.5 * (myHand.Count - 1)));

            Vector3 location = new Vector3(-xLocation + i * xInterval, startingPoint.y, startingPoint.z + i * zInterval);

            cardLocations[i].Location = location;
            cardLocations[i].Index = i;

            if (i<=myHand.Count)
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

    // tempppppp
    public void RemoveCard(int cardValue)
    {
        myHand.Remove(cardValue);

        FormatHand();
    }

    // Called by Server to add card to hand
    public void AddCard(int newCard)
    {
        if (isServer)
        {
            RpcAddCard(newCard);
        }
        else
        {
            CmdAddCard(newCard);
        }

    }

    [ClientRpc]
    public void RpcAddCard(int newCard)
    {
        if (!isLocalPlayer)
        {
            return;
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

        myHand.Add(card.AssingedValue);

        FormatHand();
    }

    [Command]
    public void CmdAddCard(int newCard)
    {
        RpcAddCard(newCard);
    }

    [ClientRpc]
    public void RpcGetNumber(int number)
    {
        MyInt = number;

        recievedMyInt = true;
    }

    [ClientRpc]
    public bool RpcCheckMyCard(int cardValue)
    {
        bool validCard = myGM.CheckCard(cardValue);

        return validCard;
    }

    [Command]
    public bool CmdCheckMyCard(int cardValue)
    {
        bool validCard = RpcCheckMyCard(cardValue);

        return validCard;
    }

    public void GetMyName(string myName)
    {
        gameObject.name = myName;
    }

    // Check if valid play
    public bool CheckMyCard(int num)
    {
        bool validCard = false;

        if (isServer)
        {
            validCard = RpcCheckMyCard(num);
        }
        else
        {
            validCard = CmdCheckMyCard(num);
        }

        return validCard;
    }

    // Check if it is my turn
    public bool TurnCheck()
    {
        bool itIsMyTurn = (MyInt == myGM.playerTurn);

        return itIsMyTurn;
    }
}
