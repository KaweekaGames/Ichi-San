using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMarker : MonoBehaviour
{
    public Card myCard;

    public CardHolder ShiftPosition(CardHolder bullyCardHolder)
    {
        CardHolder newCardHolder = myCard.ShiftPosition(bullyCardHolder);

        return newCardHolder;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.tag == "CardMarker" && myCard.touched)
        {
            myCard.CallForShift(col);
        }
    }

}
