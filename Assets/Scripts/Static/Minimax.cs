using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=l-hh51ncgDI
public static class Minimax
{
    static public (Move move, float value, float dValue) BestMove<Move>(IMinimaxState<Move> state, int depth)
    {
        float crtValue = state.Evaluate();
        (float moveValue, Move move) = Evaluate(state, true, float.NegativeInfinity, float.PositiveInfinity, depth);
        float dValue = moveValue - crtValue;
        return (move, moveValue, dValue);
    }

    static (float value, Move move) Evaluate<Move>(IMinimaxState<Move> state, bool playerTurn, float alpha, float beta, int depth)
    {
        List<Move> moves = state.Moves(playerTurn);

        if (moves.Count == 0)
            return (state.CantPlayEvaluation(playerTurn), state.NoMove());

        if (depth <= 0)
            return (state.Evaluate(), state.NoMove());

        bool maximize = playerTurn;
        float bestValue = maximize ? float.NegativeInfinity : float.PositiveInfinity;
        List<Move> bestMoves = new List<Move>();

        foreach (Move move in moves)
        {
            float value = Evaluate(
                state.Play(move, playerTurn),
                !playerTurn,
                alpha,
                beta,
                depth-1)
                .value;

            if (value > bestValue == maximize ||
                bestMoves.Count == 0)
            {
                bestValue = value;

                bestMoves.Clear();
                bestMoves.Add(move);
            }

            else if (value == bestValue)
                bestMoves.Add(move);

            if (maximize) alpha = Mathf.Max(value, alpha);
            else          beta  = Mathf.Min(value, beta);

            if (beta < alpha)
                break;
        }

        return (bestValue, bestMoves.Random());
    }
}

public interface IMinimaxState<Move>
{
    public float Evaluate();
    public float CantPlayEvaluation(bool playerTurn);
    public List<Move> Moves(bool playerTurn);
    public Move NoMove();
    public IMinimaxState<Move> Play(Move move, bool playerTurn);
}
