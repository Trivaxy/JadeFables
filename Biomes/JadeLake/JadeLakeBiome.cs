namespace JadeFables.Biomes.JadeLake
{
    public class JadeLakeBiome : ModBiome
    {
        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("JadeFables/JadeLakeWaterStyle"); // Sets a water style for when inside this biome

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Jade Lake");
		}

		public override bool IsBiomeActive(Player player)
		{
			return GetInstance<JadeLakeSystem>().JadeSandTileCount >= 300;
		}
	}
}
