using System;
using System.Collections.Generic;
using NUnit.Framework;
using ToDonegeon.Gameplay;

namespace ToDonegeon.Tests
{
    public class LootTableTests
    {
        [Test]
        public void Roll_Is_Deterministic_For_A_Seed()
        {
            Item a = LootTable.Roll(new Random(42));
            Item b = LootTable.Roll(new Random(42));

            Assert.That(a.Kind, Is.EqualTo(b.Kind));
            Assert.That(a.Magnitude, Is.EqualTo(b.Magnitude));
            Assert.That(a.Name, Is.EqualTo(b.Name));
        }

        [Test]
        public void Every_Roll_Is_A_Usable_Item()
        {
            var rng = new Random(1);
            for (int i = 0; i < 200; i++)
                Assert.That(LootTable.Roll(rng).Magnitude, Is.GreaterThan(0));
        }

        [Test]
        public void All_Three_Kinds_Turn_Up_Across_Many_Rolls()
        {
            var rng = new Random(7);
            var kinds = new HashSet<ItemKind>();
            for (int i = 0; i < 300; i++) kinds.Add(LootTable.Roll(rng).Kind);

            Assert.That(kinds, Does.Contain(ItemKind.Potion));
            Assert.That(kinds, Does.Contain(ItemKind.Weapon));
            Assert.That(kinds, Does.Contain(ItemKind.Charm));
        }

        [Test]
        public void Null_Rng_Is_Rejected()
        {
            Assert.Throws<ArgumentNullException>(() => LootTable.Roll(null));
        }
    }
}
