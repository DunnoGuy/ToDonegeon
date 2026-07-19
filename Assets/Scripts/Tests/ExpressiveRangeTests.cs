using System;
using System.Collections.Generic;
using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Pipeline;
using ToDonegeon.Analysis;

namespace ToDonegeon.Tests
{
    public class DungeonMetricsTests
    {
        private const int Width = 64;
        private const int Height = 48;

        private static DungeonMetrics Measure(int seed)
            => DungeonMetrics.Measure(new BspDungeonGenerator().Generate(Width, Height, seed));

        [Test]
        public void Metrics_Are_Deterministic()
        {
            DungeonMetrics a = Measure(7);
            DungeonMetrics b = Measure(7);

            Assert.That(a.RoomCount, Is.EqualTo(b.RoomCount));
            Assert.That(a.RoomDensity, Is.EqualTo(b.RoomDensity));
            Assert.That(a.Linearity, Is.EqualTo(b.Linearity));
            Assert.That(a.Leniency, Is.EqualTo(b.Leniency));
        }

        [Test]
        public void Metrics_Lie_In_Expected_Ranges()
        {
            DungeonMetrics m = Measure(3);

            Assert.That(m.RoomCount, Is.GreaterThan(0));
            Assert.That(m.IsValid, Is.True);
            foreach (float value in new[] { m.RoomDensity, m.WalkableDensity, m.CorridorShare, m.Linearity, m.Leniency })
                Assert.That(value, Is.InRange(0f, 1f));
            Assert.That(m.WalkableDensity, Is.GreaterThanOrEqualTo(m.RoomDensity), "walkable includes rooms + corridors");
        }
    }

    public class ExpressiveRangeTests
    {
        private const int Width = 64;
        private const int Height = 48;
        private const int Bins = 10;

        private static ExpressiveRangeReport Run(int count)
            => Run(new RoomCarverSettings(), null, count);

        private static ExpressiveRangeReport Run(RoomCarverSettings carver, CorridorSettings corridor, int count)
            => ExpressiveRangeAnalysis.Run(new BspDungeonGenerator(null, carver, null, corridor, 8),
                Width, Height, 0, count,
                new MetricAxis("RoomDensity", m => m.RoomDensity),
                new MetricAxis("Linearity", m => m.Linearity),
                Bins);

        private static float Max(ExpressiveRangeReport report, Func<DungeonMetrics, float> selector)
        {
            float max = float.MinValue;
            foreach (DungeonMetrics m in report.Samples)
                if (selector(m) > max) max = selector(m);
            return max;
        }

        [Test]
        public void Every_Sample_Falls_In_Exactly_One_Bin()
        {
            ExpressiveRangeReport report = Run(200);

            int sum = 0;
            for (int x = 0; x < Bins; x++)
                for (int y = 0; y < Bins; y++)
                    sum += report.Histogram[x, y];

            Assert.That(sum, Is.EqualTo(200));
            Assert.That(report.ValidRate, Is.EqualTo(1f));
            Assert.That(report.Coverage, Is.GreaterThan(0f).And.LessThanOrEqualTo(1f));
        }

        [Test]
        public void Meets_The_Report_Targets()
        {
            ExpressiveRangeReport report = Run(1000);

            Assert.That(report.Coverage, Is.GreaterThanOrEqualTo(0.60f), "expressive-range coverage target");
            Assert.That(report.PeakCellShare, Is.LessThanOrEqualTo(0.15f), "no mode collapse");
            Assert.That(report.ValidRate, Is.EqualTo(1f), "every dungeon playable");
        }

        [Test]
        public void Metrics_Csv_Has_A_Header_And_One_Row_Per_Sample()
        {
            string csv = Run(50).ToMetricsCsv();

            Assert.That(csv, Does.StartWith("seed,roomCount"));
            Assert.That(csv.Trim().Split('\n').Length, Is.EqualTo(51)); // header + 50 rows
        }

        [Test]
        public void Caves_Reach_Regions_Rectangles_Never_Do()
        {
            ExpressiveRangeReport rectangles = Run(new RoomCarverSettings(), null, 300);
            ExpressiveRangeReport caves = Run(new RoomCarverSettings(shape: ChamberShape.Cave), null, 300);

            Assert.That(Max(caves, m => m.RoomDensity), Is.GreaterThan(Max(rectangles, m => m.RoomDensity)),
                "caves pack denser footprints than any rectangle layout");
            Assert.That(Max(caves, m => m.CorridorShare), Is.GreaterThan(Max(rectangles, m => m.CorridorShare)),
                "organic chambers lean on more corridor than any rectangle layout");
        }

        [Test]
        public void Cave_Generator_Meets_The_Validity_Target()
        {
            ExpressiveRangeReport caves = Run(new RoomCarverSettings(shape: ChamberShape.Cave), null, 300);
            Assert.That(caves.ValidRate, Is.EqualTo(1f));
        }

        [Test]
        public void Comparison_Table_Lists_Every_Variant()
        {
            var runs = new List<(string, ExpressiveRangeReport)>
            {
                ("rectangles", Run(50)),
                ("caves", Run(new RoomCarverSettings(shape: ChamberShape.Cave), null, 50))
            };

            string markdown = ExpressiveRangeComparison.ToMarkdown(runs);
            Assert.That(markdown, Does.Contain("| rectangles |"));
            Assert.That(markdown, Does.Contain("| caves |"));
            Assert.That(markdown, Does.Contain("roomDensity"));
        }
    }
}
