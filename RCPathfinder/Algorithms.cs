using RandomizerCore.Logic;
using RCPathfinder.Actions;
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
                if (node is null) throw new NullReferenceException(nameof(Node));

                // RCPathfinderDebugMod.Instance?.LogDebug(node.PrintActions());

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
                if (!ss.ResultNodes.Contains(node) && sp.Destinations.Contains(node.CurrentPosition))
                {
                    // RCPathfinderDebugMod.Instance?.LogDebug($"Destination found: {node.Position.Name}");

                    ss.AddResultNode(node);

                    bool terminate = sp.TerminationCondition switch
                    {
                        TerminationConditionType.Any => true,
                        TerminationConditionType.AnyUniqueDestination => ss.FoundStartDestinationPairs.All(pair => pair.destination != node.CurrentPosition),
                        TerminationConditionType.AnyUniqueStartAndDestination => !ss.FoundStartDestinationPairs.Contains((node.StartPosition, node.CurrentPosition)),
                        TerminationConditionType.EveryDestination => ss.RemainingStartDestinationPairs.All(pair => pair.destination == node.CurrentPosition),
                        TerminationConditionType.EveryStartAndDestination => ss.RemainingStartDestinationPairs.All(pair => pair == (node.StartPosition, node.CurrentPosition)),
                        _ => false
                    };

                    ss.FoundStartDestinationPairs.Add((node.StartPosition, node.CurrentPosition));
                    ss.RemainingStartDestinationPairs.Remove((node.StartPosition, node.CurrentPosition));

                    if (terminate)
                    {
                        // RCPathfinderDebugMod.Instance?.LogDebug("Termination condition reached");
                        sd.LocalPM.RemoveTempItems();
                        ss.Push(node);
                        ss.SearchTime += timer.ElapsedMilliseconds;
                        return true;
                    }
                }

                // Set states to current position and traverse to adjacent nodes
                // Since LocalPM is in temp mode, we don't need to reset any states straight afterwards
                sd.LocalPM.SetState(node.CurrentPosition, node.CurrentStates);
                
                foreach (AbstractAction action in sd.GetActions(node))
                {
                    if (TryTraverse(sd.LocalPM, sp, node, action, out Node? child))
                    {
                        ss.Push(child);
                    }
                }
            }

            // RCPathfinderDebugMod.Instance?.LogDebug("Search fully exhausted");
            sd.LocalPM.RemoveTempItems();
            ss.SearchTime += timer.ElapsedMilliseconds;
            return false;
        }
        
        private static bool TryTraverse(ProgressionManager pm, SearchParams sp, Node parent, AbstractAction action, out Node? child)
        {
            if (sp.DisallowBacktracking && parent.IsPreviouslyVisitedPosition(action.Destination))
            {
                child = null;
                return false;
            }

            if (sp.Stateless && action is StateLogicAction stla)
            {
                action = new StateIgnoringAction(stla);
            }

            return parent.TryTraverse(pm, action, out child);
        }
    }


    
}
