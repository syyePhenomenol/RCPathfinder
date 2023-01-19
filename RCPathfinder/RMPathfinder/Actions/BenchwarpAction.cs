using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC.StateVariables;
using RCPathfinder;

namespace RMPathfinder.Actions
{
    /// <summary>
    /// TODO: Make it work with Benchwarp/BenchRando
    /// </summary>
    public record BenchwarpAction : AbstractAction
    {
        private readonly BenchResetVariable brv;
        private readonly ProgressionManager pm;

        public BenchwarpAction(Term newPosition, ProgressionManager pm) : base(newPosition.Name, "bewa", newPosition)
        {
            brv = new(newPosition.Name, pm.lm);
            this.pm = pm;
        }

        public override bool TryDo(Term position, StateUnion stateUnion, out StateUnion? newState)
        {
            List<State> stateList = new();

            foreach (State state in stateUnion)
            {
                stateList.Add(brv.ResetSingle(pm, new LazyStateBuilder(state)).GetState());
            }

            newState = new(stateList);
            return true;
        }
    }
}
