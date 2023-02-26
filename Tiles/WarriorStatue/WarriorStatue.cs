using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace JadeFables.Tiles.WarriorStatue
{
	public class WarriorStatue : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileMergeDirt[Type] = true;

			TileObjectData.newTile.Width = 5;
			TileObjectData.newTile.Height = 8;
			TileObjectData.newTile.Origin = new Point16(0, 7);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.addTile(Type); 
			DustType = DustID.Stone;

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Warrior Statue");
			AddMapEntry(Color.Gray, name);
		}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
		public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new EntitySource_TileBreak(i, j), (i + 3) * 16, (j + 5) * 16, 16, 32, ModContent.ItemType<WarriorStatueItem>());

        public override bool RightClick(int i, int j) {
            int gillsCost = Item.silver * 25;
            Player player = Main.LocalPlayer;

            if (!player.CanBuyItem(gillsCost)) {
                return false;
            }

            player.BuyItem(gillsCost);
            // 15 minute buff
            player.AddBuff(BuffID.Gills, 60 * 60 * 15, false);

            SoundEngine.PlaySound(SoundID.Coins);
            SoundEngine.PlaySound(SoundID.Item155 with {PitchVariance = 0.125f});
            return true;
        }
    }

    internal class WarriorStatueItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Warrior Statue");
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<WarriorStatue>();
            Item.width = 10;
            Item.height = 24;


            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.White;
        }
    }
}