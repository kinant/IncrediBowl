using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndMessage : MonoBehaviour {

    public Text scoreText;
    public Text awardText;

    public void SetText(int score, string text) {
        scoreText.text = "Your score was: " + score.ToString();
        awardText.text = text;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
