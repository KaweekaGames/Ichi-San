using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    // set to disabled when not active
    public SpriteRenderer MySpriteRenderer;

    // card value given by GameManager to player
    public int AssingedValue = 0;

    // local player
    public Player MyPlayer;
    
    // placement on board
    Vector3 CurrentLocation;

    // holder for location to go back to if move is invalid
    Vector3 oldLocation;

    // sprites for cards
    [SerializeField]
    Sprite[] spadeSprites;
    [SerializeField]
    Sprite[] clubSprites;
    [SerializeField]
    Sprite[] diamondSprites;
    [SerializeField]
    Sprite[] heartSprites;


    // use to set value and sprite of card
    public void SetValue(int value)
    {
        AssingedValue = value;

        int suit = AssingedValue / 100;

        int type = AssingedValue % 100;

        switch (suit)
        {
            case 1:
                MySpriteRenderer.sprite = spadeSprites[type];
                break;

            case 2:
                MySpriteRenderer.sprite = clubSprites[type];
                break;

            case 3:
                MySpriteRenderer.sprite = diamondSprites[type];
                break;

            case 4:
                MySpriteRenderer.sprite = heartSprites[type];
                break;

            default:
                MySpriteRenderer.sprite = spadeSprites[0];
                break;
        }
    }

    // use to place on board
    public void SetPosition(Vector3 newLocation)
    {
        transform.position = newLocation;

        CurrentLocation = transform.position;
    }

    // called to move card
    public void MoveLocation(Vector3 newLocation)
    {
        //Itween move to location
    }

    void OnCollisionEnter2D (Collider2D col)
    {
        if (col.tag == "DiscardPile")
        {
            if (MyPlayer.TurnCheck())
            {
                MyPlayer.CheckMyCard(AssingedValue);

                MyPlayer.CheckedCard = this;
            }
            else
            {
                MoveLocation(CurrentLocation);
            } 
        }
        else if (col.tag == "Card")
        {
            oldLocation = CurrentLocation;

            CurrentLocation = col.tranform.position;

            Card hitCard = col.GetComponent<Card>();

            hitCard.MoveLocation(oldLocation);
        }
    }
}
