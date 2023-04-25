using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace JadeFables.Items.BullfrogTree.BullfrogLegs
{
    public class BullfrogLegDust : ModDust
    {
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return lightColor.MultiplyRGB(Color.Lerp(Color.White, Color.Brown, (float)dust.fadeIn / 60f)) * (float)Math.Sin(dust.fadeIn / 60f * 3.14f);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.rotation = Main.rand.NextFloat(6.28f);
            dust.frame = new Rectangle(0, 0, 36, 36);
            dust.alpha = Main.rand.Next(15);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            float rotVel = (dust.velocity.Y / 40f) * (dust.alpha > 7 ? -1 : 1);

            Vector2 currentCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 18 * dust.scale;
            dust.scale *= 0.99f;
            Vector2 nextCenter = dust.position + Vector2.One.RotatedBy(dust.rotation + rotVel) * 18 * dust.scale;

            dust.rotation += rotVel;
            dust.position += currentCenter - nextCenter;

            dust.fadeIn++;

            dust.velocity *= 0.98f;


            if (dust.fadeIn > 60)
                dust.active = false;
            return false;
        }
    }
}