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

    // result of the player bumping a hostile
    public readonly struct BumpOutcome
    {
        public int DamageDealt { get; }
        public bool TargetKilled { get; }
        public int DamageTaken { get; }

        public BumpOutcome(int damageDealt, bool targetKilled, int damageTaken)
        {
            DamageDealt = damageDealt;
            TargetKilled = targetKilled;
            DamageTaken = damageTaken;
        }
    }

    // Minimal combat resolution for the prototype (flat attack values).
    // To be reworked for the MVP.
    public static class Combat
    {
        public static StrikeOutcome Strike(Character attacker, Character defender)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (defender == null) throw new ArgumentNullException(nameof(defender));

            int dealt = defender.Health.TakeDamage(attacker.AttackPower);
            return new StrikeOutcome(dealt, defender.Health.IsDead);
        }

        public static BumpOutcome ResolveBump(Character attacker, Character defender)
        {
            StrikeOutcome hit = Strike(attacker, defender);
            int taken = hit.TargetKilled ? 0 : Strike(defender, attacker).DamageDealt;
            return new BumpOutcome(hit.DamageDealt, hit.TargetKilled, taken);
        }
    }
}
