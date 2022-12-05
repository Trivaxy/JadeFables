using JadeFables.Dusts;
using Microsoft.Xna.Framework;
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

namespace JadeFables.NPCs
{
    internal class AirBubble : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Air Bubble");
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = true;
            Projectile.friendly = false;
            Projectile.hide = true;
            Projectile.timeLeft = 200;
        }

        public override void AI()
        {
            Point tilePos = new Point((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            if (Framing.GetTileSafely(tilePos).LiquidAmount == 0)
                Projectile.active = false;
        }

        public void DrawBubble(Vector2 screenPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - screenPos, null, Color.White, 0f, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
        }

        public void DrawMetaball()
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(tex, (Projectile.Center - Main.screenPosition) / 2, null, Color.White, 0f, tex.Size() / 2, Projectile.scale / 2, SpriteEffects.None, 0f);
        }
    }
}