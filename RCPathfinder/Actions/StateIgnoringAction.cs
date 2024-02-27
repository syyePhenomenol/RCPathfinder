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

        public LogicDef Logic { get; init; }

        public StateIgnoringAction(Term term, LogicDef logic, float cost = 1f) : base(term.Name, new(logic.GetTerms().Where(t => t.Type is TermType.State)), term, cost)
        {
            Logic = logic;
        }

        public StateIgnoringAction(StateLogicAction stlo) : this(stlo.Destination, stlo.Logic, stlo.Cost) { }

        public override bool TryDo(ProgressionManager pm, Term currentPosition, StateUnion currentStates, out Term? newPosition, out StateUnion? newStates)
        {
            if (Logic.CanGet(pm))
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
