using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using JadeFables.Biomes.JadeLake;

namespace JadeFables.Tiles.BlossomWall
{
	public class BlossomWall : ModWall
	{
        public static Vector2 TileAdj => (Lighting.Mode == Terraria.Graphics.Light.LightMode.Retro || Lighting.Mode == Terraria.Graphics.Light.LightMode.Trippy) ? Vector2.Zero : Vector2.One * 12;


        public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			WallID.Sets.Conversion.Grass[Type] = true;
			DustType = 282;
			HitSound = SoundID.Grass;
			AddMapEntry(Color.HotPink);
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (i > Main.screenPosition.X / 16 && i < Main.screenPosition.X / 16 + Main.screenWidth / 16 && j > Main.screenPosition.Y / 16 && j < Main.screenPosition.Y / 16 + Main.screenHeight / 16)
			{
				Texture2D tex = Request<Texture2D>("JadeFables/Tiles/BlossomWall/BlossomWallFlow").Value;
				var rand = new Random(i + (j * 100000));

				float offset = i * j % 6.28f + (float)rand.NextDouble() / 8f;
				float sin = (float)Math.Sin(Main.GameUpdateCount / 45f + offset);

				spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 2.2f - Main.screenPosition,
				new Rectangle(rand.Next(4) * 26, 0, 24, 24), Lighting.GetColor(i, j), MathF.Sin(offset) + sin * 0.09f, new Vector2(12, 12), 1 + sin / 14f, 0, 0);
			}
		}
	}

    public class BlossomWallItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cherry Blossom Wall");
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
            Item.createWall = ModContent.WallType<BlossomWall>(); // The ID of the wall that this item should place when used. ModContent.WallType<T>() method returns an integer ID of the wall provided to it through its generic type argument (the type in angle brackets).
        }
    }

    class DryadSellsBlossom : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Dryad && Main.LocalPlayer.InModBiome<JadeLakeBiome>())
            {
                shop.item[nextSlot].SetDefaults(ModContent.ItemType<BlossomWallItem>());
                nextSlot++;
            }
        }
    }
}