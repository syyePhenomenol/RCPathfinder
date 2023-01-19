using System.Diagnostics;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;
using Rmp = RMPathfinder.RMPathfinder;

namespace RMPathfinder
{
    internal static class Testing
    {
        private static readonly Random rng = new(0);

        internal static void GlobalTest()
        {
            if (Rmp.Rmss is null) throw new NullReferenceException(nameof(Rmp.Rmss));

            Term[] inLogicTransitions = Rmp.Rmss.Positions.Values.Where(p => RandomizerMod.RandomizerMod.RS.TrackerData.pm.Has(p) && p.Name.Contains("[")).ToArray();
            
            Rmp.Rmss.UpdateProgression();

            SearchParams? sp = new
                (
                    GetRandomTerm(inLogicTransitions),
                    Rmp.Rmss.GetCurrentState(),
                    new Term[] { GetRandomTerm(inLogicTransitions) },
                    1000f,
                    false
                );

            Rmp.Instance?.LogDebug($"Starting RCPathfinder test:");

            Stopwatch globalSW = Stopwatch.StartNew();
            Stopwatch sw = Stopwatch.StartNew();

            int testCount = 1;
            int successes = 0;

            for (int i = 0; i < testCount; i++)
            {
                sp.StartPosition = GetRandomTerm(inLogicTransitions);
                sp.Destinations = new Term[] { GetRandomTerm(inLogicTransitions) };

                SearchState search = new(sp);

                Rmp.Instance?.LogDebug($"Trying {sp.StartPosition} -> ? -> {sp.Destinations[0]}");

                sw.Restart();

                if (DoTest(Rmp.Rmss, sp, search))
                {
                    successes++;
                }
                else
                {
                    Rmp.Instance?.LogDebug($"{sp.StartPosition} to {sp.Destinations[0]} failed");
                }

                sw.Stop();

                float localElapsedMS = sw.ElapsedTicks * 1000f / Stopwatch.Frequency;
                Rmp.Instance?.LogDebug($"Explored {search.NodesTraversed} nodes in {localElapsedMS} ms. Average nodes/ms: {search.NodesTraversed / localElapsedMS}");
            }

            globalSW.Stop();

            float globalElapsedMS = globalSW.ElapsedTicks * 1000f / Stopwatch.Frequency;

            Rmp.Instance?.LogDebug($"Total computation time: {globalElapsedMS} ms");
            Rmp.Instance?.LogDebug($"Total successes: {successes}/{testCount}");
            Rmp.Instance?.LogDebug($"Average serarch time: {globalElapsedMS / testCount} ms");
        }

        private static bool DoTest(SearchSettings ss, SearchParams sp, SearchState search)
        {
            var result = Algorithms.DijkstraSearch(ss, sp, search);

            if (result.Count is 1)
            {
                Rmp.Instance?.LogDebug($"  Success!");
                foreach (AbstractAction action in result.First().Value.Actions)
                {
                    Rmp.Instance?.LogDebug($"    {action.FullName}");
                }
            }
            else
            {
                if (search.Queue.Count > 0)
                {
                    Rmp.Instance?.LogDebug($"  Search terminated after reaching max cost.");
                }
                else
                {
                    Rmp.Instance?.LogDebug($"  Search exhausted with no route found.");
                }
            }

            return result.Count is 1;
        }

        private static Term GetRandomTerm(Term[] terms)
        {
            return terms[rng.Next(terms.Length)];
        }
    }
}
