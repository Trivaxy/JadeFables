using Terraria.Graphics.Effects;
using JadeFables.Core;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using JadeFables.Helpers;

namespace JadeFables.Items.Jade.JadeKunai
{
    public class JadeKunai : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.useTime = Item.useAnimation = 24;

            Item.noUseGraphic = true;

            Item.autoReuse = true;

            Item.DamageType = DamageClass.Throwing;
            Item.damage = 19;
            Item.knockBack = 3f;
            Item.crit = 4;

            Item.shoot = ProjectileType<JadeKunaiProjectile>();
            Item.shootSpeed = 10;

            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Blue;

            Item.UseSound = SoundID.Item1;
        }

        const int KunaiCount = 3;
        public Projectile[]? lastShotKunai;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            lastShotKunai = new Projectile[KunaiCount];
            for (int i = 0; i < KunaiCount; i++)
            {
                Projectile kunai = Projectile.NewProjectileDirect(
                source,
                position,
                velocity,
                type,
                damage,
                knockback,
                player.whoAmI
                );

                kunai.tileCollide = false;

                lastShotKunai[i] = kunai;
            }

            

            swingDirection = -swingDirection;

            return false;
        }

        private static int swingDirection = -1;
        private Vector2 directionToMouse;
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            Vector2 shoulderPos = player.RotatedRelativePoint(player.MountedCenter) + new Vector2(-4 * player.direction, -2);

            if (player.whoAmI == Main.myPlayer)
            {
                directionToMouse = shoulderPos.DirectionTo(Main.MouseWorld);
            }
            float arc = MathHelper.TwoPi * 0.66f;
            float mouseRot = directionToMouse.ToRotation();
            float rotFunc = MathF.Sin(MathF.Pow((float)player.itemAnimation / player.itemAnimationMax, 5) * 1.095f);
            float armRotation = mouseRot + swingDirection * (arc * rotFunc - arc * 0.5f);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation - MathHelper.PiOver2);

            if (lastShotKunai is not null)
            {
                float armLen = 30;
                player.heldProj = lastShotKunai[0].whoAmI;
                if (rotFunc < 0.45f)
                {
                    float maxRot = MathHelper.PiOver4 * Main.rand.NextFloat(0.1f, 0.225f);
                    float currRot = 0;
                    foreach (Projectile kunai in lastShotKunai)
                    {
                        kunai.velocity = directionToMouse.RotatedBy(currRot - maxRot * 0.5f) * kunai.velocity.Length() * Main.rand.NextFloat(0.85f,1.2f);
                        kunai.Center = shoulderPos + directionToMouse * armLen;
                        kunai.tileCollide = true;
                        currRot += maxRot / (KunaiCount - 1);
                    }
                    
                    lastShotKunai = null;
                }
                else
                {
                    foreach (Projectile kunai in lastShotKunai)
                    {
                        kunai.Center = shoulderPos + armRotation.ToRotationVector2() * armLen;
                        kunai.rotation = shoulderPos.DirectionTo(kunai.Center).ToRotation();
                    }
                }
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.WriteVector2(directionToMouse);
        }

        public override void NetReceive(BinaryReader reader)
        {
            directionToMouse = reader.ReadVector2();
        }
    }

    public class JadeKunaiProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 13;
            Projectile.height = 13;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2000;
            Projectile.extraUpdates = 2;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        int stabTimer;
        public override void AI()
        {
            if (StabActive)
            {
                if (stabTimer < 100)
                {
                    stabTimer++;
                }
                else
                {
                    stabTimer = 100;
                }
                
                Projectile.Center = stabbedTarget.Center + sTOffset;

                Projectile.velocity *= 0.75f;
                Projectile.Center += Projectile.velocity;
            }
            else
            {
                if (stabTimer > 0)
                {
                    stabTimer--;
                }

                if (CollideProg == 1f)
                {
                    Projectile.velocity.Y += 0.005f;
                    Projectile.rotation = Projectile.velocity.ToRotation();
                }
            }

            if (Projectile.timeLeft % 3 == 0)
            {
                ManageTrailCache();
                ManageTrail();

                if (trailFadeIn < 1)
                {
                    trailFadeIn += 0.02f;
                }
                else
                {
                    trailFadeIn = 1;
                }
            }

            if (stabImpactTimer > 0)
            {
                stabImpactTimer *= 0.97f;
            }
            else
            {
                stabImpactTimer = 0;
            }
        }

        public override bool PreAI() => !(Main.player[Projectile.owner]?.HeldItem?.ModItem is JadeKunai kunaiItem && kunaiItem.lastShotKunai?.FirstOrDefault(p => p.whoAmI == Projectile.whoAmI) is not null);
        public override bool ShouldUpdatePosition() => PreAI() && !StabActive;

        bool StabActive => stabbedTarget is not null && stabbedTarget.life > 0 && stabbedTarget.active;
        NPC? stabbedTarget;
        Vector2 sTOffset;
        float stabImpactTimer;
        public override bool? CanHitNPC(NPC target)
        {
            if (StabActive || Projectile.timeLeft < TimeLeftOnCollide)
            {
                return false;
            }
            return null;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.GetGlobalNPC<JadeKunaiStackNPC>().KunaiStack++;
            if (target.GetGlobalNPC<JadeKunaiStackNPC>().KunaiStack == 10)
                target.GetGlobalNPC<JadeKunaiStackNPC>().flashOpacity = 1;

            Dust.NewDustPerfect(
                Projectile.Center, 
                DustType<JadeKunaiDust>(), 
                Vector2.Zero, 
                125,
                Color.Green,
                Main.rand.NextFloat(0.75f, 1.25f)
                );

            stabImpactTimer = 1;
            sTOffset = Projectile.Center - target.Center;
            stabbedTarget = target;
        }

        const int TimeLeftOnCollide = 40;
        float CollideProg => Math.Clamp((float)Projectile.timeLeft / TimeLeftOnCollide, 0f, 1f);
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > TimeLeftOnCollide)
            {
                Projectile.timeLeft = TimeLeftOnCollide;
                Projectile.velocity = oldVelocity * 0.001f;
            }
            else
            {
                Projectile.velocity = oldVelocity;
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            if (stabbedTarget is not null)
            {
                stabbedTarget.GetGlobalNPC<JadeKunaiStackNPC>().KunaiStack--;
            }
        }

        private Vector2[]? trailCache;
        private const int TrailCacheLenght = 9;
        private void ManageTrailCache()
        {
            if (trailCache is null)
            {
                trailCache = new Vector2[TrailCacheLenght];
                for (int i = 0; i < trailCache.Length; i++)
                {
                    trailCache[i] = Projectile.Center;
                }
            }
            else
            {
                for (int i = 1; i < trailCache.Length; i++)
                {
                    float prog = (float)i / trailCache.Length;
                    trailCache[i - 1] = trailCache[i]; //+ MathF.Sin(MathHelper.PiOver2 + Main.GameUpdateCount * MathHelper.Pi + i * MathHelper.PiOver2) * Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * MathF.Pow(prog * 2, 2) * 1f;
                }
                trailCache[^1] = Projectile.Center;
            }
        }
        private float trailFadeIn = 0;
        private Trail? trail;
        private void ManageTrail()
        {
            trail ??= new Trail(Main.instance.GraphicsDevice, TrailCacheLenght, new TriangularTip(3), factor => (-MathF.Pow(factor * 2 - 1 , 2) + 1) * MathF.Sin(MathF.Pow(factor, 4) * MathHelper.Lerp(1.674f, 1.821f, 0.5f * (MathF.Sin(Main.GameUpdateCount * 0.5f) + 1))) * 12,// * (MathF.Sin(Main.GameUpdateCount * 0.1f + factor * 16f) + 1) / 2, 
                factor => 
                {
                    Color colorMain = Color.Lerp(Color.Orange, Color.YellowGreen, Main.GameUpdateCount * 0.3f);
                    return Color.Lerp(Color.HotPink * 0.35f, colorMain, factor.X) * 0.75f * ((100 - stabTimer) / 100f) * trailFadeIn * CollideProg;
                }
                );
            trail.Positions = trailCache;
            trail.NextPosition = Projectile.Center;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            void BeginDefault() => Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            if (trail is not null)
            {
                Effect effect = Filters.Scene["GlowTrailShader"].GetShader().Shader;

                Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
                Matrix view = Main.GameViewMatrix.ZoomMatrix;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["transformMatrix"].SetValue(world * view * projection);
                effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("JadeFables/Assets/GlowTrail", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
                effect.Parameters["repeats"].SetValue(0.2f);
                effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.1f);

                Main.spriteBatch.End();

                trail.Render(effect);

                BeginDefault();
            }

            Vector2 KunaiOrigin(Texture2D tex) => new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);

            Texture2D kunaiTexture = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(
                kunaiTexture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * CollideProg,
                Projectile.rotation,
                KunaiOrigin(kunaiTexture),
                new Vector2(Projectile.scale + 0.33f * Projectile.velocity.LengthSquared() / 100, 
                Projectile.scale + MathF.Pow(stabImpactTimer, 4) * 0.75f),
                SpriteEffects.None,
                0
                );

            Texture2D glowTex = Request<Texture2D>(Texture + "_WeirdGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            Color color = Color.Lerp(Color.Green, Color.White, stabImpactTimer) * ((0.4f + stabImpactTimer * 0.7f) + (MathF.Sin(Main.GameUpdateCount * 0.05f) / 8));
            color.A = 0;
            Main.EntitySpriteDraw(
                glowTex,
                Projectile.Center - Main.screenPosition,
                null,
                color * CollideProg,
                Projectile.rotation,
                KunaiOrigin(glowTex),
                Projectile.scale,
                SpriteEffects.None,
                0
                );


            return false;
        }
    }

    public class JadeKunaiDust : ModDust
    {
        static Terraria.Graphics.Shaders.ArmorShaderData? glowDustShader;
        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, 0, 1, 64);
            dust.shader = glowDustShader ??= new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(JadeFables.Instance.Assets.Request<Effect>("Effects/GlowingDust", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value), "GlowingDustPass");
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.rotation = dust.velocity.ToRotation();
            dust.position += dust.velocity;

            dust.frame.Width = (int)MathHelper.Lerp(1, 64, dust.velocity.LengthSquared() * 0.05f);
            dust.frame.X = 64 - dust.frame.Width;

            dust.velocity *= 0.98f;

            dust.color *= 0.9f;
            dust.scale *= 0.9f;
            if (dust.scale < 0.03f)
            {
                dust.active = false;
            }

            dust.shader.UseColor(dust.color);

            return false;
        }
    }

    public class JadeKunaiStackNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;


        public float flashOpacity = 0f;

        public int KunaiStack { get; set; }

        public override void ResetEffects(NPC npc)
        {
            flashOpacity -= 0.03f;
        }
    }
}
