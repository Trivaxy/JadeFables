//TODO:
//Actual sprite
//Sparkles (maybe)
//Tooltip (maybe)
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using JadeFables.Dusts;

namespace JadeFables.Items.Jade.JadeChunk
{
    public class JadeChunk : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;

            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 35);
            Item.rare = ItemRarityID.Blue;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = Request<Texture2D>(Texture).Value;
            Texture2D texGlow = Request<Texture2D>(Texture + "_Glow").Value;
            Main.spriteBatch.Draw(texGlow, Item.Center - Main.screenPosition, null, new Color(105, 208, 86, 0), rotation, texGlow.Size() / 2f, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, lightColor, rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texGlow, Item.Center - Main.screenPosition, null, (new Color(105, 208, 86, 0) * 0.5f) * (float)Utils.Clamp(Math.Sin(Main.GlobalTimeWrappedHourly), 0, 1), rotation, texGlow.Size() / 2f, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void PostUpdate()
        {
            if (Main.rand.NextBool(90))
                Dust.NewDustPerfect(Item.Center + Main.rand.NextVector2Circular(10, 10), DustType<JadeSparkle>(), Vector2.Zero);
        }
    }
}
