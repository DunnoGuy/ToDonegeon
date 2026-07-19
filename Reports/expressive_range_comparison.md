# Expressive-range comparison

Ranges are min-max across the samples of each variant.

| variant | samples | coverage | peak cell | valid | roomDensity | walkableDensity | corridorShare | linearity |
|---|---|---|---|---|---|---|---|---|
| rectangles | 1000 | 66.0% | 9.1% | 100.0% | 0.36-0.61 | 0.4-0.63 | 0.02-0.11 | 0.3-0.64 |
| caves | 1000 | 65.0% | 9.1% | 100.0% | 0.4-0.72 | 0.32-0.57 | 0.02-0.16 | 0.33-0.64 |
| random-walk | 1000 | 60.0% | 10.9% | 100.0% | 0.45-0.67 | 0.35-0.39 | 0.06-0.17 | 0.28-0.64 |
| caves + loops | 1000 | 67.0% | 7.0% | 100.0% | 0.4-0.72 | 0.33-0.57 | 0.03-0.17 | 0.29-0.64 |
| caves, repair off (raw) | 1000 | 59.0% | 10.2% | 47.9% | 0.41-0.72 | 0.33-0.57 | 0.02-0.16 | 0.33-0.64 |
