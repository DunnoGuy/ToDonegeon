# Expressive-range analysis

- Samples: **1000**
- Heatmap axes: **RoomDensity** (x) x **Linearity** (y), 10x10 bins over the observed range
- Coverage: **66.0%** of 100 cells (target >= 60.0% -> PASS)
- Peak cell: **9.1%** of samples (target <= 15.0% -> PASS)
- Validity: **100.0%** pass connectivity (target >= 99.0% -> PASS)

| Metric | min | mean | max |
|---|---|---|---|
| roomCount | 12 | 15.183 | 26 |
| roomDensity | 0.362 | 0.475 | 0.609 |
| walkableDensity | 0.404 | 0.502 | 0.627 |
| corridorShare | 0.02 | 0.054 | 0.106 |
| linearity | 0.304 | 0.481 | 0.636 |
| leniency | 0.275 | 0.644 | 0.925 |

Heatmap CSV: rows = y axis (first row = highest), columns = x axis (left = lowest).
