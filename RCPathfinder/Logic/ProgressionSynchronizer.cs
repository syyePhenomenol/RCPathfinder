using RandomizerCore.Logic;

namespace RCPathfinder.Logic;

/// <summary>
/// Builds and updates a local ProgressionManager based on a reference ProgressionManager.
/// Supports new logic terms and changes in the local PM.
/// </summary>
public abstract class ProgressionSynchronizer
{
    public ProgressionSynchronizer(ProgressionManager referencePM, LogicExtender logicExtender)
    {
        ReferencePM = referencePM;
        LogicExtender = logicExtender;
        LocalPM = new(logicExtender.LocalLM, referencePM.ctx);

        // Do automatic updating on new waypoints, transitions and placements
        LocalPM.mu.AddWaypoints(LocalPM.lm.Waypoints.Where(w => !ReferencePM.lm.Terms.Contains(w.term)));
        LocalPM.mu.AddTransitions(
            LocalPM.lm.TransitionLookup.Values.Where(t => !ReferencePM.lm.Terms.Contains(t.term))
        );
        LocalPM.mu.AddPlacements(logicExtender.GetLocalPlacements());

        Update();
    }

    public ProgressionManager ReferencePM { get; }
    public LogicExtender LogicExtender { get; }

    public ProgressionManager LocalPM { get; }

    public void Update()
    {
        // Copies ReferencePM to LocalPM and sets proxy terms
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

        foreach (var kvp in LogicExtender.SimpleBools)
        {
            LocalPM.Set(kvp.Key, ReferencePM.Get(kvp.Value));
        }

        foreach (var kvp in LogicExtender.ReferenceBools)
        {
            LocalPM.Set(kvp.Key, kvp.Value.CanGet(ReferencePM) ? 1 : 0);
        }

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
