using System;
using NUnit.Framework;
using ToDonegeon.Gameplay;

namespace ToDonegeon.Tests
{
    public class ItemTests
    {
        [Test]
        public void Rejects_A_Blank_Name()
        {
            Assert.Throws<ArgumentException>(() => new Item("  ", ItemKind.Potion, 5));
        }

        [Test]
        public void Rejects_A_Magnitude_Below_One()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Item("Draught", ItemKind.Potion, 0));
        }
    }

    public class InventoryTests
    {
        [Test]
        public void Added_Items_Keep_Their_Order()
        {
            var bag = new Inventory();
            var first = new Item("Draught", ItemKind.Potion, 8);
            var second = new Item("Dagger", ItemKind.Weapon, 2);

            bag.Add(first);
            bag.Add(second);

            Assert.That(bag.Count, Is.EqualTo(2));
            Assert.That(bag.Items[0].Name, Is.EqualTo("Draught"));
            Assert.That(bag.Items[1].Name, Is.EqualTo("Dagger"));
        }

        [Test]
        public void Removing_Takes_The_Item_Out()
        {
            var bag = new Inventory();
            bag.Add(new Item("Charm", ItemKind.Charm, 5));

            bool removed = bag.TryRemoveAt(0, out Item item);

            Assert.That(removed, Is.True);
            Assert.That(item.Name, Is.EqualTo("Charm"));
            Assert.That(bag.Count, Is.EqualTo(0));
        }

        [Test]
        public void A_Bad_Index_Removes_Nothing()
        {
            var bag = new Inventory();
            bag.Add(new Item("Draught", ItemKind.Potion, 8));

            Assert.That(bag.TryRemoveAt(3, out _), Is.False);
            Assert.That(bag.Count, Is.EqualTo(1));
        }
    }

    public class PlayerItemUseTests
    {
        [Test]
        public void A_Potion_Restores_Health_And_Is_Consumed()
        {
            var player = new PlayerCharacter(30, 5);
            player.Health.TakeDamage(20); // 10 / 30
            player.Inventory.Add(new Item("Draught", ItemKind.Potion, 8));

            bool used = player.TryUseItem(0, out Item item);

            Assert.That(used, Is.True);
            Assert.That(item.Kind, Is.EqualTo(ItemKind.Potion));
            Assert.That(player.Health.Current, Is.EqualTo(18));
            Assert.That(player.Inventory.Count, Is.EqualTo(0));
        }

        [Test]
        public void A_Weapon_Raises_Attack()
        {
            var player = new PlayerCharacter(30, 5);
            player.Inventory.Add(new Item("Dagger", ItemKind.Weapon, 3));

            player.TryUseItem(0, out _);

            Assert.That(player.AttackPower, Is.EqualTo(8));
        }

        [Test]
        public void A_Charm_Raises_Maximum_Health()
        {
            var player = new PlayerCharacter(30, 5);
            player.Inventory.Add(new Item("Charm", ItemKind.Charm, 10));

            player.TryUseItem(0, out _);

            Assert.That(player.Health.Max, Is.EqualTo(40));
            Assert.That(player.Health.Current, Is.EqualTo(40));
        }

        [Test]
        public void Using_An_Empty_Slot_Does_Nothing()
        {
            var player = new PlayerCharacter(30, 5);

            Assert.That(player.TryUseItem(0, out _), Is.False);
            Assert.That(player.AttackPower, Is.EqualTo(5));
        }
    }
}
