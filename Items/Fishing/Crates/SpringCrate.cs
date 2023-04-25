using JadeFables.Items.SpringChestLoot.FireworkPack;
using JadeFables.Items.SpringChestLoot.Gong;
using JadeFables.Items.SpringChestLoot.TanookiLeaf;
using JadeFables.Items.SpringChestLoot.Chopsticks;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;
using Terraria.GameContent.ItemDropRules;
using JadeFables.Biomes.JadeLake;
using JadeFables.Core;
using static JadeFables.Items.Fishing.Crates.CrateDropRules;
using JadeFables.Items.SpringChestLoot.Hwacha;
using JadeFables.Tiles.JadeFountain;
using Terraria.Localization;
using JadeFables.Items.SpringChestLoot.DuelingSpirits;

namespace JadeFables.Items.Fishing.Crates
{
    public static class CrateDropRules
    {
        public static IItemDropRule goldCoin = ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 4, 5, 12);

        public static IItemDropRule[] ores = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 1, 20, 35),
        };
        public static IItemDropRule[] hardmodeOres = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CobaltOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.PalladiumOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.MythrilOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.TitaniumOre, 1, 20, 35),
        };

        public static IItemDropRule[] bars = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 1, 6, 16),
        };
        public static IItemDropRule[] hardmodeBars = new IItemDropRule[] //technically the stack size should have a 50% chance to have 1 subtracted from it but even im not nitpicky enough to care
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.TitaniumBar, 1, 6, 16),
        };

        public static IItemDropRule[] potions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 2, 4),
        };
        public static IItemDropRule[] recoveryPotions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 1, 5, 17),
            ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 1, 5, 17),
        };
        public static IItemDropRule[] extraBait = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.MasterBait, 1, 2, 6),
            ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 1, 2, 6)
        };
    }

    public class SpringCrate : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsFishingCrate[Type] = true;
            ItemID.Sets.IsFishingCrateHardmode[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 99;
            Item.createTile = ModContent.TileType<SpringCrateTile>();
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemType<FireworkPack>(), ItemType<TanookiLeaf>(), ItemType<GongItem>(), ItemType<Chopsticks>(), ItemType<Hwacha>(), ItemType<DuelingSpirits>()));
            itemLoot.Add(ItemDropRule.Common(4, ItemType<JadeFountainItem>()));
            itemLoot.Add(ItemDropRule.Common(6, ItemType<Jade.FestivalLantern.FestivalLantern>()));
            itemLoot.Add(goldCoin);
            //note: SequentialRulesNotScalingWithLuck unfortunately makes droprules later in the sequential list less common, as it will exit out as soon as any droprule succeeds.
            itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(1, new OneFromRulesRule(4, bars), new OneFromRulesRule(7, ores)));
            itemLoot.Add(new OneFromRulesRule(4, potions));
            itemLoot.Add(new OneFromRulesRule(2, recoveryPotions));
            itemLoot.Add(new OneFromRulesRule(2, extraBait));
        }
    }
    public class SpringCrateTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileTable[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide | AnchorType.Table, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(35, 175, 95), name);
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 0;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<SpringCrate>());
        }
    }

    public class DragonCrate : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsFishingCrate[Type] = true;
            ItemID.Sets.IsFishingCrateHardmode[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ModContent.ItemType<SpringCrate>());
            Item.createTile = ModContent.TileType<DragonCrateTile>();
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemType<FireworkPack>(), ItemType<TanookiLeaf>(), ItemType<GongItem>(), ItemType<Chopsticks>(), ItemType<Hwacha>(), ItemType<DuelingSpirits>()));
            itemLoot.Add(ItemDropRule.Common(4, ItemType<JadeFountainItem>()));
            itemLoot.Add(ItemDropRule.Common(6, ItemType<Jade.FestivalLantern.FestivalLantern>()));
            itemLoot.Add(goldCoin);
            itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(1, new OneFromRulesRule(6, hardmodeBars), new OneFromRulesRule(14, hardmodeOres), new OneFromRulesRule(12, bars), new OneFromRulesRule(14, ores)));
            itemLoot.Add(new OneFromRulesRule(4, potions));
            itemLoot.Add(new OneFromRulesRule(2, recoveryPotions));
            itemLoot.Add(new OneFromRulesRule(2, extraBait));
        }
    }
    public class DragonCrateTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileTable[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide | AnchorType.Table, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(200, 65, 140), name);
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 0;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<DragonCrate>());
        }
    }
}
