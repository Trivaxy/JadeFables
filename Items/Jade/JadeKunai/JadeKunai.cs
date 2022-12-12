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
        public override string Texture => "Terraria/Images/Item_" + ItemID.ThrowingKnife;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.useTime = Item.useAnimation = 24;

            Item.noUseGraphic = true;

            Item.autoReuse = true;

            Item.DamageType = DamageClass.Throwing;
            Item.damage = 8;
            Item.knockBack = 5f;
            Item.crit = 4;

            Item.shoot = ProjectileType<JadeKunaiProjectile>();
            Item.shootSpeed = 10;

            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Blue;

            Item.UseSound = SoundID.Item1;
        }

        public Projectile? lastShotKunai;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            lastShotKunai = Projectile.NewProjectileDirect(
                source,
                position,
                velocity,
                type,
                damage,
                knockback,
                player.whoAmI
                );

            lastShotKunai.tileCollide = false;

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

            float mouseRot = directionToMouse.ToRotation();
            float rotFunc = MathF.Sin(MathF.Pow((float)player.itemAnimation / player.itemAnimationMax, 5) * 1.095f);
            float armRotation = mouseRot + swingDirection * (MathHelper.Pi * rotFunc - MathHelper.PiOver2);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation - MathHelper.PiOver2);

            if (lastShotKunai is not null)
            {
                float armLen = 30;
                player.heldProj = lastShotKunai.whoAmI;
                if (rotFunc < 0.45f)
                {
                    lastShotKunai.velocity = directionToMouse * lastShotKunai.velocity.Length();
                    lastShotKunai.tileCollide = true;
                    lastShotKunai = null;
                }
                else
                {
                    lastShotKunai.Center = shoulderPos + armRotation.ToRotationVector2() * armLen;
                    lastShotKunai.rotation = shoulderPos.DirectionTo(lastShotKunai.Center).ToRotation();
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
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 2;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
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

                Projectile.velocity.Y += 0.005f;
                Projectile.rotation = Projectile.velocity.ToRotation();
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

        public override bool PreAI() => !(Main.player[Projectile.owner]?.HeldItem?.ModItem is JadeKunai kunaiItem && kunaiItem.lastShotKunai?.whoAmI == Projectile.whoAmI);
        public override bool ShouldUpdatePosition() => PreAI() && !StabActive;

        bool StabActive => stabbedTarget is not null && stabbedTarget.life > 0 && stabbedTarget.active;
        NPC? stabbedTarget;
        Vector2 sTOffset;
        float stabImpactTimer;
        public override bool? CanHitNPC(NPC target) => !StabActive;
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            stabImpactTimer = 1;
            stabbedTarget = target;
            sTOffset = Projectile.Center - target.Center;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) 
        {
            if (Main.rand.NextBool(3))
            {
                Item.NewItem(Projectile.GetSource_Death(), Projectile.Hitbox, ItemType<JadeKunai>());
            }
            return true;
        }

        private Vector2[]? trailCache;
        private const int TrailCacheLenght = 7;
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
            else if (stabbedTarget is null)
            {
                for (int i = 1; i < trailCache.Length; i++)
                {
                    trailCache[i - 1] = trailCache[i];
                }
                trailCache[^1] = Projectile.Center;
            }
            else
            {
                for (int i = 0; i < trailCache.Length; i++)
                {
                    trailCache[i] = trailCache[i];
                }
            }
        }
        private float trailFadeIn = 0;
        private Trail? trail;
        private void ManageTrail()
        {
            trail ??= new Trail(Main.instance.GraphicsDevice, trailCache.Length, new TriangularTip(3), factor => MathF.Sin(factor * factor * 1.7f) * 16 * (MathF.Sin(Main.GameUpdateCount * 0.35f + factor * 16f) + 1) / 2, 
                factor => 
                {
                    Color colorMain = Color.Lerp(Color.Orange, Color.YellowGreen, Main.GameUpdateCount * 0.3f);
                    return Color.Lerp(Color.HotPink * 0.35f, colorMain, factor.X) * 0.75f * ((100 - stabTimer) / 100f) * trailFadeIn;
                }
                );
            trail.Positions = trailCache;
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

            Vector2 KunaiOrigin(Texture2D tex) => new Vector2(tex.Width * 0.66f, tex.Height * 0.5f);

            Texture2D kunaiTexture = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(
                kunaiTexture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                KunaiOrigin(kunaiTexture),
                new Vector2(Projectile.scale + 0.33f * Projectile.velocity.LengthSquared() / 100, Projectile.scale + MathF.Pow(stabImpactTimer, 4) * 0.75f),
                SpriteEffects.None,
                0
                );

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            Texture2D glowTex = Request<Texture2D>(Texture + "_WeirdGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Main.EntitySpriteDraw(
                glowTex,
                Projectile.Center - Main.screenPosition,
                null,
                Color.Lerp(Color.MistyRose, Color.White, (MathF.Sin(Main.GameUpdateCount * 0.05f) + 1) / 2) * (0.45f + stabImpactTimer * 0.7f),
                Projectile.rotation,
                KunaiOrigin(glowTex),
                Projectile.scale / 5f,
                SpriteEffects.None,
                0
                );

            Main.spriteBatch.End();
            BeginDefault();

            return false;
        }
    }
}
