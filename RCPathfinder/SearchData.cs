using System.Collections.ObjectModel;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerCore.StringLogic;
using RCPathfinder.Actions;

namespace RCPathfinder;

public class SearchData
{
    private readonly Dictionary<Term, Term> _simpleProxyBools = [];
    private readonly Dictionary<Term, LogicDef> _referenceProxyBools = [];

    public SearchData(ProgressionManager pm)
    {
        ReferencePM = pm;
        LogicManagerBuilder localLmb = new(MakeLocalLM(new(pm.lm)));

        // Replace projection tokens with new bool term tokens. This is done so
        // that only StatePaths with the current term as state provider are evaluated on.
        foreach ((var name, var lc) in localLmb.LogicLookup.Select(kvp => (kvp.Key, kvp.Value)).ToArray())
        {
            foreach (var token in lc.Tokens.Where(t => t is ProjectedToken))
            {
                var pt = (ProjectedToken)token;
                Term newTerm;

                switch (pt.Inner)
                {
                    case SimpleToken st:
                        newTerm = localLmb.GetOrAddTerm($"{st.Write()}-Simple_Bool", TermType.SignedByte);
                        _simpleProxyBools[newTerm] = ReferencePM.lm.GetTermStrict(st.Write());
                        break;
                    case ReferenceToken rt:
                        newTerm = localLmb.GetOrAddTerm($"{rt.Target}-Reference_Bool", TermType.SignedByte);
                        _referenceProxyBools[newTerm] = ReferencePM.lm.GetLogicDefStrict(rt.Target);
                        break;
                    default:
                        throw new InvalidDataException();
                }

                localLmb.DoSubst(new(name, pt.Write(), newTerm.Name));
            }
        }

        LocalPM = new(new(localLmb), pm.ctx);
        StateTermLookup = new(LocalPM.lm.Terms.GetTermList(TermType.State).ToDictionary(p => p.Name, p => p));
        StandardActionLookup = new(
            MakeStandardActions()
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => new ReadOnlyCollection<StandardAction>([.. kvp.Value.Distinct().OrderBy(a => a.Target.Id)])
                )
        );
        StartJumpActions = new([.. MakeStartJumpActions().Distinct().OrderBy(a => a.Target?.Id)]);
    }

    public ProgressionManager LocalPM { get; }
    public ProgressionManager ReferencePM { get; }
    public ReadOnlyDictionary<string, Term> StateTermLookup { get; }
    public ReadOnlyDictionary<Term, ReadOnlyCollection<StandardAction>> StandardActionLookup { get; }
    public ReadOnlyCollection<StartJumpAction> StartJumpActions { get; }

    public IEnumerable<Term> GetAllStateTerms()
    {
        return StateTermLookup.Values;
    }

    public IEnumerable<AbstractAction> GetAllActions()
    {
        return StandardActionLookup
            .Values.SelectMany(a => a)
            .OrderBy(a => a.Source.Id)
            .ThenBy(a => a.Target.Id)
            .Cast<AbstractAction>()
            .Concat(StartJumpActions);
    }

    /// <summary>
    /// Override this method to modify the value of terms before running a search.
    /// </summary>
    public virtual void UpdateProgression()
    {
        foreach (var term in ReferencePM.lm.Terms)
        {
            switch (term.Type)
            {
                case TermType.State:
                    // We manually and temporarily set the states of a node's current term just before trying to traverse.
                    // The states that are set here don't matter for traversal but can be used for other evaluation,
                    // such as propagating current progression to state-valued terms that are not in the ReferencePM.
                    LocalPM.SetState(term, ReferencePM.GetState(term));
                    break;
                default:
                    LocalPM.Set(term, ReferencePM.Get(term));
                    break;
            }
        }

        foreach (var kvp in _simpleProxyBools)
        {
            LocalPM.Set(kvp.Key, ReferencePM.Get(kvp.Value));
        }

        foreach (var kvp in _referenceProxyBools)
        {
            LocalPM.Set(kvp.Key, kvp.Value.CanGet(ReferencePM) ? 1 : 0);
        }
    }

    /// <summary>
    /// Override this method to customize logic in the locally stored ProgressionManager.
    /// </summary>
    /// <param name="lmb"></param>
    protected virtual LogicManagerBuilder MakeLocalLM(LogicManagerBuilder lmb)
    {
        return lmb;
    }

    /// <summary>
    /// Override this method to add new StandardActions or replace existing ones.
    /// </summary>
    protected virtual Dictionary<Term, List<StandardAction>> MakeStandardActions()
    {
        Dictionary<Term, List<StandardAction>> actionLookup = [];

        // Default logic actions
        foreach (var destination in StateTermLookup.Values)
        {
            var ld = LocalPM.lm.GetLogicDefStrict(destination.Name);

            if (ld is StateLogicDef sld)
            {
                if (ld is not DNFLogicDef dld)
                {
                    dld = LocalPM.lm.CreateDNFLogicDef(sld.Name, sld.ToLogicClause());
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

        if (LocalPM.ctx is null)
        {
            return actionLookup;
        }

        // Default placement actions
        foreach (var gp in LocalPM.ctx.EnumerateExistingPlacements())
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

    protected virtual IEnumerable<StartJumpAction> MakeStartJumpActions()
    {
        return [];
    }
}
