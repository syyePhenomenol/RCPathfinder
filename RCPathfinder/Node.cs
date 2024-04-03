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
        internal StateUnionCollection StateUnionCollection { get; }

        /// <summary>
        /// Checks if the action can be done, then checks if the new position/states have not already been visited.
        /// </summary>
        internal static bool TryTraverse(ProgressionManager pm, SearchParams sp, Node parent, AbstractAction action, out Node? child)
        {
            if (sp.Stateless && action is StateLogicAction stla)
            {
                action = new StateIgnoringAction(stla);
            }

            if (action.TryDo(pm, parent.CurrentPosition, parent.CurrentStates, out Term? newPosition, out StateUnion? newStates)) 
            {
                if (newPosition is null || newStates is null) throw new NullReferenceException();

                if (!sp.AllowBacktracking && parent.IsPreviouslyVisitedPosition(newPosition))
                {
                    child = default;
                    return false;
                }

                if (parent.StateUnionCollection.TryAddStates(newPosition, newStates, out var unvisitedStates))
                {
                    if (unvisitedStates is null) throw new NullReferenceException();

                    child = new(parent, action, newPosition, unvisitedStates);
                    return true;
                }
            }

            child = default;
            return false;
        }

        internal Node(StartPosition startPosition, StateUnion startStates, StateUnionCollection suc)
        {
            StartPosition = startPosition;
            CurrentPosition = startPosition.Term;
            CurrentStates = startStates;
            _actions = new();
            Cost = startPosition.Cost;
            StateUnionCollection = suc;
        }

        internal Node(Node parent, AbstractAction action, Term newPosition, StateUnion newStates)
        {
            StartPosition = parent.StartPosition;
            CurrentPosition = newPosition;
            CurrentStates = newStates;
            _actions = new(parent._actions) { action };
            Cost = parent.Cost + action.Cost;
            Depth = parent.Depth + 1;
            StateUnionCollection = parent.StateUnionCollection;
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
