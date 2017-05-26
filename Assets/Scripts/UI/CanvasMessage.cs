using UnityEngine;
using UnityEngine.UI;

// This class handles the UI Canvas that displays a message at the end of each game
public class CanvasMessage : MonoBehaviour {

    public Text scoreText; // the text to display the score
    public Text awardText; // the text to display any awards

    // This function sets the text to display on the UI Canvas
    public void SetText(int score, string text) {
        // set the score text
        scoreText.text = "Your score was: " + score.ToString();

        // set the award text
        awardText.text = text;
    }

    // This function is called to hide the UI Canvas. Called when the laser pointer is pointing at the button
    // and the trigger is pressed
    public void Hide()
    {
        // deactivate the canvas
        gameObject.SetActive(false);
    }
}
