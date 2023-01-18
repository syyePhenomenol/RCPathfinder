using RandomizerCore.Collections;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RCPathfinder
{
    public class SearchState
    {
        public Dictionary<Term, List<State>> Visited { get; }
        public HashSet<Term> Indeterminate { get; }
        public PriorityQueue<(float, int), Node> Queue { get; }
        public float Depth { get; private set; }

        public SearchState(SearchParams sp)
        {
            Visited = new();
            if (sp.StartState.Count > 0)
            {
                Visited.Add(sp.StartPosition, new(sp.StartState));
            }

            Indeterminate = new();
            if (sp.StartState.Count is 0)
            {
                Indeterminate.Add(sp.StartPosition);
            }

            Queue = new();
            Queue.Enqueue((0f, 0), new(sp.StartPosition, sp.StartState));
        }

        public bool TryPop(out Node? node)
        {
            return Queue.TryExtractMin(out (float, int) _, out node);
        }

        public void Push(Node? node)
        {
            if (node is null) throw new NullReferenceException();

            Queue.Enqueue((node.Cost, node.Depth), node);

            Depth = Math.Max(Depth, node.Depth);
        }
    }
}
