using System.Collections.ObjectModel;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RCPathfinder.Actions;

namespace RCPathfinder;

/// <summary>
/// Constructs and manages a set of actions to represent movement between state-valued terms.
/// </summary>
public class SearchData
{
    public SearchData(LogicManager lm, RandoContext? ctx = null)
    {
        LM = lm;
        Context = ctx;

        StateTermLookup = new(lm.Terms.GetTermList(TermType.State).ToDictionary(p => p.Name, p => p));
        StandardActionLookup = new(
            MakeStandardActions()
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => new ReadOnlyCollection<StandardAction>([.. kvp.Value.Distinct().OrderBy(a => a.Target.Id)])
                )
        );
    }

    public LogicManager LM { get; }
    public RandoContext? Context { get; }

    public ReadOnlyDictionary<string, Term> StateTermLookup { get; }
    public ReadOnlyDictionary<Term, ReadOnlyCollection<StandardAction>> StandardActionLookup { get; }

    public IEnumerable<Term> GetAllStateTerms()
    {
        return StateTermLookup.Values;
    }

    public IEnumerable<StandardAction> GetAllStandardActions()
    {
        return StandardActionLookup.Values.SelectMany(a => a).OrderBy(a => a.Source.Id).ThenBy(a => a.Target.Id);
    }

    /// <summary>
    /// Override this method to add new StandardActions or replace existing ones.
    /// </summary>
    protected internal virtual Dictionary<Term, List<StandardAction>> MakeStandardActions()
    {
        Dictionary<Term, List<StandardAction>> actionLookup = [];

        // Default logic actions
        foreach (var destination in StateTermLookup.Values)
        {
            var ld = LM.GetLogicDefStrict(destination.Name);

            if (ld is StateLogicDef sld)
            {
                if (ld is not DNFLogicDef dld)
                {
                    dld = LM.CreateDNFLogicDef(sld.Name, sld.ToLogicClause());
                }

                foreach (var start in dld.GetTerms().Where(t => t.Type is TermType.State))
                {
                    AddAction(new StateLogicAction(start, destination, dld));
                }
            }
            else
            {
                foreach (var start in ld.GetTerms().Where(t => t.Type is TermType.State))
                {
                    AddAction(new LogicAction(start, destination, ld));
                }
            }
        }

        if (Context is null)
        {
            return actionLookup;
        }

        // Default placement actions
        foreach (var gp in Context.EnumerateExistingPlacements())
        {
            if (
                StateTermLookup.TryGetValue(gp.Location.Name, out var source)
                && StateTermLookup.TryGetValue(gp.Item.Name, out var target)
            )
            {
                AddAction(new PlacementAction(source, target));
            }
        }

        return actionLookup;

        void AddAction(StandardAction a)
        {
            if (actionLookup.TryGetValue(a.Source, out var actionList))
            {
                actionList.Add(a);
                return;
            }

            actionLookup[a.Source] = [a];
        }
    }

    /// <summary>
    /// Provide non-standard actions to be traversed based on the current node.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected internal virtual IEnumerable<AbstractAction> GetAdditionalActions(Node node)
    {
        return [];
    }
}
