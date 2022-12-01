using JadeFables.Helpers.FastNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace JadeFables.Biomes.JadeLake
{
    internal static class JadeLakeWorldGen
    {
        public static void SurfaceItemPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Progress message jade springs placeholder";

            Main.spawnTileX = Main.maxTilesX / 2;
            Main.spawnTileY = Main.maxTilesY / 3;
            Main.worldSurface = Main.spawnTileY;

            Point16 biomePosition = new Point16(Main.spawnTileX, Main.spawnTileY);

            int biomeSize = 128;
            float widtMult = 1.3f;

            //float freq = 25;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 50f)) / 5);
            //float amp = 0.05f;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 36f)) / 2);

            float mainBodyfreq = 3;
            float mainBodyamp = 0.5f;
            float mainBodyHeightMult = 1.8f;

            //float mainBodyfreq = 3;
            //float mainBodyamp = 0.4f;
            //float mainBodyHeightMult = 1.8f;for fractal
            FastNoise fastnoise = new FastNoise(WorldGen.genRand.Next());

            int RANDOFFSET = (int)Main.GameUpdateCount * 3;//debug

            for (int i = -(int)(biomeSize * widtMult); i < (biomeSize * widtMult); i++)
            {
                float height = biomeSize * mainBodyHeightMult;
                for (int j = 0; j < height; j++)
                {
                    float normalizedX = ((float)i / (float)(biomeSize * widtMult * 1.0f));
                    float sineCap = 
                        (float)Math.Sin(Math.Sin(Math.Sin(Math.Sin(Math.Sin(
                        ((normalizedX / 2f) + 0.5f) * (float)Math.PI)
                        )))) * 1.5f;

                    float noiseVal = fastnoise.GetCubicFractal(i * mainBodyfreq, (j*1.4f) + RANDOFFSET) * mainBodyamp;
                    float normalizedY = ((float)j / height);

                    float sinh = (float)Math.Sinh(-j + 5.1f) / 5;
                    bool belowHeight = (i + (int)(biomeSize * widtMult) - 5) > sinh && (-i + (int)(biomeSize * widtMult) - 5) > sinh;

                    if (belowHeight && (normalizedY / sineCap) < (1f - (mainBodyamp * 0.5f)) + noiseVal - 0.09f)
                        WorldGen.PlaceTile(biomePosition.X + i, biomePosition.Y + j, ModContent.TileType<Tiles.JadeSand.JadeSandTile>(), true, true);
                    else if(belowHeight && (normalizedY / sineCap) < (1f - (mainBodyamp * 0.5f)) + noiseVal)
                        WorldGen.PlaceTile(biomePosition.X + i, biomePosition.Y + j, ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>(), true, true);

                    //debug
                    //else
                        //WorldGen.PlaceTile(biomePosition.X + i, biomePosition.Y + j, TileID.DiamondGemsparkOff, true, true);
                }
            }

            //return;
            int mainPoolSideBuffer = 20;
            int radius = (biomeSize - mainPoolSideBuffer);

            float freq = 25;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 50f)) / 5);
            float amp = 0.05f;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 36f)) / 2);
            WavyArc(biomePosition, radius, TileID.AmberGemspark, freq, amp, true, widtMult, 2f, placement: PlaceSandBlob);

            SupportSand(new Rectangle(biomePosition.X - (int)(biomeSize * widtMult), biomePosition.Y - biomeSize, (int)(biomeSize * widtMult), (int)(biomeSize * mainBodyHeightMult)));
        }

        public static void PlaceSandBlob(int x, int y)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    //if (j == 1)
                    //    Main.tile[x + i, y + j].Get<TileTypeData>().Type = TileID.AdamantiteBeam;// (ushort)ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>();
                    //else
                        Main.tile[x + i, y + j].Get<TileTypeData>().Type = (ushort)ModContent.TileType<Tiles.JadeSand.JadeSandTile>();
                    Main.tile[x + i, y + j].Get<TileWallWireStateData>().HasTile = true;
                }
            }
        }

        public static void SupportSand(Rectangle worldArea)
        {
            for (int i = worldArea.X; i < worldArea.X + worldArea.Width; i++)
            {
                for (int j = worldArea.Y; j < worldArea.Y + worldArea.Height; j++)
                {
                    if (Main.tile[i, j].TileType == (ushort)TileType<Tiles.JadeSand.JadeSandTile>() && Main.tile[i, j].HasTile && !Main.tile[i, j + 1].HasTile)
                    {
                        Main.tile[i, j].Get<TileTypeData>().Type = (ushort)ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>();
                        Main.tile[i, j].Get<TileWallWireStateData>().HasTile = true;
                    }
                }
            }
        }

        //public static void SupportSand(Rectangle worldArea)
        //{
        //    for (int i = worldArea.X; i < worldArea.X + worldArea.Width; i++)
        //    {
        //        for (int j = worldArea.Y; j < worldArea.Y + worldArea.Height; j++)
        //        {
        //            if (Main.tile[i, j].TileType == (ushort)TileType<Tiles.JadeSand.JadeSandTile>() && Main.tile[i, j].HasTile && !Main.tile[i, j + 1].HasTile)
        //            {
        //                Main.tile[i, j].Get<TileTypeData>().Type = (ushort)ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>();
        //                WorldGen.
        //            }
        //        }
        //    }
        //}

        public static void WavyArc(Point16 centerPoint, int radius, int tileType, float freq, float amp, bool clearInside = true, float widthMult = 1, float increment = 2, float startRadian = 0, float endRadian = (float)Math.PI, Action<int, int>? placement = null)
        {
            bool onplace = placement != null;

            FastNoise fastnoise = new FastNoise(WorldGen.genRand.Next());
            float inc = (float)Math.Tau / (increment * radius * widthMult * (freq / 5f));
            for (float i = 0; i < (float)Math.PI; i += inc)
            {
                Vector2 dist = new(radius * (1 + fastnoise.GetCubic(0, i * radius * freq) * amp), 0);
                Vector2 pos = dist.RotatedBy(i, Vector2.Zero);

                if (clearInside)
                {
                    float clearLen = ((dist.X * widthMult));
                    for (int j = 0; j < clearLen; j++)
                    {
                        float multDist = j / clearLen;
                        int posX = (int)(centerPoint.X + ((pos.X * multDist) * widthMult));
                        int posY = (int)(centerPoint.Y + (pos.Y * multDist));
                        if (Main.tile[posX, posY].TileType != tileType)
                            Main.tile[posX, posY].Get<TileWallWireStateData>().HasTile = false;
                        //WorldGen.PlaceTile((int)(biomePosition.X + ((pos.X * multDist) * widtMult)), (int)(Main.spawnTileY + (pos.Y * multDist)), TileID.AmberGemspark, true, true);
                    }
                }

                if (onplace)
                    placement((int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y));
                else
                {
                    Main.tile[(int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y)].Get<TileTypeData>().Type = (ushort)tileType;
                    Main.tile[(int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y)].Get<TileWallWireStateData>().HasTile = true;
                    //WorldGen.PlaceTile((int)(biomePosition.X + (pos.X * widtMult)), (int)(biomePosition.Y + pos.Y), tileType, true, true);
                }
            }
        }
    }
}
