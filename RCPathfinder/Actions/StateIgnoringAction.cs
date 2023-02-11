using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// An action which can be performed if the position meets the logical prerequisites.
    /// Ignores state and propagates the previous state.
    /// </summary>
    public class StateIgnoringAction : AbstractAction
    {
        public override string Prefix => "stig";

        private readonly LogicDef logic;

        public StateIgnoringAction(Term term, LogicDef logic, float cost = 1f) : base(term.Name, new(logic.GetTerms().Where(t => t.Type is TermType.State)), term, cost)
        {
            this.logic = logic;
        }

        public override bool TryDo(ProgressionManager pm, Term currentPosition, StateUnion currentStates, out Term? newPosition, out StateUnion? newStates)
        {
            if (logic.CanGet(pm))
            {
                newPosition = Destination;
                newStates = currentStates;
                return true;
            }
            newPosition = default;
            newStates = default;
            return false;
        }
    }
}
