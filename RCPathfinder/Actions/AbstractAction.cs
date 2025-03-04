using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// Action that propagates to a single destination with certain cost.
    /// </summary>
    public abstract class AbstractAction
    {
        public abstract string Prefix { get; }
        public abstract Term Target { get; }
        public abstract float Cost { get; }
        public abstract string DebugString { get; }

        /// <summary>
        /// The pm contains the node's current states if it is needed for logic evaluation.
        /// Whether or not the output states have been visited is not checked here.
        /// Please do not modify the ProgressionManager here, as this is bad for performance.
        /// </summary>
        public abstract bool TryDo(Node node, ProgressionManager pm, out StateUnion? satisfiableStates);

        public abstract bool TryDoStateless(Node node, ProgressionManager pm);
    }
}
