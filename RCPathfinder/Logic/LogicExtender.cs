using RandomizerCore;
using RandomizerCore.Logic;

namespace RCPathfinder.Logic;

public abstract class LogicExtender
{
    public LogicExtender(LogicManager referenceLM)
    {
        ReferenceLM = referenceLM;
        LocalLM = new(ModifyReferenceLM(new(referenceLM)));
    }

    public LogicManager ReferenceLM { get; }
    public LogicManager LocalLM { get; }

    /// <summary>
    /// Edit and add logic in the locally stored LogicManager.
    /// </summary>
    /// <param name="lmb"></param>
    protected internal abstract LogicManagerBuilder ModifyReferenceLM(LogicManagerBuilder lmb);

    /// <summary>
    /// Get placements of the new terms in the LocalLM.
    /// </summary>
    protected internal abstract IEnumerable<GeneralizedPlacement> GetLocalPlacements();
}
