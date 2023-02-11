using RandomizerCore.Logic;
using RCPathfinder.Actions;

namespace RCPathfinder
{
    public static class Algorithms
    {
        public static bool DijkstraSearch(SearchData sd, SearchParams sp, SearchState ss)
        {
            ss.ResetForNewSearch();

            HashSet<Term> dests = new(sp.Destinations);

            HashSet<Term> remainingStarts = new(sp.StartPositions.Select(s => s.Term));
            HashSet<Term> remainingDests = new(sp.Destinations);

            while (ss.TryPop(out Node? node))
            {
                if (node is null) throw new NullReferenceException(nameof(Node));

                // Cost limit reached
                if (node.Cost > sp.MaxCost)
                {
                    ss.Push(node);
                    return true;
                }

                // Destination reached
                if (!ss.ResultNodes.Contains(node) && dests.Contains(node.Position))
                {
                    remainingStarts.Remove(node.StartPosition);
                    remainingDests.Remove(node.Position);

                    ss.AddResultNode(node);

                    bool terminate = sp.TerminationCondition switch
                    {
                        TerminationConditionType.Any => true,
                        TerminationConditionType.EveryDestination => !remainingDests.Any(),
                        TerminationConditionType.EveryStartAndDestination => !remainingStarts.Any() && !remainingDests.Any(),
                        _ => false
                    };

                    if (terminate)
                    {
                        ss.Push(node);
                        return true;
                    }
                }

                // Add states to current position and traverse to adjacent nodes
                sd.LocalPM.SetState(node.Position, node.States);

                foreach (AbstractAction action in sd.GetActions(node))
                {
                    if (Node.TryTraverse(sd.LocalPM, ss, node, action, out Node? child))
                    {
                        ss.Push(child);
                    }
                }

                sd.LocalPM.SetState(node.Position, sd.DefaultState);
            }

            return false;
        }
    }
}
