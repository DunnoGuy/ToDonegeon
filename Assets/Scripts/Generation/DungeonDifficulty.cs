using System;
using System.Collections.Generic;

namespace ToDonegeon.Generation
{
    // A threat-based read on how dangerous a populated dungeon is: the summed fighting power of every
    // enemy and the boss, where one creature's power is its health times its attack.
    public readonly struct DungeonDifficulty
    {
        public int EnemyCount { get; }
        public long TotalThreat { get; }
        public long BossThreat { get; }

        public long EnemyThreat => TotalThreat - BossThreat;

        public DungeonDifficulty(int enemyCount, long totalThreat, long bossThreat)
        {
            EnemyCount = enemyCount;
            TotalThreat = totalThreat;
            BossThreat = bossThreat;
        }

        public static DungeonDifficulty Measure(IReadOnlyList<Entity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            long total = 0;
            long boss = 0;
            int enemies = 0;
            foreach (Entity entity in entities)
            {
                if (!entity.Stats.HasValue) continue;

                CombatStats stats = entity.Stats.Value;
                long threat = (long)stats.MaxHealth * stats.AttackPower;
                total += threat;

                if (entity.Kind == EntityKind.Boss) boss += threat;
                else if (entity.Kind == EntityKind.Enemy) enemies++;
            }

            return new DungeonDifficulty(enemies, total, boss);
        }
    }
}
