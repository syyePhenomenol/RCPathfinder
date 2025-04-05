using RandomizerCore.Collections;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder;

public static class Extensions
{
    /// <summary>
    /// Tries to find what states in left are better than those in right.
    /// Better means lower state values!
    /// Outputs both the difference and the union.
    /// Returns true if the resulting difference is not empty.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="difference"></param>
    /// <param name="union"></param>
    public static bool TrySubtractAndUnion(
        this StateUnion left,
        StateUnion right,
        out StateUnion difference,
        out StateUnion union
    )
    {
        List<State> states = [];

        for (var i = 0; i < left.Count; i++)
        {
            for (var j = 0; j < right.Count; j++)
            {
                if (right[j].IsComparablyLE(left[i]))
                {
                    // Right state is better than left state
                    goto continue_outer;
                }
            }

            states.Add(left[i]);
            continue_outer:
            continue;
        }

        difference = new(states);
        states.AddRange(right);
        union = new(states);

        return difference.Any();
    }

    public static bool TryPop<T1, T2>(this PriorityQueue<T1, T2> pq, out T2? result)
        where T1 : IComparable<T1>
    {
        return pq.TryExtractMin(out _, out result);
    }

    public static void SetState(this ProgressionManager pm, Position position)
    {
        pm.SetState(position.Term, position.States);
    }
}
