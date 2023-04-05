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
using Terraria.Audio;
using JadeFables.Core.Systems;
using Terraria.WorldBuilding;

namespace JadeFables.Items.Jade.JadeKunai
{
    public class JadeKunai : ModItem
    {
        public override void Load()
        {
            for (int j = 1; j <= 3; j++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + j);
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.useTime = Item.useAnimation = 24;

            Item.noUseGraphic = true;

            Item.autoReuse = true;

            Item.DamageType = DamageClass.Ranged;
            Item.damage = 19;
            Item.knockBack = 3f;
            Item.crit = 4;

            Item.shoot = ProjectileType<JadeKunaiProjectile>();
            Item.shootSpeed = 10;

            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Blue;
            Item.noMelee = true;

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
        public override void HoldItem(Player player)
        {
            if (player.itemTime == 0)
                return;
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
                float armLen = 10;
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

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<JadeChunk.JadeChunk>(12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
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

        bool stabbed;

        int fadeTimer;
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
                if (stabbed)
                {
                    Projectile.Kill();
                    return;
                }

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

            if (fadeTimer < 60)
                fadeTimer++;
        }

        public override bool PreAI() => true;
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
            return base.CanHitNPC(target);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var gNPC = target.GetGlobalNPC<JadeKunaiStackNPC>();
            gNPC.KunaiStack++;
            if (gNPC.KunaiStack >= 10)
            {
                if (!gNPC.getCrit)
                    gNPC.getCrit = true;
                else
                    gNPC.damageIncrease += 0.3f;


                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.type == Type && proj.ModProjectile != null && (proj.ModProjectile as JadeKunaiProjectile).stabbedTarget != null && (proj.ModProjectile as JadeKunaiProjectile).stabbedTarget == target)
                    {
                        for (int d = 0; d < 8; d++)
                        {
                            Dust.NewDustPerfect(proj.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), proj.Center.DirectionTo(Main.player[Projectile.owner].Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(5f), 0, new Color(0, Main.rand.Next(100, 255), 0, 150), Main.rand.NextFloat(0.4f, 0.8f));
                        }

                        for (int d = 1; d < 4; d++)
                        {
                            Vector2 unitY = Vector2.UnitY.RotatedByRandom(0.2f) * -Main.rand.NextFloat(1f, 3f);
                            Gore.NewGorePerfect(proj.GetSource_FromThis(), proj.Center + Main.rand.NextVector2Circular(2f, 2f), unitY + proj.Center.DirectionTo(Main.player[Projectile.owner].Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(5f), Mod.Find<ModGore>("JadeKunai_Gore" + d).Type, 1f).timeLeft = 30;
                        }

                        proj.Kill();
                    }
                }

                SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with {Pitch = 0.5f}, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Shatter with { Pitch = -0.35f }, Projectile.Center);

                Projectile.NewProjectileDirect(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, ModContent.ProjectileType<JadeKunaiHitEffect>(), 0, 0f, Projectile.owner).scale = 0.15f;
                for (int i = 0; i < 15; i++)
                {
                    for (int d = 0; d < 5; d++)
                    {
                        Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.One.RotatedBy(MathHelper.TwoPi * (d / 5f)) * MathHelper.Lerp(1f, 5f, i / 15f), 0, new Color(0, 255, 0, 100), MathHelper.Lerp(1f, 0.25f, i / 15f));
                    }
                }

                CameraSystem.Shake += 5;

                gNPC.flashOpacity = 1;
            }

            Dust.NewDustPerfect(
                Projectile.Center, 
                DustType<JadeKunaiDust>(), 
                Vector2.Zero, 
                125,
                Color.Green,
                Main.rand.NextFloat(0.75f, 1.25f)
                );

            Projectile.NewProjectileDirect(Projectile.GetSource_OnHit(target), Projectile.Center + Projectile.velocity, Vector2.Zero, ModContent.ProjectileType<JadeKunaiHitEffect>(), 0, 0f, Projectile.owner).scale = 0.05f;

            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.25f), 0, new Color(0, 255, 0, 150), 0.5f);
            }

            stabImpactTimer = 1;
            sTOffset = Projectile.Center - target.Center;
            stabbedTarget = target;
            Projectile.tileCollide = false;
            stabbed = true;
        }

        const int TimeLeftOnCollide = 40;
        float CollideProg => Math.Clamp((float)Projectile.timeLeft / TimeLeftOnCollide, 0f, 1f);
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (StabActive)
                return false;
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

                if (Projectile.timeLeft < 1970 && Projectile.tileCollide)
                    trail.Render(effect);

                BeginDefault();
            }

            Vector2 KunaiOrigin(Texture2D tex) => new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);

            float fade = fadeTimer / 60f;

            lightColor *= fade;

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

            color *= fade;

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

    class JadeKunaiHitEffect : ModProjectile
    {
        public int maxTimeLeft;
        public float originalScale;
        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(2);

            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.timeLeft = Main.rand.Next(20, 45);
            maxTimeLeft = Projectile.timeLeft;

            Projectile.tileCollide = false;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            if (Projectile.timeLeft == maxTimeLeft)
                originalScale = Projectile.scale;

            Projectile.scale = MathHelper.Lerp(originalScale, 0, 1f - Projectile.timeLeft / (float)maxTimeLeft);

            Projectile.rotation += 0.025f;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D bloom = ModContent.Request<Texture2D>("JadeFables/Assets/GlowAlpha").Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 255, 100, 0) * (Projectile.timeLeft / (float)maxTimeLeft), Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0, 0);

            Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, new Color(0, 255, 100, 0) * (Projectile.timeLeft / (float)maxTimeLeft) * 0.25f, Projectile.rotation, bloom.Size() / 2f, MathHelper.Lerp(originalScale * 25f, 0f, 1f - (Projectile.timeLeft / (float)maxTimeLeft)), 0, 0);

            return false;
        }
    }

    public class JadeKunaiStackNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;


        public float flashOpacity = 0f;

        public int KunaiStack { get; set; }

        public bool getCrit;

        public float damageIncrease;

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (getCrit)
            {
                modifiers.SetCrit();

                modifiers.ScalingBonusDamage += damageIncrease;

                getCrit = false;

                CameraSystem.Shake += 9;

                Projectile.NewProjectileDirect(item.GetSource_OnHit(npc), npc.Center, Vector2.Zero, ModContent.ProjectileType<JadeKunaiHitEffect>(), 0, 0f, player.whoAmI).scale = 0.075f;


                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDustPerfect(npc.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), npc.DirectionTo(player.Center).RotatedByRandom(0.45f) * Main.rand.NextFloat(10f), 0, new Color(0, 255, 0, 150), 0.85f);
                    Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.UnitY * -Main.rand.NextFloat(1f, 5f), 0, new Color(0, 255, 0, 150), Main.rand.NextFloat(0.3f, 0.75f));
                }

                Helper.PlayPitched("FancySwoosh", 1f, 0f, npc.Center);
                Helper.PlayPitched("FancySwoosh", 1f, 1f, npc.Center);
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.type == ModContent.ProjectileType<JadeKunaiProjectile>())
            {
                if (damageIncrease >= 0.6f)
                    modifiers.ScalingBonusDamage += 0.1f;
            }
            else
            {
                if (getCrit)
                {
                    Player player = Main.player[projectile.owner];
                    modifiers.SetCrit();

                    modifiers.ScalingBonusDamage += damageIncrease;

                    getCrit = false;

                    CameraSystem.Shake += 9;

                    Projectile.NewProjectileDirect(projectile.GetSource_OnHit(npc), npc.Center, Vector2.Zero, ModContent.ProjectileType<JadeKunaiHitEffect>(), 0, 0f, projectile.owner).scale = 0.075f;

                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDustPerfect(npc.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), -npc.DirectionTo(player.Center).RotatedByRandom(0.45f) * Main.rand.NextFloat(10f), 0, new Color(0, 255, 0, 150), 0.85f);
                        Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.UnitY * -Main.rand.NextFloat(1f, 5f), 0, new Color(0, 255, 0, 150), Main.rand.NextFloat(0.3f, 0.75f));
                    }

                    Helper.PlayPitched("FancySwoosh", 1f, 0f, npc.Center);
                    Helper.PlayPitched("FancySwoosh", 1f, 1f, npc.Center);
                }
            }
        }

        public override void ResetEffects(NPC npc)
        {
            flashOpacity -= 0.03f;

            damageIncrease = Utils.Clamp(damageIncrease, 0f, 0.6f);
            if (!getCrit)
                damageIncrease = 0f;

            KunaiStack = Utils.Clamp(KunaiStack, 0, 10);

            if (!npc.active)
            {
                KunaiStack = 0;
                damageIncrease = 0;
                getCrit = false;
            }
        }

        public override void AI(NPC npc)
        {
            if (getCrit)
            {
                int rand = 10;
                if (damageIncrease > 0)
                    rand = (int)MathHelper.Lerp(10, 2, damageIncrease / 0.6f);

                if (Main.rand.NextBool(rand))
                    Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.UnitY * -Main.rand.NextFloat(1f, 5f), 0, new Color(0, 255, 0, 150), Main.rand.NextFloat(0.3f, 0.75f));
            }
        }
    }
}
