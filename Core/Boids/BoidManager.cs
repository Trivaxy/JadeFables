using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using JadeFables.Biomes.JadeLake;

namespace JadeFables.Core.Boids
{
	public class BoidHost : ILoadable
	{
		internal List<Flock> Flocks = new List<Flock>();
		private const int SPAWNRATE = 40;

		public void Draw(SpriteBatch spriteBatch)
		{
			foreach (Flock fishflock in Flocks)
			{
				fishflock.Draw(spriteBatch);
			}
		}

		public void Update()
		{
			foreach (Flock fishflock in Flocks)
				fishflock.Update();

			Player player = Main.LocalPlayer;
			//Test
			if (Main.GameUpdateCount % SPAWNRATE == 39 && Main.LocalPlayer.InModBiome<JadeLakeBiome>())
			{
				int flock = Main.rand.Next(0, Flocks.Count);
				int fluff = 1000;

				var rand = new Vector2(
					Main.rand.Next(-Main.screenWidth / 2 - fluff, Main.screenWidth / 2 + fluff),
					Main.rand.Next(-Main.screenHeight / 2 - fluff, Main.screenHeight / 2 + fluff));

				if (!new Rectangle(0, 0, Main.screenWidth, Main.screenHeight).Contains(rand.ToPoint()))
				{
					Vector2 position = Main.LocalPlayer.Center + rand;
					Point tP = position.ToTileCoordinates();
					if (WorldGen.InWorld(tP.X, tP.Y, 10))
					{
						Tile tile = Framing.GetTileSafely(tP.X, tP.Y);
						if (tile.LiquidAmount > 100)
							Flocks[flock].Populate(position, Main.rand.Next(2, 6), 40f);
					}
				}
			}
		}

		public void Load(Mod mod)
		{
			const int AmbientFishTextureCount = 4;
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

		//TODO: Move to update hook soon
		private void Main_DrawWoF(On.Terraria.Main.orig_DrawWoF orig, Main self)
		{
			if (Flocks != null)
			{
				Update();
				Draw(Main.spriteBatch);
			}
			orig(self);
		}

		public void Unload()
		{
			if (Flocks != null)
				Flocks.Clear();

			On.Terraria.Main.DrawWoF -= Main_DrawWoF;
		}
	}
}