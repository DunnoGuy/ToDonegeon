using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ToDonegeon.Analysis
{
    // Result of an expressive-range run
    public sealed class ExpressiveRangeReport
    {
        private const float CoverageTarget = 0.60f;
        private const float PeakCellTarget = 0.15f;
        private const float ValidityTarget = 0.99f;

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public int Count { get; }
        public string XAxisName { get; }
        public string YAxisName { get; }
        public int Bins { get; }
        public int[,] Histogram { get; }
        public float Coverage { get; }
        public float PeakCellShare { get; }
        public float ValidRate { get; }
        public IReadOnlyList<DungeonMetrics> Samples { get; }

        public ExpressiveRangeReport(int count, string xAxisName, string yAxisName, int bins,
            int[,] histogram, float coverage, float peakCellShare, float validRate,
            IReadOnlyList<DungeonMetrics> samples)
        {
            Count = count;
            XAxisName = xAxisName;
            YAxisName = yAxisName;
            Bins = bins;
            Histogram = histogram;
            Coverage = coverage;
            PeakCellShare = peakCellShare;
            ValidRate = validRate;
            Samples = samples;
        }

        public string ToMetricsCsv()
        {
            var sb = new StringBuilder();
            sb.Append("seed,roomCount,roomDensity,walkableDensity,corridorShare,linearity,difficulty,valid\n");
            foreach (DungeonMetrics m in Samples)
            {
                sb.Append(m.Seed).Append(',')
                  .Append(m.RoomCount).Append(',')
                  .Append(F(m.RoomDensity)).Append(',')
                  .Append(F(m.WalkableDensity)).Append(',')
                  .Append(F(m.CorridorShare)).Append(',')
                  .Append(F(m.Linearity)).Append(',')
                  .Append(F(m.Difficulty)).Append(',')
                  .Append(m.IsValid ? 1 : 0).Append('\n');
            }
            return sb.ToString();
        }

        // Rows are the Y axis, columns the X axis
        public string ToHeatmapCsv()
        {
            var sb = new StringBuilder();
            for (int y = Bins - 1; y >= 0; y--)
            {
                for (int x = 0; x < Bins; x++)
                {
                    if (x > 0) sb.Append(',');
                    sb.Append(Histogram[x, y]);
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }

        public string ToSummaryMarkdown()
        {
            var sb = new StringBuilder();
            sb.Append("# Expressive-range analysis\n\n");
            sb.Append($"- Samples: **{Count}**\n");
            sb.Append($"- Heatmap axes: **{XAxisName}** (x) x **{YAxisName}** (y), {Bins}x{Bins} bins over the observed range\n");
            sb.Append($"- Coverage: **{P(Coverage)}** of {Bins * Bins} cells (target >= {P(CoverageTarget)} -> {Pass(Coverage >= CoverageTarget)})\n");
            sb.Append($"- Peak cell: **{P(PeakCellShare)}** of samples (target <= {P(PeakCellTarget)} -> {Pass(PeakCellShare <= PeakCellTarget)})\n");
            sb.Append($"- Validity: **{P(ValidRate)}** pass connectivity (target >= {P(ValidityTarget)} -> {Pass(ValidRate >= ValidityTarget)})\n\n");

            sb.Append("| Metric | min | mean | max |\n");
            sb.Append("|---|---|---|---|\n");
            AppendStat(sb, "roomCount", m => m.RoomCount);
            AppendStat(sb, "roomDensity", m => m.RoomDensity);
            AppendStat(sb, "walkableDensity", m => m.WalkableDensity);
            AppendStat(sb, "corridorShare", m => m.CorridorShare);
            AppendStat(sb, "linearity", m => m.Linearity);
            AppendStat(sb, "difficulty", m => m.Difficulty);
            sb.Append("\nHeatmap CSV: rows = y axis (first row = highest), columns = x axis (left = lowest).\n");
            return sb.ToString();
        }

        private void AppendStat(StringBuilder sb, string name, Func<DungeonMetrics, float> selector)
        {
            float min = float.MaxValue, max = float.MinValue, sum = 0f;
            foreach (DungeonMetrics m in Samples)
            {
                float v = selector(m);
                if (v < min) min = v;
                if (v > max) max = v;
                sum += v;
            }
            float mean = Count > 0 ? sum / Count : 0f;
            sb.Append($"| {name} | {F(min)} | {F(mean)} | {F(max)} |\n");
        }

        private static string F(float v) => v.ToString("0.###", Invariant);
        private static string P(float v) => (v * 100f).ToString("0.0", Invariant) + "%";
        private static string Pass(bool ok) => ok ? "PASS" : "FAIL";
    }
}
