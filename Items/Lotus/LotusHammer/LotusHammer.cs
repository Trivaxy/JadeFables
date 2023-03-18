using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using JadeFables.Dusts;
using IL.Terraria.DataStructures;

namespace JadeFables.Items.Lotus.LotusHammer
{
    public class LotusHammer : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lotus Hammer");
            Tooltip.SetDefault("Can pickup walls that other hammers can't");
        }

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
        public override bool Drop(int i, int j, int type, ref int dropType)
        {
            Player player = Main.player[Player.FindClosest(new Vector2(i, j) * 16, 16,16)];
            if (player.HeldItem.type != ModContent.ItemType<LotusHammer>())
                return true;
            switch (type)
            {
                case WallID.DirtUnsafe:
                case WallID.DirtUnsafe1:
                case WallID.DirtUnsafe2:
                case WallID.DirtUnsafe3:
                case WallID.DirtUnsafe4:
                    dropType = ItemID.DirtWall;
                    break;
                case WallID.GrassUnsafe:
                    dropType = ItemID.GrassWall;
                    break;
                case WallID.FlowerUnsafe:
                    dropType = ItemID.FlowerWall;
                    break;
            }
            return true;
        }
    }
}
