using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


[Serializable]
public class PlayerData
{
    public string name = "Name";

    public enum Type
    {
        Click,
        Random,
        Minimax,
    }

    public Type type = Type.Random;
    [Range(1, 7)] public int minimaxDepth = 2;
}


public class InGamePlayer
{
    public PlayerData data;
    public Board board;
    public TMP_Text nameText;
    public Animator nameTextAnimator;
    public InGamePlayer opponent;

    public Counter chosenHole;

    public IEnumerator ChoseHole()
    {
        chosenHole = null;

        if (data.type == PlayerData.Type.Click)
        {
            Counter hole;

#if UNITY_EDITOR
            
            while (Time.timeScale == 0 || // TODO
                !Input.GetMouseButtonDown(0) ||
                !Physics.Raycast(
                    Camera.main.ScreenPointToRay(Input.mousePosition),
                    out RaycastHit hit,
                    100,
                    GameManager.Inst.HoleLayer) ||
                !hit.transform.TryGetComponentInParent(out hole) ||
                !board.Has(hole) ||
                hole.Value <= 0)
                yield return null;

#elif UNITY_ANDROID

            while (true)
            {
                // wait down
                while (Input.touchCount == 0)
                    yield return null;

                float tDown = Time.time;

                bool downAsHit = Physics.Raycast(
                    Camera.main.ScreenPointToRay(Input.GetTouch(0).position),
                    out RaycastHit downHit,
                    100,
                    GameManager.Inst.HoleLayer);

                // wait up
                Vector2 touchPosition = Input.GetTouch(0).position;

                while (Input.touchCount > 0)
                {
                    touchPosition = Input.GetTouch(0).position;
                    yield return null;
                }

                float tUp = Time.time;
                float dt = tUp - tDown;
                float dtMaxClick = 0.3f;

                bool upAsHit = Physics.Raycast(
                    Camera.main.ScreenPointToRay(touchPosition),
                    out RaycastHit upHit,
                    100,
                    GameManager.Inst.HoleLayer);

                if (Time.timeScale == 0 || // TODO
                    dt > dtMaxClick ||
                    !downAsHit ||
                    !upAsHit ||
                    !downHit.transform.TryGetComponentInParent(out hole) ||
                    upHit.transform.GetComponentInParent<Counter>() != hole ||
                    !board.Has(hole) ||
                    hole.Value <= 0)
                    continue;

                break;
            }

#endif

            chosenHole = hole;
        }
        else if (data.type == PlayerData.Type.Random)
        {
            List<Counter> choices = new List<Counter>();

            foreach (Counter hole in board.Holes)
                if (hole.Value > 0)
                    choices.Add(hole);

            if (choices.Count == 0)
                yield break;


            float reflectionTime = board.CountFilledHoles() > 1 ? GameManager.Inst.BotReflectionTime(Random.Range(-2f, 2f)) :
                                                                  GameManager.Inst.BotMinTime;

            yield return new WaitForSeconds(reflectionTime);

            chosenHole = choices.Random();
        }

        else if (data.type == PlayerData.Type.Minimax)
        {
            (int holeIdx, float value, float dValue) = Minimax.BestMove(new AwaleState(board, opponent.board), data.minimaxDepth);

            float reflectionTime = board.CountFilledHoles() > 1 ? GameManager.Inst.BotReflectionTime(dValue) :
                                                                  GameManager.Inst.BotMinTime;

            yield return new WaitForSeconds(reflectionTime);

            chosenHole = board.Holes[holeIdx];
        }
    }
}


class AwaleState : IMinimaxState<int>
{
    AwaleData data;

    public AwaleState(Board playerBoard, Board opponentBoard)
    {
        data = new AwaleData(playerBoard, opponentBoard);
    }

    public AwaleState(AwaleData data)
    {
        this.data = data;
    }

    public float CantPlayEvaluation(bool playerTurn)
    {
        // hungry
        BoardData boardTurn = data.GetBoard(playerTurn);
        BoardData opponentBoard = data.OpponentBoard(boardTurn);

        for (int holeIdx = 0; holeIdx < opponentBoard.holes.Length; holeIdx++)
        {
            if (opponentBoard.holes[holeIdx] <= 0)
                continue;

            boardTurn.score += opponentBoard.holes[holeIdx];
            opponentBoard.holes[holeIdx] = 0;
        }

        return Evaluate();
    }

    public List<int> Moves(bool playerTurn)
    {
        BoardData board = data.GetBoard(playerTurn);

        List<int> moves = new List<int>();

        for (int holeIdx = 0; holeIdx < board.holes.Length; holeIdx++)
            if (board.holes[holeIdx] > 0)
                moves.Add(holeIdx);

        return moves;
    }

    public int NoMove() => -1;

    public IMinimaxState<int> Play(int move, bool playerTurn)
        => new AwaleState(data.Play(move, playerTurn));

    public float Evaluate()
    {
        if (data.b1.score >= GameManager.Inst.ScoreMinWin)
            return float.PositiveInfinity;

        if (data.b2.score >= GameManager.Inst.ScoreMinWin)
            return float.NegativeInfinity;

        return data.b1.score - data.b2.score;
    }
}

class AwaleData
{
    public BoardData b1, b2;

    public AwaleData(BoardData b1, BoardData b2)
    {
        this.b1 = b1;
        this.b2 = b2;
    }

    public AwaleData(Board b1, Board b2)
    {
        this.b1 = new BoardData(b1);
        this.b2 = new BoardData(b2);
    }

    AwaleData Clone() => new AwaleData(b1.Clone(), b2.Clone());

    public AwaleData Play(int holeIdx, bool fromB1)
    {
        AwaleData clone = Clone();
        BoardData fromBoard = fromB1 ? clone.b1 : clone.b2;
        BoardData crtBoard = fromBoard;
        int startingHoleIdx = holeIdx;

        // take
        int tokens = fromBoard.holes[holeIdx];
        fromBoard.holes[holeIdx] = 0;

        // put
        while (tokens > 0)
        {
            // next hole
            holeIdx++;

            // switch player
            if (holeIdx >= crtBoard.holes.Length)
            {
                holeIdx = 0;
                crtBoard = clone.OpponentBoard(crtBoard);
            }

            // skip starting hole
            if (crtBoard == fromBoard &&
                holeIdx == startingHoleIdx)
            {
                // next hole
                holeIdx++;

                // switch player
                if (holeIdx >= crtBoard.holes.Length)
                {
                    holeIdx = 0;
                    crtBoard = clone.OpponentBoard(crtBoard);
                }
            }

            // put
            tokens--;
            crtBoard.holes[holeIdx]++;
        }

        // eat
        if (crtBoard != fromBoard)
        {
            while (holeIdx >= 0 &&
                   crtBoard.holes[holeIdx] >= GameManager.Inst.EatMin &&
                   crtBoard.holes[holeIdx] <= GameManager.Inst.EatMax)
            {
                // eat
                fromBoard.score += crtBoard.holes[holeIdx];
                crtBoard.holes[holeIdx] = 0;

                // previous hole
                holeIdx--;
            }
        }

        return clone;
    }

    public BoardData OpponentBoard(BoardData board) => board == b1 ? b2 : b1;

    public BoardData GetBoard(bool b1) => b1 ? this.b1 : b2;

    public override string ToString() => $"b1 : {b1}\nb2 : {b2}";
}

class BoardData
{
    public int[] holes;
    public int score;

    public BoardData(Board board)
    {
        holes = new int[board.Holes.Length];

        for (int i = 0; i < holes.Length; i++)
            holes[i] = board.Holes[i].Value;

        score = board.Score.Value;
    }

    BoardData(int holesLength)
    {
        holes = new int[holesLength];

        for (int i = 0; i < holes.Length; i++)
            holes[i] = 0;

        score = 0;
    }

    public BoardData Clone()
    {
        BoardData clone = new BoardData(holes.Length);

        for (int i = 0; i < holes.Length; i++)
            clone.holes[i] = holes[i];

        clone.score = score;

        return clone;
    }

    public override string ToString()
    {
        string str = $"{score} | ";

        foreach (int hole in holes)
            str += $"{hole} ";

        return str;
    }
}
