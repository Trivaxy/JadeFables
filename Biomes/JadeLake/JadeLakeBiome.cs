namespace JadeFables.Biomes.JadeLake
{
    public class JadeLakeBiome : ModBiome
    {
        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("JadeFables/JadeLakeWaterStyle"); // Sets a water style for when inside this biome

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/JadeBiomeMusic");

        public override string MapBackground => "JadeFables/Biomes/JadeLake/JadeBG";

        public override string Name => "The Springs";

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("JadeFables/JadeUndergroundBackgroundStyle");

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Jade Lake");
		}

		public override bool IsBiomeActive(Player player)
		{
			return GetInstance<JadeLakeSystem>().TotalBiomeCount >= 1000;
		}
	}
}
