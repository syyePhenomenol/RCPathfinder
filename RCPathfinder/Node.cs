using System.Collections.ObjectModel;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder.Actions;

namespace RCPathfinder
{
    public class Node
    {
        public StartPosition StartPosition { get; }
        public Term CurrentPosition { get; }
        public StateUnion CurrentStates { get; }
        public ReadOnlyCollection<AbstractAction> Actions => _actions.AsReadOnly();
        private readonly List<AbstractAction> _actions;
        public float Cost { get; }
        public int Depth { get; }

        private readonly Dictionary<Term, StateUnion> _visitedStatesLookup;

        internal Node(StartPosition startPosition, StateUnion startStates, Dictionary<Term, StateUnion> visitedStates)
        {
            StartPosition = startPosition;
            CurrentPosition = startPosition.Term;
            CurrentStates = startStates;
            _actions = [];
            Cost = startPosition.Cost;
            _visitedStatesLookup = visitedStates;

            // Ideally, this should return true as there shouldn't be two StartPositions that share the same Key and Term.
            TryAddVisitedStates(startPosition.Term, startStates, out var _);
        }

        internal Node(Node parent, AbstractAction action, StateUnion newStates)
        {
            StartPosition = parent.StartPosition;
            CurrentPosition = action.Destination;
            CurrentStates = newStates;
            _actions = new(parent._actions) { action };
            Cost = parent.Cost + action.Cost;
            Depth = parent.Depth + 1;
            _visitedStatesLookup = parent._visitedStatesLookup;
        }
        
        internal bool TryTraverse(ProgressionManager pm, AbstractAction action, out Node? child)
        {
            if (action.TryDo(pm, CurrentPosition, CurrentStates, out var satisfiableStates))
            {
                if (satisfiableStates is null) throw new NullReferenceException();
            
                if (TryAddVisitedStates(action.Destination, satisfiableStates, out var unvisitedStates))
                {
                    child = new(this, action, unvisitedStates);
                    return true;
                }
            }
                
            child = default;
            return false;            
        }

        /// <summary>
        /// Returns true if any new or better states are added.
        /// </summary>
        internal bool TryAddVisitedStates(Term position, StateUnion states, out StateUnion newStates)
        {
            if (!_visitedStatesLookup.TryGetValue(position, out var visitedStates))
            {
                newStates = states;
                _visitedStatesLookup.Add(position, states);
                return true;
            }

            if (states.TrySubtractAndUnion(visitedStates, out newStates, out StateUnion newVisitedStates))
            {
                _visitedStatesLookup[position] = newVisitedStates;
                return true;
            }

            return false;
        }

        public bool IsPreviouslyVisitedPosition(Term position)
        {
            return StartPosition.Term == position || _actions.Any(a => a.Destination == position);
        }

        public string PrintActions()
        {
            if (!_actions.Any()) return "";

            string text = "";

            foreach (AbstractAction action in _actions)
            {
                text += $"-> {action.DebugString}\n";
            }

            return text.Substring(0, text.Length - 1);
        }

        public string PrintActionsShort()
        {
            if (!_actions.Any()) return "";

            string text = "";

            foreach (AbstractAction action in _actions)
            {
                text += $"-> {action.DebugStringShort}\n";
            }

            return text.Substring(0, text.Length - 1);
        }
    }
}
