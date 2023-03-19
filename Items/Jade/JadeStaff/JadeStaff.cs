//TODO:
//Description
//Balance
//Some sort of chargeup mechanic
//Visuals
using JadeFables.Core;
using JadeFables.Dusts;
using JadeFables.Helpers;
using JadeFables.Items.SpringChestLoot.FireworkPack;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace JadeFables.Items.Jade.JadeStaff
{
    class JadeStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Staff");
            Tooltip.SetDefault("update later");
        }

        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.DamageType = DamageClass.Magic;
            Item.width = 16;
            Item.height = 64;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1;
            Item.channel = true;
            Item.shoot = ProjectileType<JadeStaffProj>();
            Item.shootSpeed = 0f;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.channel = true;

            Item.value = Item.sellPrice(silver: 45);
            Item.rare = ItemRarityID.Blue;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<JadeStaffProj>());
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<JadeChunk.JadeChunk>(12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    internal class JadeStaffProj : ModProjectile
    {

        private readonly int NUMPOINTS = 20;
        private List<Vector2> cache;
        private Trail trail;

        private Player owner => Main.player[Projectile.owner];

        private float rotation;

        private int soundTimer;
        private int timer = 0;

        private Projectile dragon;

        private bool released = false;
        private bool thrownDragon = false;
        private int pulsesCompleted = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Staff");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.scale = 0;
        }

        public override void AI()
        {
            if (!thrownDragon)
            {
                
                if (dragon == null)
                {
                    dragon = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), owner.Center + rotation.ToRotationVector2() * 80, Vector2.Zero, ModContent.ProjectileType<JadeStaffDragon>(), Projectile.damage * 2, Projectile.knockBack, owner.whoAmI);
                }
                if (dragon != null && dragon.active)
                {
                    dragon.timeLeft = 130;
                    dragon.Center = owner.Center + rotation.ToRotationVector2() * 80;
                    (dragon.ModProjectile as JadeStaffDragon).flip = owner.direction == -1;
                }
                

                if (soundTimer++ % 18 == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing with { PitchVariance = 0.3f, Pitch = 0.2f, Volume = 0.35f }, owner.Center);
                }

                //Fire Pulse
                if (timer % 60 == 0 && timer != 0)
                {

                    SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_betsy_fireball_shot_0") with { Pitch = -.53f, MaxInstances = -1, Volume = 0.8f };
                    SoundEngine.PlaySound(style, Projectile.Center);

                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<JadeStaffFirePulse>(), Projectile.damage, 0, owner.whoAmI);
                    pulsesCompleted++;
                }

                Projectile.scale += 0.05f;
                Projectile.scale = MathHelper.Min(Projectile.scale, 1);
            }
            else
            {
                Projectile.scale -= 0.05f;
                if (Projectile.scale <= 0)
                    Projectile.active = false;
            }
            if ((!owner.channel || released) && Projectile.scale >= 1)
            {
                released = true;
                float rotDifference = (((((rotation + (1.57f * owner.direction)) - owner.DirectionTo(Main.MouseWorld).ToRotation()) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
                if (!thrownDragon && Math.Abs(rotDifference) < 0.2f)
                {
                    thrownDragon = true;
                    if (dragon != null)
                    {
                        (dragon.ModProjectile as JadeStaffDragon).Launch();
                        dragon.velocity = dragon.DirectionTo(Main.MouseWorld) * 8.5f;
                    }
                }
            }
            Projectile.timeLeft = 2;

            rotation += 0.15f * owner.direction; //0.15
            owner.itemAnimation = owner.itemTime = 2;
            Projectile.rotation = rotation;
            Projectile.velocity = Vector2.Zero;

            owner.itemRotation = Projectile.rotation;

            if (owner.direction != 1)
                owner.itemRotation -= 3.14f;

            owner.heldProj = Projectile.whoAmI;
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
            Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
            timer++;
            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawPrimitives();
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            Color glowColor = new Color(200,69,0);
            glowColor.A = 0;
            Vector2 glowPos = Projectile.Center + ((rotation - (0.1f * owner.direction)).ToRotationVector2() * 40 * Projectile.scale) + new Vector2(0, owner.gfxOffY);

            float glowScale = (Projectile.scale + (0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.25f))) * 0.7f;

            Main.spriteBatch.Draw(glowTex, glowPos - Main.screenPosition, null, glowColor, 0, glowTex.Size() / 2, glowScale, SpriteEffects.None, 0f);

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0, owner.gfxOffY) - Main.screenPosition, null, lightColor, (Projectile.rotation + 0.78f) + (owner.direction == -1 ? 0f : 1.57f), new Vector2(owner.direction == -1 ? 0 : tex.Width, tex.Height), Projectile.scale, owner.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            //Portal
            Texture2D texture = Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Misc/Ranged/Bows/TheSaharaNoString").Value;


            return false;
        }

        private void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < NUMPOINTS; i++)
                {
                    cache.Add(Projectile.Center + (rotation.ToRotationVector2() * 46 * Projectile.scale));
                }
            }
            cache.Add(Projectile.Center + (rotation.ToRotationVector2() * 46 * Projectile.scale));
            while (cache.Count > NUMPOINTS)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUMPOINTS, new TriangularTip(2), factor => 18 * factor, factor =>
            {
                return Color.OrangeRed * (1 - factor.X);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + (rotation.ToRotationVector2() * 46 * Projectile.scale);
        }

        private void DrawPrimitives()
        {
            if (trail == null || trail == default)
                return;

            Main.spriteBatch.End();

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            Effect fireEffect = Filters.Scene["EnergyTrail"].GetShader().Shader;
            fireEffect.Parameters["transformMatrix"].SetValue(world * view * projection);
            fireEffect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("JadeFables/Assets/FireTrail").Value);
            fireEffect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.1f);
            fireEffect.Parameters["repeats"].SetValue(2);
            trail.Render(fireEffect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    internal class JadeStaffDragon : ModProjectile
    {
        private readonly int NUMPOINTS = 20;
        private readonly int NUMPOINTS2 = 30;
        private List<Vector2> cache;
        private Trail trail;

        private List<Vector2> cache2;
        private Trail trail2;
        private Player owner => Main.player[Projectile.owner];

        private bool launched = false;

        public bool flip;

        private float burnProgress => EaseFunction.EaseCircularIn.Ease(1 - (Projectile.timeLeft / 130f));

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Staff");
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400; 
        }

        int timeAfterLaunch = 0;
        float velValue = 8.5f;
        float eyeRot = MathHelper.PiOver4;
        float eyeScale = 0.75f;
        public override void AI()
        {
            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }

            if (timeAfterLaunch < 45 && timeAfterLaunch > 1) // < 25
            {
                Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 8;
            }

            if (launched)
            {
                Projectile.rotation = Projectile.velocity.ToRotation(); 

                if (timeAfterLaunch >= 25 && timeAfterLaunch < 55)
                {
                    velValue = MathHelper.Lerp(velValue, 4, 0.04f);
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * velValue;
                    //Projectile.velocity = Projectile.rotation.ToRotationVector2() * -1 * velValue;

                }
                else if (timeAfterLaunch >= 55 && timeAfterLaunch < 60)
                {
                    if (timeAfterLaunch == 55)
                    {

                        int a = Projectile.NewProjectile(null, Projectile.Center, Projectile.velocity * -0.5f, ModContent.ProjectileType<JadeStaffFirePulse>(), 0, 0, Projectile.owner);

                        if (Main.projectile[a].ModProjectile is JadeStaffFirePulse pulse)
                        {
                            pulse.dim = new Vector2(0.25f, 1f) * 0.5f;
                            pulse.oval = true;
                            pulse.canDamage = false;
                        }
                        
                        SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, Projectile.Center);

                    }
                    velValue = Math.Clamp(MathHelper.Lerp(velValue, 56, 0.5f), 0, 40);
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * velValue;
                    //Projectile.velocity = Projectile.rotation.ToRotationVector2() * -1 * velValue;

                }
                else if (timeAfterLaunch >= 60)
                {
                    velValue = MathHelper.Lerp(velValue, 8, 0.2f);
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * velValue;
                    //Projectile.velocity = Projectile.rotation.ToRotationVector2() * -1 * velValue;
                }

                if (timeAfterLaunch > 30)
                {

                    if (timeAfterLaunch == 31)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
                    }

                    eyeRot = Math.Clamp(MathHelper.Lerp(eyeRot, MathHelper.TwoPi + 0.7f, 0.07f), MathHelper.PiOver4, MathHelper.TwoPi);
                    float progress = eyeRot / MathHelper.TwoPi; //from 0-1;

                    eyeScale = 0.25f + (float)Math.Sin(progress * Math.PI) * 1.2f;   //sin(pi * x) always gives you a sin wave on values from 0-1
                }

                timeAfterLaunch++;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawPrimitives();

            //Eye
            Texture2D eye = ModContent.Request<Texture2D>("JadeFables/Items/Jade/JadeStaff/DragonEyeAlt").Value;
            Vector2 eyePos = new Vector2(-50 * (Projectile.velocity.Length() / 13), 10 * (flip ? 1 : -1)).RotatedBy(Projectile.rotation);

            //Vector2 eyeScale = new Vector2(0.5f, (Projectile.velocity.Length() / 13) * 0.5f);

            if (timeAfterLaunch > 30 && timeAfterLaunch < 55)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

                Main.spriteBatch.Draw(eye, Projectile.Center - Main.screenPosition + eyePos, eye.Frame(1, 1, 0, 0), Color.OrangeRed, Projectile.rotation + eyeRot, eye.Size() / 2, eyeScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(eye, Projectile.Center - Main.screenPosition + eyePos, eye.Frame(1, 1, 0, 0), Color.OrangeRed, Projectile.rotation + eyeRot * -1, eye.Size() / 2, eyeScale, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(eye, Projectile.Center - Main.screenPosition + eyePos, eye.Frame(1, 1, 0, 0), Color.White, Projectile.rotation + eyeRot, eye.Size() / 2, eyeScale * 0.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(eye, Projectile.Center - Main.screenPosition + eyePos, eye.Frame(1, 1, 0, 0), Color.White, Projectile.rotation + eyeRot * -1, eye.Size() / 2, eyeScale * 0.5f, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
        private void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < NUMPOINTS; i++)
                {
                    cache.Add(Projectile.Center - owner.Center);
                }

                cache2 = new List<Vector2>();
                for (int i = 0; i < NUMPOINTS2; i++)
                {
                    cache2.Add(Projectile.Center - owner.Center);
                }
            }
            if (launched)
                cache.Add(Projectile.Center);
            else
                cache.Add(Projectile.Center - owner.Center);

            if (launched)
                cache2.Add(Projectile.Center);
            else
                cache2.Add(Projectile.Center - owner.Center);

            while (cache.Count > NUMPOINTS)
            {
                cache.RemoveAt(0);
            }

            while (cache2.Count > NUMPOINTS2)
            {
                cache2.RemoveAt(0);
            }
        }

        public void Launch()
        {
            launched = true;
            List<Vector2> newCache = new List<Vector2>();
            foreach (Vector2 point in cache)
            {
                newCache.Add(point + owner.Center);
            }
            cache = newCache;

            newCache = new List<Vector2>();
            foreach (Vector2 point in cache2)
            {
                newCache.Add(point + owner.Center);
            }
            cache2 = newCache;
        }
        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUMPOINTS, new TriangularTip(2), factor => 23, factor =>
            {
                return Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, NUMPOINTS2, new TriangularTip(2), factor => 23 * factor, factor =>
            {
                return Color.OrangeRed * (1 - factor.X) * (Projectile.timeLeft / 90f);
            });

            if (!launched)
            {
                List<Vector2> newCache = new List<Vector2>();
                foreach (Vector2 point in cache)
                {
                    newCache.Add(point + owner.Center);
                }
                trail.Positions = newCache.ToArray();
            }
            else
                trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center;

            if (!launched)
            {
                List<Vector2> newCache2 = new List<Vector2>();
                foreach (Vector2 point in cache2)
                {
                    newCache2.Add(point + owner.Center);
                }
                trail2.Positions = newCache2.ToArray();
            }
            else
                trail2.Positions = cache2.ToArray();
            trail2.NextPosition = Projectile.Center;
        }

        private void DrawPrimitives()
        {
            if (trail == null || trail == default)
                return;

            Main.spriteBatch.End();
            Effect effect = Terraria.Graphics.Effects.Filters.Scene["JadeDragonShader"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture).Value);
            effect.Parameters["noiseTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_Map").Value);
            effect.Parameters["flip"].SetValue(flip);
            effect.Parameters["topIndex"].SetValue(0);
            effect.Parameters["bottomIndex"].SetValue(1);
            effect.Parameters["gradientStart"].SetValue(0);
            effect.Parameters["gradientEnd"].SetValue(1);
            effect.Parameters["multiplyNoiseScale"].SetValue(0.5f);
            effect.Parameters["generalProgress"].SetValue(burnProgress);
            effect.Parameters["disabled"].SetValue(false);
            effect.Parameters["noiseRange"].SetValue(1f);
            effect.Parameters["timeLeftSkeleton"].SetValue(1f);



            Effect fireEffect = Filters.Scene["EnergyTrail"].GetShader().Shader;
            fireEffect.Parameters["transformMatrix"].SetValue(world * view * projection);
            fireEffect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("JadeFables/Assets/FireTrail").Value);
            fireEffect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.03f);
            fireEffect.Parameters["repeats"].SetValue(2);
            trail.Render(effect);
            trail2.Render(fireEffect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    internal class JadeStaffFirePulse : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fire Pulse");
        }

        public bool oval = false;
        public Vector2 dim = new Vector2(1f, 1f);
        public bool canDamage = true;
        public bool rotDir = false;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.scale = 0.1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 45;
        }

        private int timer = 0;
        private Vector2 startingCenter = Vector2.Zero;

        public override bool? CanDamage()
        {
            if (Projectile.timeLeft < 20)
                return false;
            return canDamage;
        }
        public override void AI()
        {
            if (timer == 0 && !oval)
            {
                Projectile.rotation = Main.rand.NextFloat(6.28f);
                startingCenter = Projectile.Center;
            }

            Projectile.scale = MathHelper.Lerp(Projectile.scale, 0.7f, 0.15f); 
            Projectile.rotation += rotDir ? 0.03f : -0.03f;
            if (oval)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            if (!oval)
            {
                Projectile.width = (int)(375 * Projectile.scale);
                Projectile.height = (int)(375 * Projectile.scale);
                Projectile.Center = startingCenter;
                Projectile.velocity = Vector2.Zero;
            }


            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            
            Texture2D texture = Mod.Assets.Request<Texture2D>("Assets/energyball_10red").Value;

            Effect myEffect = ModContent.Request<Effect>("JadeFables/Effects/FireBallShader", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["caustics"].SetValue(ModContent.Request<Texture2D>("JadeFables/Items/Jade/JadeStaff/JadePulseShaderStar").Value);
            myEffect.Parameters["distort"].SetValue(ModContent.Request<Texture2D>("JadeFables/Assets/noise").Value);
            myEffect.Parameters["gradient"].SetValue(ModContent.Request<Texture2D>("JadeFables/Assets/energyball_10red").Value);
            myEffect.Parameters["uTime"].SetValue(timer * 0.03f); 

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, myEffect, Main.GameViewMatrix.TransformationMatrix);
            myEffect.CurrentTechnique.Passes[0].Apply();

            int height1 = texture.Height;
            Vector2 origin1 = new Vector2((float)texture.Width / 2f, (float)height1 / 2f);

            if (Projectile.timeLeft > 20)
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin1, Projectile.scale * dim, SpriteEffects.None, 0.0f);
            if (Projectile.timeLeft > 10)
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin1, Projectile.scale * dim, SpriteEffects.None, 0.0f);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin1, Projectile.scale * dim, SpriteEffects.None, 0.0f);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin1, Projectile.scale * dim, SpriteEffects.None, 0.0f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.HasBuff(BuffID.OnFire))
            {
                for (int i = 0; i < 15; i++)
                {
                    int dust = Dust.NewDust(target.position, target.width, target.height, DustID.Torch, newColor: Color.Red, Scale: Main.rand.NextFloat(2f, 2.2f));
                    Main.dust[dust].velocity = Vector2.Normalize(target.Center - Main.dust[dust].position) * -1 * Main.rand.NextFloat(1.5f, 4f);
                    Main.dust[dust].noGravity = true;

                }

            }

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_betsy_fireball_shot_1") with { Pitch = -.53f, PitchVariance = 0.35f };
            SoundEngine.PlaySound(style, target.Center);


            target.AddBuff(BuffID.OnFire, 120);
        }
    }

    
}