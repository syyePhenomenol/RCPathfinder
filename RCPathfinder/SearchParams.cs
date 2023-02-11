using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public record SearchParams
    {
        /// <summary>
        /// To remove redandant searches, consider pruning start positions that are close to each other.
        /// StartPositions that share the same Key will correspond to Nodes that share the same visited states lookup.
        /// StartPositions is ignored when a search is resuming from a previous SearchState.
        /// </summary>
        public StartPosition[] StartPositions { get; set; }
        /// <summary>
        /// Every start position has this state.
        /// </summary>
        public StateUnion StartState { get; set; }
        /// <summary>
        /// To remove redandant searches, consider pruning destination positions that are close to each other.
        /// </summary>
        public Term[] Destinations { get; set; }
        /// <summary>
        /// MaxCost always has priority over other termination conditions.
        /// </summary>
        public float MaxCost { get; set; }
        /// <summary>
        /// A condition for which the search will terminate before reaching max cost or exhaustion.
        /// </summary>
        public TerminationConditionType TerminationCondition { get; set; }

        public SearchParams(StartPosition[] startPositions, StateUnion startState, Term[] destinations, float maxCost, TerminationConditionType terminationCondition)
        {
            StartPositions = startPositions;
            StartState = startState;
            Destinations = destinations;
            MaxCost = maxCost;
            TerminationCondition = terminationCondition;
        }
    }

    public enum TerminationConditionType
    {
        None,
        Any, // terminates after getting reaching any destination
        EveryDestination, // terminates after reaching every destination
        EveryStartAndDestination // terminates after reaching every destination from every start (i.e. n(start) * n(destination) routes)
    }
}
