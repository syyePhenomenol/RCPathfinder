using System.Collections.ObjectModel;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerCore.LogicItems;
using RCPathfinder.Actions;

namespace RCPathfinder
{
    public class SearchData
    {
        public ProgressionManager LocalPM { get; }
        protected ProgressionManager ReferencePM { get; }

        public ReadOnlyCollection<Term> Positions { get; }
        public ReadOnlyDictionary<string, Term> PositionLookup { get; }
        public ReadOnlyCollection<AbstractAction> Actions { get; }
        public ReadOnlyDictionary<Term, ReadOnlyCollection<AbstractAction>> ActionLookup { get; }

        public StateUnion DefaultState { get; }

        public SearchData(ProgressionManager reference)
        {
            ReferencePM = reference;
            LocalPM = CreateLocalPM();

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

            DefaultState = LocalPM.lm.StateManager.DefaultStateSingleton;
        }

        /// <summary>
        /// Override this method to customize logic in the locally stored ProgressionManager.
        /// </summary>
        protected virtual ProgressionManager CreateLocalPM()
        {
            return new(ReferencePM.lm, ReferencePM.ctx);
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
                        // Treat state-valued terms as stateless, unless traversal is done from that term's position
                        LocalPM.SetState(term, ReferencePM.Has(term) ? DefaultState : null);
                        break;
                    default:
                        LocalPM.Set(term, ReferencePM.Get(term));
                        break;
                }
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
