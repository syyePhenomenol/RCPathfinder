using RCPathfinder.Actions;
using System.Diagnostics;

namespace RCPathfinder
{
    public static class Algorithms
    {
        public static bool DijkstraSearch(SearchData sd, SearchParams sp, SearchState ss)
        {
            return DijkstraSearch(sd, sp, ss, false);
        }

        public static bool DijkstraSearch(SearchData sd, SearchParams sp, SearchState ss, bool stateless)
        {
            ss.ResetForNewSearch();

            Stopwatch timer = Stopwatch.StartNew();

            while (ss.TryPop(out Node? node))
            {
                if (node is null) throw new NullReferenceException(nameof(Node));

                // Time limit reached. Continue search stateless if not already done so
                if (timer.ElapsedMilliseconds > sp.MaxTime && !stateless && sp.ContinueStateless)
                {
                    ss.Push(node);
                    ss.SearchTime += timer.ElapsedMilliseconds;
                    ss.HasTimedOut = true;
                    return DijkstraSearch(sd, sp, ss, true);
                }

                // Cost limit reached
                if (node.Cost > sp.MaxCost)
                {
                    ss.Push(node);
                    ss.SearchTime += timer.ElapsedMilliseconds;
                    return false;
                }

                // Destination reached
                if (!ss.ResultNodes.Contains(node) && sp.Destinations.Contains(node.Position))
                {
                    ss.AddResultNode(node);

                    bool terminate = sp.TerminationCondition switch
                    {
                        TerminationConditionType.Any => true,
                        TerminationConditionType.AnyUniqueDestination => ss.FoundStartDestinationPairs.All(pair => pair.destination != node.Position),
                        TerminationConditionType.AnyUniqueStartAndDestination => !ss.FoundStartDestinationPairs.Contains((node.StartPosition, node.Position)),
                        TerminationConditionType.EveryDestination => ss.RemainingStartDestinationPairs.All(pair => pair.destination == node.Position),
                        TerminationConditionType.EveryStartAndDestination => ss.RemainingStartDestinationPairs.All(pair => pair == (node.StartPosition, node.Position)),
                        _ => false
                    };

                    ss.FoundStartDestinationPairs.Add((node.StartPosition, node.Position));
                    ss.RemainingStartDestinationPairs.Remove((node.StartPosition, node.Position));

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
                    if (Node.TryTraverse(sd.LocalPM, ss, node, action, out Node? child, stateless))
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
