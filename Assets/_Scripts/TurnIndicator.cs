using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurnIndicator : MonoBehaviour
{
    public Sprite[] IndicatorSprites2;
    public Sprite[] IndicatorSprites3;
    public Sprite[] IndicatorSprites4;

    public Color[] TurnColors;

    public GameManager MyGM;

    int numberOfPlayers = 2;

    [SerializeField]
    float rotationSpeed;
    [SerializeField]
    float rotationDelay;
    [SerializeField]
    EaseType rotationEase;
    [SerializeField]
    float timer;

    float countDownTimer;
    int currentTurn = 0;
    int zRotation = -1;
    bool goSpin = false;

    SpriteRenderer mySpriteRenderer;

    // Use this for initialization
    void Start()
    {
        mySpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!goSpin && MyGM.ExpectedPlayerCount == MyGM.PlayerCount)
        {
            goSpin = true;
            ChangeColor(MyGM.PlayerTurn);
        }

        if (goSpin)
        {
            Spin();
        }
    }

    public void Spin()
    {
        zRotation = MyGM.Clockwise;
        numberOfPlayers = MyGM.PlayerCount;

        if (currentTurn != MyGM.PlayerTurn)
        {
            currentTurn = MyGM.PlayerTurn;
            ChangeColor(currentTurn);
        }

        Vector3 spinDirection = new Vector3(0, 0, zRotation);
        gameObject.RotateBy(spinDirection, rotationSpeed, rotationDelay, rotationEase);
    }

    public void ChangeColor(int playerNum)
    {
        gameObject.ColorTo(TurnColors[playerNum], .5f, 0);
    }


}
