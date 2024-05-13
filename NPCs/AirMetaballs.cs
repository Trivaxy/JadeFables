using JadeFables.Core;
using Terraria;
using Terraria.ID;
using Terraria.Enums;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace JadeFables.NPCs
{
    internal class BloodMetaballs : MetaballActor
    {
        public override bool Active => false;

        public override Color outlineColor => Color.Green * 0.8f;

        public virtual Color inColor => Color.Transparent;

        public override bool Invisible => true;

        public override void DrawShapes(SpriteBatch spriteBatch)
        {
            Effect borderNoise = Filters.Scene["BorderNoise"].GetShader().Shader;

            if (borderNoise is null)
                return;

            borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            borderNoise.CurrentTechnique.Passes[0].Apply();

            var bubbles = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<AirBubble>());
            foreach (Projectile proj in bubbles)
            {
                (proj.ModProjectile as AirBubble).DrawMetaball();
            }

            spriteBatch.End();
            spriteBatch.Begin();
        }

        public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
        {
            Rectangle sourceRect = new Rectangle(0, 0, target.Width * 2, target.Height * 2);

            spriteBatch.Draw(target, sourceRect, Color.Pink);
            return false;
        }
    }
}
