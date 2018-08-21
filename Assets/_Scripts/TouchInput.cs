using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInput : MonoBehaviour
{
    public LayerMask TouchInputMask;

    RaycastHit2D hit;

    SpriteRenderer hitSpriteRenderer;

    RaycastHit2D[] allHits;

    Camera theCamera;

    // Use this for initialization
    void Start()
    {
        theCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR

        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            Vector2 worldPoint = theCamera.ScreenToWorldPoint(Input.mousePosition);

            allHits = Physics2D.RaycastAll(worldPoint, Vector2.zero, TouchInputMask);

            if(allHits.Length >0)
            {
                hit = allHits[0];
                hitSpriteRenderer = hit.transform.GetComponent<SpriteRenderer>();

                if (allHits.Length>1)
                {
                    foreach (RaycastHit2D oneHit in allHits)
                    {
                        SpriteRenderer oneHitSpriteRenderer = oneHit.transform.GetComponent<SpriteRenderer>();

                        if (hitSpriteRenderer.sortingOrder < oneHitSpriteRenderer.sortingOrder)
                        {
                            hit = oneHit;
                        }
                    } 
                }

                GameObject recipient = hit.transform.gameObject;

                if (Input.GetMouseButtonDown(0))
                {
                    recipient.SendMessage("OnTouchDown", hit.point, SendMessageOptions.DontRequireReceiver);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    recipient.SendMessage("OnTouchUp", hit.point, SendMessageOptions.DontRequireReceiver);
                }

                if (Input.GetMouseButton(0))
                {
                    recipient.SendMessage("OnTouchStay", hit.point, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

#endif

        //if (Input.touchCount > 0 )
        //{
        //    Touch touch = Input.GetTouch(0);

        //    Vector2 worldPoint = theCamera.ScreenToWorldPoint(Input.mousePosition);

        //    hit = Physics2D.Raycast(worldPoint, Vector2.zero, TouchInputMask);

        //    if (hit)
        //    {
        //        GameObject recipient = hit.transform.gameObject;

        //        if (touch.phase == TouchPhase.Began)
        //        {
        //            recipient.SendMessage("OnTouchDown", hit.point, SendMessageOptions.DontRequireReceiver);
        //            Debug.Log("hello");
        //        }

        //        if (touch.phase == TouchPhase.Ended)
        //        {
        //            recipient.SendMessage("OnTouchUp", hit.point, SendMessageOptions.DontRequireReceiver);
        //        }

        //        if (touch.phase == TouchPhase.Stationary)
        //        {
        //            recipient.SendMessage("OnTouchStay", hit.point, SendMessageOptions.DontRequireReceiver);
        //        }

        //        if (touch.phase == TouchPhase.Moved)
        //        {
        //            recipient.SendMessage("OnTouchMove", hit.point, SendMessageOptions.DontRequireReceiver);
        //        }

        //        if (touch.phase == TouchPhase.Canceled)
        //        {
        //            recipient.SendMessage("OnTouchCancel", hit.point, SendMessageOptions.DontRequireReceiver);
        //        }
        //    }
        //}
    }
}
