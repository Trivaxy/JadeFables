using JadeFables.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace JadeFables.NPCs
{
    internal class FishingSpot : ModProjectile
    {

        public float time;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fishing Spot");
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 16;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.timeLeft = 500;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(64, 8), ModContent.DustType<JadeSparkle>(), -Vector2.UnitY * 0.3f);
            }
            Projectile.timeLeft = 2;
            time += 0.004f;
            Projectile.velocity = Vector2.Zero;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Effect effect = Filters.Scene["FishingSpot"].GetShader().Shader;

            effect.Parameters["time"].SetValue(time);

            Color color = Color.Lerp(Color.Cyan, Color.Green, 0.5f);
            effect.Parameters["inputColor"].SetValue(color.ToVector4() * 1.3f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Green, 3.14f, tex.Bounds.Top(), new Vector2(0.5f, 0.3f), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}