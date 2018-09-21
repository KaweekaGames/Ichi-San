using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanel : MonoBehaviour
{
    [SerializeField]
    RectTransform locationTransform;

    RectTransform rectTransform;

    [SerializeField]
    Vector3 displayPosition;
    [SerializeField]
    float transitionTime;
    [SerializeField]
    float transitionDelay;
    [SerializeField]
    EaseType transitionEase;

    Vector3 startPosition;
    int state = 1;

    // Use this for initialization
    void Start()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();

        startPosition = rectTransform.position;

        displayPosition = locationTransform.position;
    }

    public void MovePanel()
    {

        if (state == 1)
        {
            gameObject.MoveTo(displayPosition, transitionTime, transitionDelay, transitionEase); 
        }
        else
        {
            gameObject.MoveTo(startPosition, transitionTime, transitionDelay, transitionEase);
        }

        state = -1 * state;
    }
}
