using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ToDonegeon.Analysis
{
    // Lays several expressive-range runs side by side as one markdown table
    public static class ExpressiveRangeComparison
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public static string ToMarkdown(IReadOnlyList<(string label, ExpressiveRangeReport report)> runs)
        {
            if (runs == null) throw new ArgumentNullException(nameof(runs));

            var sb = new StringBuilder();
            sb.Append("# Expressive-range comparison\n\n");
            sb.Append("Ranges are min-max across the samples of each variant.\n\n");
            sb.Append("| variant | samples | coverage | peak cell | valid | roomDensity | walkableDensity | corridorShare | linearity |\n");
            sb.Append("|---|---|---|---|---|---|---|---|---|\n");
            foreach ((string label, ExpressiveRangeReport report) in runs)
            {
                sb.Append($"| {label} | {report.Count} | {P(report.Coverage)} | {P(report.PeakCellShare)} | {P(report.ValidRate)} | ")
                  .Append($"{RangeOf(report, m => m.RoomDensity)} | {RangeOf(report, m => m.WalkableDensity)} | ")
                  .Append($"{RangeOf(report, m => m.CorridorShare)} | {RangeOf(report, m => m.Linearity)} |\n");
            }
            return sb.ToString();
        }

        private static string RangeOf(ExpressiveRangeReport report, Func<DungeonMetrics, float> selector)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (DungeonMetrics m in report.Samples)
            {
                float v = selector(m);
                if (v < min) min = v;
                if (v > max) max = v;
            }
            return $"{F(min)}-{F(max)}";
        }

        private static string F(float v) => v.ToString("0.##", Invariant);
        private static string P(float v) => (v * 100f).ToString("0.0", Invariant) + "%";
    }
}
