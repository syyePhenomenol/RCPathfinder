using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;

namespace RMPathfinder.Actions
{
    public record StartLinkedAction : AbstractAction
    {
        public StartLinkedAction(Term newPosition) : base(newPosition.Name, "stli", newPosition) { }

        public override bool TryDo(Term position, StateUnion state, out StateUnion? newState)
        {
            newState = state;
            return true;
        }
    }
}
