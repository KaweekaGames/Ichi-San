using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyButton : MonoBehaviour
{
    public Text MyButtonText;


    public void ChangeTheText()
    {
        int ran = Random.Range(0, 1000);

        MyButtonText.text = ran.ToString();
    }
}
