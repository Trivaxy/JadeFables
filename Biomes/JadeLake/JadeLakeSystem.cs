using JadeFables.Dusts;
using JadeFables.Tiles.HardenedJadeSand;
using JadeFables.Tiles.JadeSand;
using JadeFables.Tiles.JadeSandstone;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;
using JadeFables.Tiles.SpringChest;
using JadeFables.Items.SpringChestLoot.FireworkPack;
using JadeFables.Items.SpringChestLoot.TanookiLeaf;
using JadeFables.Items.SpringChestLoot.Gong;
using JadeFables.Tiles.JadeFountain;
using Terraria.ModLoader;
using JadeFables.Items.Potions.Heartbeat;
using JadeFables.Items.Potions.Spine;
using JadeFables.Items.Potions.JasmineTea;
using JadeFables.Items.SpringChestLoot.Hwacha;
using JadeFables.Items.Jade.FestivalLantern;

namespace JadeFables.Biomes.JadeLake
{
    public class JadeLakeSystem : ModSystem
    {
        public int JadeSandTileCount;
        public int JadeSandstoneTileCount;
        public int HardenedJadeSandTileCount;
        public int TotalBiomeCount => JadeSandstoneTileCount + JadeSandTileCount + HardenedJadeSandTileCount;

        /// <summary>
        /// Whether or not the visual effects applied to water in the Jade Biome will be active, regardless
        /// of if the player is currently in the Jade Biome or not.
        /// </summary>
        public bool forceLakeAesthetic;
        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            JadeSandTileCount = tileCounts[TileType<JadeSandTile>()];
            HardenedJadeSandTileCount = tileCounts[TileType<HardenedJadeSandTile>()];
            JadeSandstoneTileCount = tileCounts[TileType<JadeSandstoneTile>()];
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            //int CleanupIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Gems In Ice Biome"));
            int CleanupIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Tile Cleanup"));
            int EndIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));

            tasks.Insert(CleanupIndex, new PassLegacy("Jade Spring", JadeLakeWorldGen.SurfaceItemPass));
            tasks.Insert(EndIndex, new PassLegacy("Jade Spring 2", JadeLakeWorldGen.PolishPass));

            //debug
            //tasks.RemoveAll(x => x.Name != "Jade Spring");
        }
        bool pressed = false;
        public override void Load()
        {
            Terraria.On_Main.DoDraw += AddLighting;
        }

        private void AddLighting(Terraria.On_Main.orig_DoDraw orig, Main self, GameTime gameTime) {
            orig(self, gameTime);

            float progress = MathHelper.Min(TotalBiomeCount, 3000) / 3000f;

            if (TotalBiomeCount == 0 && !forceLakeAesthetic)
                return;

            progress = forceLakeAesthetic ? 1f : progress;

            WaterLight.waterPoolTiles.Clear();
            WaterLight.waterDepthTiles.Clear();

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                if (Vector2.DistanceSquared(Main.LocalPlayer.Center, new Vector2(x * 16f, Main.LocalPlayer.Center.Y)) < (Main.screenWidth / 2) * (Main.screenWidth / 2))
                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Water) {
                            float modifiedProgress = MathHelper.Min(TotalBiomeCount, 3000);
                            modifiedProgress = forceLakeAesthetic ? 3000f : modifiedProgress;

                            if (Main.tile[x, y - 1].LiquidAmount <= 0 && !Main.tile[x, y - 1].HasTile)
                            {
                                WaterLight.waterPoolTiles.Add((x, y));
                            }
                            else WaterLight.waterDepthTiles.Add((x, y));
                        }
                    }
            }
        }

        public override void ResetNearbyTileEffects() {
            forceLakeAesthetic = false;
        }

        public override void PostUpdateEverything()
        {
            /*if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemCloseBrackets))
            {
                if (!pressed)
                {
                    pressed = true;

                    for (int i = 400; i < Main.maxTilesX - 400; i++)
                        for (int j = 100; j < Main.maxTilesY - 400; j++)
                        {
                            //Main.tile[i, j].ClearTile();
                            Main.tile[i, j].Get<TileTypeData>().Type = TileID.Stone;
                            Main.tile[i, j].Get<TileWallWireStateData>().HasTile = true;
                        }

                    JadeLakeWorldGen.SurfaceItemPass(new GenerationProgress(), default);
                    JadeLakeWorldGen.PolishPass(new GenerationProgress(), default);
                    PopulateChests();
                    Main.NewText("regened");
                }
            }
            else
                pressed = false;*/

            float progress = MathHelper.Min(TotalBiomeCount, 3000) / 3000f;
            if (TotalBiomeCount == 0 && !forceLakeAesthetic)
                return;

            progress = forceLakeAesthetic ? 1f : progress;

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                if (Vector2.DistanceSquared(Main.LocalPlayer.Center, new Vector2(x * 16f, Main.LocalPlayer.Center.Y)) < (Main.screenWidth / 2) * (Main.screenWidth / 2))
                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Water)
                        {
                            if (Main.tile[x, y - 1].LiquidAmount <= 0 && !Main.tile[x, y - 1].HasTile)
                            {
                                if (Main.rand.NextBool((int)(30 / progress)))
                                {
                                    Vector2 velocity = Vector2.UnitY.RotatedByRandom(0.1f) * -Main.rand.NextFloat(1f, 1.5f);
                                    Dust.NewDustPerfect(new Vector2(x * 16 + Main.rand.Next(-8, 8), y * 16 + 8), DustID.Smoke, velocity, 240, default, Main.rand.NextFloat(1f, 2f));
                                    for (int i = 0; i < 3; i++)
                                        Dust.NewDustPerfect(new Vector2(x * 16 + Main.rand.Next(-8, 8), y * 16 + 8), ModContent.DustType<SpringMist>(), velocity, 0, Color.White, Main.rand.NextFloat(0.25f, 1.25f));

                                    if (Main.rand.NextBool((int)(40 / progress)))
                                        Dust.NewDustPerfect(new Vector2(x * 16, y * 16 + 8), DustType<Dusts.JadeBubble>(), -Vector2.UnitY.RotatedByRandom(1f) * 2, 0, default, Main.rand.NextFloat(0.5f, 1f));
                                }
                            }

                            Color color = Lighting.GetColor(new Point(x, y));

                            float avg = color.R + color.G + color.B;
                            avg /= 255;
                            avg /= 3;
                            if (Main.rand.NextBool((int)(2000 / progress)) && avg > 0.3f)
                                Dust.NewDustPerfect(new Vector2(x * 16, y * 16 + 8), DustType<Dusts.WhiteSparkle>(), Vector2.Zero, 0, default, 0.5f);
                        }
                    }
            }
        }

        public override void PostWorldGen()
        {
            PopulateChests();
        }

        public static void PopulateChests()
        {
            int[] primaryLoot = new int[] { ModContent.ItemType<FireworkPack>(), ModContent.ItemType<TanookiLeaf>(), ModContent.ItemType<GongItem>(), ModContent.ItemType<Items.SpringChestLoot.Chopsticks.Chopsticks>(),ModContent.ItemType<Hwacha>()};
            int[] secondaryLoot = new int[] { ModContent.ItemType<JadeFountainItem>(), ItemID.MagicConch, ItemID.SandcastleBucket, ModContent.ItemType<FestivalLantern>()};

            int[] ternaryLoot = new int[] {
                ModContent.ItemType<Tiles.JadeTorch.JadeTorch>(),
                ItemID.Rope,
                ItemID.BouncyBomb,
                ItemID.SwiftnessPotion,
                ItemID.RegenerationPotion,
                ItemID.GoldBar,
                ItemID.PlatinumBar,
                ItemID.LuckPotionLesser,
                ItemID.LesserHealingPotion, };

            int[] ternaryLootRare = new int[] {
                ItemID.MagicPowerPotion,
                ItemID.TeleportationPotion,
                ItemID.HealingPotion,
                ItemID.LuckPotion,
                ModContent.ItemType<JasmineTea>(),
                ModContent.ItemType<HeartbeatPotion>(),
                ModContent.ItemType<SpinePotion>()};

            int[] ternaryLootSingle = new int[]
            {
                ItemID.SuspiciousLookingEye,
                ItemID.AngelStatue
            };
            for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
            {
                List<int> ternaryLootList = ternaryLoot.ToList();
                List<int> ternaryLootRareList = ternaryLootRare.ToList();
                List<int> ternaryLootSingleList = ternaryLootSingle.ToList();
                Chest chest = Main.chest[chestIndex];
                if (chest != null && Main.tile[chest.x, chest.y].TileType/*.frameX == 47 * 36*/ == ModContent.TileType<SpringChest>()) // if glass chest
                {
                    int primaryLootChoice = chestIndex % primaryLoot.Length;
                    chest.item[0].SetDefaults(primaryLoot[primaryLootChoice]);

                    int secondaryLootChoice = Main.rand.Next(secondaryLoot.Length);
                    chest.item[1].SetDefaults(secondaryLoot[secondaryLootChoice]);

                    int slotsToFill = WorldGen.genRand.Next(5, 8);
                    for (int inventoryIndex = 2; inventoryIndex < slotsToFill; inventoryIndex++)
                    {
                        if (inventoryIndex == slotsToFill - 1)
                        {
                            chest.item[inventoryIndex].SetDefaults(ItemID.GoldCoin);
                            chest.item[inventoryIndex].stack = Main.rand.Next(1, 5);
                        }
                        else if (Main.rand.NextBool(5))
                        {
                            int type = ternaryLootRareList[Main.rand.Next(ternaryLootRareList.Count)];
                            ternaryLootRareList.Remove(type);
                            chest.item[inventoryIndex].SetDefaults(type);
                            chest.item[inventoryIndex].stack = Main.rand.Next(2, 5);
                        }
                        else if (Main.rand.NextBool(6) && ternaryLootSingleList.Count > 0)
                        {
                            int type = ternaryLootSingleList[Main.rand.Next(ternaryLootSingleList.Count)];
                            ternaryLootSingleList.Remove(type);
                            chest.item[inventoryIndex].SetDefaults(type);
                            chest.item[inventoryIndex].stack = 1;
                        }
                        else
                        {
                            int type = ternaryLootList[Main.rand.Next(ternaryLootList.Count)];
                            ternaryLootList.Remove(type);
                            chest.item[inventoryIndex].SetDefaults(type);
                            if (chest.item[inventoryIndex].type == ItemID.Rope)
                            {
                                chest.item[inventoryIndex].stack = Main.rand.Next(30, 50);
                            }
                            else
                                chest.item[inventoryIndex].stack = Main.rand.Next(2, 6);
                        }
                    }
                }
            }
        }
    }
    public class WaterLight : GlobalWall
    {
        public static HashSet<(int, int)> waterPoolTiles = new();
        public static HashSet<(int, int)> waterDepthTiles = new();

        public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
        {
            if (waterPoolTiles.Count <= 0) return;

            if (waterPoolTiles.Contains((i, j)) || waterDepthTiles.Contains((i,j)))
            {
                JadeLakeSystem jadeLakeSystem = GetInstance<JadeLakeSystem>();

                float modifiedProgress = MathHelper.Min(jadeLakeSystem.TotalBiomeCount, 3000);
                modifiedProgress = jadeLakeSystem.forceLakeAesthetic ? 3000f : modifiedProgress;

                Color color = new Color(0, 220, 200);
                float brightness = 0.9f;
                if (waterDepthTiles.Contains((i, j))) brightness = 0.175f;
                brightness *= modifiedProgress / 3000f;

                if (jadeLakeSystem.TotalBiomeCount == 0)
                {
                    waterPoolTiles.Clear();
                    waterDepthTiles.Clear();
                }

                r += color.R / (255f / brightness);
                g += color.G / (255f / brightness);
                b += color.B / (255f / brightness);
            }
        }
    }
}
