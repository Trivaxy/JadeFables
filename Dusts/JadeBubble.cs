using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System;

namespace JadeFables.Dusts
{
    public class JadeBubble : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 10, 10);
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(dust.frame.Width / 2, dust.frame.Height / 2) * dust.scale;
                dust.customData = 1;
            }

            dust.alpha += 5;

            if (dust.alpha > 255)
                dust.active = false;

            dust.position += dust.velocity;

            Lighting.AddLight(dust.position, new Color(174, 235, 30).ToVector3() * 0.35f);
            return false;
        }
    }
}
