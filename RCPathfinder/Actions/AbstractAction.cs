using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// Action that propagates a single start position to a single destination.
    /// </summary>
    public abstract class AbstractAction(Term start, Term destination, float cost)
    {
        public abstract string Prefix { get; }
        public Term Start { get; } = start;
        public Term Destination { get; } = destination;
        public float Cost { get; } = cost;
        public string DebugString => $"{Prefix} - {Start.Name} -> {Cost} -> {Destination.Name}";

        /// <summary>
        /// The pm has no states except for the current position.
        /// Whether or not the output states have been visited is not checked here.
        /// Please do not modify the ProgressionManager here, as this is bad for performance.
        /// </summary>
        public abstract bool TryDo(ProgressionManager pm, StateUnion currentStates, out StateUnion? satisfiableStates);
    }
}
