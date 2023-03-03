using Terraria.ModLoader;

namespace JadeFables.Biomes.JadeLake
{
	public class JadeUndergroundBackgroundStyle : ModUndergroundBackgroundStyle
	{
		public override void FillTextureArray(int[] textureSlots)
		{
			textureSlots[0] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Biomes/JadeLake/JadeBiomeUnderground0");
			textureSlots[1] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Biomes/JadeLake/JadeBiomeUnderground1");
			textureSlots[2] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Biomes/JadeLake/JadeBiomeUnderground2");
			textureSlots[3] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Biomes/JadeLake/JadeBiomeUnderground3");
		}
	}
	
}