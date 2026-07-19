using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using ToDonegeon.Generation;
using ToDonegeon.Pipeline;
using ToDonegeon.Analysis;
using Debug = UnityEngine.Debug;

namespace ToDonegeon.EditorTools
{
    // Editor menu entry that runs the expressive-range analysis and writes the metrics CSV, heatmap
    // CSV, and markdown summary to a Reports folder beside the project
    public static class ExpressiveRangeMenu
    {
        private const int SampleCount = 1000;
        private const int Width = 64;
        private const int Height = 48;
        private const int Bins = 10;

        [MenuItem("Tools/ToDonegeon/Run Expressive-Range Analysis (N=1000)")]
        public static void Run()
        {
            var generator = new BspDungeonGenerator();

            var stopwatch = Stopwatch.StartNew();
            ExpressiveRangeReport report = ExpressiveRangeAnalysis.Run(
                generator, Width, Height, baseSeed: 0, count: SampleCount,
                xAxis: new MetricAxis("RoomDensity", m => m.RoomDensity),
                yAxis: new MetricAxis("Linearity", m => m.Linearity),
                bins: Bins);
            stopwatch.Stop();

            string directory = ReportsDirectory();
            File.WriteAllText(Path.Combine(directory, "expressive_range_metrics.csv"), report.ToMetricsCsv());
            File.WriteAllText(Path.Combine(directory, "expressive_range_heatmap.csv"), report.ToHeatmapCsv());
            File.WriteAllText(Path.Combine(directory, "expressive_range_summary.md"), report.ToSummaryMarkdown());

            Debug.Log($"[ExpressiveRange] {SampleCount} dungeons in {stopwatch.ElapsedMilliseconds} ms — " +
                $"coverage {report.Coverage:P1}, peak cell {report.PeakCellShare:P1}, valid {report.ValidRate:P1}.\n" +
                $"Wrote metrics + heatmap + summary to {directory}");
        }

        // Runs the rectangle baseline against the organic variants and writes one comparison table
        [MenuItem("Tools/ToDonegeon/Run Expressive-Range Comparison (N=1000)")]
        public static void RunComparison()
        {
            var caveWithPockets = new RoomCarverSettings(shape: ChamberShape.Cave,
                caveSettings: new CaveSettings(keepOnlyLargestComponent: false));

            var runs = new List<(string, ExpressiveRangeReport)>
            {
                ("rectangles", Analyse(new RoomCarverSettings(), null, 8)),
                ("caves", Analyse(new RoomCarverSettings(shape: ChamberShape.Cave), null, 8)),
                ("random-walk", Analyse(new RoomCarverSettings(shape: ChamberShape.RandomWalk), null, 8)),
                ("caves + loops", Analyse(new RoomCarverSettings(shape: ChamberShape.Cave), new CorridorSettings(0.5f, 3), 8)),
                ("caves, repair off (raw)", Analyse(caveWithPockets, null, 1)),
            };

            string directory = ReportsDirectory();
            File.WriteAllText(Path.Combine(directory, "expressive_range_comparison.md"),
                ExpressiveRangeComparison.ToMarkdown(runs));

            Debug.Log($"[ExpressiveRange] wrote variant comparison to {directory}");
        }

        private static ExpressiveRangeReport Analyse(RoomCarverSettings carver, CorridorSettings corridor, int attempts)
        {
            var generator = new BspDungeonGenerator(null, carver, null, corridor, attempts);
            return ExpressiveRangeAnalysis.Run(generator, Width, Height, baseSeed: 0, count: SampleCount,
                xAxis: new MetricAxis("RoomDensity", m => m.RoomDensity),
                yAxis: new MetricAxis("Linearity", m => m.Linearity),
                bins: Bins);
        }

        private static string ReportsDirectory()
        {
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Reports"));
            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
