using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;

namespace RMPathfinder.Actions
{
    public record TransitionAction : AbstractAction
    {
        private readonly ProgressionManager pm;
        private readonly LogicDef transitionSourceLogic;

        public TransitionAction(ProgressionManager pm, Term position, Term newPosition) : base(position.Name, "tran", newPosition)
        {
            this.pm = pm;
            transitionSourceLogic = pm.lm.LogicLookup[position.Name];
        }

        /// <summary>
        /// Ignores the position parameter. GetActions() handles which position has this action available.
        /// </summary>
        public override bool TryDo(Term position, StateUnion state, out StateUnion? newState)
        {
            newState = state;
            return transitionSourceLogic.CanGet(pm);
        }
    }
}
