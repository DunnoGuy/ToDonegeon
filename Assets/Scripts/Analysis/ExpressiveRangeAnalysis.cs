using System;
using System.Collections.Generic;
using ToDonegeon.Pipeline;

namespace ToDonegeon.Analysis
{
    // The expressive-range analysis: generate N dungeons, measure each, and bin two chosen metrics into a heatmap.
    public static class ExpressiveRangeAnalysis
    {
        public static ExpressiveRangeReport Run(
            BspDungeonGenerator generator, int width, int height, int baseSeed, int count,
            MetricAxis xAxis, MetricAxis yAxis, int bins)
        {
            if (generator == null) throw new ArgumentNullException(nameof(generator));
            if (xAxis == null) throw new ArgumentNullException(nameof(xAxis));
            if (yAxis == null) throw new ArgumentNullException(nameof(yAxis));
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
            if (bins < 1) throw new ArgumentOutOfRangeException(nameof(bins));

            var samples = new List<DungeonMetrics>(count);
            int valid = 0;
            for (int i = 0; i < count; i++)
            {
                Dungeon dungeon = generator.Generate(width, height, baseSeed + i);
                DungeonMetrics metrics = DungeonMetrics.Measure(dungeon);
                samples.Add(metrics);
                if (metrics.IsValid) valid++;
            }

            Range(samples, xAxis, out float xMin, out float xMax);
            Range(samples, yAxis, out float yMin, out float yMax);

            var histogram = new int[bins, bins];
            int peak = 0;
            foreach (DungeonMetrics m in samples)
            {
                int bx = BinIndex(xAxis.Selector(m), xMin, xMax, bins);
                int by = BinIndex(yAxis.Selector(m), yMin, yMax, bins);
                int cell = ++histogram[bx, by];
                if (cell > peak) peak = cell;
            }

            int occupied = 0;
            for (int x = 0; x < bins; x++)
                for (int y = 0; y < bins; y++)
                    if (histogram[x, y] > 0) occupied++;

            float coverage = occupied / (float)(bins * bins);
            float peakShare = peak / (float)count;
            float validRate = valid / (float)count;

            return new ExpressiveRangeReport(count, xAxis.Name, yAxis.Name, bins, histogram,
                coverage, peakShare, validRate, samples);
        }

        private static void Range(List<DungeonMetrics> samples, MetricAxis axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            foreach (DungeonMetrics m in samples)
            {
                float v = axis.Selector(m);
                if (v < min) min = v;
                if (v > max) max = v;
            }
        }

        private static int BinIndex(float value, float min, float max, int bins)
        {
            if (max <= min) return 0;
            int idx = (int)((value - min) / (max - min) * bins);
            if (idx < 0) return 0;
            if (idx >= bins) return bins - 1;
            return idx;
        }
    }
}
