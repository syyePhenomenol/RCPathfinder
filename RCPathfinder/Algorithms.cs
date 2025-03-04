using System.Diagnostics;

namespace RCPathfinder
{
    public static class Algorithms
    {
        public static bool DijkstraSearch(SearchData sd, SearchParams sp, SearchState ss)
        {
            ss.ResetForNewSearch();
            Stopwatch timer = Stopwatch.StartNew();
            sd.LocalPM.StartTemp();

            while (ss.TryPop(out Node? node))
            {
                if (node is null)
                {
                    throw new NullReferenceException(nameof(Node));
                }
                
                // RCPathfinderDebugMod.Instance?.LogDebug($"POP: {node.Cost}, {node.DebugString}");

                // Time limit reached.
                if (timer.ElapsedMilliseconds > sp.MaxTime)
                {
                    // RCPathfinderDebugMod.Instance?.LogDebug("Timed out");

                    sd.LocalPM.RemoveTempItems();
                    ss.Push(node);
                    ss.SearchTime += timer.ElapsedMilliseconds;
                    ss.HasTimedOut = true;

                    // RCPathfinderDebugMod.Instance?.LogDebug("Finishing search");
                    return false;
                }

                // Cost limit reached
                if (node.Cost > sp.MaxCost)
                {   
                    // RCPathfinderDebugMod.Instance?.LogDebug("Max cost reached");

                    sd.LocalPM.RemoveTempItems();
                    ss.Push(node);
                    ss.SearchTime += timer.ElapsedMilliseconds;
                    return false;
                }

                // Depth limit reached
                if (node.Depth > sp.MaxDepth)
                {
                    // RCPathfinderDebugMod.Instance?.LogDebug("Max depth reached");

                    sd.LocalPM.RemoveTempItems();
                    ss.Push(node);
                    ss.SearchTime += timer.ElapsedMilliseconds;
                    return false;
                }

                // Destination reached
                if (!ss.ResultNodes.Contains(node) && sp.Destinations.Contains(node.Current.Term))
                {
                    // RCPathfinderDebugMod.Instance?.LogDebug($"Destination found: {node.DebugString}");
                    
                    ss.AddResultNode(node);

                    bool terminate = sp.TerminationCondition switch
                    {
                        TerminationConditionType.Any => true,
                        TerminationConditionType.AnyUniqueDestination => ss.FoundStartDestinationPairs.All(pair => pair.destination != node.Term),
                        TerminationConditionType.AnyUniqueStartAndDestination => !ss.FoundStartDestinationPairs.Contains((node.Start, node.Term)),
                        TerminationConditionType.EveryDestination => ss.RemainingStartDestinationPairs.All(pair => pair.destination == node.Term),
                        TerminationConditionType.EveryStartAndDestination => ss.RemainingStartDestinationPairs.All(pair => pair == (node.Start, node.Term)),
                        _ => false
                    };

                    ss.FoundStartDestinationPairs.Add((node.Start, node.Term));
                    ss.RemainingStartDestinationPairs.Remove((node.Start, node.Term));

                    if (terminate)
                    {
                        // RCPathfinderDebugMod.Instance?.LogDebug("Termination condition reached");
                        sd.LocalPM.RemoveTempItems();
                        ss.Push(node);
                        ss.SearchTime += timer.ElapsedMilliseconds;
                        return true;
                    }
                }

                // The state of other terms in the PM should not be used for logic evaluation
                if (node.Current is not ArbitraryPosition)
                {
                    sd.LocalPM.SetState(node.Current);
                }

                if (node.EvaluateAndGetChildren(sd, sp, out List<Node> children))
                {
                    foreach (Node child in children)
                    {
                        ss.Push(child);
                    }
                }
                else
                {
                    ss.AddTerminalNode(node);
                }
            }

            // RCPathfinderDebugMod.Instance?.LogDebug("Search fully exhausted");
            sd.LocalPM.RemoveTempItems();
            ss.SearchTime += timer.ElapsedMilliseconds;
            return false;
        }
    }
}
