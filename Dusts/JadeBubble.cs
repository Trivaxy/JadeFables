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
            dust.alpha += 5;

            if (dust.alpha > 255)
                dust.active = false;

            dust.position += dust.velocity;

            dust.velocity *= 0.94f;

            Lighting.AddLight(dust.position, new Color(174, 235, 30).ToVector3() * 0.35f);
            return false;
        }
    }
}
