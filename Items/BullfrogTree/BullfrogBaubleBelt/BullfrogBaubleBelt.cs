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
using JadeFables.Items.BullfrogTree.FrogInFroggle;

namespace JadeFables.Items.BullfrogTree.BullfrogBaubleBelt
{
    [AutoloadEquip(EquipType.Waist)]
    public class BullfrogBaubleBelt : ModItem
    {
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
            player.GetModPlayer<BullfrogLegsPlayer>().Enable(Item);
            player.GetModPlayer<FrogInFrogglePlayer>().Enable();

            Lighting.AddLight(player.Center, new Color(142, 196, 251).ToVector3() * 0.95f);
            player.hasMagiluminescence = true;
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
    }
}
