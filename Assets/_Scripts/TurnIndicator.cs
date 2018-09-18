using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurnIndicator : MonoBehaviour
{
    public Sprite[] IndicatorSprites2;
    public Sprite[] IndicatorSprites3;
    public Sprite[] IndicatorSprites4;

    public int SpinDirection = 1;
    public int NumberOfPlayers = 2;

    [SerializeField]
    float rotationSpeed;
    [SerializeField]
    float rotationDelay;
    [SerializeField]
    EaseType rotationEase;
    [SerializeField]
    float timer;

    float countDownTimer;

    SpriteRenderer mySpriteRenderer;

    // Use this for initialization
    void Start()
    {
        mySpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Spin(SpinDirection);
    }

    public void Spin(int zRotation)
    {
        Vector3 spinDirection = new Vector3(0, 0, zRotation);
        gameObject.RotateBy(spinDirection, rotationSpeed, rotationDelay, rotationEase);
    }

    public void ChangePlayer(int playerNum)
    {
        if (NumberOfPlayers == 2)
        {
            mySpriteRenderer.sprite = IndicatorSprites2[playerNum];
        }
        else if (NumberOfPlayers == 3)
        {
            mySpriteRenderer.sprite = IndicatorSprites3[playerNum];
        }
        else
        {
            mySpriteRenderer.sprite = IndicatorSprites4[playerNum];
        }
    }
}
