using JadeFables.Dusts;
using JadeFables.Helpers.FastNoise;
using JadeFables.Tiles.JadeGrass;
using JadeFables.Tiles.JadeSand;
using JadeFables.Tiles.JadeWaterfall;
using JadeFables.Tiles.SpringChest;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace JadeFables.Biomes.JadeLake
{
    internal static partial class JadeLakeWorldGen
    {

        public static void PolishPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Polishing jade biome";

            
            Rectangle worldRect = new Rectangle(0,0,Main.maxTilesX, Main.maxTilesY);

            //Places spring chests
            PlaceJadeChests(worldRect, 40);

            //Places jade grass
            JadeGrassPopulation(worldRect, 0.1f, 5f);

            PlaceJadeSandPiles(worldRect, 5);
        }

        public static void JadeGrassPopulation(Rectangle rect, float threshhold, float noiseFreq)
        {
            //Debug method to wipe jade grass
            Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<JadeGrassProj>()).ToList().ForEach(n => n.active = false);

            FastNoise fastnoise = new FastNoise(WorldGen.genRand.Next(0, 1000000));
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top + 1; j < rect.Top + rect.Height; j++)
                {
                    Tile tileAbove = Framing.GetTileSafely(i, j - 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);

                    if (!tileAbove.HasTile && mainTile.HasTile && mainTile.TileType == ModContent.TileType<JadeSandTile>())
                    {
                        float noiseVal = fastnoise.GetPerlin(i * noiseFreq, j * noiseFreq);
                        if (noiseVal > threshhold)
                        {
                            tileAbove.HasTile = true;
                            tileAbove.TileType = (ushort)ModContent.TileType<JadeGrassTile>();
                        }
                    }
                }
            }
        }

        public static void PlaceJadeChests(Rectangle rect, int chance)
        {
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top; j < rect.Top + rect.Height - 1; j++)
                {
                    Tile tileBelow = Framing.GetTileSafely(i, j + 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);

                    if (tileBelow.HasTile && tileBelow.TileType == ModContent.TileType<JadeSandTile>() && WorldGen.genRand.NextBool(chance))
                    {
                        WorldGen.PlaceChest(i, j, (ushort)ModContent.TileType<SpringChest>());
                    }
                }
            }
        }

        public static void PlaceJadeSandPiles(Rectangle rect, int chance)
        {
            int[] piles = new int[] { ModContent.TileType<JadeSandCastle1>(), ModContent.TileType<JadeSandCastle2>(), ModContent.TileType<JadeSandPile1>(), ModContent.TileType<JadeSandPile2>(), ModContent.TileType<JadeSandPile3>() };
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top; j < rect.Top + rect.Height - 1; j++)
                {
                    Tile tileBelow = Framing.GetTileSafely(i, j + 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);

                    if (tileBelow.HasTile && tileBelow.TileType == ModContent.TileType<JadeSandTile>() && WorldGen.genRand.NextBool(chance))
                    {
                        WorldGen.PlaceObject(i, j, piles[WorldGen.genRand.Next(piles.Length)]);
                    }
                }
            }
        }
    }
}