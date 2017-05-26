using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This class handles the UI elements for the last UI Frame Score that is displayed. It is basically the same as the frame score
// but the 10th frame has 3 possible throws, depending on the conditions
// NOTE: The game calculates the last frame score (10th frame), by utilizing "imaginary" 11th and 12th frames depending on all the possible combinations of scores. 
// For instance, if the user scores 2 strikes on the 10th frame, the game will go up to a 12th frame in order to calculate the final score. 
// If the user scores a strike on the first throw or a spare on the second throw of the 10th frame, he is entitled to an "11th" frame.
public class TenthFrameScore : MonoBehaviour {

    public Text throw1; // text for the first throw
    public Text throw2; // text for the second throw
    public Text throw3; // text for the third throw
    public Text frameTotal; // text for the total score on the frame (also the total game score)

    private void Start()
    {
        // clear any text
        Clear();
    }

    // this function clears/resets all texts
    public void Clear() {
        throw1.text = " ";
        throw2.text = " ";
        throw3.text = " ";
        frameTotal.text = " ";
    }

    // set the text for the first throw
    public void SetFrameThrowOneText(int score) {

        // if we made a strike, show an X, otherwise just show the score
        if (score == 10)
        {
            throw1.text = "X";
        }
        else
        {
            throw1.text = score.ToString();
        }
    }

    // set the text for the second throw
    public void SetFrameThrowTwoText(int score, int frameIndex)
    {
        // if we are on the 10th frame, this means that we did not get a strike in the first shot, so we have to check for this condition
        if (frameIndex == 10)
        {
            // if the score is a 10, then we have scored a spare, and we set the text
            if (score == 10)
            {
                throw2.text = "/";
            }
            else
            {
                throw2.text = score.ToString();
            }
        }
        // if we are not on the 10th frame, but on the "11th" (played when the 10th frame results in a strike)
        else {
            // if the score is again 10, we have double strikes for the 10th frame, so set the text
            if (score == 10)
            {
                throw2.text = "X";
            }
            // if it was not a strike, then just set the text
            else
            {
                throw3.text = score.ToString();
            }
        }
    }

    // sets the text for the 3rd throw (if the user reaches this point)
    public void SetFrameThrowThreeText(int score, int frameIndex)
    {
        // we have to check if we are on the "12th" or "11th" frame. If we are on the "12th" frame, that means that the user currently has
        // double strikes on the 10th frame
        if (frameIndex == 12)
        {
            // if the score is 10, then we have triple strikes on the 10th frame, so set the text
            if (score == 10)
            {
                throw3.text = "X";
            }
            // otherwise, we set the text to the score
            else
            {
                throw3.text = score.ToString();
            }
        }
        // if we are not on the "12th" frame, that means we are on the "11th". This usually means that the user is coming from
        // a spare in the 10th frame
        else
        {
            // if the user has scored a 10, then it is a strike
            if (score == 10)
            {
                throw3.text = "X";
            }
            //otherwise, set the text to the score
            else
            {
                throw3.text = score.ToString();
            }
        }
    }

    // sets the text for the total frame score
    public void SetFrameTotalText(int score)
    {
        frameTotal.text = score.ToString();
    }

}
