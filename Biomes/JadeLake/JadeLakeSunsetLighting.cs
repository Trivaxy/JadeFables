﻿using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace JadeFables.Biomes.JadeLake
{
	class JadeLakeSunsetLighting : ModSystem
	{
		public override void Load()
		{
			On.Terraria.Graphics.Light.TileLightScanner.GetTileLight += SunsetLighting;
		}

		private void SunsetLighting(On.Terraria.Graphics.Light.TileLightScanner.orig_GetTileLight orig, Terraria.Graphics.Light.TileLightScanner self, int x, int y, out Vector3 outputColor)
		{
			orig(self, x, y, out outputColor);

			if (!WorldGen.InWorld(x, y))
				return;

			Tile tile = Framing.GetTileSafely(x, y);

			if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<JadeLakeBiome>()) && (!tile.HasTile || !Main.tileBlockLight[tile.TileType]))
			{
				Color baseColor = Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f);
				outputColor += baseColor.ToVector3() * 0.75f;
			}
		}
	}
}