//TODO:
//Sellprice
//Rarity
//Balance
//Sprites

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using JadeFables.Core;
using ReLogic.Content;
using JadeFables.Helpers;
using Terraria.Graphics.Effects;
using SteelSeries.GameSense;
using IL.Terraria.Audio;
using Terraria.Audio;

namespace JadeFables.Items.SpringChestLoot.FireworkPack
{
	public class FireworkPack : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Firework Pack");
			Tooltip.SetDefault("15 damage \nEnemies launch damaging fireworks when they die");
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<FireworkPackPlayer>().equipped = true;
		}
	}

	public class FireworkPackPlayer : ModPlayer
	{
		public bool equipped = false;

		public override void ResetEffects()
		{
			equipped = false;
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
		{
			if (target.life <= 0)
				SummonFireworks(target, Main.player[proj.owner]);
		}

		public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
		{
			if (target.life <= 0)
				SummonFireworks(target, Main.player[target.target]);
		}

		private void SummonFireworks(NPC target, Player owner)
		{
			if (!owner.GetModPlayer<FireworkPackPlayer>().equipped)
				return;

            if (target.SpawnedFromStatue)
                return;

			int amt = Main.rand.Next(1, 4);
			for (int i = 0; i < amt; i++)
			{
                Vector2 vel = Main.rand.NextVector2CircularEdge(2, 2);
                vel.Y *= -Math.Sign(vel.Y);
                Projectile proj = Projectile.NewProjectileDirect(target.GetSource_Death(), target.Center, vel, ModContent.ProjectileType<FireworkPackProj>(), 15, 3, owner.whoAmI);
                proj.timeLeft = Main.rand.Next(60, 80);
            }
		}
	}

	internal class FireworkPackProj : ModProjectile
	{
        private Color color = Color.White;

        private bool loop = false;

        private int loopDirection = 1;
        private float loopCounter = 0;

        private Vector2 initialDirection = Vector2.Zero;

        NPC lastHit;

        private float progress => 1 - ((Projectile.timeLeft - 60) / 60f);

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Firework");
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.timeLeft = 60;
			Projectile.penetrate = 1;
        }

        public override void AI()
        {
            if (color == Color.White)
                color = Main.hslToRgb(Main.rand.NextFloat(), Main.rand.NextFloat(0.65f, 1f), Main.rand.NextFloat(0.5f,0.8f));


            if (Projectile.timeLeft < 40)
            {
                var nearestNPC = Main.npc.Where(n => n.active && n.CanBeChasedBy() && n.Distance(Projectile.Center) < 700).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
                if (nearestNPC != default)
                {
                    Vector2 direction = Projectile.DirectionTo(nearestNPC.Center);
                    float rotDifference = ((((direction.ToRotation() - Projectile.velocity.ToRotation()) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

                    if (Projectile.timeLeft < 3)
                        Projectile.timeLeft = 3;
                    Projectile.velocity = Projectile.velocity.RotatedBy(rotDifference * MathHelper.Lerp(0.05f, 0.5f, progress));
                }
            }
            Projectile.velocity *= 1.01f;


            if (Main.rand.NextBool(70) && !loop)
            {
                loop = true;
                loopDirection = Main.rand.NextBool() ? 1 : -1;
                initialDirection = Vector2.Normalize(Projectile.velocity);
            }

            if (loop)
            {
                loopCounter += 0.02f;
                Projectile.velocity = initialDirection.RotatedBy(EaseFunction.EaseCircularInOut.Ease(loopCounter) * 6.28f * loopDirection) * Projectile.velocity.Length() * 1.02f;
                if (loopCounter > 1)
                {
                    loopCounter = 0;
                    loop = false;
                }
            }
            else
            {
                Projectile.velocity = Projectile.velocity.RotatedByRandom(0.07f);
                Projectile.velocity *= 1.02f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

            CreateDust();
        }

        public override void Kill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item40 with { Pitch = Main.rand.NextFloat(-0.6f, -0.2f)}, Projectile.Center);
            for (int i = 0; i < 32; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<FireworkPackGlowDust>(), Main.rand.NextVector2Circular(12, 12), 0, AdjacentColor(), Main.rand.NextFloat(1.5f, 3f));
            }

            /*for (int j = 0; j < 15; j++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(9, 9), ModContent.DustType<FireworkPackDust1>(), Main.rand.NextVector2Circular(16, 16), 0, AdjacentColor(), Main.rand.NextFloat(1f, 1.5f));
                dust.alpha = Main.rand.Next(150);
            }*/
            Projectile proj;
            if (lastHit == null)
                proj = Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FireworkPackExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            else
                proj = Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FireworkPackExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner, lastHit.whoAmI);
            (proj.ModProjectile as FireworkPackExplosion).color = color;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            lastHit = target;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.velocity.Length() < 4f)
                return false;
            return base.CanHitNPC(target);
        }

        private void CreateDust()
        {
            Lighting.AddLight(Projectile.Center, color.ToVector3());
            for (int i = 0; i < 4; i++)
            {
                var pos = (Projectile.Center) - (Projectile.velocity * Main.rand.NextFloat(2));
                Dust dust = Dust.NewDustPerfect(pos, ModContent.DustType<FireworkPackDust1>(), Vector2.Normalize(-Projectile.velocity).RotatedByRandom(0.6f) * Main.rand.NextFloat(3.5f), 0, AdjacentColor());
                dust.scale = Main.rand.NextFloat(0.15f, 0.35f) * 0.75f;
                dust.alpha = Main.rand.Next(50);
                dust.rotation = Main.rand.NextFloatDirection();
            }

            for (int j = 0; j < 1; j++)
            {
                var pos = (Projectile.Center) - (Projectile.velocity * Main.rand.NextFloat(2));
                Dust dust2 = Dust.NewDustPerfect(pos, ModContent.DustType<FireworkPackDust2>(), Vector2.Normalize(-Projectile.velocity).RotatedByRandom(3.0f) * Main.rand.NextFloat(5.5f), 0, AdjacentColor());
                dust2.scale = Main.rand.NextFloat(0.1f, 0.25f) * 0.75f;
                dust2.alpha = Main.rand.Next(50);
                dust2.rotation = Main.rand.NextFloatDirection();
            }

            var pos2 = (Projectile.Center) - (Projectile.velocity * Main.rand.NextFloat(2));
            Dust dust3 = Dust.NewDustPerfect(pos2, ModContent.DustType<FireworkPackGlowDust>(), Vector2.Normalize(-Projectile.velocity).RotatedByRandom(2.6f) * Main.rand.NextFloat(3.5f), 0, AdjacentColor());
            dust3.scale = Main.rand.NextFloat(0.25f, 1f) * 0.5f;
            dust3.alpha = Main.rand.Next(50);
        }

        private Color AdjacentColor()
        {
            int r = color.R + Main.rand.Next(-20,20);
            int g = color.G + Main.rand.Next(-20, 20);
            int b = color.B + Main.rand.Next(-20, 20);
            return new Color(r, g, b);
        }
    }

    public class FireworkPackExplosion : ModProjectile
    {

        public Color color;

        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        private float Progress => 1 - (Projectile.timeLeft / 10f);

        private float Radius => 66 * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fireworks");
        }

        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI == (int)Projectile.ai[0])
                return false;
            return base.CanHitNPC(target);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
            line.Normalize();
            line *= Radius;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
            {
                return true;
            }
            return false;
        }

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            float radius = Radius;
            for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
            {
                double rad = (i / 32f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
                offset *= radius;
                cache.Add(Projectile.Center + offset);
            }

            while (cache.Count > 33)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 38 * (1 - Progress), factor =>
            {
                return color;
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 10 * (1 - Progress), factor =>
            {
                return Color.White;
            });
            float nextplace = 33f / 32f;
            Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
            offset *= Radius;

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + offset;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = Projectile.Center + offset;
        }

        public override void PostDraw(Color lightColor)
        {
            return;
            Main.spriteBatch.End();
            Effect effect = Filters.Scene["GlowTrailShader"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("JadeFables/Assets/GlowTrail").Value);
            effect.Parameters["time"].SetValue(0);
            effect.Parameters["repeats"].SetValue(1);

            trail?.Render(effect);
            trail2?.Render(effect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
        }
    }

    public class FireworkPackDust1 : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.scale *= 0.3f;
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color ret;

            Vector3 HSL = Main.rgbToHsl(dust.color);
            HSL.X += 0.5f;
            Color newColor = Main.hslToRgb(HSL);

            ret = Color.Lerp(dust.color, newColor, dust.alpha / 255f);

            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData == null)
            {
                dust.position -= Vector2.One * 35 * dust.scale;
                dust.customData = 0;
            }

            if ((int)dust.customData < 10)
            {
                dust.scale *= 1.1f;
                dust.customData = (int)dust.customData + 1;
            }
            else
            {
                if (dust.alpha > 140)
                {
                    dust.scale *= 0.96f;
                }
                else
                {
                    dust.scale *= 0.95f;
                }
            }


            if (dust.velocity.Length() > 3)
                dust.velocity *= 0.85f;
            else
                dust.velocity *= 0.92f;

            if (dust.alpha > 60)
            {
                dust.alpha += 8;
            }
            else
            {
                dust.alpha += 6;
            }

            Lighting.AddLight(dust.position, ((Color)(GetAlpha(dust, Color.White))).ToVector3() * 0.5f);

            dust.position += dust.velocity;

            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }

    public class FireworkPackDust2 : ModDust
    {

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.scale *= 0.3f;
            dust.frame = new Rectangle(0, 0, 34, 36);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color ret;

            Vector3 HSL = Main.rgbToHsl(dust.color);
            HSL.X += 0.5f;
            Color newColor = Main.hslToRgb(HSL);

            ret = Color.Lerp(dust.color, newColor, dust.alpha / 255f);

            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData == null)
            {
                dust.position -= Vector2.One * 35 * dust.scale;
                dust.customData = 0;
            }

            if ((int)dust.customData < 10)
            {
                dust.scale *= 1.07f;
                dust.customData = (int)dust.customData + 1;
            }
            else
            {
                if (dust.alpha > 140)
                {
                    dust.scale *= 0.98f;
                }
                else
                {
                    dust.scale *= 0.98f;
                }
            }


            if (dust.velocity.Length() > 3)
                dust.velocity *= 0.92f;
            else
                dust.velocity *= 0.96f;

            dust.alpha += 20;

            Lighting.AddLight(dust.position, ((Color)(GetAlpha(dust, Color.White))).ToVector3() * 0.5f);

            dust.position += dust.velocity;

            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
    }

    class FireworkPackGlowDust : ModDust
    {
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        float Curve(float input) //shrug it works, just a cubic regression for a nice looking curve
        {
            return -2.65f + 19.196f * input - 32.143f * input * input + 15.625f * input * input * input;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.scale *= 0.3f;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.velocity *= 2;
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(ModContent.Request<Effect>("JadeFables/Effects/GlowingDust", AssetRequestMode.ImmediateLoad).Value), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.fadeIn == 0)
                dust.position -= Vector2.One * 32 * dust.scale;

            //dust.rotation += dust.velocity.Y * 0.1f;
            dust.position += dust.velocity;
            dust.velocity *= 0.86f;
            dust.shader.UseColor(dust.color);
            dust.scale *= 0.95f;
            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

            if (dust.fadeIn > 25)
                dust.active = false;

            return false;
        }
    }
}
