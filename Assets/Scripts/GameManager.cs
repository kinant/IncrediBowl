using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public enum ShotState {
        First, Second
    }

    public PinSetter pinSetterScript;

    private ShotState _shotState = ShotState.First;
    private static GameManager _instance;

    public static GameManager Instance {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void ChangeShot() {
        if (_shotState == ShotState.First)
        {
            _shotState = ShotState.Second;
        }
        else {
            _shotState = ShotState.First;
        }
    }

    private void Start()
    {
        pinSetterScript.InitNewPins();
    }

    public ShotState currShotState {
        get { return _shotState;}
    }
}
