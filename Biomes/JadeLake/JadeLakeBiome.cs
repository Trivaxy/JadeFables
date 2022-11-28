namespace JadeFables.Biomes.JadeLake
{
    public class JadeLakeBiome : ModBiome
    {
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
