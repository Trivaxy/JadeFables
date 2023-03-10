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
    internal class AirBubble : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Air Bubble");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = true;
            Projectile.friendly = false;
            Projectile.hide = true;
            Projectile.timeLeft = 500;
        }

        public override void AI()
        {
            Point tilePos = new Point((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            if (Framing.GetTileSafely(tilePos).LiquidAmount == 0)
                Projectile.active = false;

            var collidingPlayers = Main.player.Where(n => n.active && !n.dead && n.breath < n.breathMax && n.Hitbox.Intersects(Projectile.Hitbox)).FirstOrDefault();

            if (collidingPlayers != default)
            {
                collidingPlayers.breath += (int)(200 * Projectile.scale);
                collidingPlayers.breath = (int)MathHelper.Min(collidingPlayers.breath, collidingPlayers.breathMax);

                Terraria.Audio.SoundStyle sound = SoundID.Item86;
                if (Projectile.scale > 0.3f) sound = SoundID.Item87;
                Terraria.Audio.SoundEngine.PlaySound(sound with { Pitch = 0.3f - Projectile.scale}, Projectile.Center);

                Projectile.active = false;
            }
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