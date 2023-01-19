using System.Collections.ObjectModel;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC;
using RCPathfinder;
using RMPathfinder.Actions;
using RM = RandomizerMod.RandomizerMod;

namespace RMPathfinder
{
    public class RMSearchSettings : SearchSettings
    {
        public ReadOnlyDictionary<Term, AbstractAction[]> Actions { get; }
        public AbstractAction[] SpecialActions { get; }
        internal bool VanillaInfectedTransitions { get; }

        private static readonly string[] infectionTransitions =
        {
            "Crossroads_03[bot1]",
            "Crossroads_06[right1]",
            "Crossroads_10[left1]",
            "Crossroads_19[top1]"
        };

        public TransitionData TD { get; }

        public RMSearchSettings() : base(RM.RS.TrackerData.pm)
        {
            TD = new();

            Dictionary<Term, HashSet<AbstractAction>> actions = Positions.Values.ToDictionary(p => p, p => new HashSet<AbstractAction>());
            HashSet<AbstractAction> everywhereActions = new();

            // Add movement within scenes + benchwarps
            foreach (Term term in Positions.Values)
            {
                if (term.Name.StartsWith("Bench-"))
                {
                    everywhereActions.Add(new BenchwarpAction(term, LocalPM));
                }

                if (LocalPM.lm.GetLogicDefStrict(term.Name) is LogicDef ld)
                {
                    foreach (Term parentTerm in ld.GetTerms().Where(t => Positions.ContainsKey(t.Name)))
                    {
                        //RMPathfinder.Instance.LogDebug($"rm-{term.Name} added to {parentTerm.Name}");
                        actions[parentTerm].Add(new StateLogicAction($"rm-{term.Name}", term, LocalPM));
                    }
                }
            }

            // Add start warp
            if (LocalPM.ctx?.InitialProgression is ProgressionInitializer pi && pi.StartStateTerm is Term startTerm)
            {
                everywhereActions.Add(new StartWarpAction(LocalPM, startTerm));

                foreach (Term term in pi.StartStateLinkedTerms)
                {
                    actions[startTerm].Add(new StartLinkedAction(term));
                }
            }

            // Add transitions
            foreach (KeyValuePair<string, string> kvp in TD.TransitionLookup)
            {
                if (Positions.TryGetValue(kvp.Key, out Term source)
                    && Positions.TryGetValue(kvp.Value, out Term target))
                {
                    actions[source].Add(new TransitionAction(LocalPM, source, target));
                }
            }

            Actions = new(actions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()));
            SpecialActions = everywhereActions.ToArray();

            // To remove transitions that are blocked by infection from being included in the pathfinder
            VanillaInfectedTransitions = infectionTransitions.All(TD.VanillaTransitions.Contains);
        }

        public StateUnion GetCurrentState()
        {
            StateManager sm = LocalPM.lm.StateManager;
            StateBuilder sb = new(sm);
            PlayerData pd = PlayerData.instance;

            // USEDSHADE
            sb.SetBool(sm.GetBoolStrict("OVERCHARMED"), pd.GetBool("overcharmed"));
            // SPENTALLSOUL
            // CANNOTREGAINSOUL
            // CANNOTSHADESKIP
            // HASTAKENDAMAGE
            // HASTAKENDOUBLEDAMAGE
            // HASALMOSTDIED
            sb.SetBool(sm.GetBoolStrict("BROKEHEART"), pd.GetBool("brokenCharm_23"));
            sb.SetBool(sm.GetBoolStrict("BROKEGREED"), pd.GetBool("brokenCharm_24"));
            sb.SetBool(sm.GetBoolStrict("BROKESTRENGTH"), pd.GetBool("brokenCharm_25"));
            sb.SetBool(sm.GetBoolStrict("NOFLOWER"), !pd.GetBool("hasXunFlower"));
            // NOPASSEDCHARMEQUIP
            for (int i = 1; i <= 40; i++)
            {
                sb.SetBool(sm.GetBoolStrict($"CHARM{i}"), pd.GetBool($"equippedCharm_{i}"));
                sb.SetBool(sm.GetBoolStrict($"noCHARM{i}"), !pd.GetBool($"gotCharm_{i}"));
            }

            // SPENTSOUL
            // SPENTRESERVESOUL
            // SOULLIMITER
            // REQUIREDMAXSOUL
            // SPENTHP
            // SPENTBLUEHP
            sb.SetInt(sm.GetIntStrict("USEDNOTCHES"), pd.GetInt("charmSlotsFilled"));
            sb.SetInt(sm.GetIntStrict("MAXNOTCHCOST"), pd.GetInt("charmSlots"));

            return new((State)new(sb));
        }

        public override AbstractAction[] GetActions(Node node)
        {
            IEnumerable<AbstractAction> actions;

            if (!Actions.TryGetValue(node.Position, out AbstractAction[] normal))
            {
                throw new InvalidDataException(node.Position.Name);
            }

            if (node.Depth is 0)
            {
                actions = SpecialActions.Concat(normal);
            }
            else
            {
                actions = normal;
            }

            return actions.Where(a => a is not TransitionAction || (IsVanillaOrCheckedTransition(a.BaseName) && !IsInfectedTransition(a.BaseName)))
                .Where(a => ReferencePM.Has(a.NewPosition)).ToArray();
        }

        private bool IsVanillaOrCheckedTransition(string transition)
        {
            return RM.RS.TrackerData.HasVisited(transition) || TD.VanillaTransitions.Contains(transition);
        }

        private bool IsInfectedTransition(string transition)
        {
            return infectionTransitions.Contains(transition)
                && VanillaInfectedTransitions
                && PlayerData.instance.GetBool("crossroadsInfected");
        }
    }
}
