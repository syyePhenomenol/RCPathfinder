using System.Collections.ObjectModel;
using System.Diagnostics;
using RandomizerCore.Logic;

namespace RCPathfinder
{
    public static class Algorithms
    {
        /// <summary>
        /// Returns a SearchNode for every specified destination where possible.
        /// If no destinations were provided, instead returns all the SearchNodes reached at the specified max depth.
        /// Search can resume from previous SearchState if provided, in which case the starting position/state are ignored.
        /// </summary>
        public static ReadOnlyDictionary<Term, Node> DijkstraSearch(SearchSettings ss, SearchParams sp, SearchState? search = null)
        {
            HashSet<Term> destinations = new(sp.Destinations);

            search ??= new(sp);

            Dictionary<Term, Node> results = new();

            int nodeCount = 0;

            Stopwatch sw = Stopwatch.StartNew();

            while (search.TryPop(out Node? node))
            {
                if (node is null) throw new NullReferenceException(nameof(Node));

                nodeCount++;

                if (destinations.Contains(node.Position) && !results.ContainsKey(node.Position))
                {
                    results.Add(node.Position, node);

                    if (sp.SingleResultTermination || results.Count == destinations.Count)
                    {
                        search.Push(node);
                        break;
                    }
                }

                foreach (AbstractAction action in ss.GetActions(node))
                {
                    if (Node.TryTraverse(node, action, search, out Node? child))
                    {
                        //RMPathfinder.RMPathfinder.Instance.LogDebug($"{child.PrintActions()}");
                        search.Push(child);
                    }
                }

                // Cost limit reached
                if (search.Depth > sp.MaxCost) break;
            }

            sw.Stop();

            RMPathfinder.RMPathfinder.Instance.LogDebug($"Explored {nodeCount} nodes in {sw.ElapsedMilliseconds} ms. Average nodes/ms: {(float)nodeCount / sw.ElapsedMilliseconds}");

            return new(results);
        }
    }
}
