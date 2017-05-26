using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

// this script handles all the speech recognition
// we use speech recognition to change a ball's size and color
public class SpeechRecognizer : MonoBehaviour {

    // a list keywords that can be mentioned in the microphone
    public string[] keywords = new string[] { "large", "medium", "small", "red", "green", "blue" };

    // the confidence level for phrase recognition
    public ConfidenceLevel confidence;

    // a reference to the bowling ball
    public GameObject ball;

    // private reference to the phrase recognizer
    private PhraseRecognizer recognizer;

    // private reference to the balls material
    private Material ballMaterial;

    // to keep track of the phrase that was said
    protected string phraseSaid = "right";

	// Use this for initialization
	void Start () {
        // we first check that we have some keywords set
        if (keywords != null) {
            // if so, we initiallize and start the recognizer
            recognizer = new KeywordRecognizer(keywords, confidence);

            // subscribe to the OnPhraseRecognized event, which is called when a valid phrase is recognized
            recognizer.OnPhraseRecognized += HandlePhraseRecognized;

            // start the speech recognition engine
            recognizer.Start();
        }

        // if we have set the ball, we get a reference to it's material
        if (ball != null) {
            ballMaterial = ball.GetComponent<Renderer>().material;
        }

	}

    // called when the application is quit
    private void OnApplicationQuit()
    {
        // we check if we have initialized the recognizer and that it is running
        if (recognizer != null && recognizer.IsRunning) {

            // if so, we unsubscribe from the events and stop the engine
            recognizer.OnPhraseRecognized -= HandlePhraseRecognized;
            recognizer.Stop();
        }
    }

    // called in the event that a phrase is recognized
    private void HandlePhraseRecognized(PhraseRecognizedEventArgs args)
    {
        // get the text from the arguments
        phraseSaid = args.text;

        // a switch to check for which phrase was said
        // based on the phrase said, we either change the color of the ball or it's scale.
        // it is pretty straight forward
        switch (phraseSaid) {
            case "red":
                ballMaterial.color = Color.red;
                break;
            case "green":
                ballMaterial.color = Color.green;
                break;
            case "blue":
                ballMaterial.color = Color.blue;
                break;
            case "small":
                ball.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                break;
            case "medium":
                ball.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                break;
            case "large":
                ball.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                break;
            default:
                break;
        }
    }
}
