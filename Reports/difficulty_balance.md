# Difficulty balance and progression

Threat = summed health x attack of every enemy and the boss on a floor. Each row samples many floors at one descent depth.

- **Progression**: mean threat should climb with depth.
- **Balance**: spread (std dev / mean) within a depth should stay small, so a given depth plays consistently whatever the seed.

| Depth | Map | Mean threat | Min | Max | Spread | Enemies | Valid |
|---|---|---|---|---|---|---|---|
| 1 | 48x32 | 1241.8 | 648 | 2280 | 24.1% | 6.4 | 100.0% |
| 2 | 54x36 | 2044.5 | 888 | 3480 | 23.4% | 7.8 | 100.0% |
| 3 | 60x40 | 4319.1 | 2064 | 7836 | 21.5% | 14.8 | 100.0% |
| 4 | 66x44 | 6829.8 | 2976 | 11376 | 21.8% | 18.6 | 100.0% |
| 5 | 72x48 | 13820.2 | 7320 | 25836 | 23.9% | 32.5 | 100.0% |
| 6 | 78x52 | 20813.2 | 9876 | 34932 | 20.0% | 39.9 | 100.0% |
| 7 | 84x56 | 35975.1 | 20916 | 55356 | 16.2% | 58.1 | 100.0% |
| 8 | 90x60 | 47730.6 | 29592 | 76428 | 16.0% | 63.9 | 100.0% |
| 9 | 96x64 | 62545.3 | 38424 | 96156 | 16.3% | 70.8 | 100.0% |
| 10 | 96x64 | 73593.1 | 45216 | 113292 | 16.3% | 70.8 | 100.0% |

- Progression (mean threat strictly rising with depth): **PASS**
- Balance (worst within-depth spread 24.1%, target <= 35.0%): **PASS**
- Validity (worst depth 100.0% connected, target 100%): **PASS**
