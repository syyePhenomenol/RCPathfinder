using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// An action which can be performed if both the term and state meet the logical prerequisites.
    /// Propagates the resulting state.
    /// </summary>
    public class StateLogicAction(Term start, Term destination, DNFLogicDef logic) : LogicAction(start, destination, logic)
    {
        public override string Prefix => "stlo";
        public override float Cost => 1f;

        public override bool TryDo(Node node, ProgressionManager pm, out StateUnion? satisfiableStates)
        {
            // Gets valid states based on the single term entry in the pm
            // The "current" parameter refers to the existing StateUnion of the *destination*, which is why it is set null.
            return ((DNFLogicDef)Logic).CheckForUpdatedState(pm, null, [], Source, out satisfiableStates);
        }
    }
}
