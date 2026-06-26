using NUnit.Framework;
using ToDonegeon.Gameplay;

namespace ToDonegeon.Tests
{
    public class CharacterTests
    {
        [Test]
        public void Player_Is_A_Character_With_Shared_Stats()
        {
            var player = new PlayerCharacter(30, 5);

            Assert.That(player, Is.InstanceOf<Character>());
            Assert.That(player.Health.Max, Is.EqualTo(30));
            Assert.That(player.AttackPower, Is.EqualTo(5));
            Assert.That(player.IsAlive, Is.True);
        }

        [Test]
        public void Gold_Accumulates_And_Ignores_Nonpositive()
        {
            var player = new PlayerCharacter(30, 5);

            player.AddGold(20);
            player.AddGold(-5);
            player.AddGold(10);

            Assert.That(player.Gold, Is.EqualTo(30));
        }
    }
}
