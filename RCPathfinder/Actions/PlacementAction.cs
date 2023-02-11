﻿using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder.Actions
{
    /// <summary>
    /// An action that links a specific position to a specific destination without state change.
    /// Pruning before including this action is recommended.
    /// </summary>
    public class PlacementAction : AbstractAction
    {
        public override string Prefix => "plac";

        public PlacementAction(Term startPosition, Term destination, float cost = 1f) : base(startPosition.Name, new HashSet<Term> { startPosition }, destination, cost) { }

        public override bool TryDo(ProgressionManager pm, Term currentPosition, StateUnion currentStates, out Term? newPosition, out StateUnion? newStates)
        {
            if (currentPosition == StartPositions[0])
            {
                newPosition = Destination;
                newStates = currentStates;
                return true;
            }
            newPosition = null;
            newStates = null;
            return false;
        }
    }
}
