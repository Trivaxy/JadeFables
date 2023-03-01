using JadeFables.Items.SpringChestLoot.FireworkPack;
using JadeFables.Items.SpringChestLoot.Gong;
using JadeFables.Items.SpringChestLoot.TanookiLeaf;
using JadeFables.Items.SpringChestLoot.Chopsticks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using JadeFables.Items.Potions.Heartbeat;
using JadeFables.Items.Potions.Spine;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;
using JadeFables.Items.Fishing.DragonCrate;

namespace JadeFables.Items.Fishing.SpringCrate
{
	public class SpringCrate : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spring Crate");
			Tooltip.SetDefault("Right click to open\n");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(gold : 1);
			Item.rare = ItemRarityID.Green;
			Item.maxStack = 30;
            Item.createTile = ModContent.TileType<SpringCrateTile>();
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.autoReuse = true;
            Item.useStyle = 1;
            Item.consumable = true;
        }

		public override bool CanRightClick() => true;

		public override void RightClick(Player player)
		{
			SpawnLoot(player, Item);
		}

		private static void SpawnLoot(Player player, Item item)
		{
            int[] primaryLoot = new int[] { ModContent.ItemType<FireworkPack>(), ModContent.ItemType<TanookiLeaf>(), ModContent.ItemType<GongItem>(), ModContent.ItemType<Chopsticks>() };
            int[] possibleOres = new int[] { ItemID.CopperOre, ItemID.TinOre, ItemID.IronOre, ItemID.LeadOre, ItemID.SilverOre, ItemID.TungstenOre, ItemID.PlatinumOre, ItemID.GoldOre };
            int[] possibleBars = new int[] { ItemID.CopperBar, ItemID.TinBar, ItemID.IronBar, ItemID.LeadBar, ItemID.SilverBar, ItemID.TungstenBar, ItemID.PlatinumBar, ItemID.GoldBar };
			int[] possiblePotions = new int[] { ItemID.ObsidianSkinPotion, ItemID.SpelunkerPotion, ItemID.HunterPotion, ItemID.GravitationPotion, ItemID.HeartreachPotion, ItemID.MiningPotion, ModContent.ItemType<HeartbeatPotion>(), ModContent.ItemType<SpinePotion>() };
			int[] possibleBait = new int[] { ItemID.JourneymanBait, ItemID.MasterBait };
			int[] possiblePotions2 = new int[] { ItemID.HealingPotion, ItemID.ManaPotion };

            Item.NewItem(new EntitySource_ItemOpen(player, item.type), player.Center, primaryLoot[Main.rand.Next(primaryLoot.Length)], 1);

			if (Main.rand.NextBool(4))
				Item.NewItem(new EntitySource_ItemOpen(player, item.type), player.Center, ItemID.GoldCoin, Main.rand.Next(5, 13));

            if (Main.rand.NextBool(7))
                Item.NewItem(new EntitySource_ItemOpen(player, item.type), player.Center, possibleOres[Main.rand.Next(possibleOres.Length)], Main.rand.Next(30, 50));

            if (Main.rand.NextBool(4))
                Item.NewItem(new EntitySource_ItemOpen(player, item.type), player.Center, possibleBars[Main.rand.Next(possibleBars.Length)], Main.rand.Next(10, 21));

			if (Main.rand.NextBool(4))
                Item.NewItem(new EntitySource_ItemOpen(player, item.type), player.Center, possiblePotions[Main.rand.Next(possiblePotions.Length)], Main.rand.Next(2, 5));

            if (Main.rand.NextBool(2))
                Item.NewItem(new EntitySource_ItemOpen(player, item.type), player.Center, possiblePotions2[Main.rand.Next(possiblePotions2.Length)], Main.rand.Next(5, 18));

            if (Main.rand.NextBool(2))
                Item.NewItem(new EntitySource_ItemOpen(player, item.type), player.Center, possibleBait[Main.rand.Next(possibleBait.Length)], Main.rand.Next(2, 7));

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
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Spring Crate");
            AddMapEntry(Color.DarkGreen, name);
            DustType = DustID.WoodFurniture;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ModContent.ItemType<SpringCrate>());
        }
    }
}
