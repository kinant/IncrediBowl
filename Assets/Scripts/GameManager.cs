using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Frame
{
    public int frameIndex;
    public int firstThrow = -1;
    public int secondThrow = -1;
    public bool isSpare;
    public bool isStrike;
    public int frameScore = -1;
}

public class GameManager : MonoBehaviour {

    public enum ShotState {
        First, Second
    }

    public PinSetter pinSetterScript;
    public PinSweeper pinSweeperScript;
    public ScoreTrigger scoreTrigger;

    public LinkedList<Frame> frames;
    public Pin[] pins;
    public Transform[] pinStartPositions;
    public Transform pinParentTransform;
    public Transform pinsNewFrameStart;

    private ShotState _shotState = ShotState.First;
    private const int MAX_PINS = 10;
    private int pinsRemaining = MAX_PINS;

    private int currShotScore {
        get { return pinsRemaining - scoreTrigger.numPinsStanding; }
        set { ; }
    }

    private int currFrame = 1;

    private static GameManager _instance;

    public static GameManager Instance {
        get { return _instance; }
    }

    private void OnEnable()
    {
        //BaseEventManager.OnSweeperCompleteSweep += new EventHandler(HandleBallThrow);
        BaseEventManager.OnBallReachedPit += new EventHandler(HandleBallThrow);
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
        frames = new LinkedList<Frame>();
        StartNewFrame();
    }

    private void HandleBallThrow() {
        // ball thrown, start pin sweeper and pin setter mechanism
        if (_shotState == ShotState.First)
        {
            StartCoroutine(SweepAndSetMechanism());
        }
        else {
            StartCoroutine(SweepAndResetMechanism());
        }
    }

    private void SetShotScore() {
        if (_shotState == ShotState.First)
        {
           // Debug.Log("First shot score: " + currShotScore);
            frames.Last.Value.firstThrow = currShotScore;
            pinsRemaining = scoreTrigger.numPinsStanding;

            // check if we have a strike
            if (frames.Last.Value.firstThrow == 10)
            {
                Debug.Log("SRTIKE!");
                EndFrame();
                StartNewFrame();
            }
            else {
                _shotState = ShotState.Second;
            }

            currShotScore = 0;
        }
        else {
            // Debug.Log("Second shot score: " + currShotScore);
            frames.Last.Value.secondThrow = currShotScore;
           
        }

        UpdateScores();
        PrintFrames();
    }

    IEnumerator SweepAndSetMechanism() {
        // first the pin sweeper goes down
        pinSweeperScript.SweeperDown();
        yield return new WaitForSeconds(1.25f);

        // the pin setter goes down, picks up the pins, then goes up.
        pinSetterScript.ActivateSetter();
        yield return new WaitForSeconds(2.50f);

        // the pin sweeper sweeps and unsweeps
        pinSweeperScript.SweeperSweep();
        yield return new WaitForSeconds(0.55f);

        // we can now set the score for this shot...
        SetShotScore();

        pinSweeperScript.SweeperUnsweep();
        yield return new WaitForSeconds(0.55f);

        // last, the pin setter goes down and drops the pins
        pinSetterScript.ActivateSetter();
        yield return new WaitForSeconds(2.10f);

        // the sweeper goes up
        pinSweeperScript.SweeperUp();
    }

    IEnumerator SweepAndResetMechanism() {
        pinSweeperScript.SweeperDown();
        yield return new WaitForSeconds(1.25f);

        SetShotScore();

        // the pin sweeper sweeps and unsweeps
        pinSweeperScript.SweeperSweep();
        yield return new WaitForSeconds(0.55f);

        pinSweeperScript.SweeperUnsweep();
        yield return new WaitForSeconds(0.55f);

        // SetShotScore();
        EndFrame();
        // last, the pin setter goes down and drops the pins
        pinSetterScript.ActivateSetter();
        yield return new WaitForSeconds(2.10f);

        // the sweeper goes up
        pinSweeperScript.SweeperUp();

        // Reset
        currShotScore = 0;
        _shotState = ShotState.First;
        StartNewFrame();
    }

    private void StartNewFrame() {
        Frame frame = new Frame();
        frame.frameIndex = currFrame;
        pinsRemaining = MAX_PINS;
        frames.AddLast(frame);
    }

    private void EndFrame() {
        _shotState = ShotState.First;
        currFrame++;
        // set new pins
        ResetPins();
    }

    private void ResetPins() {
        for (int i = 0; i < MAX_PINS; i++) {
            pins[i].gameObject.transform.parent = pinSetterScript.gameObject.transform;
            pins[i].transform.position = pinStartPositions[i].position;
            pins[i].ResetPin();
        }

        pinSetterScript.PickUpPins();
        pinSetterScript.InitNewFrame();
    }

    public ShotState currShotState {
        get { return _shotState;}
    }

    private void UpdateScores()
    {
        foreach (Frame f in frames)
        {
            CalcScore(f);
        }
    }

    private void CalcScore(Frame frame)
    {

        if (frame.frameScore > -1)
        {
            return;
        }

        LinkedListNode<Frame> node = frames.Find(frame);

        // it was a strike...
        if (frame.isStrike)
        {
            int bonus = GetStrikeBonus(node);
            frame.frameScore = (bonus < 0) ? -1 : frame.firstThrow + bonus + GetPreviousFrameScore(node);
        }
        // it was a spare...
        else if (frame.isSpare)
        {
            int bonus = GetSpareBonus(node);
            frame.frameScore = (bonus < 0) ? -1 : frame.firstThrow + frame.secondThrow + bonus + GetPreviousFrameScore(node);
        }
        // it was a normal shot...
        else
        {

            // we do not have a second shot yet...so no update to score
            if (frame.secondThrow < 0)
            {
                return;
            }

            frame.frameScore = frame.firstThrow + frame.secondThrow + GetPreviousFrameScore(node);
        }
    }

    private int GetSpareBonus(LinkedListNode<Frame> node)
    {
        // check that we have enough "future" shots
        if (node.Next == null)
        {
            return -1;
        }

        if (node.Next.Value.isStrike)
        {
            return 10;
        }
        else
        {

            return node.Next.Value.firstThrow;
        }
    }

    private int GetStrikeBonus(LinkedListNode<Frame> node)
    {

        // check that we have enough "future" shots
        if (node.Next == null)
        {
            return -1;
        }

        if (node.Next.Value.isStrike && node.Next.Next == null && node.Value.frameIndex != 10)
        {
            return -1;
        }

        // if we got a strike on the 10th frame...
        if (node.Value.frameIndex == 10)
        {
            if (node.Next.Value.firstThrow != -1 && node.Next.Value.secondThrow != -1)
            {
                // get the values for the "11th" frame
                return node.Next.Value.firstThrow + node.Next.Value.secondThrow;
            }
            else
            {
                return -1;
            }
        }

        // condition 1: next shot was a strike
        if (node.Next.Value.isStrike && node.Next.Value.frameIndex != 11)
        {
            // if so, the next shot is for the frame after
            // check if it was a strike or not
            if (node.Next.Next.Value.isStrike)
            {
                return 20;
            }
            else
            {
                return 10 + node.Next.Next.Value.firstThrow;
            }
        }
        else
        {
            return node.Next.Value.firstThrow + node.Next.Value.secondThrow;
        }
    }

    private int GetPreviousFrameScore(LinkedListNode<Frame> node)
    {
        if (node.Previous == null)
        {
            return 0;
        }

        return node.Previous.Value.frameScore;
    }

    private void PrintFrames()
    {
        foreach (Frame frame in frames)
        {

            if (frame.frameIndex == 11)
            {
                break;
            }

            Debug.Log("======== FRAME " + frame.frameIndex + " ========");

            if (frame.isStrike)
            {
                Debug.Log("Throw 1: X");

                if (frame.frameIndex == 10)
                {
                    LinkedListNode<Frame> n = frames.Find(frame);

                    if (n.Next == null)
                    {
                        continue;
                    }

                    int val1 = n.Next.Value.firstThrow;
                    int val2 = n.Next.Value.secondThrow;

                    if (val1 > -1)
                    {
                        if (val1 == 10)
                        {
                            Debug.Log("Throw 2: X");
                        }
                        else
                        {
                            Debug.Log("Throw 2: " + n.Next.Value.firstThrow);
                        }
                    }
                    if (val2 > -1)
                    {

                        if (val2 == 10)
                        {
                            Debug.Log("Throw 3: X");
                        }
                        else
                        {
                            Debug.Log("Throw 3:" + n.Next.Value.secondThrow);
                        }
                    }
                }
            }
            else if (frame.isSpare)
            {
                Debug.Log("Throw 1: " + frame.firstThrow);
                Debug.Log("Throw 2: /");
            }
            else
            {
                Debug.Log("Throw 1: " + frame.firstThrow);
                if (frame.secondThrow < 0)
                {
                    Debug.Log("Throw 2: ");
                }
                else
                {
                    Debug.Log("Throw 2: " + frame.secondThrow);
                }
            }

            if (frame.frameScore < 0)
            {
                Debug.Log("Frame Score:  ");
            }
            else
            {
                Debug.Log("Frame Score: " + frame.frameScore);
            }

            Debug.Log("====================");
            Debug.Log(" ");
        }
    }
}

