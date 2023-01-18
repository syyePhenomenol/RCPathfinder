using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public record SearchParams
    {
        public Term StartPosition { get; set; }
        public StateUnion StartState { get; set; }
        public Term[] Destinations { get; set; }
        public float MaxCost { get; set; }
        public bool SingleResultTermination { get; set; }

        public SearchParams(Term startPosition, StateUnion startState, Term[] destinations, float maxCost, bool singleResultTermination)
        {
            StartPosition = startPosition;
            StartState = startState;
            Destinations = destinations;
            MaxCost = maxCost;
            SingleResultTermination = singleResultTermination;
        }
    }
}
