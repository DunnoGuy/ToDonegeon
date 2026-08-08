using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ToDonegeon.Generation;
using ToDonegeon.Pipeline;

namespace ToDonegeon.Analysis
{
    // Checks the difficulty tiers and measures the threat
    public static class DifficultyProgression
    {
        private const double SpreadTarget = 0.35;

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public static IReadOnlyList<DepthDifficulty> Run(
            int firstDepth, int lastDepth, int samplesPerDepth, int baseSeed)
        {
            if (firstDepth < 1)
                throw new ArgumentOutOfRangeException(nameof(firstDepth), "Depth must be at least 1.");
            if (lastDepth < firstDepth)
                throw new ArgumentOutOfRangeException(nameof(lastDepth), "Last depth must not precede the first.");
            if (samplesPerDepth < 1)
                throw new ArgumentOutOfRangeException(nameof(samplesPerDepth), "Need at least one sample.");

            var rows = new List<DepthDifficulty>(lastDepth - firstDepth + 1);
            for (int depth = firstDepth; depth <= lastDepth; depth++)
                rows.Add(MeasureDepth(depth, samplesPerDepth, baseSeed));
            return rows;
        }

        private static DepthDifficulty MeasureDepth(int depth, int samples, int baseSeed)
        {
            FloorPlan floor = DifficultyPlan.ForTier(depth);
            var bsp = new BspSettings(maxDepth: floor.MaxBspDepth);
            var populator = new PopulatorSettings(
                maxEnemiesPerRoom: floor.MaxEnemiesPerRoom, difficultyTier: floor.Tier);
            var generator = new BspDungeonGenerator(bsp, null, populator);

            var threats = new long[samples];
            long min = long.MaxValue, max = long.MinValue, sum = 0;
            double enemySum = 0;
            int valid = 0;

            for (int i = 0; i < samples; i++)
            {
                Dungeon dungeon = generator.Generate(floor.Width, floor.Height, baseSeed + i);
                DungeonDifficulty difficulty = DungeonDifficulty.Measure(dungeon.Entities);
                long threat = difficulty.TotalThreat;

                threats[i] = threat;
                sum += threat;
                enemySum += difficulty.EnemyCount;
                if (threat < min) min = threat;
                if (threat > max) max = threat;
                if (dungeon.IsValid) valid++;
            }

            double mean = (double)sum / samples;
            double variance = 0;
            foreach (long t in threats) variance += (t - mean) * (t - mean);
            variance /= samples;

            return new DepthDifficulty(depth, floor.Width, floor.Height, mean, min, max,
                Math.Sqrt(variance), enemySum / samples, valid / (float)samples);
        }

        public static string ToMarkdown(IReadOnlyList<DepthDifficulty> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            var sb = new StringBuilder();
            sb.Append("# Difficulty balance and progression\n\n");
            sb.Append("Threat = summed health x attack of every enemy and the boss on a floor. ");
            sb.Append("Each row samples many floors at one descent depth.\n\n");
            sb.Append("- **Progression**: mean threat should climb with depth.\n");
            sb.Append("- **Balance**: spread (std dev / mean) within a depth should stay small, so a ");
            sb.Append("given depth plays consistently whatever the seed.\n\n");

            sb.Append("| Depth | Map | Mean threat | Min | Max | Spread | Enemies | Valid |\n");
            sb.Append("|---|---|---|---|---|---|---|---|\n");
            foreach (DepthDifficulty r in rows)
                sb.Append($"| {r.Depth} | {r.Width}x{r.Height} | {N(r.MeanThreat)} | {r.MinThreat} | " +
                          $"{r.MaxThreat} | {P(r.ThreatSpread)} | {N(r.MeanEnemies)} | {P(r.ValidRate)} |\n");

            AppendVerdicts(sb, rows);
            return sb.ToString();
        }

        private static void AppendVerdicts(StringBuilder sb, IReadOnlyList<DepthDifficulty> rows)
        {
            bool monotone = true;
            for (int i = 1; i < rows.Count; i++)
                if (rows[i].MeanThreat <= rows[i - 1].MeanThreat) monotone = false;

            double worstSpread = 0;
            float worstValid = 1f;
            foreach (DepthDifficulty r in rows)
            {
                if (r.ThreatSpread > worstSpread) worstSpread = r.ThreatSpread;
                if (r.ValidRate < worstValid) worstValid = r.ValidRate;
            }

            sb.Append('\n');
            sb.Append($"- Progression (mean threat strictly rising with depth): **{Verdict(monotone)}**\n");
            sb.Append($"- Balance (worst within-depth spread {P(worstSpread)}, target <= {P(SpreadTarget)}): " +
                      $"**{Verdict(worstSpread <= SpreadTarget)}**\n");
            sb.Append($"- Validity (worst depth {P(worstValid)} connected, target 100%): " +
                      $"**{Verdict(worstValid >= 1f)}**\n");
        }

        private static string Verdict(bool ok) => ok ? "PASS" : "FAIL";
        private static string N(double v) => v.ToString("0.#", Invariant);
        private static string P(double v) => (v * 100.0).ToString("0.0", Invariant) + "%";
    }
}
