using System;

namespace ToDonegeon.Gameplay
{
    // Rolls a random item for a chest.
    public static class LootTable
    {
        private const int PotionWeight = 5;
        private const int WeaponWeight = 3;
        private const int CharmWeight = 2;

        private const int PotionMin = 8, PotionMax = 16;
        private const int WeaponMin = 1, WeaponMax = 3;
        private const int CharmMin = 4, CharmMax = 8;

        public static Item Roll(Random rng)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            switch (PickKind(rng))
            {
                case ItemKind.Weapon: return new Item("Blade", ItemKind.Weapon, Between(rng, WeaponMin, WeaponMax));
                case ItemKind.Charm: return new Item("Vitality charm", ItemKind.Charm, Between(rng, CharmMin, CharmMax));
                default: return new Item("Healing potion", ItemKind.Potion, Between(rng, PotionMin, PotionMax));
            }
        }

        private static ItemKind PickKind(Random rng)
        {
            int roll = rng.Next(PotionWeight + WeaponWeight + CharmWeight);
            if (roll < PotionWeight) return ItemKind.Potion;
            if (roll < PotionWeight + WeaponWeight) return ItemKind.Weapon;
            return ItemKind.Charm;
        }

        private static int Between(Random rng, int min, int max) => rng.Next(min, max + 1);
    }
}
