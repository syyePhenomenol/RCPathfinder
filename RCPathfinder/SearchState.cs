using System.Collections.ObjectModel;
using RandomizerCore.Collections;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public class SearchState
    {   
        /// <summary>
        /// The combinations of starts and destinations for which a path has been found.
        /// </summary>
        public HashSet<(StartPosition start, Term destination)> FoundStartDestinationPairs { get; }

        /// <summary>
        /// The combinations of starts and destinations for which a path has not been found yet.
        /// </summary>
        public HashSet<(StartPosition start, Term destination)> RemainingStartDestinationPairs { get; }

        /// <summary>
        /// An unordered collection of nodes in the queue.
        /// </summary>
        public ReadOnlyCollection<((float cost, int depth) priority, Node node)> QueueNodes => new(_queue.UnorderedItems.ToArray());
        private readonly PriorityQueue<(float cost, int depth), Node> _queue;
        /// <summary>
        /// A collection of result nodes, including those from previous searches.
        /// </summary>
        public ReadOnlyCollection<Node> ResultNodes => new(_resultNodes.ToArray());
        private readonly HashSet<Node> _resultNodes;
        /// <summary>
        /// A collection of result nodes, only from the current search.
        /// </summary>
        public ReadOnlyCollection<Node> NewResultNodes => new(_newResultNodes.ToArray());
        private readonly HashSet<Node> _newResultNodes;
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

        /// <summary>
        /// Collections of visited states by a StartPosition key.
        /// </summary>
        private readonly Dictionary<string, StateUnionCollection> _stateUnionCollections;

        public SearchState(SearchParams sp)
        {
            FoundStartDestinationPairs = new();
            RemainingStartDestinationPairs = new(sp.StartPositions.SelectMany(s => sp.Destinations.Select(d => (s, d))));

            _queue = new();
            _resultNodes = new();
            _newResultNodes = new();

            _stateUnionCollections = sp.StartPositions.GroupBy(p => p.Key).ToDictionary<IGrouping<string, StartPosition>, string, StateUnionCollection>(k => k.Key, k => new(k, sp.StartState));

            foreach (StartPosition start in sp.StartPositions ?? Enumerable.Empty<StartPosition>())
            {
                _queue.Enqueue((start.Cost, 0), new(start, sp.StartState ?? StateUnion.Empty, _stateUnionCollections[start.Key]));
            }
        }

        internal void ResetForNewSearch()
        {
            _newResultNodes.Clear();
        }

        public void AddResultNode(Node node)
        {
            _resultNodes.Add(node);
            _newResultNodes.Add(node);
        }

        public bool TryPop(out Node? node)
        {
            if (_queue.TryPop(out node))
            {
                NodesPopped++;
                return true;
            }
            node = default;
            return false;
        }

        public void Push(Node? node)
        {
            if (node is null) throw new NullReferenceException();

            _queue.Enqueue((node.Cost, node.Depth), node);
        }
    }
}
