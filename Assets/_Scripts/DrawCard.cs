using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCard : MonoBehaviour
{
    public Player MyPlayer;

    public Vector3 MyLocation;

    Vector3 locationHolder;

    // card movement variables
    public float MoveTime = .1f;
    public float Delay = 0f;
    public EaseType MoveEase;
    public float shiftedZPosition = -1.6f;
    bool touched = false;
    bool inHand;
    Vector3 targetPos;
    Vector3 landingSpot;

    private void Start()
    {
        MyLocation = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (touched)
        {
            gameObject.MoveUpdate(targetPos, .1f);
        }
    }

    void OnTouchDown()
    {
        if (touched == false)
        {
            touched = true;
        }
    }

    void OnTouchUp()
    {
        if (touched == true && !inHand)
        {
            touched = false;
            gameObject.MoveTo(MyLocation, MoveTime, Delay, MoveEase);
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
        if (touched == true && !inHand)
        {
            touched = false;
            gameObject.MoveTo(MyLocation, MoveTime, Delay, MoveEase);
        }
    }

    // called to move card back to draw pile
    public void ReturnToPile()
    {
        gameObject.MoveTo(MyLocation, MoveTime, Delay, MoveEase);
    }

    // Actions when leaving Draw Pile
    void OnTriggerExit2D(Collider2D col)
    {
        if (touched && col.transform.tag == "DrawPile")
        {

            Debug.Log("exiting");
            if (MyPlayer.TurnCheck())
            {
                landingSpot = MyPlayer.FindLandingSpot();

                gameObject.MoveTo(landingSpot, MoveTime, 0f, MoveEase);
                touched = false;
                inHand = true;

                StartCoroutine("Wait");
            }
            else
            {
                touched = false;
                ReturnToPile();
            }
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(.4f);
        MyPlayer.DrawCard();
        gameObject.transform.position = MyLocation;
        Destroy(gameObject);
    }
}
