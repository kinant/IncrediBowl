using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This class handles the UI elements for the UI Frame Score that is displayed
public class FrameScore : MonoBehaviour {

    public Text throw1; // for first throw score
    public Text throw2; // for second throw score
    public Text frameTotal; // for displaying the frame total score

    private void Start()
    {
        // clear any text on the frame
        Clear();
    }

    // This function resets/clears the UI frame score
    public void Clear() {
        throw1.text = " ";
        throw2.text = " ";
        frameTotal.text = " ";
    }

    // This function sets the text for the first throw
    public void SetFrameThrowOneText(int score) {

        // if we have a strike, we display an X
        if (score == 10)
        {
            throw2.text = "X";
        }
        // else, just show the score (0-9)
        else
        {
            throw1.text = score.ToString();
        }
    }

    // This function sets the text for the second throw
    public void SetFrameThrowTwoText(int score)
    {
        // if we have a spare (score of 10), we show a "/"
        if (score == 10)
        {
            throw2.text = "/";
        }
        // else just show the score
        else
        {
            throw2.text = score.ToString();
        }
    }

    // This function sets the text total score for the frame
    public void SetFrameTotalText(int score)
    {
        frameTotal.text = score.ToString();
    }
}
