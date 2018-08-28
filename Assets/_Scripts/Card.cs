using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    // set to disabled when not active
    public SpriteRenderer MySpriteRenderer;

    // card value given by GameManager to player
    public int AssingedValue = 0;

    // index referencing place in hand (left to right)
    public int Index;

    // local player
    public Player MyPlayer;
    
    // holder for location to go back to if move is invalid
    Vector3 oldLocation;

    // holder for Card Holder
    public CardHolder MyCardHolder;

    // card movement variables
    public float constantMoveTime = .1f;
    public float delay = .3f;
    public float shiftDelay = .2f;
    public float scaleTime = .1f;
    public float shiftTime = .1f;
    public float shiftedZPosition = -1.6f;
    public EaseType moveEase;
    public EaseType snapEase;
    public EaseType discardEase;
    public Vector3 scaleFactor;
    public bool onDiscardPile = false;
    public bool touched;
    Vector3 targetPos;
    Vector3 originalScale;

    // sprites for cards
    [SerializeField]
    Sprite[] spadeSprites;
    [SerializeField]
    Sprite[] clubSprites;
    [SerializeField]
    Sprite[] diamondSprites;
    [SerializeField]
    Sprite[] heartSprites;


    private void Update()
    {
        if (touched)
        {
            gameObject.MoveUpdate(targetPos, constantMoveTime);
        }
    }

    // use to set value and sprite of card
    public void SetValue(int value)
    {
        AssingedValue = value;

        gameObject.name = ("Card " + AssingedValue);

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

        originalScale = transform.localScale;
    }

    // called to move card back to player's hand
    public void ReturnToHand()
    {
        gameObject.MoveTo(MyCardHolder.Location, constantMoveTime, delay, moveEase);
        //gameObject.ScaleTo(originalScale, scaleTime, 0f);
    }

    // called by CardMarker child to parent card to  move to new position (bully card's old position) in player's hand
    // swaps CardHolder references 
    public CardHolder ShiftPosition(CardHolder bullyCardHolder)
    {
        CardHolder oldCardHolder = MyCardHolder;
        MyCardHolder = bullyCardHolder;

        gameObject.MoveTo(MyCardHolder.Location, shiftTime, shiftDelay, snapEase);

        return oldCardHolder;
    }

    // called by child CardMarker of bully card when hitting another card in player's hand to swap CardHolder references
    public void CallForShift(Collider2D col)
    {
        CardHolder bullyCardHolder = MyCardHolder;

        CardMarker hitCardMarker = col.transform.GetComponent<CardMarker>();

        // calls function that swaps CardHolder references between player controlled bully card and card being hit
        MyCardHolder = hitCardMarker.ShiftPosition(bullyCardHolder);
    }

    // Actions when placing card on Discharge Pile
    void OnTriggerEnter2D(Collider2D col)
    {
        // do not run this function if I am the discard pile
        if (gameObject.tag == "DiscardPile")
        {
            return;
        }

        // called when player chosen card is placed onto the discard pile to check if it is a valid card
        if (col.transform.tag == "DiscardPile")
        {
            if (MyPlayer.TurnCheck() && !MyPlayer.Locked)
            {
                gameObject.MoveTo(col.transform.position, constantMoveTime, 0f, discardEase);
                touched = false;
                onDiscardPile = true;

                StartCoroutine("Wait");
            }
            else
            {
                touched = false;
                ReturnToHand();
            }
        }
    }

    void OnTouchDown()
    {
        if (touched == false &&!MyPlayer.Locked)
        {
            touched = true;
            //gameObject.ScaleTo(scaleFactor, scaleTime, 0);
        }
    }

    void OnTouchUp()
    {
        if (touched == true && !onDiscardPile)
        {
            touched = false;
            //gameObject.ScaleTo(originalScale, scaleTime, 0f);
            gameObject.MoveTo(MyCardHolder.Location, constantMoveTime, delay, moveEase);
        }
    }

    void OnTouchStay(Vector2 point)
    {
        if (touched == true)
        {
            targetPos = new Vector3(point.x, point.y, shiftedZPosition); 
        }
    }

    void OnTouchCancel()
    {
        
    }

    private void OnMouseExit()
    {
        if (touched == true && !onDiscardPile)
        {
            touched = false;
            //gameObject.ScaleTo(originalScale, scaleTime, 0f);
            gameObject.MoveTo(MyCardHolder.Location, constantMoveTime, delay, moveEase); 
        }
    }

    public void InValidCard()
    {
        onDiscardPile = false;
        ReturnToHand();
    }

    public void ValidCard()
    {
        MyCardHolder.Occupied = false;
        MyPlayer.RemoveCard(AssingedValue);
        Destroy(gameObject);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(.4f);
        MyPlayer.CheckMyCard(this);
    }
}
