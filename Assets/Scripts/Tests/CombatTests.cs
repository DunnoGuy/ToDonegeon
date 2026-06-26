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

        [Test]
        public void Killing_Blow_Takes_No_Counter()
        {
            var player = new PlayerCharacter(30, 10);
            var enemy = new Character(8, 5);

            BumpOutcome outcome = Combat.ResolveBump(player, enemy);

            Assert.That(outcome.TargetKilled, Is.True);
            Assert.That(outcome.DamageDealt, Is.EqualTo(8));
            Assert.That(outcome.DamageTaken, Is.EqualTo(0));
            Assert.That(player.Health.Current, Is.EqualTo(30), "no counter from a dead enemy");
        }

        [Test]
        public void Surviving_Enemy_Counter_Attacks()
        {
            var player = new PlayerCharacter(30, 4);
            var enemy = new Character(20, 6);

            BumpOutcome outcome = Combat.ResolveBump(player, enemy);

            Assert.That(outcome.TargetKilled, Is.False);
            Assert.That(outcome.DamageDealt, Is.EqualTo(4));
            Assert.That(enemy.Health.Current, Is.EqualTo(16));
            Assert.That(outcome.DamageTaken, Is.EqualTo(6));
            Assert.That(player.Health.Current, Is.EqualTo(24));
        }
    }
}
