using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// Generic action for evaluating logic.
    /// </summary>
    public class LogicAction(Term source, Term target, LogicDef logic) : StandardAction(source, target)
    {
        public override string Prefix => "logi";
        public override float Cost => 1f;
        public LogicDef Logic { get; } = logic;

        public override bool TryDo(Node node, ProgressionManager pm, out StateUnion? satisfiableStates)
        {
            if (Logic.CanGet(pm))
            {
                satisfiableStates = node.Current.States;
                return true;
            }

            satisfiableStates = default;
            return false;
        }

        public override bool TryDoStateless(Node node, ProgressionManager pm)
        {
            return Logic.CanGet(pm);
        }
    }
}
