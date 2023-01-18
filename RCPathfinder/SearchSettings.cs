using System.Collections.ObjectModel;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;

namespace RCPathfinder
{
    public abstract class SearchSettings
    {
        public ProgressionManager LocalPM { get; }
        protected ProgressionManager ReferencePM { get; }

        public ReadOnlyDictionary<string, Term> Positions { get; }

        public SearchSettings(ProgressionManager reference)
        {
            ReferencePM = reference;
            LocalPM = new(reference.lm, reference.ctx);

            // Temporarily remove initial progression to reset the PM
            if (LocalPM.ctx is not null)
            {
                ILogicItem ip = LocalPM.ctx.InitialProgression;
                LocalPM.ctx.InitialProgression = new EmptyItem("No Initial Progression");
                LocalPM.Reset();
                LocalPM.ctx.InitialProgression = ip;
            }

            // Treat all state-valued terms as positions
            Dictionary<string, Term> positions = ReferencePM.lm.Terms.Where(t => t.Type is TermType.State)
                .ToDictionary(t => t.Name, t => t);
            Positions = new(positions);
        }

        public virtual void UpdateProgression()
        {
            foreach (Term term in LocalPM.lm.Terms.Where(t => t.Type is not TermType.State))
            {
                LocalPM.Set(term, ReferencePM.Get(term));
            }
        }

        public abstract AbstractAction[] GetActions(Node node);
    }
}
