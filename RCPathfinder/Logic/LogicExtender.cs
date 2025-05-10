using System.Collections.ObjectModel;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.StringLogic;

namespace RCPathfinder.Logic;

public abstract class LogicExtender
{
    public LogicExtender(LogicManager referenceLM)
    {
        ReferenceLM = referenceLM;

        var lmb = ModifyReferenceLM(new(referenceLM));

        Dictionary<Term, Term> simpleBools = [];
        Dictionary<Term, LogicDef> referenceBools = [];

        foreach ((var name, var lc) in lmb.LogicLookup.Select(kvp => (kvp.Key, kvp.Value)).ToArray())
        {
            foreach (var token in lc.Tokens.Where(t => t is ProjectedToken))
            {
                var pt = (ProjectedToken)token;
                Term newTerm;

                switch (pt.Inner)
                {
                    case SimpleToken st:
                        newTerm = lmb.GetOrAddTerm($"{st.Write()}-Simple_Bool", TermType.SignedByte);
                        simpleBools[newTerm] = referenceLM.GetTermStrict(st.Write());
                        break;
                    case ReferenceToken rt:
                        newTerm = lmb.GetOrAddTerm($"{rt.Target}-Reference_Bool", TermType.SignedByte);
                        referenceBools[newTerm] = referenceLM.GetLogicDefStrict(rt.Target);
                        break;
                    default:
                        throw new InvalidDataException();
                }

                lmb.DoSubst(new(name, pt.Write(), newTerm.Name));
            }
        }

        LocalLM = new(lmb);
        SimpleBools = new(simpleBools);
        ReferenceBools = new(referenceBools);
    }

    public LogicManager ReferenceLM { get; }
    public LogicManager LocalLM { get; }

    internal ReadOnlyDictionary<Term, Term> SimpleBools { get; }
    internal ReadOnlyDictionary<Term, LogicDef> ReferenceBools { get; }

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
