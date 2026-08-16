using NUnit.Framework;
using ToDonegeon.Gameplay;

namespace ToDonegeon.Tests
{
    public class HealthTests
    {
        [Test]
        public void Damage_Clamps_At_Zero_And_Reports_Amount_Dealt()
        {
            var health = new Health(10);

            Assert.That(health.TakeDamage(3), Is.EqualTo(3));
            Assert.That(health.Current, Is.EqualTo(7));
            Assert.That(health.TakeDamage(100), Is.EqualTo(7), "only the remaining HP can be dealt");
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void Heal_Never_Exceeds_Max()
        {
            var health = new Health(10);
            health.TakeDamage(8);

            Assert.That(health.Heal(100), Is.EqualTo(8));
            Assert.That(health.Current, Is.EqualTo(10));
        }

        [Test]
        public void Rejects_Nonpositive_Max()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new Health(0));
        }

        [Test]
        public void RaiseMax_Lifts_The_Ceiling_And_Heals_By_The_Same()
        {
            var health = new Health(10);
            health.TakeDamage(4); // 6 / 10

            health.RaiseMax(5);

            Assert.That(health.Max, Is.EqualTo(15));
            Assert.That(health.Current, Is.EqualTo(11));
        }
    }
}
