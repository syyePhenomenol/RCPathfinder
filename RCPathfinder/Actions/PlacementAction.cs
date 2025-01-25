using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// An action that links a specific position to a specific destination without state change.
    /// </summary>
    public class PlacementAction(Term start, Term destination, float cost = 1f) : AbstractAction(start, destination, cost)
    {
        public override string Prefix => "plac";

        public override bool TryDo(ProgressionManager pm, StateUnion currentStates, out StateUnion? satisfiableStates)
        {
            satisfiableStates = currentStates;
            return true;
        }
    }
}
