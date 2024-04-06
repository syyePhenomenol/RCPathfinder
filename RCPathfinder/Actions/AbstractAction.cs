using System.Collections.ObjectModel;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    public abstract class AbstractAction
    {
        public string Name { get; }
        public string DebugString => $"{Prefix} - {(StartPositions.Any() ? StartPositions.Select(p => p.Name).Aggregate((a, b) => $"{a}, {b}") : "NONE")} -> {Destination.Name}";
        public string DebugStringShort => $"{Prefix} - ... -> {Destination.Name}";
        public abstract string Prefix { get; }
        public ReadOnlyCollection<Term> StartPositions { get; }
        public Term Destination { get; }
        public float Cost { get; set; }

        public AbstractAction(string name, HashSet<Term> startPositions, Term dest, float cost)
        {
            Name = name;
            StartPositions = new(startPositions.OrderBy(t => t.Id).ToArray());
            Destination = dest;
            Cost = cost;
        }

        /// <summary>
        /// The pm has no states except for the current position.
        /// Whether or not the output states have been visited is not checked here.
        /// Please do not modify the ProgressionManager here, as this is bad for performance.
        /// </summary>
        public abstract bool TryDo(ProgressionManager pm, Term currentPosition, StateUnion currentStates, out StateUnion? satisfiableStates);
    }
}
