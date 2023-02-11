using System.Collections.ObjectModel;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder.Actions;

namespace RCPathfinder
{
    public class Node
    {
        public string Key { get; }
        public Term StartPosition { get; }
        public Term Position { get; }
        public StateUnion States { get; }
        public ReadOnlyCollection<AbstractAction> Actions => actions.AsReadOnly();
        private readonly List<AbstractAction> actions;
        public float Cost { get; }
        public int Depth { get; }

        /// <summary>
        /// Checks if the action can be done, then checks if the new position/states have not already been visited.
        /// </summary>
        internal static bool TryTraverse(ProgressionManager pm, SearchState search, Node parent, AbstractAction action, out Node? child)
        {
            if (action.TryDo(pm, parent.Position, parent.States, out Term? newPosition, out StateUnion? newStates)) 
            {
                if (newPosition is null || newStates is null) throw new NullReferenceException();

                if (!search.Visited.TryGetValue(parent.Key, out var visited)) throw new KeyNotFoundException();
                if (!search.Indeterminate.TryGetValue(parent.Key, out var indeterminate)) throw new KeyNotFoundException();

                if (newStates.Count == 0)
                {
                    if (!indeterminate.Contains(newPosition))
                    {
                        child = new(parent, action, newPosition, newStates);
                        indeterminate.Add(newPosition);
                        return true;
                    }

                    child = default;
                    return false;
                }

                if (!visited.TryGetValue(newPosition, out var visitedStates))
                {
                    child = new(parent, action, newPosition, newStates);
                    visited[newPosition] = new(newStates);
                    return true;
                }

                if (newStates.TrySubtract(visitedStates, out StateUnion unvisitedStates))
                {
                    child = new(parent, action, newPosition, unvisitedStates);
                    visitedStates.AddRange(unvisitedStates);
                    return true;
                }
            }

            child = default;
            return false;
        }

        internal Node(string key, Term position, float cost, StateUnion states)
        {
            Key = key;
            StartPosition = position;
            Position = position;
            Cost = cost;
            States = states;
            actions = new();
        }

        internal Node(Node parent, AbstractAction action, Term newPosition, StateUnion newStates)
        {
            Key = parent.Key;
            StartPosition = parent.StartPosition;
            Position = newPosition;
            States = newStates;
            actions = new(parent.actions) { action };
            Cost = parent.Cost + action.Cost;
            Depth = parent.Depth + 1;
        }

        public string PrintActions()
        {
            string text = "";

            foreach (AbstractAction action in actions)
            {
                text += $"-> {action.DebugName}\n";
            }

            return text.Substring(0, text.Length - 1);
        }
    }
}
