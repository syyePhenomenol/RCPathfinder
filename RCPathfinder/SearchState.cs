using System.Collections.ObjectModel;
using RandomizerCore.Collections;
using RandomizerCore.Logic;

namespace RCPathfinder
{
    public class SearchState
    {   
        private readonly PriorityQueue<(float cost, int depth), Node> _queue;
        private readonly List<Node> _resultNodes;
        private readonly List<Node> _newResultNodes;
        private readonly List<Node> _terminalNodes;

        /// <summary>
        /// The combinations of starts and destinations for which a path has been found.
        /// </summary>
        public HashSet<(Position start, Term destination)> FoundStartDestinationPairs { get; }
        /// <summary>
        /// The combinations of starts and destinations for which a path has not been found yet.
        /// </summary>
        public HashSet<(Position start, Term destination)> RemainingStartDestinationPairs { get; }
        /// <summary>
        /// An unordered collection of nodes in the queue.
        /// </summary>
        public ReadOnlyCollection<((float cost, int depth) priority, Node node)> QueueNodes => new([.._queue.UnorderedItems]);
        /// <summary>
        /// A collection of result nodes, including those from previous searches.
        /// </summary>
        public ReadOnlyCollection<Node> ResultNodes => new(_resultNodes);
        /// <summary>
        /// A collection of result nodes, only from the current search.
        /// </summary>
        public ReadOnlyCollection<Node> NewResultNodes => new(_newResultNodes);
        /// <summary>
        /// A collection of popped nodes that did not have any valid children.
        /// </summary>
        public ReadOnlyCollection<Node> TerminalNodes => new(_terminalNodes);
        /// <summary>
        /// How many nodes were popped from the queue. Also includes previous searches.
        /// </summary>
        public int NodesPopped { get; private set; }
        /// <summary>
        /// Total time spent searching. Also includes previous searches.
        /// </summary>
        public float SearchTime { get; internal set; }
        /// <summary>
        /// If the search has timed out.
        /// </summary>
        public bool HasTimedOut { get; internal set; }

        public SearchState(SearchParams sp)
        {
            FoundStartDestinationPairs = [];
            RemainingStartDestinationPairs = new(sp.StartPositions.SelectMany(s => sp.Destinations.Select(d => (s, d))));

            _queue = new();
            _resultNodes = [];
            _newResultNodes = [];
            _terminalNodes = [];

            if (sp.StartPositions is null || !sp.StartPositions.Any())
            {
                return;
            }

            foreach (Position start in sp.StartPositions)
            {
                Push(new(start));
            }
        }

        internal bool TryPop(out Node? node)
        {
            if (_queue.TryPop(out node))
            {
                NodesPopped++;
                return true;
            }
            node = default;
            return false;
        }

        internal void Push(Node? node)
        {
            if (node is null) throw new NullReferenceException();

            _queue.Enqueue((node.Cost, node.Depth), node);
        }

        internal void AddResultNode(Node node)
        {
            _resultNodes.Add(node);
            _newResultNodes.Add(node);
        }

        internal void AddTerminalNode(Node node)
        {
            _terminalNodes.Add(node);
        }

        internal void ResetForNewSearch()
        {
            _newResultNodes.Clear();
        }
    }
}
