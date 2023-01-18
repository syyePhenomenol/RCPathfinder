using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public static class Extensions
    {
        /// <summary>
        /// Removes states from left that are equal to those in right.
        /// </summary>
        public static List<State> Subtract(this StateUnion left, List<State> right)
        {
            List<State> states = new();
            for (int i = 0; i < left.Count; i++)
            {
                for (int j = 0; j < right.Count; j++)
                {
                    if (State.IsComparablyLE(left[i], right[j]) && State.IsComparablyLE(right[j], left[i]))
                    {
                        goto continue_outer;
                    }
                }
                states.Add(left[i]);
            continue_outer: continue;
            }

            return states;
        }
    }
}
