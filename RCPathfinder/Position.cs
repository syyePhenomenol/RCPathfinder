using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder;

public class Position(Term term, StateUnion states, float cost)
{
    public Term Term => term;
    public StateUnion States => states;
    public float Cost => cost;

    public string DebugString => $"({Term.Name}, {Cost})";
}

public class ArbitraryPosition(StateUnion states, float cost) : Position(ArbitraryTerm.Instance, states, cost) { }

public class ArbitraryTerm() : Term(int.MaxValue, "Arbitrary", TermType.State)
{
    public static ArbitraryTerm Instance { get; } = new();
}
