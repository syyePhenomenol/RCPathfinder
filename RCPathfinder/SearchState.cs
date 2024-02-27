using System.Collections.ObjectModel;
using RandomizerCore.Collections;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public class SearchState
    {        
        /// <summary>
        /// Visited states per position term. Separate collection by a specified key.
        /// </summary>
        internal Dictionary<string, Dictionary<Term, List<State>>> Visited { get; }
        /// <summary>
        /// The terms for which an indeterminate state has been reached. Separate collection by a specified key.
        /// </summary>
        internal Dictionary<string, HashSet<Term>> Indeterminate { get; }

        /// <summary>
        /// The combinations of starts and destinations for which a path has been found.
        /// </summary>
        public HashSet<(Term start, Term destination)> FoundStartDestinationPairs { get; }

        /// <summary>
        /// The combinations of starts and destinations for which a path has not been found yet.
        /// </summary>
        public HashSet<(Term start, Term destination)> RemainingStartDestinationPairs { get; }

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

        public SearchState(SearchParams sp)
        {
            Visited = sp.StartPositions.Select(p => p.Key).Distinct().ToDictionary(k => k, k => new Dictionary<Term, List<State>>());
            Indeterminate = sp.StartPositions.Select(p => p.Key).Distinct().ToDictionary(k => k, k => new HashSet<Term>());
            FoundStartDestinationPairs = new();
            RemainingStartDestinationPairs = new(sp.StartPositions.SelectMany(s => sp.Destinations.Select(d => (s.Term, d))));

            _queue = new();

            foreach (StartPosition start in sp.StartPositions)
            {
                if (sp.StartState.Count > 0)
                {
                    Visited[start.Key][start.Term] = new(sp.StartState);
                }
                else
                {
                    Indeterminate[start.Key].Add(start.Term);
                }

                _queue.Enqueue((start.Cost, 0), new(start.Key, start.Term, start.Cost, sp.StartState));
            }

            _resultNodes = new();
            _newResultNodes = new();
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
