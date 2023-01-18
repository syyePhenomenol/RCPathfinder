using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.RMPathfinder
{
    public record StartLinkedAction : AbstractAction
    {
        public StartLinkedAction(Term newPosition) : base($"sl-{newPosition.Name}", newPosition) { }

        public override bool TryDo(Term position, StateUnion state, out StateUnion? newState)
        {
            newState = state;
            return true;
        }
    }
}
