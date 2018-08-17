using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    // set to disabled when not active
    public SpriteRenderer MySpriteRenderer;

    // value given by GameManager to player
    public int AssingedValue = 0;

    // placement on board
    Vector3 CurrentLocation;

    // holder for location to go back to if move is invalid
    Vector3 oldLocation;

    // reference to player object
    public Player myPlayer;

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

        if(AssingedValue == 0)
        {
            MySpriteRenderer.sprite = spadeSprites[0];
            MySpriteRenderer.enabled = false;

            return;
        }

        int suit = AssingedValue / 100;

        int type = AssingedValue % 100;

        switch (suit)
        {
            case 1:
                MySpriteRenderer.sprite = spadeSprites[type];
                MySpriteRenderer.enabled = true;
                break;

            case 2:
                MySpriteRenderer.sprite = clubSprites[type];
                MySpriteRenderer.enabled = true;
                break;

            case 3:
                MySpriteRenderer.sprite = diamondSprites[type];
                MySpriteRenderer.enabled = true;
                break;

            case 4:
                MySpriteRenderer.sprite = heartSprites[type];
                MySpriteRenderer.enabled = true;
                break;

            default:
                MySpriteRenderer.sprite = spadeSprites[0];
                MySpriteRenderer.enabled = false;
                break;
        }
    }

    // use to place on board
    public void SetPosition(Vector3 newLocation)
    {
        transform.position = newLocation;
    }
}
