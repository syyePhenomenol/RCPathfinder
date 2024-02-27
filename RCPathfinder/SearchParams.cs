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
        /// Cost before algorithm is terminated. Has priority over other termination conditions except for MaxTime.
        /// </summary>
        public float MaxCost { get; set; }
        /// <summary>
        /// A condition for which the search will terminate before reaching max cost or exhaustion.
        /// </summary>
        public TerminationConditionType TerminationCondition { get; set; }
        /// <summary>
        /// Time before algorithm is terminated. Has priority over all other termination conditinos.
        /// </summary>
        public float MaxTime { get; set; }
        /// <summary>
        /// If timed out, whether or not to continue searching without state propagation.
        /// </summary>
        public bool ContinueStateless { get; set; }

        public SearchParams(StartPosition[] startPositions, StateUnion startState, Term[] destinations,
            float maxCost, TerminationConditionType terminationCondition) : this(startPositions, startState, destinations, maxCost, terminationCondition, float.MaxValue, false) { }

        public SearchParams(StartPosition[] startPositions, StateUnion startState, Term[] destinations,
            float maxCost, TerminationConditionType terminationCondition, float maxTime, bool continueStateless)
        {
            StartPositions = startPositions;
            StartState = startState;
            Destinations = destinations;
            MaxCost = maxCost;
            TerminationCondition = terminationCondition;
            MaxTime = maxTime;
            ContinueStateless = continueStateless;
        }
    }

    public enum TerminationConditionType
    {
        None, // exhausts search space
        Any, // terminates after reaching any destination
        AnyUniqueDestination, // terminates after reaching any new destination
        AnyUniqueStartAndDestination, // terminates after reaching any new start/destination pair
        EveryDestination, // terminates after reaching every destination
        EveryStartAndDestination // terminates after reaching every destination from every start (i.e. n(start) * n(destination) routes)
    }
}
