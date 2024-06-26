﻿using RandomizerCore.Logic;
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

        public StateLogicDef Logic { get; init ;}

        public StateLogicAction(Term term, StateLogicDef logic, float cost = 1f) : base(term.Name, new(logic.GetTerms().Where(t => t.Type is TermType.State)), term, cost)
        {
            Logic = logic;
        }

        public override bool TryDo(ProgressionManager pm, Term currentPosition, StateUnion currentStates, out StateUnion? satisfiableStates)
        {
            // Gets valid states based on the single currentPosition entry in the pm
            return Logic.CheckForUpdatedState(pm, null, new(), currentPosition, out satisfiableStates);
        }
    }
}
