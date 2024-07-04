using System.Collections.ObjectModel;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    public abstract class AbstractAction(string name, HashSet<Term> startPositions, Term dest, float cost)
    {
        public string Name { get; } = name;
        public string DebugString => $"{Prefix} - {(StartPositions.Any() ? StartPositions.Select(p => p.Name).Aggregate((a, b) => $"{a}, {b}") : "NONE")} -> {Destination.Name}";
        public string DebugStringShort => $"{Prefix} - ... -> {Destination.Name}";
        public abstract string Prefix { get; }
        public ReadOnlyCollection<Term> StartPositions { get; } = new(startPositions.OrderBy(t => t.Id).ToArray());
        public Term Destination { get; } = dest;
        public float Cost { get; set; } = cost;

        /// <summary>
        /// The pm has no states except for the current position.
        /// Whether or not the output states have been visited is not checked here.
        /// Please do not modify the ProgressionManager here, as this is bad for performance.
        /// </summary>
        public abstract bool TryDo(ProgressionManager pm, Term currentPosition, StateUnion currentStates, out StateUnion? satisfiableStates);
    }
}
