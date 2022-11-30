using JadeFables.Helpers.FastNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace JadeFables.Biomes.JadeLake
{
    internal static class JadeLakeWorldGen
    {
        public static void SurfaceItemPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Progress message jade springs placeholder";

            FastNoise fastnoise = new FastNoise();

            Main.spawnTileX = Main.maxTilesX / 2;
            Main.spawnTileY = Main.maxTilesY / 3;
            Main.worldSurface = Main.spawnTileY;

            float freq = 25;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 50f)) / 5);
            float amp = 0.3f;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 36f)) / 2);

            float widtMult = 1.5f;
            int radius = 32;

            float inc = (float)Math.Tau / (8 * radius * widtMult * (freq / 5f));
            for (float i = 0; i < (float)Math.PI; i += inc)
            {
                Vector2 dist = new(radius * (1 + fastnoise.GetCubic(0, i * radius * freq) * amp), 0);
                Vector2 pos = dist.RotatedBy(i, Vector2.Zero);
                WorldGen.PlaceTile((int)(Main.spawnTileX + (pos.X * widtMult)), (int)(Main.spawnTileY + pos.Y), TileID.AmberGemspark, true, true);
            }

            //int size = 64;
            //WorldGen.TileRunner(Main.spawnTileX, Main.spawnTileY + 10, size, Main.rand.Next(1, 1), TileID.DiamondGemspark, true, 0f, 0f, true, true);
        }
    }
}
