using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public abstract record AbstractAction
    {
        public string Name { get; }
        public Term NewPosition { get; }
        public float Cost { get; }

        public AbstractAction(string name, Term newPosition, float cost = 1f)
        {
            Name = name;
            NewPosition = newPosition;
            Cost = cost;
        }

        public abstract bool TryDo(Term position, StateUnion state, out StateUnion? newState);
    }
}
