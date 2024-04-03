using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    internal class StateUnionCollection
    {
        /// <summary>
        /// Visited non-empty lists of states.
        /// </summary>
        internal Dictionary<Term, List<State>> NonEmptyStates { get; }
        
        /// <summary>
        /// The terms for which an indeterminate state has been reached.
        /// </summary>
        internal HashSet<Term> EmptyStates { get; }

        internal StateUnionCollection(IEnumerable<StartPosition> startPositions, StateUnion? startState)
        {
            NonEmptyStates = new();
            EmptyStates = new();

            foreach (var startPosition in startPositions)
            {
                TryAddStates(startPosition.Term, startState, out _);
            }

        }

        internal bool TryAddStates(Term position, StateUnion? states, out StateUnion? unvisitedStates)
        {
            unvisitedStates = states;

            if (states is null)
            {
                return false;
            }

            if (states.Count == 0)
            {
                return EmptyStates.Add(position);
            }
                
            if (!NonEmptyStates.TryGetValue(position, out var visitedStates))
            {
                NonEmptyStates.Add(position, new(states));
                return true;
            }

            if (states.TrySubtract(visitedStates, out unvisitedStates))
            {
                visitedStates.AddRange(unvisitedStates);
                return true;
            }

            return false;
        }
    }
}