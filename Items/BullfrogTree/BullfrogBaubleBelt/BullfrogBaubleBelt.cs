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

namespace JadeFables.Items.BullfrogTree.BullfrogBaubleBelt
{
    [AutoloadEquip(EquipType.Waist)]
    public class BullfrogBaubleBelt : ModItem
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
            // DisplayName.SetDefault("Bullfrog Bauble Belt");
            // Tooltip.SetDefault("Jumping while moving boosts you further forward\nAdds horizontal movement to double jumps\nIncreases movement speed and acceleration\nProvides light when worn");
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.accessory = true;
            Item.hasVanityEffects = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            int jumpBoost = 10;
            Lighting.AddLight(player.Center, new Color(142, 196, 251).ToVector3() * 0.95f);
            player.hasMagiluminescence = true;

            if (player.controlJump && player.velocity.Y == 0 && !jumping)
            {
                if (!player.controlLeft && !player.controlRight)
                    return;

                SoundEngine.PlaySound(SoundID.Run, player.Center);
                DoJump(player, 10);

                Projectile.NewProjectileDirect(player.GetSource_Accessory(Item), player.Bottom, Vector2.Zero, ModContent.ProjectileType<BullfrogLegRingAlt>(), 0, 0, player.whoAmI).rotation = 1.57f - (0.78f * Math.Sign(player.velocity.X));
            }

            if (!player.canJumpAgain_Basilisk && basiliskJump)
                DoJump(player, jumpBoost);

            if (!player.canJumpAgain_Blizzard && blizzardJump)
                DoJump(player, jumpBoost);

            if (!player.canJumpAgain_Sail && sailJump)
                DoJump(player, jumpBoost);

            if (!player.canJumpAgain_Sandstorm && sandstormJump)
                DoJump(player, jumpBoost);

            if (!player.canJumpAgain_Cloud && cloudJump)
                DoJump(player, jumpBoost);

            if (!player.canJumpAgain_Fart && fartJump)
                DoJump(player, jumpBoost);

            if (!player.canJumpAgain_Santank && santankJump)
                DoJump(player, jumpBoost);

            if (!player.canJumpAgain_Unicorn && unicornJump)
                DoJump(player, jumpBoost);

            if (!player.canJumpAgain_WallOfFleshGoat && fleshJump)
                DoJump(player, jumpBoost);



            basiliskJump = player.canJumpAgain_Basilisk;
            sailJump = player.canJumpAgain_Sail;
            blizzardJump = player.canJumpAgain_Blizzard;
            sandstormJump = player.canJumpAgain_Sandstorm;
            cloudJump = player.canJumpAgain_Cloud;
            fartJump = player.canJumpAgain_Fart;
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
            int[] incompadible = new int[] { ModContent.ItemType<BullfrogLegs.BullfrogLegs>(), ModContent.ItemType<Bulfrauble.Bulfrauble>(), ModContent.ItemType<FrogInFroggle.FrogInFroggle>() };
            if (equippedItem.type == ModContent.ItemType<BullfrogBaubleBelt>() && incompadible.Contains(incomingItem.type))
            {
                return false;
            }

            if (incomingItem.type == ModContent.ItemType<BullfrogBaubleBelt>() && incompadible.Contains(equippedItem.type))
            {
                return false;
            }

            return base.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<FrogInFroggle.FrogInFroggle>(), 1);
            recipe.AddIngredient(ModContent.ItemType<Bulfrauble.Bulfrauble>(), 1);
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
