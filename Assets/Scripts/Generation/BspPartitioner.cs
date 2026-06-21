using System;

namespace ToDonegeon.Generation
{
    // stage 1 of the generation pipeline (macro-partition into areas): recursively subdivides a
    // rectangular footprint into a BspTree of leaf regions. randomness comes from a seeded source,
    // so a seed always reproduces the same partition.
    public sealed class BspPartitioner
    {
        private readonly BspSettings _settings;

        public BspPartitioner(BspSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public BspTree Partition(GridRect bounds, int seed) => Partition(bounds, new Random(seed));

        // draws all randomness from rng, so the partition can share one deterministic stream
        // with later pipeline stages.
        public BspTree Partition(GridRect bounds, Random rng)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            return new BspTree(Build(bounds, 0, rng));
        }

        private BspNode Build(GridRect region, int depth, Random rng)
        {
            if (ShouldStop(region, depth) || !TrySplit(region, rng, out GridRect left, out GridRect right))
                return new BspNode(region);

            return new BspNode(region, Build(left, depth + 1, rng), Build(right, depth + 1, rng));
        }

        private bool ShouldStop(GridRect region, int depth)
        {
            if (depth >= _settings.MaxDepth) return true;

            // a region that already fits the target size is an acceptable leaf.
            return region.Width <= _settings.MaxLeafSize && region.Height <= _settings.MaxLeafSize;
        }

        private bool TrySplit(GridRect region, Random rng, out GridRect left, out GridRect right)
        {
            bool canSplitVertically = region.Width >= _settings.MinSplittableSize;
            bool canSplitHorizontally = region.Height >= _settings.MinSplittableSize;

            if (!canSplitVertically && !canSplitHorizontally)
            {
                left = default;
                right = default;
                return false;
            }

            if (CutVertically(region, canSplitVertically, canSplitHorizontally, rng))
            {
                int cut = CutOffset(region.Width, rng);
                left = new GridRect(region.X, region.Y, cut, region.Height);
                right = new GridRect(region.X + cut, region.Y, region.Width - cut, region.Height);
            }
            else
            {
                int cut = CutOffset(region.Height, rng);
                left = new GridRect(region.X, region.Y, region.Width, cut);
                right = new GridRect(region.X, region.Y + cut, region.Width, region.Height - cut);
            }

            return true;
        }

        // whether to cut with a vertical line (dividing the width into left/right) rather than a
        // horizontal line (dividing the height into bottom/top). a region far from square is cut
        // across its long axis; an otherwise free choice is a fair coin flip.
        private bool CutVertically(GridRect region, bool canVertical, bool canHorizontal, Random rng)
        {
            if (canVertical && canHorizontal)
            {
                if (region.Width > region.Height * _settings.MaxAspectRatio) return true;
                if (region.Height > region.Width * _settings.MaxAspectRatio) return false;
                return rng.Next(2) == 0;
            }

            return canVertical;
        }

        // distance from the region's minimum edge to the cut
        private int CutOffset(int length, Random rng)
        {
            int min = _settings.MinLeafSize;
            int max = length - _settings.MinLeafSize;

            float t = (float)rng.NextDouble();
            float ratio = _settings.MinSplitRatio + (_settings.MaxSplitRatio - _settings.MinSplitRatio) * t;
            int offset = (int)Math.Round(length * ratio);

            if (offset < min) return min;
            if (offset > max) return max;
            return offset;
        }
    }
}
