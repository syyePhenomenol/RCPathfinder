using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions;

/// <summary>
/// An action which can be performed if both the term and state meet the logical prerequisites.
/// Propagates the resulting state.
/// </summary>
/// <param name="start"></param>
/// <param name="destination"></param>
/// <param name="logic"></param>
public class StateLogicAction(Term start, Term destination, DNFLogicDef logic) : LogicAction(start, destination, logic)
{
    public override string Prefix => "stlo";
    public override float Cost => 1f;

    public override bool TryDo(Node node, ProgressionManager pm, out StateUnion? satisfiableStates)
    {
        List<State> resultStates = [];

        if (((DNFLogicDef)Logic).EvaluateStateFrom(pm, Source, resultStates))
        {
            satisfiableStates = new(resultStates);
            return true;
        }

        satisfiableStates = null;
        return false;
    }
}
