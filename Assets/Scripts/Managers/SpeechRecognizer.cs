using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechRecognizer : MonoBehaviour {

    public string[] keywords = new string[] { "large", "medium", "small", "red", "green", "blue" };
    public ConfidenceLevel confidence;

    public GameObject ball;

    private PhraseRecognizer recognizer;
    private Material ballMaterial;

    protected string phraseSaid = "right";

	// Use this for initialization
	void Start () {
        if (keywords != null) {
            recognizer = new KeywordRecognizer(keywords, confidence);
            recognizer.OnPhraseRecognized += HandlePhraseRecognized;
            recognizer.Start();
        }

        if (ball != null) {
            ballMaterial = ball.GetComponent<Renderer>().material;
        }

	}

    private void OnApplicationQuit()
    {
        if (recognizer != null && recognizer.IsRunning) {
            recognizer.OnPhraseRecognized -= HandlePhraseRecognized;
            recognizer.Stop();
        }
    }

    private void HandlePhraseRecognized(PhraseRecognizedEventArgs args)
    {
        phraseSaid = args.text;
        Debug.Log("WILL SPAWN: " + phraseSaid);

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
