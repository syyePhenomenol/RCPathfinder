using RandomizerCore;
using RandomizerMod.RC;
using RM = RandomizerMod.RandomizerMod;

namespace RMPathfinder
{
    public class TransitionData
    {
        public HashSet<string> VanillaTransitions { get; }
        public HashSet<string> RandomizedTransitions { get; }
        public Dictionary<string, string> TransitionLookup { get; }

        public TransitionData()
        {
            VanillaTransitions = new();
            RandomizedTransitions = new();
            TransitionLookup = new();

            // Add transition placements
            foreach (GeneralizedPlacement gp in RM.RS.Context.Vanilla)
            {
                if (IsTransition(gp.Location.Name) && IsTransition(gp.Item.Name))
                {
                    VanillaTransitions.Add(gp.Location.Name);
                    VanillaTransitions.Add(gp.Item.Name);
                    TransitionLookup[gp.Location.Name] = gp.Item.Name;
                }
            }
            if (RM.RS.Context.transitionPlacements is not null)
            {
                foreach (TransitionPlacement tp in RM.RS.Context.transitionPlacements)
                {
                    RandomizedTransitions.Add(tp.Source.Name);
                    RandomizedTransitions.Add(tp.Target.Name);
                    TransitionLookup[tp.Source.Name] = tp.Target.Name;
                }
            }
        }

        public bool IsTransitionRando()
        {
            return RandomizedTransitions.Any();
        }

        /// <summary>
        /// Prefer this way so that connection transitions can be recognised
        /// </summary>
        private static bool IsTransition(string term)
        {
            return term.Contains('[') && term.Contains(']');
        }
    }
}
