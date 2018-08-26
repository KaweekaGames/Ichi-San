﻿using System.Collections;
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
    public GameManager MyGm;

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

    public bool ImReady = false;

    bool recievedMyInt = false;


    public override void OnStartClient()
    {
        GetMyName(myName);

        if (isLocalPlayer)
        {
            cardLocations = new List<CardHolder>();

            LayoutHandArea();
        }

        myHand = new List<int>();

        MyGm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (MyGm == null)
        {
            MyGm = FindObjectOfType<GameManager>();

            if (MyGm == null)
            {
                return;
            }
        }

        //if (myHand == null)
        //{
        //    myHand = new List<int>();
        //}

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
        Debug.Log("laying out hand");

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

        if (myHand == null)
        {
            myHand = new List<int>();
        }

        if (myHand.Count < 11)
        {
            xInterval = wideInterval;
        }
        else if (myHand.Count >= 11 && myHand.Count < 17)
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
        //else
        //{
        //    CmdAddCard(newCard);
        //}

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

        myHand.Add(card.AssingedValue);

        FormatHand();
    }

    //[Command]
    //public void CmdAddCard(int newCard)
    //{
    //    RpcAddCard(newCard);
    //}

    [ClientRpc]
    public void RpcGetNumber(int number)
    {
        MyInt = number;

        recievedMyInt = true;
    }

    //[ClientRpc]
    //public void RpcCheckMyCard(int cardValue)
    //{
    //    Debug.Log("checking the card" + gameObject.name);

    //    MyGm.CheckCard(cardValue);
    //}

    [ClientRpc]
    public void RpcTakeAction(int actionNumber)
    {
        Debug.Log("action number is " + actionNumber);

        if (!isLocalPlayer || CheckedCard == null)
        {
            return;
        }

        if (actionNumber == 0)
        {
            CheckedCard.InValidCard();
            CheckedCard = null;
        }
        else if(actionNumber > 0)
        {
            CheckedCard.ValidCard();
            CheckedCard = null;
        }
    }

    [Command]
    public void CmdCheckMyCard(int cardValue)
    {
        //RpcCheckMyCard(cardValue);
        MyGm.CheckCard(cardValue);
    }

    [Command]
    public void CmdTakeAction(int actionNumber)
    {
        RpcTakeAction(actionNumber);

    }

    // Check if valid play
    public void CheckMyCard(Card discardedCard)
    {
        if (!isLocalPlayer || MyInt != MyGm.playerTurn)
        {
            return;
        }

        CheckedCard = discardedCard;

        int value = CheckedCard.AssingedValue;

        if (isServer)
        {
            Debug.Log("RPC checking the card");
            //RpcCheckMyCard(value);
            MyGm.CheckCard(value);
        }
        else
        {
            Debug.Log("Cmd checking the card");
            CmdCheckMyCard(value);
        }
    }

    // Called by Server in response of played card
    public void TakeAction(int actionNumber)
    {
        if (isServer)
        {
            RpcTakeAction(actionNumber);

        }
        else
        {
            CmdTakeAction(actionNumber);
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
        bool itIsMyTurn = (MyInt == MyGm.playerTurn);

        return itIsMyTurn;
    }
}
