﻿using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using JadeFables.Dusts;

namespace JadeFables.Items.Jade.JadeAxe
{
    public class JadeAxe : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;
            Item.axe = 15;
            Item.useTurn = true;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 8;
            Item.knockBack = 5f;
            Item.crit = 4;

            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Blue;

            Item.UseSound = SoundID.Item1;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(100))
            {
                Vector2 pos = hitbox.TopLeft();
                pos.X += Main.rand.Next(hitbox.Width);
                pos.Y += Main.rand.Next(hitbox.Height);

                Dust.NewDustPerfect(pos, ModContent.DustType<JadeSparkle>(), Vector2.Zero);
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<JadeChunk.JadeChunk>(12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
