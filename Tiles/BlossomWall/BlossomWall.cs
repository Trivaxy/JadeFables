using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace JadeFables.Tiles.BlossomWall
{
	public class BlossomWall : ModWall
	{
        public static Vector2 TileAdj => (Lighting.Mode == Terraria.Graphics.Light.LightMode.Retro || Lighting.Mode == Terraria.Graphics.Light.LightMode.Trippy) ? Vector2.Zero : Vector2.One * 12;


        public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			WallID.Sets.Conversion.Grass[Type] = true;
			DustType = DustID.Grass;
			HitSound = SoundID.Grass;
			AddMapEntry(new Color(50, 140, 90));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (i > Main.screenPosition.X / 16 && i < Main.screenPosition.X / 16 + Main.screenWidth / 16 && j > Main.screenPosition.Y / 16 && j < Main.screenPosition.Y / 16 + Main.screenHeight / 16)
			{
				Texture2D tex = Request<Texture2D>("JadeFables/Tiles/BlossomWall/BlossomWallFlow").Value;
				var rand = new Random(i * j % 192372);

				float offset = i * j % 6.28f + (float)rand.NextDouble() / 8f;
				float sin = (float)Math.Sin(Main.GameUpdateCount / 45f + offset);

				spriteBatch.Draw(tex, (new Vector2(i + 0.5f, j + 0.5f) + TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 2.2f - Main.screenPosition,
				new Rectangle(rand.Next(4) * 26, 0, 24, 24), Lighting.GetColor(i, j), offset + sin * 0.09f, new Vector2(12, 12), 1 + sin / 14f, 0, 0);
			}
		}
	}
}