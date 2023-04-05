using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using JadeFables.Biomes.JadeLake;

namespace JadeFables.Tiles.JadeSandWall
{
	public class JadeSandWall : ModWall
	{
        public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = DustID.Dirt;
            ItemDrop = ModContent.ItemType<JadeSandWallItem>();
            HitSound = SoundID.Dig;
			AddMapEntry(jadeSandDarkWall);
		}
    }

    public class JadeSandWallItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = Item.buyPrice(0, 0, 1, 0);
            Item.createWall = ModContent.WallType<JadeSandWall>(); // The ID of the wall that this item should place when used. ModContent.WallType<T>() method returns an integer ID of the wall provided to it through its generic type argument (the type in angle brackets).
        }
    }
}