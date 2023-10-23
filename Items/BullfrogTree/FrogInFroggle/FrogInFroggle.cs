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
using JadeFables.Tiles.JadeLantern;

namespace JadeFables.Items.BullfrogTree.FrogInFroggle
{
    [AutoloadEquip(EquipType.Waist)]
    public class FrogInFroggle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.accessory = true;
            Item.hasVanityEffects = true;

            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BullfrogLegsPlayer>().Enable(Item);
            player.GetModPlayer<FrogInFrogglePlayer>().Enable();
        }

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            int[] incompadible = new int[] { ModContent.ItemType<BullfrogLegs.BullfrogLegs>(), ModContent.ItemType<Bulfrauble.Bulfrauble>() };
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
    }

    public class FrogInFrogglePlayer : ModPlayer
    {
        private bool[]? lastFrameActiveJumps;
        private bool active;

        public void Enable()
        {
            active = true;
        }

        public override void PostUpdateEquips()
        {
            if (!active)
            {
                return;
            }

            lastFrameActiveJumps ??= new bool[Player.ExtraJumps.Length];
            bool shouldAddSpeed = false;
            for (int i = 0; i < Player.ExtraJumps.Length; i++)
            {
                if (Player.ExtraJumps[i].Active && !lastFrameActiveJumps[i])
                {
                    shouldAddSpeed = true;
                }

                lastFrameActiveJumps[i] = Player.ExtraJumps[i].Active;
            }
            
            if (shouldAddSpeed)
            {
                if (Player.controlLeft)
                    Player.velocity.X = -8;
                else if (Player.controlRight)
                    Player.velocity.X = 8;
                return;
            }
        }

        public override void ResetEffects()
        {
            active = false;
        }
    }
}
