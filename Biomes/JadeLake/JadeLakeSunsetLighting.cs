using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace JadeFables.Biomes.JadeLake
{
	class JadeLakeSunsetLighting : ModSystem
	{
		public static float val = 0f;
		public override void Load()
		{
			On.Terraria.Graphics.Light.TileLightScanner.GetTileLight += SunsetLighting;
		}

        public override void PreUpdateWorld()
        {
			val = (0.43f * MathHelper.Min(1, GetInstance<JadeLakeSystem>().TotalBiomeCount * 0.0003f));
        }

        private void SunsetLighting(On.Terraria.Graphics.Light.TileLightScanner.orig_GetTileLight orig, Terraria.Graphics.Light.TileLightScanner self, int x, int y, out Vector3 outputColor)
		{
			orig(self, x, y, out outputColor);

			if (!WorldGen.InWorld(x, y))
				return;

			Tile tile = Framing.GetTileSafely(x, y);

			if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<JadeLakeBiome>()) && tile.LiquidAmount == 0 && tile.WallType == 0 && (!tile.HasTile || !Main.tileBlockLight[tile.TileType]))
			{
				Color baseColor = new Color(255, 215, 215);
				outputColor += baseColor.ToVector3() * val;
			}
		}
	}
}