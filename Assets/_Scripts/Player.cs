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
    enum SortingLayer { first, second, third, fourth}

    // list of all possible places to put your cards
    List<CardHolder> cardLocations;

    // current hand
    List<int> myHand;

    // reference to local GameManager
    GameManager localGM;

    // number given by GM to determin play order
    public int MyInt;

    bool recievedMyInt = false;

    Vector3 cardPlacementLoc;


    // Build hand area
    void LayoutHandArea()
    {
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numColumns; j++)
            {
                Vector3 location = new Vector3(startingPoint.x + j * xInterval, startingPoint.y + i * yInterval, startingPoint.z);

                GameObject newCardHolder = GameObject.Instantiate(cardHolderPrefab, location, Quaternion.identity);

                CardHolder cardHolder = newCardHolder.GetComponent<CardHolder>();

                cardHolder.Location = location;

                cardHolder.SortingLayer = ((SortingLayer)i).ToString("d");

                cardHolder.OrderInLayer = j;

                cardLocations.Add(cardHolder);
            }
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if(localGM == null)
        {
            localGM = FindObjectOfType<GameManager>();

            localGM.AddLocalPlayer(this);
        }

        if (cardLocations == null)
        {
            cardLocations = new List<CardHolder>();

            LayoutHandArea();
        }

        if(myHand == null)
        {
            myHand = new List<int>();
        }
    }

    // Called by Server to add card to hand
    [ClientRpc]
    public void RpcAddCard(int newCard)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (MyInt == localGM.playerTurn)
        {
            cardPlacementLoc = new Vector3();

            bool locationFound = false;

            for (int i = 0; !locationFound; i++)
            {
                if (!cardLocations[i].Occupied)
                {
                    cardPlacementLoc = cardLocations[i].Location;

                    locationFound = true;
                }
            }

            GameObject nCard = GameObject.Instantiate(cardPrefab, cardPlacementLoc, Quaternion.identity);

            Card card = nCard.GetComponent<Card>();

            card.SetValue(newCard);

            card.SetPosition(cardPlacementLoc);

            myHand.Add(card.AssingedValue); 
        }
    }

    [ClientRpc]
    public void RpcGetNumber(int number)
    {
        MyInt = number;

        recievedMyInt = true;

        localGM.MyLocalPlayer = this;
    }
}
