using JadeFables.Dusts;
using JadeFables.Tiles.JadeSand;
using Terraria;

namespace JadeFables.Biomes.JadeLake
{
    public class JadeLakeSystem : ModSystem
    {
        public int JadeSandTileCount;
        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            JadeSandTileCount = tileCounts[TileType<JadeSandTile>()];
        }

        public override void PostUpdateEverything()
        {
            float progress = MathHelper.Min(JadeSandTileCount, 300) / 300f;
            if (JadeSandTileCount == 0)
                return;
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                if (new Vector2(x * 16f, Main.LocalPlayer.Center.Y).Distance(Main.LocalPlayer.Center) < Main.screenWidth / 2)
                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.LiquidAmount > 0)
                        {
                            if (tile.LiquidAmount > 0 && Main.tile[x, y - 1].LiquidAmount <= 0 && !Main.tile[x, y - 1].HasTile)
                            {
                                Lighting.AddLight(new Vector2(x * 16, y * 16), new Vector3(0, 220, 200) * (0.00001f * MathHelper.Min(JadeSandTileCount, 300)));
                                if (Main.rand.NextBool((int)(30 / progress)))
                                {
                                    Vector2 velocity = Vector2.UnitY.RotatedByRandom(0.1f) * -Main.rand.NextFloat(1f, 1.5f);
                                    Dust.NewDustPerfect(new Vector2(x * 16 + Main.rand.Next(-8, 8), y * 16 + 8), DustID.Smoke, velocity, 220, default, Main.rand.NextFloat(1f, 2f));
                                    Dust.NewDustPerfect(new Vector2(x * 16 + Main.rand.Next(-8, 8), y * 16 + 8), ModContent.DustType<SpringMist>(), velocity, 0, default, Main.rand.NextFloat(0.5f, 1.25f));
                                }
                            }
                            else
                                Lighting.AddLight(new Vector2(x * 16, y * 16), new Vector3(0, 200, 250) * (0.00001f * MathHelper.Min(JadeSandTileCount, 300)));

                            if (Main.rand.NextBool((int)(2000 / progress)))
                                Dust.NewDustPerfect(new Vector2(x * 16, y * 16 + 8), DustType<Dusts.WhiteSparkle>(), Vector2.Zero, 0, default, 0.5f);

                            if (Main.rand.NextBool((int)(8000 / progress)))
                                Dust.NewDustPerfect(new Vector2(x * 16, y * 16 + 8), DustType<Dusts.JadeBubble>(), -Vector2.UnitY.RotatedByRandom(0.5f), 0, default, Main.rand.NextFloat(0.75f, 1.5f));
                        }
                    }
            }
        }
    }
}
