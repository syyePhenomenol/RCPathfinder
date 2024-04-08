using System.Collections.ObjectModel;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringLogic;
using RCPathfinder.Actions;

namespace RCPathfinder
{
    public class SearchData
    {
        public ProgressionManager LocalPM { get; }
        public ProgressionManager ReferencePM { get; }
        public ReadOnlyCollection<Term> Positions { get; }
        public ReadOnlyDictionary<string, Term> PositionLookup { get; }
        public ReadOnlyCollection<AbstractAction> Actions { get; }
        public ReadOnlyDictionary<Term, ReadOnlyCollection<AbstractAction>> ActionLookup { get; }
        private Dictionary<Term, Term> _proxyBoolTermLookup;

        public SearchData(ProgressionManager pm)
        {
            ReferencePM = pm;
            _proxyBoolTermLookup = new();
            LocalPM = new(new(CreateLocalLM(new(pm.lm))), pm.ctx);

            // Temporarily remove initial progression to reset the PM
            if (LocalPM.ctx is not null)
            {
                ILogicItem ip = LocalPM.ctx.InitialProgression;
                LocalPM.ctx.InitialProgression = new EmptyItem("");
                LocalPM.Reset();
                LocalPM.ctx.InitialProgression = ip;
            }

            Positions = LocalPM.lm.Terms.GetTermList(TermType.State);
            PositionLookup = new(Positions.ToDictionary(p => p.Name, p => p));
            Actions = new(CreateActions().Distinct().ToArray());
            ActionLookup = new(CreateActionLookup().ToDictionary(kvp => kvp.Key, kvp => new ReadOnlyCollection<AbstractAction>(kvp.Value.Distinct().OrderBy(a => a.Destination.Id).ToArray())));
        }

        /// <summary>
        /// Override this method to customize logic in the locally stored ProgressionManager.
        /// By default, replaces projection tokens with new bool term tokens to evaluate StateLogicDefs
        /// by individual StateProviders at a time.
        /// </summary>
        protected virtual LogicManagerBuilder CreateLocalLM(LogicManagerBuilder lmb)
        {
            HashSet<string> projectedTokenTerms = new();
            List<string> affectedLogicDefs = new();

            foreach (var kvp in lmb.LogicLookup)
            {
                foreach (var token in kvp.Value.Tokens.Where(t => t is ProjectedToken))
                {
                    affectedLogicDefs.Add(kvp.Key);
                    projectedTokenTerms.Add(((ProjectedToken)token).Inner.Write());
                }
            }

            foreach (var term in projectedTokenTerms)
            {
                var proxyBoolTerm = lmb.GetOrAddTerm($"{term}_bool", TermType.SignedByte);
                _proxyBoolTermLookup.Add(lmb.GetTerm(term), proxyBoolTerm);

                foreach (var ld in affectedLogicDefs)
                {
                    lmb.DoSubst(new(ld, $"{term}/", proxyBoolTerm.Name));
                }
            }

            return lmb;
        }

        /// <summary>
        /// Override this method to add new AbstractActions.
        /// </summary>
        protected virtual List<AbstractAction> CreateActions()
        {
            // Default logic actions
            List<AbstractAction> actions = new(Positions.Select(p => new StateLogicAction(p, (StateLogicDef)LocalPM.lm.GetLogicDefStrict(p.Name))).Where(a => a.StartPositions.Any()));

            if (LocalPM.ctx is null) return actions;

            // Placement actions
            foreach (GeneralizedPlacement gp in LocalPM.ctx.EnumerateExistingPlacements())
            {
                if (PositionLookup.TryGetValue(gp.Location.Name, out Term startPosition)
                    && PositionLookup.TryGetValue(gp.Item.Name, out Term destination))
                {
                    actions.Add(new PlacementAction(startPosition, destination));
                }
            }

            return actions;
        }

        /// <summary>
        /// Override this method to add or remove connections between position Terms and AbstractActions.
        /// </summary>
        protected virtual Dictionary<Term, List<AbstractAction>> CreateActionLookup()
        {
            Dictionary<Term, List<AbstractAction>> actionLookup = new();

            foreach (AbstractAction action in Actions)
            {
                foreach(Term startPosition in action.StartPositions)
                {
                    if (actionLookup.TryGetValue(startPosition, out var actions))
                    {
                        actions.Add(action);
                        continue;
                    }

                    actionLookup[startPosition] = new() { action };
                }
            }

            return actionLookup;
        }

        /// <summary>
        /// Override this method to modify the value of terms before running a search.
        /// </summary>
        public virtual void UpdateProgression()
        {
            foreach (Term term in ReferencePM.lm.Terms)
            {
                switch (term.Type)
                {
                    case TermType.State:
                        // Remove all states. Manually add states in based on a node's position
                        LocalPM.SetState(term, null);
                        break;
                    default:
                        LocalPM.Set(term, ReferencePM.Get(term));
                        break;
                }
            }

            foreach (var kvp in _proxyBoolTermLookup)
            {
                LocalPM.Set(kvp.Value, ReferencePM.Get(kvp.Key));
            }
        }

        /// <summary>
        /// Override this method to add or prune actions depending on the node.
        /// </summary>
        public virtual List<AbstractAction> GetActions(Node node)
        {
            if (ActionLookup.TryGetValue(node.CurrentPosition, out var actions))
            {
                return actions.Where(a => ReferencePM.lm.GetTerm(a.Destination.Name) is null
                    || ReferencePM.Has(a.Destination)).ToList();
            }
            return new();
        }
    }
}
