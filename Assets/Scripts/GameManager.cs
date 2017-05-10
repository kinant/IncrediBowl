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
    public LinkedList<Frame> frames;

    private ShotState _shotState = ShotState.First;
    private int currShotScore = 0;
    private int currFrame = 1;

    private static GameManager _instance;

    public static GameManager Instance {
        get { return _instance; }
    }

    private void OnEnable()
    {
        BaseEventManager.OnSweeperCompleteSweep += new EventHandler(HandleBallThrow);
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

    public void PinDown() {
        currShotScore++;
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
        pinSetterScript.InitNewPins();
    }

    private void HandleBallThrow() {
        Debug.Log("BALL SHOT: ");
        Debug.Log("Num pins down: " + currShotScore);

        switch (_shotState) {
            case ShotState.First:
                if (currShotScore == 10)
                {
                    frames.Last.Value.isStrike = true;
                    EndFrame();
                }
                else {
                    _shotState = ShotState.Second;
                }

                frames.Last.Value.firstThrow = currShotScore;
                currShotScore = 0;
                break;
            case ShotState.Second:
                if (frames.Last.Value.firstThrow + currShotScore == 10)
                {
                    frames.Last.Value.isSpare = true;
                }

                frames.Last.Value.secondThrow = currShotScore;
                currShotScore = 0;
                EndFrame();
                break;
        }

        UpdateScores();
        PrintFrames();
    }

    private void StartNewFrame() {
        Frame frame = new Frame();
        frame.frameIndex = currFrame;
        frames.AddLast(frame);
    }

    private void EndFrame() {
        _shotState = ShotState.First;
        currFrame++;
        StartNewFrame();
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

