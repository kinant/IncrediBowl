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

// this script handles all game related aspects, like keeping score, setting frame scores on the UI, setting and resetting pins,
// activating or deactivating the pin setter and sweeper, etc.
public class GameManager : MonoBehaviour {

    public enum ShotState {
        First, Second
    }

    public PinSetter pinSetterScript;
    public PinSweeper pinSweeperScript;
    public ScoreTrigger scoreTrigger;
    public LargeScreenMoviePlayer moviePlayer;

    public int startingFrameIndex = 1;

    public LinkedList<Frame> frames;
    public Pin[] pins;
    public Transform[] pinStartPositions;
    public FrameScore[] frameScoresUI;
    public TenthFrameScore lastFrameScoreUI;
    public Transform pinParentTransform;
    public Transform pinsNewFrameStart;

    public GameObject endMessage;
    public GameObject bronzeTrophy;
    public GameObject silverTrophy;
    public GameObject goldTrophy;

    public AudioClip cheerSound;

    private AudioSource audioSource;

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
    private bool newGameStarted = false;

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
        // Debug.Log("FRAME UI COUNT: " + frameScoresUI.Length);
        audioSource = GetComponent<AudioSource>();
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
            if (startingNewGame && currFrame == 1) {
                ClearScores();
                startingNewGame = false;
            }

            frames.Last.Value.firstThrow = currShotScore;

            // Debug.Log("FRAME UI INDEX: " + (currFrame - 1));
            // set the score in the scoreboard...
 

            if (currFrame == 10) {
                lastFrameScoreUI.SetFrameThrowOneText(currShotScore);
            }
            else if (currFrame == 11) {
                if (frames.Last.Previous.Value.isStrike)
                {
                    lastFrameScoreUI.SetFrameThrowTwoText(currShotScore, currFrame);
                }
                else {
                    lastFrameScoreUI.SetFrameThrowThreeText(currShotScore, currFrame);
                }
            }
            else if (currFrame == 12) {
                lastFrameScoreUI.SetFrameThrowThreeText(currShotScore, currFrame);
            }
            else
            {
                frameScoresUI[currFrame - 1].SetFrameThrowOneText(currShotScore);
            }


            pinsRemaining = scoreTrigger.numPinsStanding;

            // check if we have a strike
            if (frames.Last.Value.firstThrow == 10)
            {
                frames.Last.Value.isStrike = true;

                // play the video
                moviePlayer.PlayVideo(LargeScreenMoviePlayer.VideoType.Strike);
                audioSource.PlayOneShot(cheerSound);

                EndFrame();
            }
            else if (currFrame == 11 || currFrame == 12) {
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
            
            if (currFrame == 10)
            {
                lastFrameScoreUI.SetFrameThrowTwoText(currShotScore, currFrame);
            }

            else if (currFrame == 11)
            {
                lastFrameScoreUI.SetFrameThrowThreeText(currShotScore, currFrame);
            }
            else {
                // set the score in the scoreboard...
                frameScoresUI[currFrame - 1].SetFrameThrowTwoText(currShotScore);
            }

            if (frames.Last.Value.firstThrow + frames.Last.Value.secondThrow == 10) {

                if (currFrame == 10)
                {
                    lastFrameScoreUI.SetFrameThrowTwoText(10, currFrame);
                }
                else if (currFrame == 11)
                {
                    lastFrameScoreUI.SetFrameThrowThreeText(10, currFrame);
                }
                else
                {

                    frameScoresUI[currFrame - 1].SetFrameThrowTwoText(10);
                }

                audioSource.PlayOneShot(cheerSound);
                moviePlayer.PlayVideo(LargeScreenMoviePlayer.VideoType.Spare);
                frames.Last.Value.isSpare = true;
            }
        }

        UpdateScores();
        // PrintFrames();
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

        if (currFrame == 10 && !frames.Last.Value.isStrike && !frames.Last.Value.isSpare)
        {
            Debug.Log("GAME OVER ON FRAME 10!");
            EndGame();
        }
        else if (currFrame == 11 && (!frames.Last.Value.isStrike || frames.Last.Previous.Value.isSpare))
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
        Frame lastFrame = frames.ElementAt(9);
        // next we reset the game to play again
        UpdateScores();
        CheckAwards(lastFrame.frameScore);
        // PrintFrames();
        StartNewGame();
    }

    private void CheckAwards(int score) {

        String message;

        // check for bronze trophy
        if (score >= 50 && score < 100)
        {
            message = "You unlocked bronze trophy!";
            bronzeTrophy.SetActive(true);
        }
        // else, check for silver trophy
        else if (score >= 100 && score < 150)
        {
            message = "You unlocked silver trophy!";
            silverTrophy.SetActive(true);
        }
        // else, check for gold trophy
        else if (score >= 150) {
            message = "You unlocked gold trophy!";
            goldTrophy.SetActive(true);
        }
        // else, no award
        else {
            message = "You unlocked no trophy :(";
        }

        endMessage.GetComponent<EndMessage>().SetText(score, message);
        // show the message dialog
        endMessage.SetActive(true);
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
                frame.frameScore = frame.firstThrow + bonus + GetPreviousFrameScore(frameNode);

                if (frame.frameIndex != 10)
                {
                    // set the score in the scoreboard...
                    frameScoresUI[frame.frameIndex - 1].SetFrameTotalText(frame.frameScore);
                }
                else {
                    lastFrameScoreUI.SetFrameTotalText(frame.frameScore);
                }

                frame.isPendingScore = false;
            }
        }
        else if (frame.isSpare)
        {
            // get the spare bonus...
            int bonus = GetSpareBonus(frameNode);

            if (bonus != -1) {
                frame.frameScore = frame.firstThrow + frame.secondThrow + bonus + GetPreviousFrameScore(frameNode);

                if (frame.frameIndex != 10)
                {
                    frameScoresUI[frame.frameIndex - 1].SetFrameTotalText(frame.frameScore);
                }
                else {
                    lastFrameScoreUI.SetFrameTotalText(frame.frameScore);
                }

                frame.isPendingScore = false;
            }

        }
        else {
            if (frame.secondThrow != -1) {
                frame.frameScore = frame.firstThrow + frame.secondThrow + GetPreviousFrameScore(frameNode);

                if (frame.frameIndex != 10)
                {
                    frameScoresUI[frame.frameIndex - 1].SetFrameTotalText(frame.frameScore);
                }
                else {
                    lastFrameScoreUI.SetFrameTotalText(frame.frameScore);
                }

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

    private void ClearScores() {
        foreach (FrameScore f in frameScoresUI) {
            f.Clear();
        }

        lastFrameScoreUI.Clear();
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

