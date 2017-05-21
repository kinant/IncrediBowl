using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TenthFrameScore : MonoBehaviour {

    public Text throw1;
    public Text throw2;
    public Text throw3;
    public Text frameTotal;

    private void Start()
    {
        throw1.text = " ";
        throw2.text = " ";
        throw3.text = " ";
        frameTotal.text = " ";
    }

    public void SetFrameThrowOneText(int score) {
        if (score == 10)
        {
            throw1.text = "X";
        }
        else
        {
            throw1.text = score.ToString();
        }
    }

    public void SetFrameThrowTwoText(int score, int frameIndex)
    {
        if (frameIndex == 10)
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
        else {
            if (score == 10)
            {
                throw2.text = "X";
            }
            else
            {
                throw2.text = score.ToString();
            }
        }
    }

    public void SetFrameThrowThreeText(int score, int frameIndex)
    {
        if (frameIndex == 12)
        {
            if (score == 10)
            {
                throw3.text = "X";
            }
            else
            {
                throw3.text = score.ToString();
            }
        }
        else
        {
            if (score == 10)
            {
                throw3.text = "/";
            }
            else
            {
                throw3.text = score.ToString();
            }
        }
    }

    public void SetFrameTotalText(int score)
    {
        frameTotal.text = score.ToString();
    }

}
