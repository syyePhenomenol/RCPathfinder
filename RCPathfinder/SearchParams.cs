using RandomizerCore.Logic;

namespace RCPathfinder
{
    public record SearchParams
    {
        /// <summary>
        /// To remove redandant searches, consider pruning start terms that are logically equivalent.
        /// StartPositions is ignored when a search is resuming from a previous SearchState.
        /// </summary>
        public IEnumerable<Position>? StartPositions { get; set; }
        /// <summary>
        /// To remove redandant searches, consider pruning destination terms that are logically equivalent.
        /// </summary>
        public IEnumerable<Term>? Destinations { get; set; }
        /// <summary>
        /// Time before algorithm is terminated. Has priority over all other termination conditinos.
        /// </summary>
        public float MaxTime { get; set; } = float.PositiveInfinity;
        /// <summary>
        /// Cost before algorithm is terminated. Has priority over other termination conditions except for MaxTime.
        /// </summary>
        public float MaxCost { get; set; } = float.PositiveInfinity;
        /// <summary>
        /// Depth before algorithm is terminated. Has priority over other termination conditions except for MaxTime and MaxCost.
        /// </summary>
        public int MaxDepth { get; set; } = int.MaxValue;
        /// <summary>
        /// A condition for which the search will terminate before reaching max cost or exhaustion.
        /// </summary>
        public TerminationConditionType TerminationCondition { get; set; } = TerminationConditionType.None;
        /// <summary>
        /// Whether or not to do a stateless search.
        /// </summary>
        public bool Stateless { get; set; } = false;
        /// <summary>
        /// Whether or not, in a particular node, a previous term can be revisited.
        /// May help improve performance significantly if set to true, at the expense of not being complete.
        /// </summary>
        public bool DisallowBacktracking { get; set; } = false;
    }

    public enum TerminationConditionType
    {
        // exhausts search space
        None,
        // terminates after reaching any destination
        Any,
        // terminates after reaching any new destination
        AnyUniqueDestination,
        // terminates after reaching any new start/destination pair
        AnyUniqueStartAndDestination,
        // terminates after reaching every destination
        EveryDestination,
        // terminates after reaching every destination from every start (i.e. n(start) * n(destination) routes)
        EveryStartAndDestination
    }
}
