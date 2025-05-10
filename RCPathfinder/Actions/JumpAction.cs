using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions;

/// <summary>
/// Action that propagates to a target from an arbitrary start position.
/// </summary>
public abstract class JumpAction : AbstractAction
{
    public override string Prefix => "jump";
    public sealed override string DebugString => $"{Prefix}: {Cost} -> {Target?.Name ?? "null"}";

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
