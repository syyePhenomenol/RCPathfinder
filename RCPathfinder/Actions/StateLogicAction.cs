using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// An action which can be performed if both the position and state meet the logical prerequisites.
    /// Propagates the resulting state.
    /// </summary>
    public class StateLogicAction(Term start, Term destination, DNFLogicDef logic, float cost = 1f) : AbstractAction(start, destination, cost)
    {
        public override string Prefix => "stlo";

        public DNFLogicDef Logic { get; } = logic;

        public override bool TryDo(ProgressionManager pm, StateUnion currentStates, out StateUnion? satisfiableStates)
        {
            // Gets valid states based on the single currentPosition entry in the pm
            return Logic.CheckForUpdatedState(pm, null, [], Start, out satisfiableStates);
        }
    }
}
