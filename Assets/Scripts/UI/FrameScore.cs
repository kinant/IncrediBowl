using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameScore : MonoBehaviour {

    public Text throw1;
    public Text throw2;
    public Text frameTotal;

    private void Start()
    {
        throw1.text = " ";
        throw2.text = " ";
        frameTotal.text = " ";
    }

    public void SetFrameThrowOneText(int score) {
        if (score == 10)
        {
            throw2.text = "X";
        }
        else
        {
            throw1.text = score.ToString();
        }
    }

    public void SetFrameThrowTwoText(int score)
    {
        if (score == 10)
        {
            throw2.text = "/";
        }
        else
        {
            throw2.text = score.ToString();
        }
    }

    public void SetFrameTotalText(int score)
    {
        frameTotal.text = score.ToString();
    }

}
