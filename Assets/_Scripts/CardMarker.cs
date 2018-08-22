using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  attached to gameobject child that holds a collider for interacting with other cards in players hand
//  when triggered it calls function on parent card to switch CardHolder references with collided card
//  collider attached to this object is smaller than the collider on the Card which helps with more consistent
//  card shuffling
public class CardMarker : MonoBehaviour
{
    // parent of this object
    public Card myCard;

    // calls the ShiftPosition function on parent Card which returns it's CardHolder Reference
    public CardHolder ShiftPosition(CardHolder bullyCardHolder)
    {
        CardHolder newCardHolder = myCard.ShiftPosition(bullyCardHolder);

        return newCardHolder;
    }

    // called when player selected card hits another Card in player's hand
    void OnTriggerEnter2D(Collider2D col)
    {
        // only call function if in contact with CardMarker collider and not the Card collider
        if (col.transform.tag == "CardMarker" && myCard.touched)
        {
            // sends collided card info to parent card
            myCard.CallForShift(col);
        }
    }

}
