using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// An action which can be performed if both the position and state meet the logical prerequisites.
    /// Propagates the resulting state.
    /// </summary>
    public class StateLogicAction(Term term, StateLogicDef logic, float cost = 1f) : AbstractAction(term.Name, new(logic.GetTerms().Where(t => t.Type is TermType.State)), term, cost)
    {
        public override string Prefix => "stlo";

        public StateLogicDef Logic { get; } = logic;

        public override bool TryDo(ProgressionManager pm, Term currentPosition, StateUnion currentStates, out StateUnion? satisfiableStates)
        {
            // Gets valid states based on the single currentPosition entry in the pm
            return Logic.CheckForUpdatedState(pm, null, [], currentPosition, out satisfiableStates);
        }
    }
}
