using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurnIndicator : MonoBehaviour
{
    public Color[] TurnColors;

    public GameManager MyGM;

    [SerializeField]
    float rotationSpeed;
    [SerializeField]
    float rotationDelay;
    [SerializeField]
    EaseType rotationEase;
    [SerializeField]
    float colorTransitionTimer = .5f;

    float countDownTimer;
    int currentTurn = 0;
    int zRotation = -1;

    SpriteRenderer mySpriteRenderer;

    // Use this for initialization
    void Start()
    {
        mySpriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        ChangeColor(currentTurn);
    }

    // Update is called once per frame
    void Update()
    {
        Spin();
    }

    // Spins turn indicator after confirming if it is the right color
    public void Spin()
    {
        zRotation = MyGM.Clockwise;

        if (currentTurn != MyGM.PlayerTurn)
        {
            currentTurn = MyGM.PlayerTurn;
            ChangeColor(currentTurn);
        }

        Vector3 spinDirection = new Vector3(0, 0, zRotation);
        gameObject.RotateBy(spinDirection, rotationSpeed, rotationDelay, rotationEase);
    }

    // Matches color of turn indicator to color of current player turn
    public void ChangeColor(int playerNum)
    {
        gameObject.ColorTo(TurnColors[playerNum], colorTransitionTimer, 0);
    }


}
