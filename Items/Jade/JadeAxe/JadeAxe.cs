//TODO:
//Fruit thing
//Recipe

using System;
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
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Axe");
            Tooltip.SetDefault("Trees drop more fruit");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;
            Item.axe = 12;
            Item.useTurn = true;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 11;
            Item.knockBack = 5f;
            Item.crit = 4;

            Item.value = Item.sellPrice(silver: 20);
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
    }
}
