using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    public class BspPartitionTests
    {
        private static readonly GridRect Bounds = new GridRect(0, 0, 64, 48);

        private static BspTree Partition(int seed, BspSettings settings = null)
            => new BspPartitioner(settings ?? new BspSettings()).Partition(Bounds, seed);

        // a stable string of the (order-independent) leaf layout, for comparing partitions.
        private static string Signature(BspTree tree)
            => string.Join("|", tree.Leaves
                .Select(node => node.Region)
                .OrderBy(r => r.X).ThenBy(r => r.Y)
                .Select(r => r.ToString()));

        [Test]
        public void Same_Seed_Produces_Identical_Partition()
        {
            Assert.That(Signature(Partition(20260620)), Is.EqualTo(Signature(Partition(20260620))));
        }

        [Test]
        public void Different_Seeds_Produce_Varied_Partitions()
        {
            const int seedCount = 24;
            var signatures = new HashSet<string>();
            for (int seed = 0; seed < seedCount; seed++)
                signatures.Add(Signature(Partition(seed)));

            Assert.That(signatures.Count, Is.GreaterThan(1), "the partition should vary with the seed");
        }

        [Test]
        public void Leaves_Exactly_Tile_The_Bounds()
        {
            var leaves = Partition(7).Leaves.Select(node => node.Region).ToList();

            int areaSum = leaves.Sum(r => r.Area);
            Assert.That(areaSum, Is.EqualTo(Bounds.Area), "leaves must cover the bounds with no gaps");

            for (int i = 0; i < leaves.Count; i++)
                for (int j = i + 1; j < leaves.Count; j++)
                    Assert.That(leaves[i].Overlaps(leaves[j]), Is.False, "leaves must not overlap");
        }

        [Test]
        public void Leaves_Stay_Within_Bounds()
        {
            foreach (var region in Partition(99).Leaves.Select(node => node.Region))
            {
                Assert.That(region.X, Is.GreaterThanOrEqualTo(Bounds.X));
                Assert.That(region.Y, Is.GreaterThanOrEqualTo(Bounds.Y));
                Assert.That(region.MaxX, Is.LessThanOrEqualTo(Bounds.MaxX));
                Assert.That(region.MaxY, Is.LessThanOrEqualTo(Bounds.MaxY));
            }
        }

        [Test]
        public void Leaves_Respect_Minimum_Size()
        {
            var settings = new BspSettings();
            foreach (var region in Partition(3, settings).Leaves.Select(node => node.Region))
            {
                Assert.That(region.Width, Is.GreaterThanOrEqualTo(settings.MinLeafSize));
                Assert.That(region.Height, Is.GreaterThanOrEqualTo(settings.MinLeafSize));
            }
        }

        [Test]
        public void Leaves_Respect_Maximum_Size_When_Depth_Is_Generous()
        {
            var settings = new BspSettings(minLeafSize: 8, maxLeafSize: 20, maxDepth: 20);
            var bounds = new GridRect(0, 0, 80, 80);
            var tree = new BspPartitioner(settings).Partition(bounds, 42);

            foreach (var region in tree.Leaves.Select(node => node.Region))
            {
                Assert.That(region.Width, Is.LessThanOrEqualTo(settings.MaxLeafSize));
                Assert.That(region.Height, Is.LessThanOrEqualTo(settings.MaxLeafSize));
            }
        }

        [Test]
        public void Region_Too_Small_To_Split_Yields_A_Single_Leaf()
        {
            var settings = new BspSettings(); // MinSplittableSize == 16
            var tiny = new GridRect(0, 0, 15, 15);
            var tree = new BspPartitioner(settings).Partition(tiny, 1);

            Assert.That(tree.Root.IsLeaf, Is.True);
            Assert.That(tree.Leaves.Count, Is.EqualTo(1));
            Assert.That(tree.Leaves[0].Region, Is.EqualTo(tiny));
        }

        [Test]
        public void Internal_Node_Has_Both_Children()
        {
            var tree = Partition(5);
            Assume.That(tree.Root.IsLeaf, Is.False, "a 64x48 footprint should split at least once");

            Assert.That(tree.Root.Left, Is.Not.Null);
            Assert.That(tree.Root.Right, Is.Not.Null);
            Assert.That(tree.Root.Region, Is.EqualTo(Bounds));
        }
    }

    public class BspSettingsTests
    {
        [Test]
        public void Rejects_MaxLeaf_Smaller_Than_Twice_MinLeaf()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BspSettings(minLeafSize: 10, maxLeafSize: 15));
        }

        [Test]
        public void Rejects_Split_Ratio_Outside_The_Unit_Interval()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BspSettings(minSplitRatio: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BspSettings(maxSplitRatio: 1f));
        }

        [Test]
        public void Rejects_Min_Ratio_Above_Max_Ratio()
        {
            Assert.Throws<ArgumentException>(() => new BspSettings(minSplitRatio: 0.7f, maxSplitRatio: 0.3f));
        }
    }
}
