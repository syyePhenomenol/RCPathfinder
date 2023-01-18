using System.Collections.ObjectModel;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RC;

namespace RCPathfinder.RMPathfinder
{
    public class RMSearchSettings : SearchSettings
    {
        public ReadOnlyDictionary<int, AbstractAction[]> Actions { get; }
        public AbstractAction[] SpecialActions { get; }

        public RMSearchSettings(ProgressionManager LocalPM) : base(LocalPM)
        {
            Dictionary<int, HashSet<AbstractAction>> actions = Positions.Values.ToDictionary(p => p.Id, p => new HashSet<AbstractAction>());
            HashSet<AbstractAction> specialActions = new();

            // Add movement within scenes + benchwarps
            foreach (Term term in Positions.Values)
            {
                //if (term.Name.StartsWith("Bench-"))
                //{
                //    specialActions.Add(new WarpAction(term, LocalPM));
                //}

                if (LocalPM.lm.GetLogicDefStrict(term.Name) is LogicDef ld)
                {
                    foreach (Term parentTerm in ld.GetTerms().Where(t => Positions.ContainsKey(t.Name)))
                    {
                        //RMPathfinder.Instance.LogDebug($"rm-{term.Name} added to {parentTerm.Name}");
                        actions[parentTerm].Add(new StateLogicAction($"rm-{term.Name}", term, LocalPM));
                    }
                }
            }

            // Add start "warp"
            if (LocalPM.ctx?.InitialProgression is ProgressionInitializer pi && pi.StartStateTerm is not null)
            {
                specialActions.Add(new StartWarpAction(LocalPM, pi.StartStateTerm));

                foreach (Term term in pi.StartStateLinkedTerms)
                {
                    actions[pi.StartStateTerm].Add(new StartLinkedAction(term));
                }
            }

            // Add transition placements
            foreach (GeneralizedPlacement gp in RandomizerMod.RandomizerMod.RS.Context.Vanilla)
            {
                if (Positions.TryGetValue(gp.Item.Name, out Term target)
                    && Positions.TryGetValue(gp.Location.Name, out Term source))
                {
                    actions[source].Add(new TransitionAction(LocalPM, source, target));
                }
            }
            if (RandomizerMod.RandomizerMod.RS.Context.transitionPlacements is not null)
            {
                foreach (TransitionPlacement tp in RandomizerMod.RandomizerMod.RS.Context.transitionPlacements)
                {
                    if (Positions.TryGetValue(tp.Target.Name, out Term target)
                        && Positions.TryGetValue(tp.Source.Name, out Term source))
                    {
                        actions[source].Add(new TransitionAction(LocalPM, source, target));
                    }
                }
            }

            Actions = new(actions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()));
            SpecialActions = specialActions.ToArray();
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

            return actions.Where(a => ReferencePM.Has(a.NewPosition)).ToArray();
        }
    }
}
