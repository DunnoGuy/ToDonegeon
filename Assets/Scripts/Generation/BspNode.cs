using System;

namespace ToDonegeon.Generation
{
    // a node in the BSP partition tree. a leaf's Region becomes a room; an internal node
    // holds the two regions produced by one split.
    public sealed class BspNode
    {
        public GridRect Region { get; }
        public BspNode Left { get; }
        public BspNode Right { get; }

        public BspNode(GridRect region)
        {
            Region = region;
        }

        public BspNode(GridRect region, BspNode left, BspNode right)
        {
            Region = region;
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public bool IsLeaf => Left == null && Right == null;
    }
}
