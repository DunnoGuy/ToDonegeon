using System;
using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    public class DifficultyPlanTests
    {
        [Test]
        public void Tier_One_Is_The_Baseline_Floor()
        {
            FloorPlan plan = DifficultyPlan.ForTier(1);

            // the default 48x32 floor, unchanged
            Assert.That(plan.Width, Is.EqualTo(48));
            Assert.That(plan.Height, Is.EqualTo(32));
            Assert.That(plan.MaxBspDepth, Is.EqualTo(BspSettings.DefaultMaxDepth));
            Assert.That(plan.Tier, Is.EqualTo(1));
        }

        [Test]
        public void Every_Knob_Grows_Or_Holds_As_The_Tier_Rises()
        {
            for (int tier = 1; tier < 20; tier++)
            {
                FloorPlan low = DifficultyPlan.ForTier(tier);
                FloorPlan high = DifficultyPlan.ForTier(tier + 1);

                Assert.That(high.Width, Is.GreaterThanOrEqualTo(low.Width));
                Assert.That(high.Height, Is.GreaterThanOrEqualTo(low.Height));
                Assert.That(high.MaxBspDepth, Is.GreaterThanOrEqualTo(low.MaxBspDepth));
                Assert.That(high.MaxEnemiesPerRoom, Is.GreaterThanOrEqualTo(low.MaxEnemiesPerRoom));
            }
        }

        [Test]
        public void A_Few_Tiers_Up_The_Floor_Is_Bigger_And_Busier()
        {
            FloorPlan early = DifficultyPlan.ForTier(1);
            FloorPlan later = DifficultyPlan.ForTier(4);

            Assert.That(later.Width, Is.GreaterThan(early.Width));
            Assert.That(later.MaxEnemiesPerRoom, Is.GreaterThan(early.MaxEnemiesPerRoom));
        }

        [Test]
        public void Growth_Levels_Off_At_A_Ceiling()
        {
            FloorPlan huge = DifficultyPlan.ForTier(1000);
            FloorPlan alsoHuge = DifficultyPlan.ForTier(1001);

            Assert.That(alsoHuge.Width, Is.EqualTo(huge.Width));
            Assert.That(alsoHuge.MaxEnemiesPerRoom, Is.EqualTo(huge.MaxEnemiesPerRoom));
        }

        [Test]
        public void Tier_Below_One_Is_Rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DifficultyPlan.ForTier(0));
        }

        [Test]
        public void Produced_Numbers_Satisfy_The_Real_Settings()
        {
            FloorPlan plan = DifficultyPlan.ForTier(6);

            Assert.DoesNotThrow(() => new BspSettings(maxDepth: plan.MaxBspDepth));
            Assert.DoesNotThrow(() =>
                new PopulatorSettings(maxEnemiesPerRoom: plan.MaxEnemiesPerRoom, difficultyTier: plan.Tier));
        }
    }
}
