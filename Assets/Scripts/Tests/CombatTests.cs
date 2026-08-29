using NUnit.Framework;
using ToDonegeon.Gameplay;

namespace ToDonegeon.Tests
{
    public class CombatTests
    {
        [Test]
        public void Strike_Is_One_Way()
        {
            var attacker = new Character(20, 7);
            var defender = new Character(20, 4);

            StrikeOutcome outcome = Combat.Strike(attacker, defender);

            Assert.That(outcome.DamageDealt, Is.EqualTo(7));
            Assert.That(defender.Health.Current, Is.EqualTo(13));
            Assert.That(attacker.Health.Current, Is.EqualTo(20), "a strike has no counter");
            Assert.That(outcome.TargetKilled, Is.False);
        }
    }
}
