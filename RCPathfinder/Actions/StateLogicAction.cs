using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// An action which can be performed if both the position and state meet the logical prerequisites.
    /// Propagates the resulting state.
    /// </summary>
    public class StateLogicAction : AbstractAction
    {
        public override string Prefix => "stlo";

        private readonly StateLogicDef logic;

        public StateLogicAction(Term term, StateLogicDef logic, float cost = 1f) : base(term.Name, new(logic.GetTerms().Where(t => t.Type is TermType.State)), term, cost)
        {
            this.logic = logic;
        }

        public override bool TryDo(ProgressionManager pm, Term currentPosition, StateUnion currentStates, out Term? newPosition, out StateUnion? newStates)
        {
            if (logic.CheckForUpdatedState(pm, null, new(), currentPosition, out newStates))
            {
                newPosition = Destination;
                return true;
            }
            newPosition = default;
            return false;
        }
    }
}
