using System.Diagnostics;
using RandomizerCore.Logic;

namespace RCPathfinder;

public static class Algorithms
{
    public static bool DijkstraSearch(ProgressionManager pm, SearchData sd, SearchParams sp, SearchState ss)
    {
        if (pm.lm != sd.LM || pm.ctx != sd.Context)
        {
            throw new InvalidDataException(
                "The LogicManager and Context of the ProgressionManager do not match the SearchData."
            );
        }

        ss.ResetForNewSearch();
        var timer = Stopwatch.StartNew();
        pm.StartTemp();

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
                pm.RemoveTempItems();
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
                pm.RemoveTempItems();
                ss.Push(node);
                ss.SearchTime += timer.ElapsedMilliseconds;
                return false;
            }

            // Depth limit reached
            if (node.Depth > sp.MaxDepth)
            {
#if DEBUG
                RCPathfinderDebugMod.Instance?.LogFine("Max depth reached");
#endif
                pm.RemoveTempItems();
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
                    RCPathfinderDebugMod.Instance?.LogFine("Termination condition reached");
#endif
                    pm.RemoveTempItems();
                    ss.Push(node);
                    ss.SearchTime += timer.ElapsedMilliseconds;
                    return true;
                }
            }

            // The state of other terms in the PM should not be used for logic evaluation
            if (node.Current is not ArbitraryPosition)
            {
                pm.SetState(node.Current);
            }

            if (node.EvaluateAndGetChildren(pm, sd, sp, out var children))
            {
                foreach (var child in children)
                {
#if DEBUG
                    RCPathfinderDebugMod.Instance?.LogFine($"PUSH: {child.Cost}, {child.DebugString}");
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
        RCPathfinderDebugMod.Instance?.LogFine("Search fully exhausted");
#endif
        pm.RemoveTempItems();
        ss.SearchTime += timer.ElapsedMilliseconds;
        return false;
    }
}
