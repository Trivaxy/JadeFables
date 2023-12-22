using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using JadeFables.Dusts;

namespace JadeFables.Items.Lotus.LotusHammer
{
    public class LotusHammer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;
            Item.hammer = 30;
            Item.useTurn = true;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 5;
            Item.knockBack = 5f;
            Item.crit = 4;

            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.White;

            Item.UseSound = SoundID.Item1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<LotusFiber.LotusFiber>(10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class LotusHammerWall : GlobalWall
    {

        public static Dictionary<int, int> dropForWallType = new() //The following unsafe walls already drop when broken in vanilla: Dungeon, Hive, Lihzahrd
        {
            { WallID.DirtUnsafe, ItemID.DirtWall },
            { WallID.DirtUnsafe1, ItemID.Dirt1Echo },
            { WallID.DirtUnsafe2, ItemID.Dirt2Echo },
            { WallID.DirtUnsafe3, ItemID.Dirt3Echo },
            { WallID.DirtUnsafe4, ItemID.Dirt4Echo },
            { WallID.GrassUnsafe, ItemID.GrassWall },
            { WallID.FlowerUnsafe, ItemID.FlowerWall },
            { WallID.CaveUnsafe, ItemID.Cave1Echo },
            { WallID.Cave2Unsafe, ItemID.Cave2Echo },
            { WallID.Cave3Unsafe, ItemID.Cave3Echo },
            { WallID.Cave4Unsafe, ItemID.Cave4Echo },
            { WallID.Cave5Unsafe, ItemID.Cave5Echo },
            { WallID.Cave6Unsafe, ItemID.Cave6Echo },
            { WallID.Cave7Unsafe, ItemID.Cave7Echo },
            { WallID.Cave8Unsafe, ItemID.Cave8Echo },
            { WallID.CaveWall, ItemID.CaveWall1Echo }, //i love you terraria naming. these were not hard to miss at all!
            { WallID.CaveWall2, ItemID.CaveWall2Echo },
            { WallID.RocksUnsafe1, ItemID.Rocks1Echo },
            { WallID.RocksUnsafe2, ItemID.Rocks2Echo },
            { WallID.RocksUnsafe3, ItemID.Rocks3Echo },
            { WallID.RocksUnsafe4, ItemID.Rocks4Echo },
            { WallID.AmethystUnsafe, ItemID.AmethystEcho },
            { WallID.TopazUnsafe, ItemID.TopazEcho },
            { WallID.SapphireUnsafe, ItemID.SapphireEcho },
            { WallID.EmeraldUnsafe, ItemID.EmeraldEcho },
            { WallID.RubyUnsafe, ItemID.RubyEcho },
            { WallID.DiamondUnsafe, ItemID.DiamondEcho },
            { WallID.GraniteUnsafe, ItemID.GraniteWall },
            { WallID.MarbleUnsafe, ItemID.MarbleWall },
            { WallID.HardenedSand, ItemID.HardenedSandWall }, //hardened sand/sandstone is not labelled as unsafe but doesn't break when destroyed
            { WallID.Sandstone, ItemID.SandstoneWall },
            { WallID.CorruptionUnsafe1, ItemID.Corruption1Echo },
            { WallID.CorruptionUnsafe2, ItemID.Corruption2Echo },
            { WallID.CorruptionUnsafe3, ItemID.Corruption3Echo },
            { WallID.CorruptionUnsafe4, ItemID.Corruption4Echo },
            { WallID.CorruptGrassUnsafe, ItemID.CorruptGrassEcho },
            { WallID.EbonstoneUnsafe, ItemID.EbonstoneEcho },
            { WallID.CorruptSandstone, ItemID.CorruptSandstoneWall },
            { WallID.CorruptHardenedSand, ItemID.CorruptHardenedSandWall },
            { WallID.CrimsonUnsafe1, ItemID.Crimson1Echo },
            { WallID.CrimsonUnsafe2, ItemID.Crimson2Echo },
            { WallID.CrimsonUnsafe3, ItemID.Crimson3Echo },
            { WallID.CrimsonUnsafe4, ItemID.Crimson4Echo },
            { WallID.CrimsonGrassUnsafe, ItemID.CrimsonGrassEcho }, //this wall uses the dirt mine sound and not the grass one in vanilla lol
            { WallID.CrimstoneUnsafe, ItemID.CrimstoneEcho },
            { WallID.CrimsonSandstone, ItemID.CrimsonSandstoneWall },
            { WallID.CrimsonHardenedSand, ItemID.CrimsonHardenedSandWall },
            { WallID.HallowUnsafe1, ItemID.Hallow1Echo },
            { WallID.HallowUnsafe2, ItemID.Hallow2Echo },
            { WallID.HallowUnsafe3, ItemID.Hallow3Echo },
            { WallID.HallowUnsafe4, ItemID.Hallow4Echo },
            { WallID.HallowedGrassUnsafe, ItemID.HallowedGrassEcho },
            { WallID.PearlstoneBrickUnsafe, ItemID.PearlstoneEcho }, //this is not Pearlstone Brick, just Pearlstone
            { WallID.HallowSandstone, ItemID.HallowSandstoneWall },
            { WallID.HallowHardenedSand, ItemID.HallowHardenedSandWall },
            { WallID.HellstoneBrickUnsafe, ItemID.HellstoneBrickWall },
            { WallID.ObsidianBackUnsafe, ItemID.ObsidianBackEcho },
            { WallID.LavaUnsafe1, ItemID.Lava1Echo },
            { WallID.LavaUnsafe2, ItemID.Lava2Echo },
            { WallID.LavaUnsafe3, ItemID.Lava3Echo },
            { WallID.LavaUnsafe4, ItemID.Lava4Echo },
            { WallID.IceUnsafe, ItemID.IceEcho },
            { WallID.SnowWallUnsafe, ItemID.SnowWallEcho },
            { WallID.JungleUnsafe, ItemID.JungleWall },
            { WallID.JungleUnsafe1, ItemID.Jungle1Echo },
            { WallID.JungleUnsafe2, ItemID.Jungle2Echo },
            { WallID.JungleUnsafe3, ItemID.Jungle3Echo },
            { WallID.JungleUnsafe4, ItemID.Jungle4Echo },
            { WallID.LivingWoodUnsafe, ItemID.LivingWoodWall },
            { WallID.MudUnsafe, ItemID.MudWallEcho },
            { WallID.MushroomUnsafe, ItemID.MushroomWall }, //natural mushroom walls are slightly rounder, but no associated item for them
            { WallID.SpiderUnsafe, ItemID.SpiderEcho },
        };

        public override bool Drop(int i, int j, int type, ref int dropType)
        {
            Player player = Main.player[Player.FindClosest(new Vector2(i, j) * 16, 16, 16)];
            if (player.HeldItem.type != ModContent.ItemType<LotusHammer>())
                return true;
            if (dropForWallType.Keys.Contains(type))
                dropType = dropForWallType[type];
            return true;
        }
    }
}
