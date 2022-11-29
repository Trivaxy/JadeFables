//TODO:
//Water shader
//Better splash dust
//Better droplet gore
//Bubbles
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace JadeFables.Biomes.JadeLake
{
	public class JadeLakeWaterStyle : ModWaterStyle
	{

		public override int ChooseWaterfallStyle() {
			return ModContent.Find<ModWaterfallStyle>("JadeFables/JadeLakeWaterfallStyle").Slot;
		}

		public override int GetSplashDust() {
			return 6;
		}

		public override int GetDropletGore() {
			return 1;
		}

		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			r = 1f;
			g = 1f;
			b = 1f;
		}

		public override Color BiomeHairColor() {
			return Color.White;
		}

		public override byte GetRainVariant() {
			return (byte)Main.rand.Next(3);
		}

		public override Asset<Texture2D> GetRainTexture() {
			return ModContent.Request<Texture2D>("JadeFables/Biomes/JadeLake/JadeRain");
		}
	}
}