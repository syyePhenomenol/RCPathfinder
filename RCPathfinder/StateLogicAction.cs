using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public record StateLogicAction : AbstractAction
    {
        private readonly ProgressionManager pm;
        private readonly DNFLogicDef newPositionDNF;

        public StateLogicAction(string name, Term term, ProgressionManager pm, float cost = 1f) : base(name, "stlo", term, cost)
        {
            this.pm = pm;

            if (!this.pm.lm.LogicLookup.TryGetValue(NewPosition.Name, out LogicDef newPositionLogic)
                || newPositionLogic is not DNFLogicDef)
            {
                throw new InvalidDataException(NewPosition.Name);
            }

            newPositionDNF = (DNFLogicDef)newPositionLogic;
        }

        public override bool TryDo(Term position, StateUnion state, out StateUnion? newState)
        {
            return newPositionDNF.CheckForUpdatedState(pm, null, new(), position, out newState);
        }
    }
}
