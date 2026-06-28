using System;

namespace ToDonegeon.Analysis
{
    // A metric pulled from DungeonMetrics
    public sealed class MetricAxis
    {
        public string Name { get; }
        public Func<DungeonMetrics, float> Selector { get; }

        public MetricAxis(string name, Func<DungeonMetrics, float> selector)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }
    }
}
