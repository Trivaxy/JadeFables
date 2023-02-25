using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using System.IO;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.Audio;
using JadeFables.Core;
using JadeFables.Helpers;
using JadeFables.Helpers.FastNoise;
using JadeFables.Items.BullfrogTree.BullfrogLegs;

namespace JadeFables.Items.BullfrogTree.FrogInFroggle
{
    public class FrogInFroggle : ModItem
    {
        public bool jumping = false;

        private bool sailJump = true;
        private bool basiliskJump = true;
        private bool blizzardJump = true;
        private bool sandstormJump = true;
        private bool cloudJump = true;
        private bool fartJump = true;
        private bool santankJump = true;
        private bool unicornJump = true;
        private bool fleshJump = true;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Frog In A Froggle");
            Tooltip.SetDefault("Jumping while moving boosts you forward \nAdds horizontal movement to double jumps");
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.accessory = true;
            Item.canBePlacedInVanityRegardlessOfConditions = true;

            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.controlJump && player.velocity.Y == 0 && !jumping)
            {
                if (!player.controlLeft && !player.controlRight)
                    return;

                SoundEngine.PlaySound(SoundID.Run, player.Center);
                DoJump(player, 8);

                Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Bottom, Vector2.Zero, ModContent.ProjectileType<BullfrogLegRing>(), 0, 0, player.whoAmI, Main.rand.Next(30, 40), 1.57f + (0.78f * Math.Sign(player.velocity.X)));
                for (int i = 0; i < 6; i++)
                {
                    Dust.NewDustPerfect(player.Bottom, ModContent.DustType<BullfrogLegDust>(), new Vector2(-Math.Sign(player.velocity.X), 1).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.5f, 0.75f), 0, Color.White, Main.rand.NextFloat(0.4f, 0.7f));
                }
            }

            if (!player.canJumpAgain_Basilisk && basiliskJump)
                DoJump(player, 8);

            if (!player.canJumpAgain_Blizzard && blizzardJump)
                DoJump(player, 8);

            if (!player.canJumpAgain_Sail && sailJump)
                DoJump(player, 8);

            if (!player.canJumpAgain_Sandstorm && sandstormJump)
                DoJump(player, 8);

            if (!player.canJumpAgain_Cloud && cloudJump)
                DoJump(player, 8);

            if (!player.canJumpAgain_Fart && fartJump)
                DoJump(player, 8);

            if (!player.canJumpAgain_Santank && santankJump)
                DoJump(player, 8);

            if (!player.canJumpAgain_Unicorn && unicornJump)
                DoJump(player, 8);

            if (!player.canJumpAgain_WallOfFleshGoat && fleshJump)
                DoJump(player, 8);



            basiliskJump = player.canJumpAgain_Basilisk;
            sailJump = player.canJumpAgain_Sail;
            blizzardJump = player.canJumpAgain_Blizzard;
            sandstormJump = player.canJumpAgain_Sandstorm;
            cloudJump = player.canJumpAgain_Cloud;
            fartJump= player.canJumpAgain_Fart;
            santankJump = player.canJumpAgain_Santank;
            unicornJump = player.canJumpAgain_Unicorn;
            fleshJump = player.canJumpAgain_WallOfFleshGoat;

            if (player.controlJump)
            {
                jumping = true;
            }
            else
            {
                jumping = false;
            }
        }

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            int[] incompadible = new int[] { ModContent.ItemType<BullfrogLegs.BullfrogLegs>() };
            if (equippedItem.type == ModContent.ItemType<FrogInFroggle>() && incompadible.Contains(incomingItem.type))
            {
                return false;
            }

            if (incomingItem.type == ModContent.ItemType<FrogInFroggle>() && incompadible.Contains(equippedItem.type))
            {
                return false;
            }

            return base.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<BullfrogLegs.BullfrogLegs>());
            recipe.AddIngredient(ItemID.TsunamiInABottle, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }

        private static void DoJump(Player player, float speed)
        {
            if (!player.controlLeft && !player.controlRight)
                return;
            if (player.controlLeft)
                player.velocity.X = -speed;
            else if (player.controlRight)
                player.velocity.X = speed;
        }

    }
}
