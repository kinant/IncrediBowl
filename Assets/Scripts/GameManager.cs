using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Frame
{
    public int frameIndex;
    public int firstThrow = -1;
    public int secondThrow = -1;
    public bool isSpare;
    public bool isStrike;
    public int frameScore = 0;
    public bool isPendingScore = true;
}

public class GameManager : MonoBehaviour {

    public enum ShotState {
        First, Second
    }

    public PinSetter pinSetterScript;
    public PinSweeper pinSweeperScript;
    public ScoreTrigger scoreTrigger;

    public int startingFrameIndex = 1;

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

    private int currFrame;
    private bool startingNewGame = false;

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

    private void Start()
    {
        currFrame = startingFrameIndex;
        frames = new LinkedList<Frame>();
        StartNewFrame();
    }

    private void HandleBallThrow() {
        if (frames.Last.Value.frameIndex == 12) {
            StartCoroutine(SweepAndResetMechanism());
            return;
        }

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
            frames.Last.Value.firstThrow = currShotScore;
            pinsRemaining = scoreTrigger.numPinsStanding;

            // check if we have a strike
            if (frames.Last.Value.firstThrow == 10)
            {
                frames.Last.Value.isStrike = true;
                frames.Last.Value.isPendingScore = true;
                EndFrame();
            }
            else
            {
                _shotState = ShotState.Second;
            }

            currShotScore = 0;
        }
        else {
            frames.Last.Value.secondThrow = currShotScore;

            if (frames.Last.Value.firstThrow + frames.Last.Value.secondThrow == 10) {
                frames.Last.Value.isSpare = true;
            }
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

        EndFrame();
        // last, the pin setter goes down and drops the pins
        pinSetterScript.ActivateSetter();
        yield return new WaitForSeconds(2.10f);

        // the sweeper goes up
        pinSweeperScript.SweeperUp();
        // StartNewFrame();
    }

    private void StartNewFrame() {
        Frame frame = new Frame();
        frame.frameIndex = currFrame;
        pinsRemaining = MAX_PINS;
        frames.AddLast(frame);
    }

    private void EndFrame() {

        if (startingNewGame) {
            startingNewGame = false;
            return;
        }

        if (currFrame == 10 && !frames.Last.Value.isStrike)
        {
            Debug.Log("GAME OVER ON FRAME 10!");
            EndGame();
        }
        else if (currFrame == 11 && !frames.Last.Value.isStrike)
        {
            Debug.Log("GAME OVER ON FRAME 11");
            EndGame();
        }
        else if (currFrame == 12)
        {
            Debug.Log("GAME OVER ON FRAME 12");
            EndGame();
        }
        else {
            // increase currFrame count
            currFrame++;
        }

        // Reset
        _shotState = ShotState.First;
        ResetPins();
        StartNewFrame();
    }

    private void EndGame() {
        // Frame lastFrame = frames.ElementAt(9);
        // Debug.Log("GAME HAS ENDED WITH SCORE OF: " + lastFrame.frameScore);

        // next we reset the game to play again
        UpdateScores();
        PrintFrames();
        StartNewGame();
    }

    private void StartNewGame() {
        startingNewGame = true;
        frames.Clear();
        currFrame = 1;
    }

    private void ResetPins() {
        for (int i = 0; i < MAX_PINS; i++) {
            pins[i].gameObject.transform.parent = pinSetterScript.gameObject.transform;
            pins[i].transform.position = pinStartPositions[i].position;
            pins[i].ResetPin();
        }

        pinSetterScript.PickUpPins();
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
        LinkedListNode<Frame> frameNode = frames.Find(frame);

        if (!frame.isPendingScore)
        {
            return;
        }

        if (frame.isStrike)
        {
            // get the strike bonus
            int bonus = GetStrikeBonus(frameNode);

            if (bonus != -1)
            {
                Debug.Log("Bonus Obtained - bonus: " + bonus + " \n shot: " + frame.firstThrow + "\n prev: " + GetPreviousFrameScore(frameNode));

                frame.frameScore = frame.firstThrow + bonus + GetPreviousFrameScore(frameNode);
                frame.isPendingScore = false;
            }
        }
        else if (frame.isSpare)
        {
            // get the spare bonus...
            int bonus = GetSpareBonus(frameNode);

            if (bonus != -1) {

                Debug.Log("Bonus Obtained - bonus: " + bonus + " \n shot: " + frame.firstThrow + "\n prev: " + GetPreviousFrameScore(frameNode));

                frame.frameScore = frame.firstThrow + frame.secondThrow + bonus + GetPreviousFrameScore(frameNode);
                frame.isPendingScore = false;
            }

        }
        else {
            if (frame.secondThrow != -1) {
                frame.frameScore = frame.firstThrow + frame.secondThrow + GetPreviousFrameScore(frameNode);
                frame.isPendingScore = false;
            }
        }
    }

    private int GetSpareBonus(LinkedListNode<Frame> node)
    {
        // check that we have the next shot available
        if (node.Next == null || node.Next.Value.firstThrow == -1) {
            return -1;
        }

        // we have a next shot
        return node.Next.Value.firstThrow;
    }

    private int GetStrikeBonus(LinkedListNode<Frame> node)
    {
        // check that we have at least one future frame
        if (node.Next == null)
        {
            return -1;
        }

        // CASE 1: DOUBLE STRIKES
        if (node.Next.Value.isStrike)
        {

            // check that we have another future frame
            if (node.Next.Next == null || node.Next.Next.Value.firstThrow == -1)
            {
                return -1;
            }

            // if we are here, we have two valid future frames for double strike
            return node.Next.Value.firstThrow + node.Next.Next.Value.firstThrow;
        }

        // CASE 2: NOT DOUBLE STRIKES
        // we need the two values for this frame
        else if ((node.Next.Value.firstThrow == -1 || node.Next.Value.secondThrow == -1) && !node.Next.Value.isStrike)
        {
            return -1;
        }
        else
        {

            // we have the next two values for the frame
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

    private void PrintFrames() {
        foreach (Frame frame in frames) {
            Debug.Log("==================================================" + "\n" +
                "Frame: " + frame.frameIndex +"\n" +
                "#1: " + frame.firstThrow + "\n" +
                "#2: " + frame.secondThrow + "\n" +
                "score: " + frame.frameScore + "\n" +
                "Pending Score: " + frame.isPendingScore + "\n" +
                "is strike?" + frame.isStrike + "\n" +
                "is spare?" + frame.isSpare + "\n" +
                "==================================================");
        }
    }

}

