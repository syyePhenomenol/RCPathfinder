using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;

namespace RCPathfinder.RMPathfinder
{
    public record TransitionAction : AbstractAction
    {
        private readonly ProgressionManager pm;
        private readonly Term position;
        private readonly LogicDef transitionSourceLogic;

        public TransitionAction(ProgressionManager pm, Term position, Term newPosition) : base($"tn-{position.Name}", newPosition)
        {
            this.pm = pm;
            this.position = position;
            transitionSourceLogic = pm.lm.LogicLookup[position.Name];
        }

        /// <summary>
        /// Ignores the position parameter. GetActions() handles which position has this action available.
        /// </summary>
        public override bool TryDo(Term position, StateUnion state, out StateUnion? newState)
        {
            pm.StartTemp();
            pm.SetState(this.position, state);
            bool success = transitionSourceLogic.CanGet(pm);
            pm.RemoveTempItems();

            newState = state;
            return success;
        }
    }
}
