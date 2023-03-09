using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using JadeFables.Biomes.JadeLake;

namespace JadeFables.Core.Boids
{
	public class BoidHost : ModSystem
	{
		public List<Flock> Flocks = new List<Flock>();
		private const int SPAWNRATE = 10;

		public void Draw(SpriteBatch spriteBatch)
		{
			foreach (Flock fishflock in Flocks)
			{
				fishflock.Draw(spriteBatch);
			}
		}

		public override void PostUpdateEverything()
		{
			foreach (Flock fishflock in Flocks)
				fishflock.Update();

			Player player = Main.LocalPlayer;
			
			//Attempt to populate random flock every SPAWNRATE ticks
			if (Main.GameUpdateCount % SPAWNRATE == (SPAWNRATE - 1) && player.InModBiome<JadeLakeBiome>()) {
				int randomFlock = Main.rand.Next(0, Flocks.Count());
				int fluff = 1000;

				int fishCount = Flocks[randomFlock].FishCount;
				int maxFishCount = Flocks[randomFlock].MaxFish;
				float spawnChance = (float) (maxFishCount - fishCount) / maxFishCount;
				
				//Attempt to populate flock
				if (Main.rand.NextFloat(0, 1) >= spawnChance) return;
				
				var randOffset = new Vector2(
					Main.rand.Next(-Main.screenWidth / 2 - fluff, Main.screenWidth / 2 + fluff),
					Main.rand.Next(-Main.screenHeight / 2 - fluff, Main.screenHeight / 2 + fluff));

				//If new random offset is currently not on screen
				if (!new Rectangle(0, 0, Main.screenWidth, Main.screenHeight).Contains(randOffset.ToPoint())) {
					Vector2 position = player.Center + randOffset;
					Point tilePosition = position.ToTileCoordinates();
			
					if (WorldGen.InWorld(tilePosition.X, tilePosition.Y, 10)) {
						Tile tile = Framing.GetTileSafely(tilePosition.X, tilePosition.Y);
						
						//Check if it is a full tile of water
						if (tile.LiquidAmount > 100 && tile.LiquidType == LiquidID.Water) {
							Flocks[randomFlock].Populate(position, Main.rand.Next(2, 6), 40f);
						}
					}
				}
			}
		}

		public override void Load()
		{
			const int AmbientFishTextureCount = 5;
			Texture2D[] textures = new Texture2D[AmbientFishTextureCount];

			bool[] addedIDs = new bool[AmbientFishTextureCount];

			for (int j = 0; j < textures.Length; ++j)
			{
				int id = j + 1;

				if (!addedIDs[id - 1]) //So we don't have multiple of the same texture
				{
					textures[j] = ModContent.Request<Texture2D>($"JadeFables/NPCs/Koi/SmallKoi{id}", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
					addedIDs[id - 1] = true;
				}
				else
					j--;
			}

			Flocks.Add(new Flock(textures, 1f, Main.rand.Next(50, 200)));

			On.Terraria.Main.DrawWoF += Main_DrawWoF;
		}

		private void Main_DrawWoF(On.Terraria.Main.orig_DrawWoF orig, Main self)
		{
			if (Flocks != null)
			{
				Draw(Main.spriteBatch);
			}
			orig(self);
		}

		public override void Unload()
		{
			if (Flocks != null)
				Flocks.Clear();

			On.Terraria.Main.DrawWoF -= Main_DrawWoF;
		}
	}
}