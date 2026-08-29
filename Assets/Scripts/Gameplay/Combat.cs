using System;

namespace ToDonegeon.Gameplay
{
    // result of a single strike
    public readonly struct StrikeOutcome
    {
        public int DamageDealt { get; }
        public bool TargetKilled { get; }

        public StrikeOutcome(int damageDealt, bool targetKilled)
        {
            DamageDealt = damageDealt;
            TargetKilled = targetKilled;
        }
    }

    // Deliberately simple combat for the PoC
    public static class Combat
    {
        public static StrikeOutcome Strike(Character attacker, Character defender)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (defender == null) throw new ArgumentNullException(nameof(defender));

            int dealt = defender.Health.TakeDamage(attacker.AttackPower);
            return new StrikeOutcome(dealt, defender.Health.IsDead);
        }
    }
}
