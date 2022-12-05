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

            //these 2 control the main size and shape of the biome
            int biomeSize = 220;//vary based on world size and randomness
            float biomeWidthMult = 1.2f;//the width/height ratio
            int sideBeachSize = 12;


            float CONST_mainBodyLowerFreq = 3;
            float CONST_mainIslandBottomAmp = 0.5f;
            float CONST_mainIslandBodyHeightMult = 0.9f;

            FastNoise fastnoise = new FastNoise(Main.rand.Next());


            //used for stuff that needs to iterate over the entire biome
            Rectangle WholeBiomeRect = new Rectangle(
                biomeCenter.X - (int)((biomeSize / 2) * biomeWidthMult), 
                biomeCenter.Y - (int)(biomeSize * CONST_mainIslandBodyHeightMult), 
                (int)((biomeSize / 2) * biomeWidthMult) * 2, 
                (int)(biomeSize * CONST_mainIslandBodyHeightMult) * 2);

            //for the main island
            Rectangle LowerIslandRect = new Rectangle(
                biomeCenter.X - (int)((biomeSize / 2) * biomeWidthMult),
                biomeCenter.Y,
                (int)((biomeSize / 2) * biomeWidthMult) * 2,
                (int)(biomeSize * CONST_mainIslandBodyHeightMult));



            //clears all water in biome area
            ClearWater(WholeBiomeRect);
            //FillArea(WholeBiomeRect, TileID.EmeraldGemspark, 0);
            //FillArea(LowerIslandRect, TileID.RubyGemspark, 1);


            float offshootChance = 0.0025f;

            //Offshoot cups above the main cup, categorized by their corner
            List<(Rectangle rect, float amp, int triesLeft)> upperOffshoots = new ();
            List<(Rectangle rect, float amp, int triesLeft)> lowerOffshoots = new ();


            Cup(LowerIslandRect, fastnoise, CONST_mainIslandBottomAmp, CONST_mainBodyLowerFreq, offshootChance, LowerIslandRect.Center().ToPoint16(), 7);



            //generate main hollow area
            {
                float DEBUG_CONST_upperRadiusMult = 1.60f;
                float DEBUG_CONST_upperWidthDiv = 1.59f;
                //settings for main island arc
                float CONST_mainBodyArcFreq = 25;
                float CONST_mainBodyArcAmp = 0.05f;

                //method that gets passed to arc function to be called for every block placed around edge (the actual placing of blocks is disabled)
                void CalcOffshoots(int i, int j, float angle)
                {
                    if (Main.rand.NextFloat() < (0.03f) && (angle < 4f || angle > 5.4))
                    {
                        upperOffshoots.Add((new Rectangle(i, j, LowerIslandRect.Width, LowerIslandRect.Height), CONST_mainIslandBottomAmp, 0));
                    }
                }

                //only for wavybowl
                int radius = (int)((biomeSize / 2) * 0.9f/*side buffer?*/);
                radius = (int)MathHelper.Max(radius, 4);//this makes sure the minimum is 4 (?)


                float ratio = 1f * biomeWidthMult;// (float)WholeBiomeRect.Width / (float)WholeBiomeRect.Height;
                //top
                WavyArc(new Point16(LowerIslandRect.Center.X, LowerIslandRect.Top), (int)(radius * DEBUG_CONST_upperRadiusMult), CONST_mainBodyArcFreq, CONST_mainBodyArcAmp, true, ratio / DEBUG_CONST_upperWidthDiv, 2f, (float)Math.PI, (float)Math.PI * 2, CalcOffshoots);

                //bottom
                WavyArc(new Point16(LowerIslandRect.Center.X, LowerIslandRect.Top), radius - sideBeachSize, CONST_mainBodyArcFreq, CONST_mainBodyArcAmp, true, ratio, 2f);
            }

            CreateOffshoots(LowerIslandRect, CONST_mainIslandBottomAmp, 4, offshootChance, true, lowerOffshoots);

            //creates cups above the main cup
            foreach (var corner in upperOffshoots)
            {
                //continue;
                int direction = Math.Sign(LowerIslandRect.Center().X - corner.rect.X);//orig center - pos

                int newWidth = Main.rand.Next((int)(corner.rect.Width * 0.4f), corner.rect.Width);//old width
                newWidth = (int)(newWidth * 0.35f);
                newWidth = (int)(newWidth * Main.rand.NextFloat(0.65f, 1.45f));

                int newHeight = (int)(corner.rect.Height * 0.5f);//old height
                newHeight = (int)(newHeight * 0.7f);
                newHeight = (int)(newHeight * Main.rand.NextFloat(0.65f, 1.45f));

                float newAmp = corner.amp;
                newAmp *= 0.6f;

                Point16 newPosition = new Point16(corner.rect.X + (direction * newWidth), corner.rect.Y);

                if (Main.rand.NextBool())
                    newPosition = new Point16(newPosition.X - (20 * direction), newPosition.Y);
                else
                    newPosition = new Point16(newPosition.X, newPosition.Y - newHeight);
                var cupArea = new Rectangle(newPosition.X, newPosition.Y, newWidth, newHeight);
                Cup(cupArea, fastnoise, newAmp, CONST_mainBodyLowerFreq, 0.02f, LowerIslandRect.Center().ToPoint16(), 0);
                FillArea(cupArea, TileID.AmethystGemspark, 0);
            }

            //Creates offshoot cups
            while (lowerOffshoots.Count > 0)
            {
                var corner = lowerOffshoots.First();
                lowerOffshoots.Remove(corner);
                {
                    int direction = Math.Sign(LowerIslandRect.Center().X - corner.rect.X);
                    if (!Main.tile[corner.rect.X, corner.rect.Y].HasTile)
                        continue;

                    int newWidth = Main.rand.Next((int)(corner.rect.Width * 0.4f), corner.rect.Width);
                    newWidth = (int)(newWidth * Main.rand.NextFloat(0.65f, 1.45f));

                    int newHeight = (int)(corner.rect.Height * 0.5f);
                    newHeight = (int)(newHeight * Main.rand.NextFloat(0.65f, 1.45f));

                    Point16 newPosition = new Point16(corner.rect.X + (direction * newWidth), corner.rect.Y);

                    if (Main.rand.NextBool())
                        newPosition = new Point16(newPosition.X + (20 * direction), newPosition.Y);
                    else
                        newPosition = new Point16(newPosition.X, newPosition.Y - newHeight);
                    var cupArea = new Rectangle(newPosition.X, newPosition.Y, newWidth, newHeight);
                    FillArea(cupArea, TileID.SapphireGemspark, 0);
                    Cup(cupArea, fastnoise, corner.amp, CONST_mainBodyLowerFreq, 0.04f, LowerIslandRect.Center().ToPoint16(), corner.triesLeft - 1);
                    CreateOffshoots(cupArea, corner.amp, corner.triesLeft - 1, 0.04f, false, lowerOffshoots);
                }
            }

            /*for (int i = 0; i < 8; i++)
            {
                Vector2 offset = new Vector2(0, -biomeSize * Main.rand.NextFloat(0.55f, 1.35f)).RotatedByRandom(1.5f);
                offset.X *= 0.75f;
                float newHeightMult = mainBodyHeightMult * Main.rand.NextFloat(0.65f, 1.75f) * 0.5f;
                Cup(biomePosition + offset.ToPoint16(), (int)(biomeSize * Main.rand.NextFloat(0.15f,0.35f)), widtMult, fastnoise, newHeightMult, mainBodyamp * Main.rand.NextFloat(0.3f,0.6f), mainBodyfreq, RANDOFFSET, 0.0025f, biomePosition, 3);
            }*/

            //places sandstone under floating sand
            SupportSand(WholeBiomeRect);

            //slopes all tiles in biome
            //likely only needed for debug generation since vanilla has this pass
            SlopeTiles(WholeBiomeRect);
        }

        public static void SlopeTiles(Rectangle worldArea)//rename to something else since it reframes
        {
            for (int i = worldArea.X; i < worldArea.X + worldArea.Width; i++)
                for (int j = worldArea.Y; j < worldArea.Y + worldArea.Height; j++)
                {
                    WorldGen.TileFrame(i, j);
                    Tile.SmoothSlope(i, j, false);
                }
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

        public static void FillArea(Rectangle worldArea, int tile, int offset)
        {
            for (int i = worldArea.X; i < worldArea.X + worldArea.Width; i++)
                for (int j = worldArea.Y; j < worldArea.Y + worldArea.Height; j++)
                {
                    if((i + (j % 2) + offset) % 2 == 0)
                        WorldGen.PlaceTile(i, j, tile, true, true);
                }
        }

        //rename to lower
        public static void CreateOffshoots(Rectangle worldArea, float amp, int triesLeft, float chanceToOffshoot, bool originalPosition, List<(Rectangle rect, float amp, int triesLeft)> list)
        {
            int offshootsLeft = triesLeft;
            for (int i = worldArea.X; i < worldArea.X + worldArea.Width; i++)
                for (int j = worldArea.Y; j < worldArea.Y + worldArea.Height; j++)
                {
                    if (Main.tile[i, j].TileType == ModContent.TileType<Tiles.JadeSand.JadeSandTile>() && Main.tile[i, j].HasTile) 
                    {
                        bool generateNew = Main.rand.NextFloat() < MathHelper.Lerp(chanceToOffshoot, -chanceToOffshoot, ((j - worldArea.Y) / worldArea.Height));
                        if (generateNew && offshootsLeft > 0)
                        {
                            WorldGen.PlaceTile(i, j, TileID.DiamondGemspark, true, true);
                            var newWidth = worldArea.Width;
                            var newHeight = worldArea.Height;
                            var newAmp = amp;
                            //lowerOffshoots

                            if (originalPosition)
                            {
                                newWidth = (int)(newWidth * 0.35f);
                                newHeight = (int)(newHeight * 0.7f);
                                newAmp *= 0.6f;
                            }

                            offshootsLeft--;
                            //int direction = Math.Sign(originalPosition.X - (rect.X + i));
                            list.Add((new Rectangle(i, j, newWidth, newHeight), newAmp, triesLeft - 1));
                        }
                    }
                }
        }

        public static void Cup(Rectangle rect, FastNoise fastnoise, float amp, float freq, float chanceToOffshoot, Point16 originalPosition, int triesLeft)
        {
            fastnoise = new FastNoise(WorldGen.genRand.Next());

            int DEBUG_RANDOM_VALUE_INCREMENT = 0;

            bool newPos = !(originalPosition == rect.Center().ToPoint16());


            //Maximum number of offshoot cups allowed to generate.
            int offshootsLeft = triesLeft;

            //generates the wavy pattern on the bottom of the island
            float height = rect.Height;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < (rect.Width); i++)
                {
                    //x in the 0 - 1 range
                    float normalizedX = ((float)i / (float)(rect.Width / 2)) - 1;
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
                    bool belowSideSlopeHeight = (i - 3) > sinh && (-i + (int)(rect.Width) - 5) > sinh;

                    //? (if this should attempt to generate another island?)
                    bool generateNew = false;

                    //places water if below below a certain threshold. and skips everything below on second iterations (?)
                    if ((normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - 0.25f)
                    {
                        //if (Main.rand.NextBool(3))
                        //    Main.tile[position.X + i, position.Y + j].LiquidAmount = 255;
                        if (newPos)
                            continue;
                    }

                    //skip placing sand or sandstone if this is...?
                    if (Main.tile[rect.X + i, rect.Y + j].HasTile && (Main.tile[rect.X + i, rect.Y + j].TileType == ModContent.TileType<JadeSandTile>()))
                    {
                        continue;
                    }

                    //placement of sand and sandstone if below a certain threshold, continued from water placement
                    if (belowSideSlopeHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - 0.15f)
                    {
                        WorldGen.PlaceTile(rect.X + i, rect.Y + j, ModContent.TileType<Tiles.JadeSand.JadeSandTile>(), true, true);
                        generateNew = Main.rand.NextFloat()/*Debug:make genrand later*/ < MathHelper.Lerp(chanceToOffshoot, -chanceToOffshoot, normalizedY);//???
                    }
                    else if (belowSideSlopeHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal)
                        WorldGen.PlaceTile(rect.X + i, rect.Y + j, ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>(), true, true);

                    //Store a point to create an offshoot off of the larger cup
                    //if (generateNew && triesLeft > 0 && offshootsLeft > 0)
                    //{
                    //    offshootsLeft--;
                    //    int direction = Math.Sign(originalPosition.X - (rect.X + i));
                    //    lowerOffshoots.Add(new Point16(rect.X + i, rect.Y + j));
                    //}
                }
            }

            //deletes amber gemsparks
            //for (int i = -(int)(size * widtMult); i < (size * widtMult); i++)
            //{
            //    float height2 = size * heightMult;
            //    for (int j = (int)-height2; j < height2; j++)
            //    {
            //        if (Main.tile[position.X + i, position.Y + j].HasTile && Main.tile[position.X + i, position.Y + j].TileType == TileID.AmberGemspark)
            //        {
            //            Main.tile[position.X + i, position.Y + j].Get<TileWallWireStateData>().HasTile = false;
            //        }
            //    }
            //}

            //return;//debug
            //int CONST_maxTotalUpperOffshoots = 20;
            //int CONST_maxTotalLowerOffshoots = 20;

            //if (upperOffshoots.Count > CONST_maxTotalUpperOffshoots)
            //{
            //    int a = upperOffshoots.Count;
            //    for (int j = 0; j < a - CONST_maxTotalUpperOffshoots; j++)
            //        upperOffshoots.RemoveAt(Main.rand.Next(upperOffshoots.Count));
            //}

            //if (lowerOffshoots.Count > CONST_maxTotalLowerOffshoots)
            //{
            //    int a = lowerOffshoots.Count;
            //    for (int j = 0; j < a - CONST_maxTotalLowerOffshoots; j++)
            //        lowerOffshoots.RemoveAt(Main.rand.Next(lowerOffshoots.Count));
            //}
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
        public static void WavyArc(Point16 centerPoint, int radius, float freq, float amp, bool clearInside = true, float widthMult = 1, float increment = 2, float startRadian = 0, float endRadian = (float)Math.PI, Action<int, int, float>? placement = null)
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
                    for (float j = 0; j < clearLen; j+= 0.75f)
                    {
                        float multDist = j / clearLen;
                        int posX = (int)(centerPoint.X + ((pos.X * multDist) * widthMult));
                        int posY = (int)(centerPoint.Y + (pos.Y * multDist));
                        //if (Main.tile[posX, posY].TileType != tileType)
                            Main.tile[posX, posY].Get<TileWallWireStateData>().HasTile = false;
                        //if (!(Main.tile[posX, posY].TileType == ModContent.TileType<JadeSandTile>() || Main.tile[posX, posY].TileType == ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>()))
                        //{
                        //    Main.tile[posX, posY].Get<TileTypeData>().Type = (ushort)tileType;
                        //    Main.tile[posX, posY].Get<TileWallWireStateData>().HasTile = true;
                        //}

                        // WorldGen.PlaceTile((int)(biomePosition.X + ((pos.X * multDist) * widtMult)), (int)(Main.spawnTileY + (pos.Y * multDist)), TileID.AmberGemspark, true, true);
                    }
                }


                //passed in method
                if (onplace)
                    placement((int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y), i);

                //disabled since this isnt needed to place tiles
                //else
                //{
                //    //Main.tile[(int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y)].Get<TileTypeData>().Type = (ushort)tileType;
                //    //Main.tile[(int)(centerPoint.X + (pos.X * widthMult)), (int)(centerPoint.Y + pos.Y)].Get<TileWallWireStateData>().HasTile = true;
                //    //WorldGen.PlaceTile((int)(biomePosition.X + (pos.X * widtMult)), (int)(biomePosition.Y + pos.Y), tileType, true, true);
                //}
            }
        }
    }
}
