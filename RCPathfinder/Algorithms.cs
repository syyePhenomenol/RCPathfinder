using System.Collections.ObjectModel;
using System.Diagnostics;
using RandomizerCore.Logic;

namespace RCPathfinder
{
    public static class Algorithms
    {
        /// <summary>
        /// Returns a SearchNode for every specified destination where possible.
        /// If no destinations were provided, instead searches to exhaustion or until the max depth is reached.
        /// Search can resume from previous SearchState if provided, in which case the starting position/state are ignored.
        /// </summary>
        public static ReadOnlyDictionary<Term, Node> DijkstraSearch(SearchSettings ss, SearchParams sp, SearchState? search = null)
        {
            HashSet<Term> destinations = new(sp.Destinations);

            search ??= new(sp);

            Dictionary<Term, Node> results = new();

            while (search.TryPop(out Node? node))
            {
                if (node is null) throw new NullReferenceException(nameof(Node));

                if (destinations.Contains(node.Position) && !results.ContainsKey(node.Position))
                {
                    results.Add(node.Position, node);

                    if (sp.SingleResultTermination || results.Count == destinations.Count)
                    {
                        search.Push(node);
                        break;
                    }
                }

                ss.LocalPM.SetState(node.Position, node.State);

                foreach (AbstractAction action in ss.GetActions(node))
                {
                    if (Node.TryTraverse(node, action, search, out Node? child))
                    {
                        search.Push(child);
                    }
                }

                ss.LocalPM.SetState(node.Position, null);

                // Cost limit reached
                if (search.Depth > sp.MaxCost) break;
            }

            return new(results);
        }
    }
}
