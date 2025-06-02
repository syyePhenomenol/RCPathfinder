using RandomizerCore;
using RandomizerCore.Logic;

namespace RCPathfinder.Logic;

/// <summary>
/// Builds and updates a local ProgressionManager based on a reference ProgressionManager.
/// Supports new logic terms and changes in the local PM.
/// </summary>
public abstract class ProgressionSynchronizer
{
    public ProgressionSynchronizer(LogicExtender logicExtender, RandoContext? ctx)
    {
        LogicExtender = logicExtender;
        LocalPM = new(logicExtender.LocalLM, ctx);

        // Do automatic updating on new waypoints, transitions and placements
        LocalPM.mu.AddWaypoints(LocalPM.lm.Waypoints.Where(w => !LogicExtender.ReferenceLM.Terms.Contains(w.term)));
        LocalPM.mu.AddTransitions(
            LocalPM.lm.TransitionLookup.Values.Where(t => !LogicExtender.ReferenceLM.Terms.Contains(t.term))
        );
        LocalPM.mu.AddPlacements(logicExtender.GetLocalPlacements());

        Update();
    }

    public LogicExtender LogicExtender { get; }

    // Its LM should match the ReferenceLM of the LogicExtender.
    public abstract ProgressionManager ReferencePM { get; }
    public ProgressionManager LocalPM { get; }

    public void Update()
    {
        ReferenceUpdate();
        LocalUpdate();
    }

    public void ReferenceUpdate()
    {
        // Copies ReferencePM to LocalPM
        foreach (var term in ReferencePM.lm.Terms)
        {
            switch (term.Type)
            {
                case TermType.State:
                    LocalPM.SetState(term, ReferencePM.GetState(term));
                    break;
                default:
                    LocalPM.Set(term, ReferencePM.Get(term));
                    break;
            }
        }
    }

    public void LocalUpdate()
    {
        // Manual updates to LocalPM for its unique terms
        ManuallyUpdateTerms();

        // Automatic updates to LocalPM for its unique terms
        LocalPM.mu.StartUpdating();
        LocalPM.mu.StopUpdating();
    }

    /// <summary>
    /// Apply manual changes to terms that only appear in LocalPM, before doing automatic updates.
    /// </summary>
    protected internal virtual void ManuallyUpdateTerms() { }
}
