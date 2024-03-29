﻿namespace JadeFables.Tiles.JadeLantern
{
    public class LanternGlow : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.frame = new Rectangle(0, 0, 64, 64);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(JadeFables.Instance.Assets.Request<Effect>("Effects/GlowingDust", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value), "GlowingDustPass");
            dust.shader.UseColor(Color.Transparent);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            if (dust.alpha < 3)
                return new Color(0, 0, 0, 0);
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= Vector2.One * 32 * dust.scale;
                dust.customData = true;
            }
            dust.alpha++;

            Vector2 currentCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 32 * dust.scale;

            dust.scale *= 0.97f;
            Vector2 nextCenter = dust.position + Vector2.One.RotatedBy(dust.rotation + 0.06f) * 32 * dust.scale;

            dust.rotation += 0.06f;
            dust.position += currentCenter - nextCenter;

            dust.shader.UseColor(dust.color);

            dust.position += dust.velocity;
            dust.velocity.Y += 0.1f;

            if (!dust.noGravity)
                dust.velocity.Y += 0.1f;

            dust.velocity *= 0.99f;
            dust.color *= 0.98f;

            if (!dust.noLight)
                Lighting.AddLight(dust.position, dust.color.ToVector3());

            if (dust.scale < 0.05f)
                dust.active = false;

            return false;
        }
    }
}