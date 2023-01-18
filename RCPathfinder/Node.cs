using System.Collections.ObjectModel;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public class Node
    {
        public Term Position { get; }
        public StateUnion State { get; }
        public float Cost { get; }
        public ReadOnlyCollection<AbstractAction> Actions => actions.AsReadOnly();

        private readonly List<AbstractAction> actions;

        public int Depth { get; } = 0;

        internal static bool TryTraverse(Node parent, AbstractAction action, SearchState search, out Node? child)
        {
            if (action.TryDo(parent.Position, parent.State, out StateUnion? newState))
            {
                if (newState is null) throw new NullReferenceException();

                if (newState.Count is 0)
                {
                    if (!search.Indeterminate.Contains(action.NewPosition))
                    {
                        child = new Node(parent, action, newState);
                        search.Indeterminate.Add(action.NewPosition);
                        return true;
                    }

                    child = default;
                    return false;
                }

                if (!search.Visited.TryGetValue(action.NewPosition, out List<State> visited))
                {
                    child = new Node(parent, action, newState);
                    search.Visited[action.NewPosition] = new(newState);
                    return true;
                }

                List<State> unvisited = newState.Subtract(visited);
                if (unvisited.Count > 0)
                {
                    child = new Node(parent, action, new(unvisited));
                    visited.AddRange(unvisited);
                    return true;
                }
            }

            child = default;
            return false;
        }

        internal Node(Term position, StateUnion state)
        {
            Position = position;
            State = state;
            actions = new();
        }

        internal Node(Node parent, AbstractAction action, StateUnion newState)
        {
            Position = action.NewPosition;
            State = newState;
            Cost = parent.Cost + action.Cost;
            actions = new(parent.actions)
            {
                action
            };
            Depth = parent.Depth + 1;
        }

        public string PrintActions()
        {
            string text = "";

            foreach (AbstractAction action in actions)
            {
                text += $"-> {action.Name}";
            }

            return text;
        }
    }
}
