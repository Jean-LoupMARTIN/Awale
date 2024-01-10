using UnityEngine;



public class Board : MonoBehaviour
{
    [SerializeField] Counter score;
    public Counter Score { get => score; }

    [SerializeField] Counter[] holes;
    public Counter[] Holes { get => holes; }

    public bool Has(Counter hole) => holes.Contains(hole);

    public int CountFilledHoles()
    {
        int count = 0;

        foreach (Counter hole in holes)
            if (hole.Value > 0)
                count++;

        return count;
    }
}
