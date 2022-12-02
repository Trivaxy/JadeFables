using JadeFables.Dusts;
using JadeFables.Helpers.FastNoise;
using JadeFables.Tiles.JadeSand;
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
    internal static class JadeLakeWorldGen
    {
        public static void SurfaceItemPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Progress message jade springs placeholder";

            Main.spawnTileX = Main.maxTilesX / 2;
            Main.spawnTileY = Main.maxTilesY / 3;
            Main.worldSurface = Main.spawnTileY;

            Point16 biomePosition = new Point16(Main.spawnTileX, Main.spawnTileY);

            int biomeSize = 111;
            float widtMult = 1.3f;

            //float freq = 25;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 50f)) / 5);
            //float amp = 0.05f;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 36f)) / 2);

            float mainBodyfreq = 3;
            float mainBodyamp = 0.5f;
            float mainBodyHeightMult = 1.8f;

            //float mainBodyfreq = 3;
            //float mainBodyamp = 0.4f;
            //float mainBodyHeightMult = 1.8f;for fractal
            FastNoise fastnoise = new FastNoise(Main.rand.Next());

            int RANDOFFSET = (int)Main.GameUpdateCount * 3;//debug

            float offshootChance = 0.0025f;

            for (int i = -(int)(biomeSize * widtMult); i < (biomeSize * widtMult); i++)
            {
                float height = biomeSize * mainBodyHeightMult;
                for (int j = 0; j < height; j++)
                {
                    Main.tile[biomePosition.X + i, biomePosition.Y + j].LiquidAmount = 0;
                }
            }

            Cup(biomePosition, biomeSize, widtMult, fastnoise, mainBodyHeightMult, mainBodyamp, mainBodyfreq, RANDOFFSET, offshootChance, biomePosition, 7);

            for (int i = -(int)(biomeSize * widtMult); i < (biomeSize * widtMult); i++)
            {
                float height = biomeSize * mainBodyHeightMult;
                for (int j = 0; j < height; j++)
                {
                    Tile.SmoothSlope(biomePosition.X + i, biomePosition.Y + j);
                }
            }

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

        public static void Cup(Point16 position, int size, float widtMult, FastNoise fastnoise, float heightMult, float amp, float freq, int randoffset, float chanceToOffshoot, Point16 originalPosition, int triesLeft)
        {
            fastnoise = new FastNoise(Main.rand.Next());
            List<Point16> offshoots = new List<Point16>();

            int mainPoolSideBuffer = 20;

            if (position != originalPosition)
                mainPoolSideBuffer = 12;

            int radius = (int)((size - mainPoolSideBuffer) * 0.9f);

            radius = (int)MathHelper.Max(radius, 4);

            float freq2 = 25;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 50f)) / 5);
            float amp2 = 0.05f;// * ((2 + (float)Math.Sin(Main.GameUpdateCount / 36f)) / 2);
            if (position == originalPosition)
                WavyArc(position, radius, TileID.AmberGemspark, freq2, amp2, true, widtMult, 2f);

            int offshootsLeft = triesLeft;
            float height = size * heightMult;
            for (int j = 0; j < height; j++)
            {
                for (int i = -(int)(size * widtMult); i < (size * widtMult); i++)
                {
                    float normalizedX = ((float)i / (float)(size * widtMult * 1.0f));
                    float sineCap =
                        (float)Math.Sin(Math.Sin(Math.Sin(Math.Sin(Math.Sin(
                        ((normalizedX / 2f) + 0.5f) * (float)Math.PI)
                        )))) * 1.5f;

                    float noiseVal = fastnoise.GetCubicFractal(i * freq, (j * 1.4f) + randoffset) * amp;
                    float normalizedY = ((float)j / height);

                    float sinh = (float)Math.Sinh(-j + 5.1f) / 5;
                    bool belowHeight = (i + (int)(size * widtMult) - 5) > sinh && (-i + (int)(size * widtMult) - 5) > sinh;

                    bool generateNew = false;
                    if ((normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - 0.25f)
                    {
                        if (Main.rand.NextBool(3))
                            Main.tile[position.X + i, position.Y + j].LiquidAmount = 255;
                        if (position != originalPosition)
                            continue;
                    }

                    if (Main.tile[position.X + i, position.Y + j].HasTile && (Main.tile[position.X + i, position.Y + j].TileType == TileID.AmberGemspark || Main.tile[position.X + i, position.Y + j].TileType == ModContent.TileType<JadeSandTile>()))
                        continue;
                    if (belowHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - 0.15f)
                    {
                        WorldGen.PlaceTile(position.X + i, position.Y + j, ModContent.TileType<Tiles.JadeSand.JadeSandTile>(), true, true);
                        generateNew = Main.rand.NextFloat() < MathHelper.Lerp(chanceToOffshoot, -chanceToOffshoot, normalizedY);
                    }
                    else if (belowHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal)
                        WorldGen.PlaceTile(position.X + i, position.Y + j, ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>(), true, true);

                    if (generateNew && triesLeft > 0 && offshootsLeft > 0)
                    {
                        offshootsLeft--;
                        int direction = Math.Sign(originalPosition.X - (position.X + i));
                        offshoots.Add(new Point16(position.X + i, position.Y + j));
                    }
                    //debug
                    //else
                    //WorldGen.PlaceTile(biomePosition.X + i, biomePosition.Y + j, TileID.DiamondGemsparkOff, true, true);
                }
            }

            for (int i = -(int)(size * widtMult); i < (size * widtMult); i++)
            {
                float height2 = size * heightMult;
                for (int j = 0; j < height2; j++)
                {
                    if (Main.tile[position.X + i, position.Y + j].HasTile && Main.tile[position.X + i, position.Y + j].TileType == TileID.AmberGemspark)
                    {
                        Main.tile[position.X + i, position.Y + j].Get<TileWallWireStateData>().HasTile = false;
                    }
                }
            }

            foreach (Point16 corner in offshoots)
            {
                int direction = Math.Sign(originalPosition.X - corner.X);
                if (!Main.tile[corner.X, corner.Y].HasTile)
                    continue;


                int newRadius = Main.rand.Next((int)(size * 0.4f), size);
                float newHeightMult = heightMult * Main.rand.NextFloat(0.65f, 1.45f);
                float newAmp = amp;
                if (originalPosition == position)
                {
                    newRadius = (int)(newRadius * 0.35f);
                    newHeightMult *= 0.7f;
                    newAmp *= 0.4f;
                }

                Point16 newPosition = new Point16(corner.X + (direction * newRadius), corner.Y);

                if (Main.rand.NextBool())
                    newPosition = new Point16(newPosition.X + (20 * direction), newPosition.Y);
                else
                    newPosition = new Point16(newPosition.X, newPosition.Y - (int)(newRadius * newHeightMult * 0.5f));
                Cup(newPosition, newRadius, widtMult * Main.rand.NextFloat(0.65f, 1.45f), fastnoise, newHeightMult, newAmp, freq, randoffset, 0.04f, originalPosition, triesLeft - 1);
            }
        }

        public static void WavyArc(Point16 centerPoint, int radius, int tileType, float freq, float amp, bool clearInside = true, float widthMult = 1, float increment = 2, float startRadian = 0, float endRadian = (float)Math.PI, Action<int, int>? placement = null)
        {
            bool onplace = placement != null;

            FastNoise fastnoise = new FastNoise(Main.rand.Next());
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
                        //if (Main.tile[posX, posY].TileType != tileType)
                        //     Main.tile[posX, posY].Get<TileWallWireStateData>().HasTile = false;
                        if (!(Main.tile[posX, posY].TileType == ModContent.TileType<JadeSandTile>() || Main.tile[posX, posY].TileType == ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>()))
                        {
                            Main.tile[posX, posY].Get<TileTypeData>().Type = (ushort)tileType;
                            Main.tile[posX, posY].Get<TileWallWireStateData>().HasTile = true;
                        }
                        // WorldGen.PlaceTile((int)(biomePosition.X + ((pos.X * multDist) * widtMult)), (int)(Main.spawnTileY + (pos.Y * multDist)), TileID.AmberGemspark, true, true);
                    }
                }

                /*if (onplace)
                    placement((int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y));
                else
                {
                    Main.tile[(int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y)].Get<TileTypeData>().Type = (ushort)tileType;
                    Main.tile[(int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y)].Get<TileWallWireStateData>().HasTile = true;
                    //WorldGen.PlaceTile((int)(biomePosition.X + (pos.X * widtMult)), (int)(biomePosition.Y + pos.Y), tileType, true, true);
                }*/
            }
        }
    }
}
