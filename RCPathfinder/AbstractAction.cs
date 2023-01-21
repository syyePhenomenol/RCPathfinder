using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public abstract record AbstractAction
    {
        public string FullName => $"{Prefix}-{BaseName}";
        public string BaseName { get; }
        public string Prefix { get; }
        public Term NewPosition { get; }
        public float Cost { get; }

        public AbstractAction(string baseName, string prefix, Term newPosition, float cost = 1f)
        {
            BaseName = baseName;
            Prefix = prefix;
            NewPosition = newPosition;
            Cost = cost;
        }

        /// <summary>
        /// Please do not modify the ProgressionManager here, or if you really have to, call StartTemp() and RemoveTempItems().
        /// But this will most likely impact performance.
        /// </summary>
        public abstract bool TryDo(Term position, StateUnion state, out StateUnion? newState);
    }
}
