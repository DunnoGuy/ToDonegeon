using System;
using System.Collections.Generic;

namespace ToDonegeon.Generation
{
    // the result of a BSP partition: the root node plus its cached leaves - the regions the
    // carving stage turns into rooms and the connection stage joins.
    public sealed class BspTree
    {
        private readonly List<BspNode> _leaves = new List<BspNode>();

        public BspNode Root { get; }
        public IReadOnlyList<BspNode> Leaves => _leaves;

        public BspTree(BspNode root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            CollectLeaves(root, _leaves);
        }

        // iterative depth-first collection, left child first, so leaf order is stable for a given tree
        private static void CollectLeaves(BspNode root, List<BspNode> into)
        {
            var stack = new Stack<BspNode>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                BspNode node = stack.Pop();
                if (node.IsLeaf)
                {
                    into.Add(node);
                    continue;
                }

                stack.Push(node.Right);
                stack.Push(node.Left);
            }
        }
    }
}
