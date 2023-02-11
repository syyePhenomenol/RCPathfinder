using RandomizerCore.Collections;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public static class Extensions
    {
        /// <summary>
        /// Removes states from left that are comparably equal to those in right.
        /// Returns true if the resulting diffeence is not empty.
        /// </summary>
        public static bool TrySubtract(this StateUnion left, List<State> right, out StateUnion result)
        {
            List<State> states = new();

            for (int i = 0; i < left.Count; i++)
            {
                for (int j = 0; j < right.Count; j++)
                {
                    if (left[i].ComparablyEquals(right[j]))
                    {
                        goto continue_outer;
                    }
                }
                states.Add(left[i]);
            continue_outer: continue;
            }

            result = new(states);
            return result.Any();
        }

        public static bool ComparablyEquals(this State left, State right)
        {
            return State.IsComparablyLE(left, right) && State.IsComparablyLE(right, left);
        }

        public static bool TryPop<T1, T2>(this PriorityQueue<T1, T2> pq, out T2? result) where T1 : IComparable<T1>
        {
            return pq.TryExtractMin(out _, out result);
        }

        public static bool IsOrIsSubclassInstanceOf<T>(this object obj)
        {
            return obj is T || obj.GetType().IsSubclassOf(typeof(T));
        }
    }
}
