using JadeFables.Dusts;
using JadeFables.Helpers.FastNoise;
using JadeFables.Tiles.JadeSand;
using JadeFables.Tiles.JadeWaterfall;
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
            int biomeSize = 180;//vary based on world size and randomness
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


            //for the upper islands
            Rectangle UpperIslandRect = new Rectangle(
                biomeCenter.X - (int)((biomeSize / 1.7f) * biomeWidthMult),
                biomeCenter.Y - (int)((biomeSize / 1.7f) * CONST_mainIslandBodyHeightMult),
                (int)((biomeSize / 1.7f) * biomeWidthMult) * 2,
                (int)((biomeSize / 1.9f) * CONST_mainIslandBodyHeightMult));





            //clears all water in biome area
            ClearWater(WholeBiomeRect);
            //FillArea(WholeBiomeRect, TileID.EmeraldGemspark, 0);
            FillArea(LowerIslandRect, TileID.RubyGemspark, 1);//lower hitbox
            FillArea(UpperIslandRect, TileID.EmeraldGemspark, 0);//upper hitbox

            Cup(LowerIslandRect, fastnoise, CONST_mainIslandBottomAmp, CONST_mainBodyLowerFreq, 0.35f, false);

            //generate main hollow area
            {
                float DEBUG_CONST_upperRadiusMult = 1.60f;
                float DEBUG_CONST_upperWidthDiv = 1.59f;
                //settings for main island arc
                float CONST_mainBodyArcFreq = 25;
                float CONST_mainBodyArcAmp = 0.05f;

                //method that gets passed to arc function to be called for every block placed around edge (the actual placing of blocks is disabled)
                //void CalcOffshoots(int i, int j, float angle)
                //{
                //    if (Main.rand.NextFloat() < (0.03f) && (angle < 4f || angle > 5.4))
                //    {
                //        upperOffshoots.Add((new Rectangle(i, j, LowerIslandRect.Width, LowerIslandRect.Height), CONST_mainIslandBottomAmp, 0));
                //    }
                //}

                //only for wavybowl
                int radius = (int)((biomeSize / 2) * 0.9f/*side buffer?*/);
                radius = (int)MathHelper.Max(radius, 4);//this makes sure the minimum is 4 (?)


                float ratio = 1f * biomeWidthMult;// (float)WholeBiomeRect.Width / (float)WholeBiomeRect.Height;
                //top half circle clear
                WavyArc(new Point16(LowerIslandRect.Center.X, LowerIslandRect.Top), (int)(radius * DEBUG_CONST_upperRadiusMult), CONST_mainBodyArcFreq, CONST_mainBodyArcAmp, true, ratio / DEBUG_CONST_upperWidthDiv, 2f, (float)Math.PI, (float)Math.PI * 2);

                //bottom half circle clear
                WavyArc(new Point16(LowerIslandRect.Center.X, LowerIslandRect.Top), radius - sideBeachSize, CONST_mainBodyArcFreq, CONST_mainBodyArcAmp, true, ratio, 2f, waterLevel: 128);
            }

            //lower and upper platforms/pools
            {
                //lower pools
                var list1 = AddIslands(LowerIslandRect, (int)(LowerIslandRect.Width * 0.33f), (int)(LowerIslandRect.Height * 0.21f), WholeBiomeRect, fastnoise, CONST_mainIslandBottomAmp, CONST_mainBodyLowerFreq * 1.3f, loopbackCount: 2, MinAdd: 3, chance: 0.00015f, 2, CONST_sizeVariation: 0.225f, 1.1f, avoidCollide: true, deleteCollide: true);
                GenerateWallPillars(list1, biomeSize / 3);

                //upper platforms and pools
                var list2 = AddIslands(UpperIslandRect, (int)(LowerIslandRect.Width * 0.38f), (int)(LowerIslandRect.Height * 0.22f), UpperIslandRect, fastnoise, CONST_mainIslandBottomAmp, CONST_mainBodyLowerFreq * 1.3f, loopbackCount: 1, MinAdd: 5, chance: 0.00065f, 1, CONST_sizeVariation: 0.225f, 0.15f, avoidCollide: false, deleteCollide: true);
                GenerateWallPillars(list2, biomeSize / 3);
            }

            //connecting caves
            {
                //side caves
                float size = biomeSize / 24;
                int len = biomeSize / 12;

                //bottom left side cave
                GenerateCave(LowerIslandRect.X + (int)size, (LowerIslandRect.Y - (int)size) + 2,
                    size, (float)Math.PI / 2, len, 2f, 20f, fastnoise, removeWalls: true);

                //bottom right side cave
                GenerateCave(LowerIslandRect.X + LowerIslandRect.Width - (int)size, (LowerIslandRect.Y - (int)size) + 2,
                    size, -(float)Math.PI / 2, len, 4f, 20f, fastnoise, removeWalls: true);



                float size2 = biomeSize / 18;
                int len2 = biomeSize / 8;

                //top left side cave
                GenerateCave(
                    LowerIslandRect.X + (int)size2 * 7,
                    (UpperIslandRect.Y - (int)size2 * 2) + 2,
                    size2, (float)Math.PI / 2, len2, 1.2f, 20f, fastnoise);

                //top right side cave
                GenerateCave(
                    LowerIslandRect.X + LowerIslandRect.Width - (int)size2 * 7,
                    (UpperIslandRect.Y - (int)size2 * 2) + 2,
                    size2, -(float)Math.PI / 2, len2, 1.2f, 20f, fastnoise);
            }

            //center upper platforms
            {
                Rectangle PlatformArea = LowerIslandRect;
                PlatformArea.Inflate(-(int)(LowerIslandRect.Width * 0.32f), -(int)(LowerIslandRect.Height * 0.24f));
                PlatformArea.Y -= (int)(LowerIslandRect.Height * 0.60f);

                Rectangle smallerPlat = LowerIslandRect;
                smallerPlat.Inflate(-(int)(LowerIslandRect.Width * 0.39f), -(int)(LowerIslandRect.Height * 0.39f));
                //smallerPlat.Y -= (int)(LowerIslandRect.Height * 0.35f);

                List<(Rectangle pos, bool water)> platformList = AddIslands(PlatformArea, smallerPlat.Width, smallerPlat.Height, PlatformArea, fastnoise, CONST_mainIslandBottomAmp * 1.6f, CONST_mainBodyLowerFreq * 2f, loopbackCount: 1, MinAdd: 5, chance: 0.0025f, 0, CONST_sizeVariation: 0.05f, 0.20f, avoidCollide: false, deleteCollide: true, true);
                //Platform(smallerPlat, fastnoise, CONST_mainIslandBottomAmp * 2, CONST_mainBodyLowerFreq * 2, 0f, LowerIslandRect.Center().ToPoint16(), 7);

                //FillArea(PlatformArea, TileID.SapphireGemspark, 0);

                GenerateWallPillars(platformList, biomeSize / 3);
            }


            //places sandstone under floating sand
            SupportSand(WholeBiomeRect);

            //slopes all tiles in biome
            //likely only needed for debug generation since vanilla has this pass
            SlopeTiles(WholeBiomeRect);
            

            //Places foreground waterfalls (has to be outside of polish pass because it only does the top half of the biome)
            PlaceForegroundWaterfalls(UpperIslandRect, 700);
        }

        public static void GenerateWallPillars(List<(Rectangle pos, bool water)> islandList, int maxPillarDistance)
        {
            foreach(var (pos, water) in islandList)
            {
                Point center = pos.Center;
                int scanStartOffset = -10000;
                int scanEndOffset = 0;

                //finds start of pillar
                bool hasSkippedStartingEmptySpace = !Main.tile[center.X, center.Y].HasTile;
                for (int g = -5; g < 20; g++)//moves start point down to bottom of island
                {
                    if (!Main.tile[center.X, center.Y + g].HasTile)
                    {
                        if (hasSkippedStartingEmptySpace)
                        {
                            scanStartOffset = g;
                            break;
                        }
                    }
                    else
                        hasSkippedStartingEmptySpace = true;
                }

                if (scanStartOffset == -10000)//if there was no valid tile found to move it up to do no generate pillar
                    continue;
                bool foundEnd = false;
                //finds end of pillar
                for (int j = scanStartOffset; j < maxPillarDistance; j++)
                {
                    if (Main.tile[center.X, center.Y + j].HasTile)
                    {
                        scanEndOffset = j;//goes slightly past end
                        foundEnd = true;
                        break;
                    }
                }

                if (foundEnd)
                {
                    const int MaxPillarWidth = 5;
                    int pillarBottomHeight = center.Y + scanEndOffset;
                    int pillarTopHeight = center.Y + scanStartOffset;

                    Point leftBottomSide = new Point(center.X, pillarBottomHeight);
                    Point rightBottomSide = new Point(center.X, pillarBottomHeight);
                    //righttopside
                    //lefttopside

                    //finds edges/width
                    {
                        //right side
                        for (int h = 0; h < MaxPillarWidth; h++)
                        {
                            if (FindGroundTile(center.X + h, pillarBottomHeight, MaxPillarWidth + 1, out int offset1))
                            {
                                rightBottomSide = new Point(center.X + h, pillarBottomHeight + offset1);
                            }
                            else
                                break;
                        }

                        //left side
                        for (int h = 0; h < MaxPillarWidth; h++)
                        {
                            if (FindGroundTile(center.X - h, pillarBottomHeight, MaxPillarWidth + 1, out int offset2))
                            {
                                leftBottomSide = new Point(center.X - h, pillarBottomHeight + offset2);
                            }
                            else
                                break;//if no valid tile is found it stops searching, so the last valid tile (or starting one) is used
                        }
                    }

                    //debug
                    //for (int j = scanStartOffset; j < scanEndOffset; j++)
                    //{
                    //    WorldGen.PlaceTile(center.X, center.Y + j, TileID.LunarOre, true, true);
                    //}

                    //actually generate it
                }
            }
        }

        public static bool FindGroundTile(int i, int j, int limit, out int offset)
        {
            for (int g = limit; g > -limit; g--)
            {
                if (Main.tile[i, j + g].HasTile && !Main.tile[i, (j + g) - 1].HasTile)
                {
                    offset = g;
                    return true;
                }
            }
            offset = 0;
            return false;
        }

        public static bool FindCeilingTile(int i, int j, int limit, out int offset)
        {
            for (int g = -limit; g < limit; g++)
            {
                if (Main.tile[i, j + g].HasTile && !Main.tile[i, (j + g) + 1].HasTile)
                {
                    offset = g;
                    return true;
                }
            }
            offset = 0;
            return false;
        }


        public static void GenerateCave(int posX, int posY, float size, float direction, int steps, float amp, float freq, FastNoise fastnoise, bool leaveSand = true, bool removeWalls = false)
        {
            int lastPosX = posX;
            int lastPosY = posY;
            Action<int, int> func = leaveSand ? 
                (int i, int j) => {
                    if (
                        Main.tile[i, j].TileType != ModContent.TileType<Tiles.JadeSand.JadeSandTile>() &&
                        Main.tile[i, j].TileType != ModContent.TileType<Tiles.HardenedJadeSand.HardenedJadeSandTile>()/* &&
                        Main.tile[i, j].TileType != ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>()*/)
                    {
                        WorldGen.KillTile(i, j, false, false, true);
                        if (removeWalls)
                            WorldGen.KillWall(i, j, false);
                    }
                } : 
                (int i, int j) => { 
                    WorldGen.KillTile(i, j, false, false, true);
                    if(removeWalls)
                        WorldGen.KillWall(i, j, false);
                };

            for (int h = 0; h < steps; h++)
            {
                float scale = size * (-((float)h / steps) + 1);                
                CircleGen(lastPosX, lastPosY, scale, func);

                float mul = fastnoise.GetCubicFractal(h * freq, 0) * amp;
                Vector2 dir = Vector2.UnitY.RotatedBy(direction + mul) * scale;
                lastPosX += (int)dir.X;
                lastPosY += (int)dir.Y;
            }
        }

        public static void CircleGen(int posX, int posY, float size, Action<int, int> iterateMethod)
        {
            for (int i = -(int)size; i < (int)size; i++)
            {
                for (int j = -(int)size; j < (int)size; j++)
                {
                    if (Vector2.Distance(new Vector2(i, j), Vector2.Zero) < size && WorldGen.InWorld(posX + i, posY + j))
                    {
                        iterateMethod(posX + i, posY + j);
                    }
                }
            }
        }

        //could return a list or do stuff here
        public static List<(Rectangle pos, bool water)> AddIslands(Rectangle target, int startSizeX, int startSizeY, Rectangle mainMax, FastNoise noise, float CONST_mainIslandBottomAmp, float CONST_mainBodyLowerFreq, int loopbackCount, int MinAdd, float chance, int collsionType, float CONST_sizeVariation = 0.225f, float waterChance = 0.25f, bool avoidCollide = false, bool deleteCollide = true, bool platform = false)
        {
            var generatedList = new List<(Rectangle pos, bool water)>();
            int CONST_MultPerLoop = 1;
            float CONST_RepeatSizeMult = 0.82f;

            (List<Rectangle> list, int xSize, int ySize) SecondPoolsCurrent =
                new(new List<Rectangle>(), (int)(startSizeX), (int)(startSizeY));

            (List<Rectangle> list, int xSize, int ySize) SecondPoolsPrevious = new(new List<Rectangle>(), 5, 5);


            int escape = 0;
            for (int h = 0; h < loopbackCount; h++)
            {
                while (SecondPoolsCurrent.list.Count < MinAdd + (h * CONST_MultPerLoop) && escape < 300)
                {
                    escape++;
                    //iterate over biome
                    for (int i = target.X; i < target.X + target.Width; i++)
                        for (int j = target.Y; j < target.Y + target.Height; j++)
                        {
                            //if chance
                            if (Main.rand.NextFloat() < chance)
                            {
                                {
                                    //create rectangle centered on point
                                    Rectangle size = new Rectangle(i, j, 0, 0);

                                    float randomMult = 1 + Main.rand.NextFloat(-CONST_sizeVariation, CONST_sizeVariation);
                                    //size.Inflade adds the value to both sides of the center
                                    size.Inflate((int)((SecondPoolsCurrent.xSize / 2) * randomMult), (int)((SecondPoolsCurrent.ySize / 2) * randomMult));

                                    bool validSpace = false;

                                    if (collsionType == 1)//makes sure either both left corners or both right corners + center are empty
                                    {
                                        validSpace = ((Main.tile[size.X, size.Y].HasTile && Main.tile[size.X, size.Y + size.Height].HasTile) ||
                                            (Main.tile[size.X + size.Width, size.Y].HasTile && Main.tile[size.X + size.Width, size.Y + size.Height].HasTile))
                                            && !Main.tile[size.X + (size.Width / 2), size.Y + (size.Height / 2)].HasTile;
                                    }
                                    else if (collsionType == 2)//makes sure top 2 corners + center are empty
                                    {
                                        validSpace = (Main.tile[size.X, size.Y].HasTile || Main.tile[size.X + size.Width, size.Y].HasTile) 
                                            && !Main.tile[size.X + (size.Width / 2), size.Y + (size.Height / 2)].HasTile;
                                    }
                                    else if (collsionType == 0)//makes sure all 4 corners + center are empty
                                    {
                                        validSpace = ((
                                            !Main.tile[size.X, size.Y].HasTile && 
                                            !Main.tile[size.X, size.Y + size.Height].HasTile && 
                                            !Main.tile[size.X + size.Width, size.Y].HasTile && 
                                            !Main.tile[size.X + size.Width, size.Y + size.Height].HasTile))
                                           && !Main.tile[size.X + (size.Width / 2), size.Y + (size.Height / 2)].HasTile;
                                    }

                                    if (validSpace)
                                    {
                                        SecondPoolsCurrent.list.Add(size);//adds pool to the list
                                        //FillArea(size, TileID.TopazGemsparkOff + h, h);
                                    }
                                }
                            }
                        }
                }

                //make them avoid eachother
                if (avoidCollide)
                {
                    //move colliding ones away from eachother (old list and new list)
                    for (int y = 0; y < SecondPoolsCurrent.list.Count; y++)
                    {
                        for (int u = 0; u < SecondPoolsPrevious.list.Count; u++)
                        {
                            var poolRect = SecondPoolsCurrent.list[y];
                            var poolRect2 = SecondPoolsPrevious.list[u];

                            if (y != u)
                            {
                                if (poolRect.Intersects(poolRect2))
                                {
                                    Vector2 dir = Vector2.Normalize(poolRect.Center.ToVector2() - poolRect2.Center.ToVector2()) * (new Vector2(SecondPoolsCurrent.xSize, SecondPoolsCurrent.ySize) / 4);
                                    Point newPos1 = (poolRect.TopLeft() + dir).ToPoint();
                                    Point newPos2 = (poolRect2.TopLeft() - dir).ToPoint();
                                    //var len = dir.Length() * 2;
                                    //for (float l = 0; l < len; l += 0.3f)
                                    //{
                                    //    var pos = Vector2.Lerp(newPos1.ToVector2(), newPos2.ToVector2(), l / len);
                                    //    WorldGen.PlaceTile((int)pos.X - DEBUG_CONST_X_OFFSET, (int)pos.Y, TileID.ArgonMoss, true, true);
                                    //    WorldGen.PlaceTile((int)pos.X - DEBUG_CONST_X_OFFSET, (int)pos.Y-1, TileID.ArgonMoss, true, true);
                                    //    WorldGen.PlaceTile((int)pos.X - DEBUG_CONST_X_OFFSET, (int)pos.Y + 1, TileID.ArgonMoss, true, true);
                                    //}
                                    //WorldGen.PlaceTile((int)newPos1.X - DEBUG_CONST_X_OFFSET, (int)newPos1.Y, TileID.XenonMoss, true, true);
                                    //WorldGen.PlaceTile((int)newPos1.X - DEBUG_CONST_X_OFFSET, (int)newPos1.Y - 1, TileID.ArgonMoss, true, true);
                                    //WorldGen.PlaceTile((int)newPos2.X - DEBUG_CONST_X_OFFSET, (int)newPos2.Y, TileID.XenonMoss, true, true);
                                    //WorldGen.PlaceTile((int)newPos2.X - DEBUG_CONST_X_OFFSET, (int)newPos2.Y - 1, TileID.ArgonMoss, true, true);
                                    SecondPoolsCurrent.list[y] = new Rectangle(newPos1.X, newPos1.Y, poolRect.Width, poolRect.Height);
                                    SecondPoolsPrevious.list[u] = new Rectangle(newPos2.X, newPos2.Y, poolRect2.Width, poolRect2.Height);
                                }
                            }
                        }
                    }

                    //move colliding ones away from eachother (new list only)
                    for (int y = 0; y < SecondPoolsCurrent.list.Count; y++)
                    {
                        for (int u = 0; u < SecondPoolsCurrent.list.Count; u++)
                        {
                            var poolRect = SecondPoolsCurrent.list[y];
                            var poolRect2 = SecondPoolsCurrent.list[u];

                            if (y != u)
                            {
                                if (poolRect.Intersects(poolRect2))
                                {
                                    Vector2 dir = Vector2.Normalize(poolRect.Center.ToVector2() - poolRect2.Center.ToVector2()) * (new Vector2(SecondPoolsCurrent.xSize, SecondPoolsCurrent.ySize) / 6);
                                    Point newPos1 = (poolRect.TopLeft() + dir).ToPoint();
                                    Point newPos2 = (poolRect2.TopLeft() - dir).ToPoint();
                                    //var len = dir.Length() * 2;
                                    //for (float l = 0; l < len; l += 0.3f)
                                    //{
                                    //    var pos = Vector2.Lerp(newPos1.ToVector2(), newPos2.ToVector2(), l / len);
                                    //    WorldGen.PlaceTile((int)pos.X - DEBUG_CONST_X_OFFSET, (int)pos.Y, TileID.ArgonMoss, true, true);
                                    //    WorldGen.PlaceTile((int)pos.X - DEBUG_CONST_X_OFFSET, (int)pos.Y - 1, TileID.ArgonMoss, true, true);
                                    //    WorldGen.PlaceTile((int)pos.X - DEBUG_CONST_X_OFFSET, (int)pos.Y + 1, TileID.ArgonMoss, true, true);
                                    //}
                                    //WorldGen.PlaceTile((int)newPos1.X - DEBUG_CONST_X_OFFSET, (int)newPos1.Y, TileID.XenonMoss, true, true);
                                    //WorldGen.PlaceTile((int)newPos1.X - DEBUG_CONST_X_OFFSET, (int)newPos1.Y - 1, TileID.ArgonMoss, true, true);
                                    //WorldGen.PlaceTile((int)newPos2.X - DEBUG_CONST_X_OFFSET, (int)newPos2.Y, TileID.XenonMoss, true, true);
                                    //WorldGen.PlaceTile((int)newPos2.X - DEBUG_CONST_X_OFFSET, (int)newPos2.Y - 1, TileID.ArgonMoss, true, true);
                                    SecondPoolsCurrent.list[y] = new Rectangle(newPos1.X, newPos1.Y, poolRect.Width, poolRect.Height);
                                    SecondPoolsCurrent.list[u] = new Rectangle(newPos2.X, newPos2.Y, poolRect2.Width, poolRect2.Height);
                                }
                            }
                        }
                    }
                }
                //delete overlapping
                if(deleteCollide)
                {
                    //prev and cur
                    {
                        int y = 0;
                        while (y < SecondPoolsCurrent.list.Count)
                        {
                            bool inc = true;
                            for (int u = 0; u < SecondPoolsPrevious.list.Count; u++)
                            {
                                var poolRectCur = SecondPoolsCurrent.list[y];
                                var poolRectPrev = SecondPoolsPrevious.list[u];

                                if (y != u)
                                {
                                    if (poolRectPrev.Contains(poolRectCur.Center))
                                    {
                                        //FillArea(new Rectangle(poolRectCur.X, poolRectCur.Y, poolRectCur.Width, poolRectCur.Height), TileID.TopazGemspark + h + 1, h);
                                        SecondPoolsCurrent.list.Remove(poolRectCur);
                                        inc = false;
                                        break;
                                    }
                                }
                            }

                            if (inc)
                                y++;
                            else
                                y = 0;
                        }
                    }

                    //cur and cur
                    {
                        int y = 0;
                        while (y < SecondPoolsCurrent.list.Count)
                        {
                            bool inc = true;
                            for (int u = 0; u < SecondPoolsCurrent.list.Count; u++)
                            {
                                var poolRect = SecondPoolsCurrent.list[y];
                                var poolRect2 = SecondPoolsCurrent.list[u];

                                if (y != u)
                                {
                                    if (poolRect.Contains(poolRect2.Center))
                                    {
                                        //FillArea(new Rectangle(poolRect.X, poolRect.Y, poolRect.Width, poolRect.Height), TileID.TopazGemsparkOff + h, h);
                                        SecondPoolsCurrent.list.Remove(poolRect);
                                        inc = false;
                                        break;
                                    }
                                }
                            }
                            if (inc)
                                y++;
                            else if (y > 0)
                                y--;
                        }
                    }
                }

                int skipped = 0;
                foreach (Rectangle poolRect in SecondPoolsCurrent.list)
                {
                    if (mainMax.Contains(poolRect.Center) && !poolRect.Contains(mainMax.Center))
                    {
                        Rectangle rect = poolRect;

                        //makes size larger if some got deleted, only for upper
                        //TODO: try for islands too
                        if (collsionType == 1 && (SecondPoolsCurrent.list.Count - skipped) < MinAdd)
                        {
                            float mul = (MinAdd - (SecondPoolsCurrent.list.Count - skipped));
                            rect.Inflate((int)(rect.Width * (0.025f * mul)), (int)(rect.Height * (0.05f * mul)));
                        }

                        bool water = Main.rand.NextFloat() < waterChance;
                        if (platform)
                        {
                            Platform(rect, noise, CONST_mainIslandBottomAmp, CONST_mainBodyLowerFreq);
                            generatedList.Add((rect, false));
                        }
                        else
                        {
                            Cup(rect, noise, CONST_mainIslandBottomAmp, CONST_mainBodyLowerFreq, water ? 0.35f : 0.55f, true, water);
                            generatedList.Add((rect, true));
                        }
                        //FillArea(new Rectangle(poolRect.X - (DEBUG_CONST_X_OFFSET - 0), poolRect.Y, poolRect.Width, poolRect.Height), TileID.TopazGemsparkOff + h, h);
                    }
                    else
                        skipped++;
                }


                SecondPoolsPrevious = SecondPoolsCurrent;
                float randomSizeMult = Main.rand.NextFloat(0.96f, 1.03f);
                SecondPoolsCurrent =
                    new(new List<Rectangle>(), (int)(SecondPoolsPrevious.xSize * CONST_RepeatSizeMult * randomSizeMult), (int)(SecondPoolsPrevious.ySize * CONST_RepeatSizeMult * randomSizeMult));
            }
            return generatedList;
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

        public static bool AnyEmptyTileSurround(int i, int j)
        {
            for (int x = -1; x < 2; x++)
                for (int y = -1; y < 2; y++)
                {
                    if (!Main.tile[i + x, j + y].HasTile)
                        return true;
                }
            return false;
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

        public static void PlaceForegroundWaterfalls(Rectangle rect, int chance)
        {
            int[] validTiles = new int[] { ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>(), ModContent.TileType<Tiles.HardenedJadeSand.HardenedJadeSandTile>(), ModContent.TileType<JadeSandTile>() };
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top; j < rect.Top + rect.Height; j++)
                {
                    Tile leftTile = Framing.GetTileSafely(i, j);
                    Tile rightTile = Framing.GetTileSafely(i + 1, j);
                    if (!leftTile.HasTile || !rightTile.HasTile)
                        continue;
                    if (validTiles.Contains(leftTile.TileType) && validTiles.Contains(rightTile.TileType))
                    {
                        if (WorldGen.genRand.NextBool(chance))
                        {
                            leftTile.TileType = (ushort)ModContent.TileType<JadeWaterfallTile>();
                            i += 15;
                        }
                    }
                }
            }
        }

        public static void Cup(Rectangle rect, FastNoise fastnoise, float amp, float freq, float depthScale = 0.25f, bool clearTop = false, bool water = false)
        {
            fastnoise = new FastNoise(Main.rand.Next(0, 1000000));
            const int waterLevel = 255;

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
                    float noiseVal = fastnoise.GetCubicFractal(i * freq, (j * 1.4f)) * amp;

                    //checks if below a threshold, creates sloped edges on top of main island
                    float sinh = (float)Math.Sinh(-j + 5.1f) / 5;
                    bool belowSideSlopeHeight = (i - 3) > sinh && (-i + (int)(rect.Width) - 5) > sinh;

                    //places water if below below a certain threshold. and skips everything below on second iterations (?)
                    if ((normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - depthScale)
                    {
                        if (water)//Main.rand.NextBool(3))
                            Main.tile[rect.X + i, rect.Y + j].LiquidAmount = waterLevel;
                        if (clearTop)
                            continue;
                    }

                    //skip placing if this is jade sand
                    //TODO: also check for hardened sand (isnt isnt super important)
                    if (Main.tile[rect.X + i, rect.Y + j].HasTile && (Main.tile[rect.X + i, rect.Y + j].TileType == ModContent.TileType<JadeSandTile>()))
                    {
                        continue;
                    }

                    //placement of sand and sandstone if below a certain threshold, continued from water placement
                    if (belowSideSlopeHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - (depthScale / 2))
                    {
                        WorldGen.PlaceTile(rect.X + i, rect.Y + j, ModContent.TileType<Tiles.JadeSand.JadeSandTile>(), true, true);
                    }
                    else if (belowSideSlopeHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - (depthScale / 4))
                    {
                        WorldGen.PlaceTile(rect.X + i, rect.Y + j, ModContent.TileType<Tiles.HardenedJadeSand.HardenedJadeSandTile>(), true, true);
                        //generateNew = Main.rand.NextFloat()/*Debug:make genrand later*/ < MathHelper.Lerp(chanceToOffshoot, -chanceToOffshoot, normalizedY);//???
                    }
                    else if (belowSideSlopeHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal)
                        WorldGen.PlaceTile(rect.X + i, rect.Y + j, ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>(), true, true);
                }
            }
        }

        public static void Platform(Rectangle rect, FastNoise fastnoise, float amp, float freq)
        {
            fastnoise = new FastNoise(WorldGen.genRand.Next());

            //generates the wavy pattern on the bottom of the island
            for (int j = 0; j < rect.Height; j++)
            {
                for (int i = 0; i < (rect.Width); i++)
                {
                    //x in the 0 - 1 range
                    float normalizedX = ((float)i / (float)(rect.Width / 2)) - 1;
                    //y in the 0 - 1 range
                    float normalizedY = ((float)j / rect.Height);

                    //creates the creates a semi-circle like shape to be multiplied with the noise in order to make sure there is falloff towards the edges
                    float sineCap =
                        (float)Math.Sin(Math.Sin(Math.Sin(Math.Sin(Math.Sin(
                        ((normalizedX / 2f) + 0.5f) * (float)Math.PI)
                        )))) * 1.5f;

                    //gets the random value (uses debug value right now to make sure the seed is somewhat random between generations)
                    float noiseVal = fastnoise.GetCubicFractal(i * freq, (j * 1.4f)) * amp;

                    //checks if below a threshold, creates sloped edges on top of main island
                    float sinh = (float)Math.Sinh(-j + 5.1f) / 5;
                    bool belowSideSlopeHeight = (i - 3) > sinh && (-i + (int)(rect.Width) - 5) > sinh;

                    //places water if below below a certain threshold. and skips everything below on second iterations (?)
                    if ((normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - 0.25f)
                    {
                        //if (Main.rand.NextBool(3))
                        //    Main.tile[rect.X + i, rect.Y + j].LiquidAmount = 255;
                    }

                    //skip placing sand or sandstone if this is...?
                    if (Main.tile[rect.X + i, rect.Y + j].HasTile && (Main.tile[rect.X + i, rect.Y + j].TileType == ModContent.TileType<JadeSandTile>()))
                        continue;

                    //placement of sand and sandstone if below a certain threshold, continued from water placement
                    if (belowSideSlopeHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - 0.14f)
                        WorldGen.PlaceTile(rect.X + i, rect.Y + j, ModContent.TileType<Tiles.JadeSand.JadeSandTile>(), true, true);
                    else if (belowSideSlopeHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal - 0.07f)
                        WorldGen.PlaceTile(rect.X + i, rect.Y + j, ModContent.TileType<Tiles.HardenedJadeSand.HardenedJadeSandTile>(), true, true);
                    else if (belowSideSlopeHeight && (normalizedY / sineCap) < (1f - (amp * 0.5f)) + noiseVal)
                        WorldGen.PlaceTile(rect.X + i, rect.Y + j, ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>(), true, true);
                }
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
        public static void WavyArc(Point16 centerPoint, int radius, float freq, float amp, bool clearInside = true, float widthMult = 1, float increment = 2, float startRadian = 0, float endRadian = (float)Math.PI, byte waterLevel = 0, Action<int, int, float>? placement = null)
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
                        WorldGen.KillWall(posX, posY);
                        Main.tile[posX, posY].LiquidAmount = waterLevel;
                        //Main.tile[posX, posY].LiquidType = 0;

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
