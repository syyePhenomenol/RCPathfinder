using System.Diagnostics;
using Modding;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder;

namespace RCPathfinder.RMPathfinder
{
    public class RMPathfinder : Mod
    {
        public override string GetVersion() => "test";

        public static RMPathfinder? Instance { get; private set; }

        private static readonly Random rng = new(0);

        public RMPathfinder()
        {
            Instance = this;
        }

        public override void Initialize()
        {
            On.GameManager.StartNewGame += GameManager_StartNewGame;
            On.GameManager.ContinueGame += GameManager_ContinueGame;
        }

        private void GameManager_ContinueGame(On.GameManager.orig_ContinueGame orig, GameManager self)
        {
            orig(self);
            GlobalTest();
        }

        private void GameManager_StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
        {
            orig(self, permadeathMode, bossRushMode);
            GlobalTest();
        }

        private void GlobalTest()
        {
            if (!RandomizerMod.RandomizerMod.IsRandoSave) return;

            RMSearchSettings ss = new(RandomizerMod.RandomizerMod.RS.TrackerData.pm);

            foreach (var action in ss.SpecialActions)
            {
                LogDebug(action.Name);
            }

            Term[] inLogicTerms = ss.Positions.Values.Where(p => RandomizerMod.RandomizerMod.RS.TrackerData.pm.Has(p) && p.Name.Contains("[")).ToArray();

            StateUnion startState = RandomizerMod.RandomizerMod.RS.TrackerData.lm.StateManager.DefaultStateSingleton;

            ss.UpdateProgression();

            foreach (Term position in inLogicTerms)
            {
                LogDebug($"{position}");

                foreach (AbstractAction action in ss.Actions[position])
                {
                    LogDebug($"-> {action.NewPosition}: {action.TryDo(position, startState, out StateUnion? _)}");
                }
            }

            SearchParams? sp = new
                (
                    GetRandomTerm(inLogicTerms),
                    startState,
                    new Term[] { GetRandomTerm(inLogicTerms) },
                    1000f,
                    false
                );

            LogDebug($"Starting RCPathfinder test:");

            Stopwatch globalSW = Stopwatch.StartNew();

            int testCount = 10;
            int successes = 0;

            for (int i = 0; i < testCount; i++)
            {
                sp.StartPosition = GetRandomTerm(inLogicTerms);
                sp.Destinations = new Term[] { GetRandomTerm(inLogicTerms) };

                LogDebug($"Trying {sp.StartPosition} -> ? -> {sp.Destinations[0]}");

                if (Test(ss, sp))
                {
                    successes++;
                }
                else
                {
                    LogDebug($"{sp.StartPosition} to {sp.Destinations[0]} failed");
                }
            }

            globalSW.Stop();

            LogDebug($"Total computation time: {globalSW.ElapsedMilliseconds} ms");
            LogDebug($"Total successes: {successes}/{testCount}");
        }

        private bool Test(SearchSettings ss, SearchParams sp)
        {
            SearchState search = new(sp);

            var result = Algorithms.DijkstraSearch(ss, sp, search);

            if (result.Count is 1)
            {
                LogDebug($"  Success!");
                foreach (AbstractAction action in result.First().Value.Actions)
                {
                    LogDebug($"    {action.Name}");
                }
            }
            else
            {
                if (search.Queue.Count > 0)
                {
                    LogDebug($"  Search terminated after reaching max cost.");
                }
                else
                {
                    LogDebug($"  Search exhausted with no route found.");
                }
            }

            return result.Count is 1;
        }

        private static Term GetRandomTerm(Term[] terms)
        {
            return terms[rng.Next(terms.Length)];
        }

        private static State GetCurrentState(StateManager sm)
        {
            return sm.DefaultState;
        }

        private static Term? GetCurrentPosition()
        {
            return null;
        }
    }
}
