﻿using System.Collections;
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
    
    // holder for location to go back to if move is invalid
    Vector3 oldLocation;

    // holder for sorting order
    int mySortingOrder;

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

    // called to move card
    public void ReturnToHand()
    {
        gameObject.ScaleTo(originalScale, scaleTime, 0f);
        gameObject.MoveTo(MyCardHolder.Location, constantMoveTime, delay, moveEase);
        MySpriteRenderer.sortingOrder = MyCardHolder.OrderInLayer;
    }

    public CardHolder ShiftPosition(CardHolder bullyCardHolder)
    {
        CardHolder oldCardHolder = MyCardHolder;
        MyCardHolder = bullyCardHolder;

        gameObject.MoveTo(MyCardHolder.Location, shiftTime, shiftDelay, snapEase);
        MySpriteRenderer.sortingOrder = MyCardHolder.OrderInLayer;

        return oldCardHolder;

    }

    public void CallForShift(Collider2D col)
    {
        CardHolder bullyCardHolder = MyCardHolder;

        CardMarker hitCardMarker = col.transform.GetComponent<CardMarker>();

        MyCardHolder = hitCardMarker.ShiftPosition(bullyCardHolder);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (gameObject.tag == "DiscardPile")
        {
            return;
        }

        if (col.transform.tag == "DiscardPile")
        {

            if (MyPlayer.TurnCheck())
            {
                MyPlayer.CheckMyCard(AssingedValue);

                MyPlayer.CheckedCard = this;
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
        if (touched == false)
        {
            touched = true;
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
            gameObject.MoveTo(MyCardHolder.Location, constantMoveTime, delay, moveEase);
            MySpriteRenderer.sortingOrder = MyCardHolder.OrderInLayer; 
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
            gameObject.ScaleTo(originalScale, scaleTime, 0f);
            MySpriteRenderer.sortingOrder = MyCardHolder.OrderInLayer;
            gameObject.MoveTo(MyCardHolder.Location, constantMoveTime, delay, moveEase); 
        }
    }
}
