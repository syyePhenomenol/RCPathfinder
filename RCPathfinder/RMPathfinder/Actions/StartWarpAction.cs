using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC.StateVariables;
using RCPathfinder;

namespace RMPathfinder.Actions
{
    public record StartWarpAction : AbstractAction
    {
        private readonly ProgressionManager pm;
        private readonly WarpToStartResetVariable startReset;

        public StartWarpAction(ProgressionManager pm, Term newPosition) : base(newPosition.Name, "stwa", newPosition)
        {
            this.pm = pm;
            startReset = new(nameof(StartWarpAction), pm.lm);
        }

        public override bool TryDo(Term position, StateUnion stateUnion, out StateUnion? newState)
        {
            List<State> stateList = new();

            foreach (State state in stateUnion)
            {
                stateList.Add(startReset.ResetSingle(pm, new LazyStateBuilder(state)).GetState());
            }

            newState = new(stateList);
            return true;
        }
    }
}
