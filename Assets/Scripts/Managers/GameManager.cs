using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// this class represents a bowling game frame
[Serializable]
public class Frame
{
    public int frameIndex; // index for the frame
    public int firstThrow = -1; // the number of pins knocked in the first throw
    public int secondThrow = -1; // the number of pins knocked in the second throw
    public bool isSpare; // a flag for a spare frame
    public bool isStrike; // a flag for a strike frame
    public int frameScore = 0; // the frame's total score
    public bool isPendingScore = true; // a flag for determining if a frame has a pending score (ie. strike or spare scored)
}

// this script handles all game related aspects, like keeping score, setting frame scores on the UI, setting and resetting pins,
// activating or deactivating the pin setter and sweeper, etc.
// The game follows the basic rules of bowling. The game consists of 10 frames, of two throws each (except for the 10th frame, which can have a third). 
// Score is based in how many total pins are knocked over (out of a possible 10 in each frame). Bonuses are added for strikes and spares. 
// Since the max bonus is the score of the next two shots, I have designed the game so that it can be played to a max of 12 frames. 
// The 11th and 12th frame in this case are imaginary, and only exist for the cases when the player scores a strike or double strike in the 10th frame.
public class GameManager : MonoBehaviour {

    // this enum is used for the shot state, which is either the first or second shot for a frame
    public enum ShotState {
        First, Second
    }

    public PinSetter pinSetterScript; // reference to the pin setter script
    public PinSweeper pinSweeperScript; // reference to the pin sweeper script
    public ScoreTrigger scoreTrigger; // reference to the score trigger
    public LargeScreenMoviePlayer moviePlayer; // reference to the large screen movie player

    public int startingFrameIndex = 1; // the starting frame index, 1 for the 1st frame

    public LinkedList<Frame> frames; // a linked list of frames for the entire game. 
                                     // We use a linked list so that we can refer to previous and next frames of a specific frame.
                                     // This is used to determine bonuses and previous frame scores

    public Pin[] pins; // an array to hold all the pin game objects (10 in total)
    public Transform[] pinStartPositions; // an array for all the starting positions of a pin (on a pin setter)
    public FrameScore[] frameScoresUI; // an array for all the UI frame scores (from frames 1-9)
    public TenthFrameScore lastFrameScoreUI; // a reference to the last frame score (not in the array since it is different)
    public Transform pinParentTransform; // the parent transform for the pins

    public GameObject canvasMessage; // a reference to the UI canvas with the end game message
    public GameObject bronzeTrophy; // a reference to the bronze trophy award gameobject
    public GameObject silverTrophy; // a reference to the silver trophy award
    public GameObject goldTrophy; // a reference to the gold trophy award

    public AudioClip cheerSound; // reference to cheering sound sfx (for a strike or a spare)

    private AudioSource audioSource; // a reference to the gameobjects audiosource (to play the cheering sounds)

    private ShotState _shotState = ShotState.First; // the state of the current shot
    private const int MAX_PINS = 10; // max number of pins
    private int pinsRemaining = MAX_PINS; // to keep track of the remaining number of pins that are still standing

    // to keep track of the current shot score, which is the number of pins that have been knocked in a specific shot
    private int currShotScore {
        get { return pinsRemaining - scoreTrigger.numPinsStanding; }
        set { ; }
    }

    private int currFrame; // to keep track of the current frame number
    private bool startingNewGame = false; // a flag for when starting a new game

    private bool newGameStarted = false; // a flag for when a new game has been actually started

    private static GameManager _instance;

    public static GameManager Instance {
        get { return _instance; }
    }

    private void OnEnable()
    {
        // we subscribe to the OnBallReachedPit event, which tells us when a ball has reached the pit at the end of the lane.
        // this is were we will consider a shot to have happened
        BaseEventManager.OnBallReachedPit += new EventHandler(HandleBallThrow);
    }

    private void OnDisable()
    {
        // Unsubscribe from event
        BaseEventManager.OnBallReachedPit -= new EventHandler(HandleBallThrow);
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
        // get the audio source
        audioSource = GetComponent<AudioSource>();

        // set the current frame to 1 (we can change starting frame index to other frames to test other frames without having to
        // play all the previous frames. Good for debugging the last frame for instance (the most difficult frame to work with).
        currFrame = startingFrameIndex;

        // initialize the frames linked list
        frames = new LinkedList<Frame>();

        // we start a new frame
        StartNewFrame();
    }

    // this function is called in the event that the ball has reached the pit at the end of the alley
    // it is used to handle a ball throw or shot
    private void HandleBallThrow() {

        // if we are in the 12th frame (2 strikes in the 10th frame)
        if (frames.Last.Value.frameIndex == 12) {
            // we proceed to start the sweep and reset mechanism, since there are no future shots in the game
            StartCoroutine(SweepAndResetMechanism());
            return;
        }

        // ball thrown, start pin sweeper and pin setter mechanism
        // check which shot we are in
        if (_shotState == ShotState.First)
        {
            // after the first shot, we only sweep and pick up and drop the pins
            // then we get a shot to drop the remaining pins
            StartCoroutine(SweepAndSetMechanism());
        }
        else {
            // after the second shot, all remaining pins are swept up and the pin setter
            // resets the pins so that they are all standing
            StartCoroutine(SweepAndResetMechanism());
        }
    }

    // this function sets the score for a given shot
    private void SetShotScore() {

        // first we check what shot we are in
        if (_shotState == ShotState.First)
        {
            // we are in the first shot of the frame

            // if we are starting a new game, and the current frame is thus 1, we should clearout the scores
            // the game allows the user to keep bowling at the end of a game to start a new one, like in a normal
            // bowling alley
            if (startingNewGame && currFrame == 1) {
                // clear out the scoreboard UI
                ClearScores();
                // set the flag
                startingNewGame = false;
            }

            // we set the value of the first throw of the last element of the frames linked list. 
            // the last frame will always be the current frame the player is playing
            frames.Last.Value.firstThrow = currShotScore;
 
            // next we have to check what frame we are in. The 10th frame has a max of 30 poins possible. 
            // to handle this possibility, we create an 11th and 12th imaginary frames so as to handle the bonus shots due to any strikes, spares
            // or double strikes in the 10th frame.
            // since the last frame UI has 4 Texts, we need to know which one to set based on which frame we are on
            if (currFrame == 10) {
                // if we are in the 10th frame, the first shot will always be the UI text for the first box in the frame
                lastFrameScoreUI.SetFrameThrowOneText(currShotScore);
            }
            // if we are in the 11th frame..
            else if (currFrame == 11) {

                // we have to check if we had a strike in the first shot of the 10th frame
                if (frames.Last.Previous.Value.isStrike)
                {
                    // if so, the first shot of the 11th frame will correspond to the 2nd box in the 10th frame UI score
                    lastFrameScoreUI.SetFrameThrowTwoText(currShotScore, currFrame);
                }
                else {
                    // if not, we had a spare, so the score belongs to the 3rd box of the UI for the 10th frame
                    lastFrameScoreUI.SetFrameThrowThreeText(currShotScore, currFrame);
                }
            }
            // if we are on frame 12, then we had two double strikes to start with in the 10th frame
            else if (currFrame == 12) {
                // if we are at this point, the shot score should always be for the 3rd box of the 10th frame UI
                lastFrameScoreUI.SetFrameThrowThreeText(currShotScore, currFrame);
            }
            // else, we are in any of frames 1-9
            else
            {
                // so we set the value of the first throw for the frame UI
                frameScoresUI[currFrame - 1].SetFrameThrowOneText(currShotScore);
            }

            // now that we have recorded the shot score, we proceed to obtain the number of 
            // pins remaining from the score trigger
            pinsRemaining = scoreTrigger.numPinsStanding;

            // check if we have a strike (10 pins down in the first shot)
            // there will be no second shot
            if (frames.Last.Value.firstThrow == 10)
            {
                // we set the flag
                frames.Last.Value.isStrike = true;

                // play the video
                moviePlayer.PlayVideo(LargeScreenMoviePlayer.VideoType.Strike);

                // play the sound
                audioSource.PlayOneShot(cheerSound);

                // no more shots for this frame, so we end it
                EndFrame();
            }
            // TODO: CHECK THIS
            else if (currFrame == 11 || currFrame == 12) {
                EndFrame();
            }
            // else, we are in frames 1-9 and we haven't knocked all the other pins down
            else
            {
                // we set the shot state to the second shot
                _shotState = ShotState.Second;
            }

            // reset the current shot score to 0, for the next shot
            currShotScore = 0;
        }
        // if we are not in the 1st shot, but in the 2nd shot...
        else {
            // set the score for the second throw of the current frame
            // the current frame is always the last frame of the frames linked list
            frames.Last.Value.secondThrow = currShotScore;
            
            // like for the 1st shot, we check if we are in frames 10-12 for the special case of setting
            // the UI for the 10th frame
            if (currFrame == 10)
            {
                // if we are in the 10th frame, the second shot corresponds to the second UI text of the 10th frame
                lastFrameScoreUI.SetFrameThrowTwoText(currShotScore, currFrame);
            }

            else if (currFrame == 11)
            {
                // if we are in the 11th frame, the second shot corresponds to the third UI text of the 10th frame
                lastFrameScoreUI.SetFrameThrowThreeText(currShotScore, currFrame);
            }
            // else, we are in frames 1-9
            else {
                // set the score in the scoreboard text UI for the second shot
                frameScoresUI[currFrame - 1].SetFrameThrowTwoText(currShotScore);
            }

            // now we check if we got a spare (the total of both throws should be 10)
            if (frames.Last.Value.firstThrow + frames.Last.Value.secondThrow == 10) {

                // we check the special conditions for the 10th frame
                if (currFrame == 10)
                {
                    // if we are in the 10th frame, and have a spare, the spare is shown in the second UI text for the frame
                    lastFrameScoreUI.SetFrameThrowTwoText(10, currFrame);
                }
                else if (currFrame == 11)
                {   // if we are in the 11th frame, and have a spare, the spare is shown in the 3rd UI text for the frame
                    lastFrameScoreUI.SetFrameThrowThreeText(10, currFrame);
                }
                // else, we are in frames 1-9 (we will never reach this point for frame 12, which has no 2nd shot)
                else
                {
                    // set the score UI text for the second throw of the frame (to a spare)
                    frameScoresUI[currFrame - 1].SetFrameThrowTwoText(10);
                }

                // play the cheer sound
                audioSource.PlayOneShot(cheerSound);

                // show the spare video
                moviePlayer.PlayVideo(LargeScreenMoviePlayer.VideoType.Spare);

                // set the flag for the frame
                frames.Last.Value.isSpare = true;
            }
        }

        // after each shot, we update the scores, so as to update any bonuses obtained and to update 
        // the scoreboard UI. This is as close as I could get to how real bowling behaves
        UpdateScores();
        // PrintFrames();
    }

    // IEnumerator for controlling the sweep and set mechanism of the pin sweeper and pin setter
    // it also determines the current shot score during the sweep. This is done after a first shot for
    // a frame
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

        // return the pinsweeper to its unsweep position
        pinSweeperScript.SweeperUnsweep();
        yield return new WaitForSeconds(0.55f);

        // the pin setter goes down and drops the pins back into place
        pinSetterScript.ActivateSetter();
        yield return new WaitForSeconds(2.10f);

        // the sweeper goes up
        pinSweeperScript.SweeperUp();
    }

    // IEnumerator for controlling the sweep and reset mechanism of the pin sweeper and pin setter
    // it also determines the current shot score during the sweep. This is done after the second shot for
    // a frame, or after a strike
    IEnumerator SweepAndResetMechanism() {

        // the pin sweeper goes down
        pinSweeperScript.SweeperDown();
        yield return new WaitForSeconds(1.25f);

        // set the score for the current shot
        SetShotScore();

        // the pin sweeper sweeps and unsweeps
        pinSweeperScript.SweeperSweep();
        yield return new WaitForSeconds(0.55f);

        pinSweeperScript.SweeperUnsweep();
        yield return new WaitForSeconds(0.55f);

        // we end the current frame
        EndFrame();

        // last, the pin setter goes down and drops a "new" set of pins
        pinSetterScript.ActivateSetter();
        yield return new WaitForSeconds(2.10f);

        // the sweeper goes up
        pinSweeperScript.SweeperUp();
    }

    // this function starts a new frame
    private void StartNewFrame() {

        // init a new frame
        Frame frame = new Frame();

        // set the index
        frame.frameIndex = currFrame;

        // reset the number of pins remaining for a frame
        pinsRemaining = MAX_PINS;

        // add it to the linked list
        frames.AddLast(frame);
    }

    // function to end a frame
    private void EndFrame() {

        // if we are starting a new game, we want to avoid a bug that repeats the first frame,
        // so we check if we are starting a new game
        if (startingNewGame) {

            // reset the flag and return, doing nothing else
            startingNewGame = false;
            return;
        }

        // we check for end game conditions...

        // if we are in the 10th frame, but we scored no strike or spare, we end the game
        if (currFrame == 10 && !frames.Last.Value.isStrike && !frames.Last.Value.isSpare)
        {
            // Debug.Log("GAME OVER ON FRAME 10!");
            EndGame();
        }
        // if we are in the 11th frame (ie. after strike of spare in the 10th frame), and we have no strike or a spare
        // in the previous fame, we end the game
        else if (currFrame == 11 && (!frames.Last.Value.isStrike || frames.Last.Previous.Value.isSpare))
        {
            // Debug.Log("GAME OVER ON FRAME 11");
            EndGame();
        }
        // if we are in the 12th frame, we always end the game
        else if (currFrame == 12)
        {
            // Debug.Log("GAME OVER ON FRAME 12");
            EndGame();
        }
        // if we are in frames 1-9, we do not end the game, and we increase the counter
        else {
            // increase currFrame count
            currFrame++;
        }

        // Reset the shot state
        _shotState = ShotState.First;

        // reset the pins
        ResetPins();

        // we start a new frame
        StartNewFrame();
    }

    // function called at the end of a game
    private void EndGame() {

        // get a reference to the last frame of a game, which is frame 10 or the 9th element of the linked list
        Frame lastFrame = frames.ElementAt(9);

        // we update the score and check for any awards
        UpdateScores();
        CheckAwards(lastFrame.frameScore);

        // Last, we start a new game
        StartNewGame();
    }

    // this function checks if the user won any awards at the end of a game
    private void CheckAwards(int score) {

        // a string to contain the message
        String message;

        // check for bronze trophy (50-99 points)
        if (score >= 50 && score < 100)
        {
            // set message and activate trophy gameobject
            message = "You unlocked bronze trophy!";
            bronzeTrophy.SetActive(true);
        }
        // else, check for silver trophy (100-149 points)
        else if (score >= 100 && score < 150)
        {
            // set message and activate trophy gameobject
            message = "You unlocked silver trophy!";
            silverTrophy.SetActive(true);
        }
        // else, check for gold trophy (150+ points)
        else if (score >= 150) {
            // set message and activate trophy gameobject
            message = "You unlocked gold trophy!";
            goldTrophy.SetActive(true);
        }
        // else, no award
        else {
            message = "You unlocked no trophy :(";
        }

        // set the text on the UI canvas
        canvasMessage.GetComponent<CanvasMessage>().SetText(score, message);
        
        // show the canvas
        canvasMessage.SetActive(true);
    }

    // starts a new game
    private void StartNewGame() {
        // set the flag
        startingNewGame = true;

        // clear the linked list
        frames.Clear();

        // reset the current frame to #1
        currFrame = 1;
    }

    // this function resets the pins
    private void ResetPins() {

        // we iterate over each pin
        for (int i = 0; i < MAX_PINS; i++) {

            // we set each pins parent to the pin setter
            pins[i].gameObject.transform.parent = pinSetterScript.gameObject.transform;

            // we set each of the pins position to the pin starting position
            pins[i].transform.position = pinStartPositions[i].position;

            // we reset the pins
            pins[i].ResetPin();
        }

        // we tell the pin setter to "pick up the pins", which basically just attaches the pins to it
        // and allows them to be dropped when it goes down the next time
        pinSetterScript.PickUpPins();
    }

    // this function updates the scores for all the frames
    private void UpdateScores()
    {
        // we iterate over each frame in the frames linked list
        foreach (Frame f in frames)
        {
            // if the frame has a pending score, we calculate it
            if (f.isPendingScore)
            {
                CalcScore(f);
            }
        }
    }

    // this function calculates the score for a frame
    // this is used for frames that are still open due to strikes or spares
    private void CalcScore(Frame frame)
    {
        // create a reference to the frame in question as a node (so we can reference its neighbors)
        LinkedListNode<Frame> frameNode = frames.Find(frame);

        // just in case, if we are not pending a score for the frame, we return (should never happen, but you never know)
        if (!frame.isPendingScore)
        {
            return;
        }

        // now we check for strikes or spares
        if (frame.isStrike)
        {
            // we got a strike so we
            // get the strike bonus
            int bonus = GetStrikeBonus(frameNode);

            // if bonus is set to -1, we still do not have enough future shots and frames to determine the total score
            // so we check that we did not get -1 returned
            if (bonus != -1)
            {
                // bonus has been determined, so we set the frame score to the value of the first throw, plus the bonus, plus the score of the
                // previous frame (this is all for a strike)
                frame.frameScore = frame.firstThrow + bonus + GetPreviousFrameScore(frameNode);

                // for setting the text in the UI, we have to check if we are on frame 10 or not
                if (frame.frameIndex != 10)
                {
                    // not the 10th frame, so index the appropiate frame in the array
                    // set the score in the scoreboard...
                    frameScoresUI[frame.frameIndex - 1].SetFrameTotalText(frame.frameScore);
                }
                else {
                    // we are on the last frame, so set the total score text on it
                    lastFrameScoreUI.SetFrameTotalText(frame.frameScore);
                }

                // we have determined the score for this frame, so it is no longer pending
                frame.isPendingScore = false;
            }
        }
        // now we check for spares in a frame
        else if (frame.isSpare)
        {
            // get the spare bonus...
            int bonus = GetSpareBonus(frameNode);

            // again, we check for the bonus. If -1 is returned, we still don't have enough future shots or frames (should be the next one)
            // to determine final score. We check for this condition here.
            if (bonus != -1) {

                // a valid bonus score was returned, so we set the total score for the frame.
                // for a spare, the total score is the frames first throw + the second throw + the bonus + the score of the previous frame
                frame.frameScore = frame.firstThrow + frame.secondThrow + bonus + GetPreviousFrameScore(frameNode);

                // again, we check if we are on frame 10 or not (frame 10 has a different UI structure)
                if (frame.frameIndex != 10)
                {
                    // not in frame 10, we use the array
                    frameScoresUI[frame.frameIndex - 1].SetFrameTotalText(frame.frameScore);
                }
                else {
                    // in frame 10, so directly set it
                    lastFrameScoreUI.SetFrameTotalText(frame.frameScore);
                }

                // we obtained a score, so not pending anymore
                frame.isPendingScore = false;
            }

        }
        // else, we do not have a strike or a spare
        else {
            // we make sure the second throw has been set (so it will not be the default of -1)
            if (frame.secondThrow != -1) {

                // we simply set the frame score to the value of both throws plus the score of the previous frame
                frame.frameScore = frame.firstThrow + frame.secondThrow + GetPreviousFrameScore(frameNode);

                // again, for the UI, we check if we are on the 10th frame or not
                if (frame.frameIndex != 10)
                {
                    // not on the 10th frame, set the UI by using the array
                    frameScoresUI[frame.frameIndex - 1].SetFrameTotalText(frame.frameScore);
                }
                else {
                    // on the 10th frame, set the UI directly
                    lastFrameScoreUI.SetFrameTotalText(frame.frameScore);
                }

                // score determined, so no longer pending
                frame.isPendingScore = false;
            }
        }
    }

    // determines the bonus for frames with a spare
    private int GetSpareBonus(LinkedListNode<Frame> node)
    {
        // check that we have the next shot available
        if (node.Next == null || node.Next.Value.firstThrow == -1) {
            // not available, return -1
            return -1;
        }

        // we have a next shot. For spares, only the next shot's value is added as a bonus, so we return the
        // value of the first throw for the nodes next neighbor
        return node.Next.Value.firstThrow;
    }

    // determines the bonus for frames with a strike
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
                // we dont, return -1
                return -1;
            }

            // if we are here, we have two valid future frames for double strike
            // since there are no second throws (due to the strikes), we return the
            // first throw of the next node in the linked list, and the node right after that.
            return node.Next.Value.firstThrow + node.Next.Next.Value.firstThrow;
        }

        // CASE 2: NOT DOUBLE STRIKES
        // we need the two values for this frame
        // since it is not a double strike, we know we have to have the first and second throws of the next node in the linked
        // list
        else if ((node.Next.Value.firstThrow == -1 || node.Next.Value.secondThrow == -1) && !node.Next.Value.isStrike)
        {
            // we dont, so we return -1
            return -1;
        }
        else
        {
            // we have the next two values for the frame
            return node.Next.Value.firstThrow + node.Next.Value.secondThrow;
        }
    }

    // gets the score of the previous frame for a specific frame node
    private int GetPreviousFrameScore(LinkedListNode<Frame> node)
    {
        // if we have no previous node (ie, the first frame, we return 0)
        if (node.Previous == null)
        {
            return 0;
        }

        // we do have a previous node, so we return its frame score
        return node.Previous.Value.frameScore;
    }

    // this function clears all the scores from the scoreboard UI
    private void ClearScores() {

        // iterate over each frame score in the frame scores UI
        foreach (FrameScore f in frameScoresUI) {
            // clear it
            f.Clear();
        }

        // we do it directly for frame 10, since it has a different structure
        lastFrameScoreUI.Clear();
    }

    // this debug function was used during development and testing to print
    // the values of all the frames. Not currently used, but still good to have
    // for debugging. 
    private void PrintFrames() {

        // iterate over each frame and print the contents
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