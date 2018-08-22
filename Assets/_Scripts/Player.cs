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
    float yInterval;
    [SerializeField]
    float zInterval;
    [SerializeField]
    int numColumns;
    [SerializeField]
    int numRows;

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
    }


    //*****
    // Functions
    //*****

    // Build hand area
    void LayoutHandArea()
    {
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numColumns; j++)
            {
                Vector3 location = new Vector3(startingPoint.x + j * xInterval, startingPoint.y + i * yInterval, startingPoint.z + j * zInterval);

                GameObject newCardHolder = GameObject.Instantiate(cardHolderPrefab, location, Quaternion.identity);

                CardHolder cardHolder = newCardHolder.GetComponent<CardHolder>();

                cardHolder.Location = location;

                cardHolder.SortingLayer = sortingLayer;

                cardHolder.OrderInLayer = j;

                cardLocations.Add(cardHolder);
            }
        }
    }

    // Called when added or subtracting card location area

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
        card.MySpriteRenderer.sortingOrder = cardHolderRef.OrderInLayer;

        myHand.Add(card.AssingedValue);
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
    public void RpcCheckMyCard(int cardValue)
    {
        bool validCard = myGM.CheckCard(cardValue);

        Debug.Log("this card's validity is " + validCard);
    }

    [Command]
    public void CmdCheckMyCard(int cardValue)
    {
        RpcCheckMyCard(cardValue);
    }

    public void GetMyName(string myName)
    {
        gameObject.name = myName;
    }

    // Check if valid play
    public void CheckMyCard(int num)
    {
        if (isServer)
        {
            RpcCheckMyCard(num);
        }
        else
        {
            CmdCheckMyCard(num);
        }
    }

    // Check if it is my turn
    public bool TurnCheck()
    {
        bool itIsMyTurn = (MyInt == myGM.playerTurn);

        return itIsMyTurn;
    }
}
