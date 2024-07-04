using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// An action that links a specific position to a specific destination without state change.
    /// Pruning before including this action is recommended.
    /// </summary>
    public class PlacementAction(Term startPosition, Term destination, float cost = 1f) : AbstractAction(startPosition.Name, new HashSet<Term> { startPosition }, destination, cost)
    {
        public override string Prefix => "plac";

        public override bool TryDo(ProgressionManager pm, Term currentPosition, StateUnion currentStates, out StateUnion? satisfiableStates)
        {
            if (currentPosition != StartPositions[0]) throw new InvalidDataException();

            satisfiableStates = currentStates;
            return true;
        }
    }
}
