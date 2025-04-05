using RandomizerCore.Logic;

namespace RCPathfinder.Actions;

/// <summary>
/// Action that propagates from a fixed single source to a fixed single target.
/// </summary>
/// <param name="source"></param>
/// <param name="target"></param>
public abstract class StandardAction(Term source, Term target) : AbstractAction
{
    public Term Source { get; } = source;
    public override Term Target => target;
    public sealed override string DebugString => $"{Prefix}: {Source.Name} -> {Cost} -> {Target.Name}";
}
