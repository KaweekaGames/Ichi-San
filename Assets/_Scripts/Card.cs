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

    // holder for sorting order
    int mySortingOrder;

    // card movement variables
    public float constantMoveTime = .1f;
    public float delay = .3f;
    public float scaleTime = .1f;
    public EaseType moveEase;
    public Vector3 scaleFactor;
    public bool onDiscardPile = false;
    bool touched;
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

        originalScale = transform.localScale;
    }

    // called to move card
    public void MoveLocation(Vector3 newLocation)
    {
        gameObject.ScaleTo(originalScale, scaleTime, 0f);
        gameObject.MoveTo(newLocation, constantMoveTime, delay, moveEase);
        MySpriteRenderer.sortingOrder = mySortingOrder;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (gameObject.tag == "DiscardPile")
        {
            return;
        }

        if (col.transform.tag == "DiscardPile")
        {
            Debug.Log("hit Pile");

            if (MyPlayer.TurnCheck())
            {
                Debug.Log("its true");
                MyPlayer.CheckMyCard(AssingedValue);

                MyPlayer.CheckedCard = this;
            }
            else
            {
                Debug.Log("its false");
                touched = false;
                MoveLocation(CurrentLocation);
            }
        }
        //else if (col.transform.tag == "Card")
        //{
        //    oldLocation = CurrentLocation;

        //    CurrentLocation = col.transform.position;

        //    Card hitCard = col.transform.GetComponent<Card>();

        //    hitCard.MoveLocation(oldLocation);
        //}
    }

    void OnTouchDown()
    {
        if (touched == false)
        {
            touched = true;
            mySortingOrder = MySpriteRenderer.sortingOrder;
            gameObject.ScaleTo(scaleFactor, scaleTime, 0);
            MySpriteRenderer.sortingOrder = 60; 
        }
    }

    void OnTouchUp()
    {
        if (touched == true && !onDiscardPile)
        {
            touched = false;
            gameObject.ScaleTo(originalScale, scaleTime, 0f);
            gameObject.MoveTo(CurrentLocation, constantMoveTime, delay, moveEase);
            MySpriteRenderer.sortingOrder = mySortingOrder; 
        }
    }

    void OnTouchStay(Vector2 point)
    {
        if (touched == true)
        {
            targetPos = new Vector3(point.x, point.y, CurrentLocation.z); 
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
            gameObject.ScaleTo(originalScale, scaleTime, 0f);
            MySpriteRenderer.sortingOrder = mySortingOrder;
            gameObject.MoveTo(CurrentLocation, constantMoveTime, delay, moveEase); 
        }
    }
}
