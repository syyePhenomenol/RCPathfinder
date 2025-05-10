using System.Collections.ObjectModel;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder.Actions;

namespace RCPathfinder;

public class Node
{
    private readonly List<AbstractAction> _actions;

    // Nodes from the same StartPosition have the same State lookup.
    private readonly Dictionary<Term, StateUnion> _visitedStatesLookup;

    public Node(Position start)
    {
        _actions = [];
        _visitedStatesLookup = [];
        Start = start;
        Current = start;
        Depth = 0;

        if (start.Term is not null)
        {
            _visitedStatesLookup[start.Term] = start.States;
        }
    }

    public Node(Node parent, AbstractAction action, StateUnion newStates)
    {
        _actions = [.. parent._actions, action];
        _visitedStatesLookup = parent._visitedStatesLookup;
        Start = parent.Start;
        Current = new(action.Target, newStates, parent.Cost + action.Cost);
        Depth = parent.Depth + 1;
    }

    public Position Start { get; }
    public Position Current { get; }

    public Term Term => Current.Term;
    public float Cost => Current.Cost;
    public int Depth { get; }

    public ReadOnlyCollection<AbstractAction> Actions => new(_actions);
    public string DebugString => string.Join(", ", [Start.DebugString, .. _actions.Select(a => a.DebugString)]);

    internal bool EvaluateAndGetChildren(ProgressionManager pm, SearchData sd, SearchParams sp, out List<Node> children)
    {
        List<AbstractAction> nextActions = [];
        children = [];

        if (sd.StandardActionLookup.TryGetValue(Current.Term, out var otoActions))
        {
            nextActions.AddRange(otoActions.Where(a => pm.Has(a.Target)).Cast<AbstractAction>());
        }

        nextActions.AddRange(sd.GetAdditionalActions(this));

        if (!nextActions.Any())
        {
            return false;
        }

        foreach (var action in nextActions)
        {
            if (action.Target is null)
            {
                continue;
            }

            if (sp.DisallowBacktracking && IsPreviouslyVisitedTerm(action.Target))
            {
                continue;
            }

            if (
                (
                    (!sp.Stateless && TryTraverse(pm, action, out var child))
                    || (sp.Stateless && TryTraverseStateless(pm, action, out child))
                ) && child is not null
            )
            {
#if DEBUG
                if (!pm.Has(action.Target))
                {
                    RCPathfinderDebugMod.Instance?.LogDebug($"Can traverse but target not in pm: {action.DebugString}");
                }
#endif
                children.Add(child);
            }
        }

        return children.Any();
    }

    private bool TryTraverse(ProgressionManager pm, AbstractAction action, out Node? child)
    {
        if (
            action.TryDo(this, pm, out var satisfiableStates)
            && satisfiableStates is not null
            && TryAddVisitedStates(action.Target, satisfiableStates, out var unvisitedStates)
        )
        {
            child = new(this, action, unvisitedStates);
            return true;
        }

        child = default;
        return false;
    }

    private bool TryTraverseStateless(ProgressionManager pm, AbstractAction action, out Node? child)
    {
        if (
            action.TryDoStateless(this, pm)
            && TryAddVisitedStates(action.Target, Current.States, out var unvisitedStates)
        )
        {
            child = new(this, action, unvisitedStates);
            return true;
        }

        child = default;
        return false;
    }

    /// <summary>
    /// Returns true if any new or better states are added.
    /// </summary>
    /// <param name="term"></param>
    /// <param name="states"></param>
    /// <param name="newStates"></param>
    private bool TryAddVisitedStates(Term? term, StateUnion states, out StateUnion newStates)
    {
        if (term is null)
        {
            newStates = StateUnion.Empty;
            return false;
        }

        if (!_visitedStatesLookup.TryGetValue(term, out var visitedStates))
        {
            newStates = states;
            _visitedStatesLookup.Add(term, states);
            return true;
        }

        if (states.TrySubtractAndUnion(visitedStates, out newStates, out var newVisitedStates))
        {
            _visitedStatesLookup[term] = newVisitedStates;
            return true;
        }

        return false;
    }

    private bool IsPreviouslyVisitedTerm(Term term)
    {
        return Start.Term == term || _actions.Any(a => a.Target == term);
    }
}
