using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions;

/// <summary>
/// An action that links a source term to a target term without logical prerequisite.
/// </summary>
/// <param name="source"></param>
/// <param name="target"></param>
public class PlacementAction(Term source, Term target) : StandardAction(source, target)
{
    public override string Prefix => "plac";
    public override float Cost => 1f;

    public override bool TryDo(Node node, ProgressionManager pm, out StateUnion? satisfiableStates)
    {
        satisfiableStates = node.Current.States;
        return true;
    }

    public override bool TryDoStateless(Node node, ProgressionManager pm)
    {
        return true;
    }
}
