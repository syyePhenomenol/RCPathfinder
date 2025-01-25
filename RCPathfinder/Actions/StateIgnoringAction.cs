using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// An action which can be performed if the position meets the logical prerequisites.
    /// Ignores state and propagates the previous state.
    /// </summary>
    public class StateIgnoringAction(Term start, Term destination, LogicDef logic, float cost = 1f) : AbstractAction(start, destination, cost)
    {
        public override string Prefix => "stig";

        public LogicDef Logic { get; } = logic;

        public StateIgnoringAction(StateLogicAction stlo) : this(stlo.Start, stlo.Destination, stlo.Logic, stlo.Cost) { }

        public override bool TryDo(ProgressionManager pm, StateUnion currentStates, out StateUnion? satisfiableStates)
        {
            if (Logic.CanGet(pm))
            {
                satisfiableStates = currentStates;
                return true;
            }
            satisfiableStates = default;
            return false;
        }
    }
}
