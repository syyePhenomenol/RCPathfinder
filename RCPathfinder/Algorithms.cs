using System.Diagnostics;

namespace RCPathfinder;

public static class Algorithms
{
    public static bool DijkstraSearch(SearchData sd, SearchParams sp, SearchState ss)
    {
        ss.ResetForNewSearch();
        var timer = Stopwatch.StartNew();
        sd.LocalPM.StartTemp();

        while (ss.TryPop(out var node))
        {
            if (node is null)
            {
                throw new NullReferenceException(nameof(Node));
            }

#if DEBUG
            RCPathfinderDebugMod.Instance?.LogFine($"POP: {node.Cost}, {node.DebugString}");
#endif
            // Time limit reached.
            if (timer.ElapsedMilliseconds > sp.MaxTime)
            {
#if DEBUG
                RCPathfinderDebugMod.Instance?.LogFine("Timed out");
#endif
                sd.LocalPM.RemoveTempItems();
                ss.Push(node);
                ss.SearchTime += timer.ElapsedMilliseconds;
                ss.HasTimedOut = true;
                return false;
            }

            // Cost limit reached
            if (node.Cost > sp.MaxCost)
            {
#if DEBUG
                RCPathfinderDebugMod.Instance?.LogFine("Max cost reached");
#endif
                sd.LocalPM.RemoveTempItems();
                ss.Push(node);
                ss.SearchTime += timer.ElapsedMilliseconds;
                return false;
            }

            // Depth limit reached
            if (node.Depth > sp.MaxDepth)
            {
#if DEBUG
                RCPathfinderDebugMod.Instance?.LogDebug("Max depth reached");
#endif
                sd.LocalPM.RemoveTempItems();
                ss.Push(node);
                ss.SearchTime += timer.ElapsedMilliseconds;
                return false;
            }

            // Destination reached
            if (!ss.ResultNodes.Contains(node) && sp.Destinations.Contains(node.Current.Term))
            {
#if DEBUG
                RCPathfinderDebugMod.Instance?.LogFine($"Destination found: {node.DebugString}");
#endif

                ss.AddResultNode(node);

                var terminate = sp.TerminationCondition switch
                {
                    TerminationConditionType.Any => true,
                    TerminationConditionType.AnyUniqueDestination => ss.FoundStartDestinationPairs.All(pair =>
                        pair.destination != node.Term
                    ),
                    TerminationConditionType.AnyUniqueStartAndDestination => !ss.FoundStartDestinationPairs.Contains(
                        (node.Start, node.Term)
                    ),
                    TerminationConditionType.EveryDestination => ss.RemainingStartDestinationPairs.All(pair =>
                        pair.destination == node.Term
                    ),
                    TerminationConditionType.EveryStartAndDestination => ss.RemainingStartDestinationPairs.All(pair =>
                        pair == (node.Start, node.Term)
                    ),
                    _ => false,
                };

                _ = ss.FoundStartDestinationPairs.Add((node.Start, node.Term));
                _ = ss.RemainingStartDestinationPairs.Remove((node.Start, node.Term));

                if (terminate)
                {
#if DEBUG
                    RCPathfinderDebugMod.Instance?.LogDebug("Termination condition reached");
#endif
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

            if (node.EvaluateAndGetChildren(sd, sp, out var children))
            {
                foreach (var child in children)
                {
#if DEBUG
                    RCPathfinderDebugMod.Instance?.LogDebug($"PUSH: {child.Cost}, {child.DebugString}");
#endif
                    ss.Push(child);
                }
            }
            else
            {
                ss.AddTerminalNode(node);
            }
        }

#if DEBUG
        RCPathfinderDebugMod.Instance?.LogDebug("Search fully exhausted");
#endif
        sd.LocalPM.RemoveTempItems();
        ss.SearchTime += timer.ElapsedMilliseconds;
        return false;
    }
}
