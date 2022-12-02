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
            
            //Debug
            Main.spawnTileX = Main.maxTilesX / 2;
            Main.spawnTileY = Main.maxTilesY / 3;
            Main.worldSurface = Main.spawnTileY;

            //very center of biome, used as origin for main arc raycasts
            Point16 biomeCenter = new Point16(Main.spawnTileX, Main.spawnTileY);

            int biomeSize = 111;//vary based on world size and randomness
            float CONST_biomeWidthMult = 1.3f;//the width/height ratio


            float CONST_mainBodyLowerFreq = 3;
            float CONST_mainBodyLowerAmp = 0.5f;
            float CONST_mainBodyLowerHeightMult = 1.8f;

            FastNoise fastnoise = new FastNoise(Main.rand.Next());

            float offshootChance = 0.0025f;

            //used for stuff that needs to iterate over the entire biome
            Rectangle MainBiomeRect = new Rectangle(biomeCenter.X - (int)(biomeSize * CONST_biomeWidthMult), biomeCenter.Y - biomeSize, (int)(biomeSize * CONST_biomeWidthMult), (int)(biomeSize * CONST_mainBodyLowerHeightMult));

            //clears all water in biome area
            ClearWater(MainBiomeRect);

            Cup(biomeCenter, biomeSize, CONST_biomeWidthMult, fastnoise, CONST_mainBodyLowerHeightMult, CONST_mainBodyLowerAmp, CONST_mainBodyLowerFreq, offshootChance, biomeCenter, 7);

            /*for (int i = 0; i < 8; i++)
            {
                Vector2 offset = new Vector2(0, -biomeSize * Main.rand.NextFloat(0.55f, 1.35f)).RotatedByRandom(1.5f);
                offset.X *= 0.75f;
                float newHeightMult = mainBodyHeightMult * Main.rand.NextFloat(0.65f, 1.75f) * 0.5f;
                Cup(biomePosition + offset.ToPoint16(), (int)(biomeSize * Main.rand.NextFloat(0.15f,0.35f)), widtMult, fastnoise, newHeightMult, mainBodyamp * Main.rand.NextFloat(0.3f,0.6f), mainBodyfreq, RANDOFFSET, 0.0025f, biomePosition, 3);
            }*/

            //places sandstone under floating sand
            SupportSand(MainBiomeRect);

            //slopes all tiles in biome
            //likely only needed for debug generation since vanilla has this pass
            SlopeTiles(MainBiomeRect);
        }

        public static void SlopeTiles(Rectangle worldArea)
        {
            for (int i = worldArea.X; i < worldArea.X + worldArea.Width; i++)
                for (int j = worldArea.Y; j < worldArea.Y + worldArea.Height; j++)
                    Tile.SmoothSlope(i, j);
        }

        /// <summary>
        /// Places sandstone under floating sand
        /// </summary>
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

        public static void ClearWater(Rectangle worldArea)
        {
            for (int i = worldArea.X; i < worldArea.X + worldArea.Width; i++)
                for (int j = worldArea.Y; j < worldArea.Y + worldArea.Height; j++)
                    Main.tile[i, j].LiquidAmount = 0;
        }

        public static void Cup(Point16 position, int size, float widtMult, FastNoise fastnoise, float heightMult, float amp, float freq, float chanceToOffshoot, Point16 originalPosition, int triesLeft)
        {
            fastnoise = new FastNoise(WorldGen.genRand.Next());
            List<Point16> offshoots = new List<Point16>();

            int DEBUG_RANDOM_VALUE_INCREMENT = 0;
            int mainPoolSideBuffer = 20;

            if (position != originalPosition)
                mainPoolSideBuffer = 12;

            int radius = (int)((size - mainPoolSideBuffer) * 0.9f/*side buffer?*/);

            //this makes sure the minimum is 4 (?)
            radius = (int)MathHelper.Max(radius, 4);

            //settings for main island arc
            float CONST_mainBodyArcFreq = 25;
            float CONST_mainBodyArcAmp = 0.05f;

            //?
            List<Point16> offshoots2 = new List<Point16>();

            //generate main hollow area
            if (position == originalPosition)
            {
                float DEBUG_CONST_upperradiusmult = 2f;
                float DEBUG_CONST_upperwidthmult = 1.65f;

                offshoots2 = new List<Point16>();
                //method that gets passed to arc function to be called for every block placed around edge (the actual placing of blocks is disabled)
                void CalcOffshoots(int i, int j)
                {
                    if (Main.rand.NextFloat() < ((position == originalPosition) ? 0.02f : 0) && (i < 4f || i > 5.4))
                        offshoots2.Add(new Point16(i, j));
                }
                //bottom
                WavyArc(position, radius, TileID.AmberGemspark, CONST_mainBodyArcFreq, CONST_mainBodyArcAmp, true, widtMult, 2f);

                //top
                WavyArc(position, (int)(radius * DEBUG_CONST_upperradiusmult), TileID.AmberGemspark, CONST_mainBodyArcFreq, CONST_mainBodyArcAmp, true, widtMult / DEBUG_CONST_upperwidthmult, 2f, (float)Math.PI, (float)Math.PI * 2, CalcOffshoots);
            }

            //?
            float height = size * heightMult;
            //?
            int offshootsLeft = triesLeft;

            //generates the wavy pattern on the bottom of the island
            for (int j = 0; j < height; j++)
            {
                for (int i = -(int)(size * widtMult); i < (size * widtMult); i++)
                {
                    //x in the 0 - 1 range
                    float normalizedX = ((float)i / (float)(size * widtMult));
                    //y in the 0 - 1 range
                    float normalizedY = ((float)j / height);

                    //creates the creates a semi-circle like shape to be multiplied with the noise in order to make sure there is falloff towards the edges
                    float sineCap =
                        (float)Math.Sin(Math.Sin(Math.Sin(Math.Sin(Math.Sin(
                        ((normalizedX / 2f) + 0.5f) * (float)Math.PI)
                        )))) * 1.5f;

                    //gets the random value (uses debug value right now to make sure the seed is somewhat random between generations)
                    float noiseVal = fastnoise.GetCubicFractal(i * freq, (j * 1.4f) + DEBUG_RANDOM_VALUE_INCREMENT) * amp;

                    //checks if below a threshold, creates sloped edges on top of main island
                    float sinh = (float)Math.Sinh(-j + 5.1f) / 5;
                    bool belowHeight = (i + (int)(size * widtMult) - 5) > sinh && (-i + (int)(size * widtMult) - 5) > sinh;

                    //? (if this should attempt to generate another island?)
                    bool generateNew = false;

                    //places water if below below a certain threshold. and skips everything below on second iterations (?)
                    if ((normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - 0.25f)
                    {
                        if (Main.rand.NextBool(3))
                            Main.tile[position.X + i, position.Y + j].LiquidAmount = 255;
                        if (position != originalPosition)
                            continue;
                    }

                    //skip placing sand or sandstone if this is...?
                    if (Main.tile[position.X + i, position.Y + j].HasTile && (Main.tile[position.X + i, position.Y + j].TileType == TileID.AmberGemspark || Main.tile[position.X + i, position.Y + j].TileType == ModContent.TileType<JadeSandTile>()))
                    {
                        continue;
                    }

                    //placement of sand and sandstone if below a certain threshold, continued from water placement
                    if (belowHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - 0.15f)
                    {
                        WorldGen.PlaceTile(position.X + i, position.Y + j, ModContent.TileType<Tiles.JadeSand.JadeSandTile>(), true, true);
                        generateNew = Main.rand.NextFloat()/*Debug:make genrand later*/ < MathHelper.Lerp(chanceToOffshoot, -chanceToOffshoot, normalizedY);//???
                    }
                    else if (belowHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal)
                        WorldGen.PlaceTile(position.X + i, position.Y + j, ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>(), true, true);

                    //?
                    if (generateNew && triesLeft > 0 && offshootsLeft > 0)
                    {
                        offshootsLeft--;
                        int direction = Math.Sign(originalPosition.X - (position.X + i));
                        offshoots.Add(new Point16(position.X + i, position.Y + j));
                    }
                }
            }

            //deletes amber gemsparks?
            for (int i = -(int)(size * widtMult); i < (size * widtMult); i++)
            {
                float height2 = size * heightMult;
                for (int j = (int)-height2; j < height2; j++)
                {
                    if (Main.tile[position.X + i, position.Y + j].HasTile && Main.tile[position.X + i, position.Y + j].TileType == TileID.AmberGemspark)
                    {
                        Main.tile[position.X + i, position.Y + j].Get<TileWallWireStateData>().HasTile = false;
                    }
                }
            }

            foreach (Point16 corner2 in offshoots2)
            {
                int direction = Math.Sign(originalPosition.X - corner2.X);

                int newRadius = Main.rand.Next((int)(size * 0.4f), size);
                float newHeightMult = heightMult * Main.rand.NextFloat(0.65f, 1.45f);
                float newAmp = amp;

                newRadius = (int)(newRadius * 0.35f);
                newHeightMult *= 0.7f;
                newAmp *= 0.6f;

                Point16 newPosition = new Point16(corner2.X + (direction * newRadius), corner2.Y);

                if (Main.rand.NextBool())
                    newPosition = new Point16(newPosition.X - (20 * direction), newPosition.Y);
                else
                    newPosition = new Point16(newPosition.X, newPosition.Y - (int)(newRadius * newHeightMult * 0.5f));
                Cup(newPosition, newRadius, widtMult * Main.rand.NextFloat(0.65f, 1.45f), fastnoise, newHeightMult, newAmp, freq, 0.02f, originalPosition, 0);
                DEBUG_RANDOM_VALUE_INCREMENT += Main.rand.Next(1000, 10000);//debug
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
                    newAmp *= 0.6f;
                }

                Point16 newPosition = new Point16(corner.X + (direction * newRadius), corner.Y);

                if (Main.rand.NextBool())
                    newPosition = new Point16(newPosition.X + (20 * direction), newPosition.Y);
                else
                    newPosition = new Point16(newPosition.X, newPosition.Y - (int)(newRadius * newHeightMult * 0.5f));
                Cup(newPosition, newRadius, widtMult * Main.rand.NextFloat(0.65f, 1.45f), fastnoise, newHeightMult, newAmp, freq, 0.04f, originalPosition, triesLeft - 1);
                DEBUG_RANDOM_VALUE_INCREMENT += Main.rand.Next(1000, 10000);//debug
            }
        }

        /// <param name="tileType">unused</param>
        /// <param name="freq">frequency of noise</param>
        /// <param name="amp">amplitude of noise</param>
        /// <param name="clearInside"> if the center should be hollowed out</param>
        /// <param name="widthMult">multiplies the x axis</param>
        /// <param name="increment">multiplier for amount of steps</param>
        /// <param name="startRadian">start angle of circle</param>
        /// <param name="endRadian">end angle of circle</param>
        /// <param name="placement">method to be ran once for each angle</param>
        public static void WavyArc(Point16 centerPoint, int radius, int tileType, float freq, float amp, bool clearInside = true, float widthMult = 1, float increment = 2, float startRadian = 0, float endRadian = (float)Math.PI, Action<int, int>? placement = null)
        {
            //if there is a passed in method to be run on each step
            bool onplace = placement != null;

            FastNoise fastnoise = new FastNoise(Main.rand.Next());//debug change rand later

            //step length
            float inc = (float)Math.Tau / (increment * radius * widthMult * (freq / 5f));

            for (float i = startRadian; i < endRadian; i += inc)
            {
                //distance from center
                Vector2 dist = new(radius * (1 + fastnoise.GetCubic(0, i * radius * freq) * amp), 0);
                //rotates distance to get position
                Vector2 pos = dist.RotatedBy(i, Vector2.Zero);

                //clearing inside
                if (clearInside)
                {
                    //raycasts from the middle
                    float clearLen = ((dist.X * widthMult));
                    for (float j = 0; j < clearLen; j+= 0.5f)
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

                //passed in method
                if (onplace)
                    placement((int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y));

                //disabled since this isnt needed to place tiles
                //else
                //{
                //    //Main.tile[(int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y)].Get<TileTypeData>().Type = (ushort)tileType;
                //    //Main.tile[(int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y)].Get<TileWallWireStateData>().HasTile = true;
                //    //WorldGen.PlaceTile((int)(biomePosition.X + (pos.X * widtMult)), (int)(biomePosition.Y + pos.Y), tileType, true, true);
                //}
            }
            /*foreach(Point16 pos in offshoots)
            {
                int posX = pos.X;
                int posY = pos.Y;
                int direction = Math.Sign(centerPoint.X - posX);
                int newRadius = (int)(radius * Main.rand.NextFloat(0.35f, 0.55f));
                float newHeightMult = Main.rand.NextFloat(0.65f, 1.75f) * 0.5f;
                Cup(new Point16(posX + (int)(direction * newRadius * 0.5f), posY), newRadius, Main.rand.NextFloat(0.85f, 1.35f), fastnoise, newHeightMult, Main.rand.NextFloat(0.15f, 0.3f), 3, 3, 0.025f, centerPoint, 3);
            }*/
        }
    }
}
