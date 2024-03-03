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
            Item.mana = 12;

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
        private float dragonDamageMultiplier = 2.5f;

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
        private int portalFrame = 0;
        private bool portalOpened = false; //When the portal has finished opening 
        private int previousQuadrant = 3; //1, 2, 3 or 4 (don't change initail value to 1 or 2)
        private int timeAfterDragonSpawned = 0;
        private bool fading = false;

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
            if (!thrownDragon && owner.channel)
            {

                if (soundTimer++ % 45 == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaivePierce with { PitchVariance = 0.3f, Pitch = -0.2f, Volume = 0.3f }, owner.Center);
                }

                //Fire Pulse
                if (timer % 60 == 0 && timer != 0 && owner.CheckMana(owner.inventory[owner.selectedItem], pay: true))
                {

                    SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_flameburst_tower_shot_" + (Main.rand.NextBool() ? 0 : 1)) with { Pitch = -.48f, PitchVariance = 0.3f, Volume = 0.6f };
                    SoundEngine.PlaySound(style, Projectile.Center);

                    Projectile a = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<NewJadeStaffPulse>(), Projectile.damage, 0, owner.whoAmI);

                    if (a.ModProjectile is NewJadeStaffPulse njsp)
                        njsp.size = 0.45f;

                    if (a.ModProjectile is JadeStaffFirePulse fire)
                        fire.rotDir = owner.direction == 1;
                }

                Projectile.scale += 0.05f;
                Projectile.scale = MathHelper.Min(Projectile.scale, 1);
            }
            else if ((!owner.channel && dragon == null) || thrownDragon)
            {
                fading = true;
                portalScale = Math.Clamp(MathHelper.Lerp(portalScale, -0.1f, 0.1f), 0, 1);

                Projectile.scale -= 0.05f;
                if (Projectile.scale <= 0)
                    Projectile.active = false;
            }
            if ((!owner.channel || released) && Projectile.scale >= 1)
            {
                released = true;
                float rotDifference = (((((rotation + (1.57f * owner.direction)) - owner.DirectionTo(Main.MouseWorld).ToRotation()) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
                if (!thrownDragon && Math.Abs(rotDifference) < 0.2f && timeAfterDragonSpawned > 20)
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

            rotation += 0.12f * owner.direction; //0.12
            owner.itemAnimation = owner.itemTime = 2;
            Projectile.rotation = rotation;
            Projectile.velocity = Vector2.Zero;

            owner.itemRotation = Projectile.rotation;

            if (owner.direction != 1)
                owner.itemRotation -= 3.14f;

            //owner.heldProj = Projectile.whoAmI; Uncommenting this make thye arm just fuckin disappear for some reason
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
            Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);

            //Portal Stuff
            if (timer > 150)
            {
                if (timer % 6 == 0)
                {
                    if (portalFrame == 3)
                        portalFrame = 0;
                    else
                        portalFrame++;
                }

                drawPortal = true;

                if (timeAfterDragonSpawned >= 20)
                    portalScale = Math.Clamp(MathHelper.Lerp(portalScale, -0.1f, 0.1f), 0, 1);
                else if (owner.channel)
                    portalScale = Math.Clamp(MathHelper.Lerp(portalScale, 1.2f, 0.1f), 0, 1);


                if (portalScale == 1)
                    portalOpened = true;

                if (portalOpened)
                {
                    //Looks dumb as fuck but is very important
                    //Contains the rotation values within +- Pi
                    //float containedRotation = Projectile.rotation.ToRotationVector2().ToRotation();

                    int currentQuadrant = getQuadrant(owner, Projectile.rotation.ToRotationVector2());
                    //Quadrant 1 -> 2 and 2 <- 1 
                    bool OneToTwo = (owner.direction == 1 && (currentQuadrant == 1 && previousQuadrant == 2));
                    bool TwoToOne = (owner.direction == -1 && (currentQuadrant == 2 && previousQuadrant == 1));

                    if (OneToTwo || TwoToOne)
                    {

                        if (dragon == null && !fading)
                        {
                            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_betsy_flame_breath") with { Pitch = .28f, Volume = 0.2f };
                            SoundEngine.PlaySound(style, Projectile.Center);

                            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Custom/dd2_betsy_summon_1") with { Pitch = .44f, };
                            SoundEngine.PlaySound(style2, Projectile.Center);

                            for (int i = 0; i < 10; i++) //4 //2,2
                            {

                                Dust d = Dust.NewDustPerfect(owner.Center + rotation.ToRotationVector2() * 80, ModContent.DustType<RoaParticle>(),
                                    new Vector2(1 * owner.direction, 0f).RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.Next(2, 8),
                                    newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.4f, .9f) * 1.5f);
                            }

                            for (int i = 0; i < 4; i++) //4 //2,2
                            {

                                Dust d = Dust.NewDustPerfect(owner.Center + rotation.ToRotationVector2() * 80, ModContent.DustType<RoaParticle>(),
                                    new Vector2(1 * owner.direction, 0f).RotatedBy(Main.rand.NextFloat(-1.2f, 1.2f)) * Main.rand.Next(2, 8),
                                    newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.2f, .6f) * 2f);
                            }

                            dragon = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), owner.Center + rotation.ToRotationVector2() * 80, Vector2.Zero, ModContent.ProjectileType<JadeStaffDragon>(), Projectile.damage, Projectile.knockBack, owner.whoAmI);
                        }

                    }

                    if (dragon != null && dragon.active && !thrownDragon)
                    {
                        timeAfterDragonSpawned++;
                        dragon.timeLeft = 130;
                        dragon.Center = owner.Center + rotation.ToRotationVector2() * 80;
                        (dragon.ModProjectile as JadeStaffDragon).flip = owner.direction == -1;
                    }

                    previousQuadrant = getQuadrant(owner, Projectile.rotation.ToRotationVector2());
                }
            }

            timer++;
            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }
        }

        bool drawPortal = false;
        float portalScale = 0f;
        public override bool PreDraw(ref Color lightColor)
        {
            DrawPrimitives();
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            Color glowColor = new Color(200, 69, 0);
            glowColor.A = 0;
            Vector2 glowPos = Projectile.Center + ((rotation - (0.1f * owner.direction)).ToRotationVector2() * 40 * Projectile.scale) + new Vector2(0, owner.gfxOffY);

            float glowScale = (Projectile.scale + (0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.25f))) * 0.7f;

            Main.spriteBatch.Draw(glowTex, glowPos - Main.screenPosition, null, glowColor, 0, glowTex.Size() / 2, glowScale, SpriteEffects.None, 0f);

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0, owner.gfxOffY) - Main.screenPosition, null, lightColor, (Projectile.rotation + 0.78f) + (owner.direction == -1 ? 0f : 1.57f), new Vector2(owner.direction == -1 ? 0 : tex.Width, tex.Height), Projectile.scale, owner.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            if (!drawPortal)
                return false;

            //Portal Drawing
            Texture2D Portal = Mod.Assets.Request<Texture2D>("Items/Jade/JadeStaff/JadeFirePortal").Value;
            Texture2D White = Mod.Assets.Request<Texture2D>("Items/Jade/JadeStaff/JadeFirePortalWhite").Value;
            Texture2D PortalGlow = Mod.Assets.Request<Texture2D>("Items/Jade/JadeStaff/JadeFirePortalGlow2").Value;
            Texture2D Glorb = Mod.Assets.Request<Texture2D>("Assets/Projectile_540").Value;

            Vector2 spawnPos = (owner.Center - Main.screenPosition + new Vector2(-2, -88f));

            Vector2 scaleVec2 = new Vector2(portalScale * 0.5f, portalScale * 1.2f) * 0.8f;
            Color glorbCol = Color.OrangeRed;
            glorbCol.A = 0;
            Main.spriteBatch.Draw(Glorb, spawnPos + new Vector2(0.5f, 7f), null, glorbCol * 0.2f, 0f, Glorb.Size() / 2, scaleVec2 * 1.1f, SpriteEffects.None, 0f);

            Vector2 scalePortalVec2 = new Vector2(portalScale, 1f) * 0.8f;
            Rectangle sFrame = new Rectangle(0, Portal.Height / 4 * portalFrame, Portal.Width, (Portal.Height / 4));
            Main.spriteBatch.Draw(Portal, new Vector2((int)spawnPos.X, (int)spawnPos.Y), sFrame, Color.White * 0.9f, 0f, sFrame.Size() / 2, scalePortalVec2 * 0.75f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(White, new Vector2((int)spawnPos.X, (int)spawnPos.Y), sFrame, Color.White * (1 - portalScale), 0f, sFrame.Size() / 2, scalePortalVec2 * 0.75f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(PortalGlow, new Vector2((int)spawnPos.X, (int)spawnPos.Y), sFrame, Color.White with { A = 0 } * portalScale, 0f, sFrame.Size() / 2, scalePortalVec2 * 0.75f, SpriteEffects.None, 0f);



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
            fireEffect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.075f);
            fireEffect.Parameters["repeats"].SetValue(2);
            trail.Render(fireEffect);

            fireEffect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("JadeFables/Assets/s06sBloom").Value);
            trail.Render(fireEffect);


            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        public override bool? CanDamage() { return false; }
        public override bool? CanCutTiles() { return false; }

        //Returns the mathematical quadrant of the vec2, with the player as the origin
        public int getQuadrant(Player myPlayer, Vector2 input)
        {
            bool LesserX = (input + myPlayer.Center).X < myPlayer.Center.X;
            bool LesserY = (input + myPlayer.Center).Y < myPlayer.Center.Y;

            if (!LesserX && LesserY) return 1;
            if (LesserX && LesserY) return 2;
            if (LesserX && !LesserY) return 3;
            if (!LesserX && !LesserY) return 4;

            return -1;
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
        private Vector2 storedVel = Vector2.Zero;

        public bool flip;

        private float burnProgress => EaseFunction.EaseCircularIn.Ease(1 - (Projectile.timeLeft / 130f));

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override bool? CanDamage()
        {
            if (timeAfterLaunch == 0)
                return true;

            return timeAfterLaunch >= 55 && hitStop <= 0;
        }

        int timeAfterLaunch = 0;
        float velValue = 8.5f;
        float eyeRot = MathHelper.PiOver4;
        float eyeScale = 0.75f;
        int hitStop = 0;

        public override void AI()
        {
            if (hitStop > 0)
            {
                hitStop--;
                Projectile.timeLeft++;
                Projectile.velocity = Vector2.Zero;
                if (hitStop == 0)
                {
                    Projectile.velocity = storedVel;
                }

                return;
            }

            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }

            if (timeAfterLaunch < 45 && timeAfterLaunch > 1) // < 25
            {
                Projectile.velocity = Projectile.velocity.MoveTowards((Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 8, 1f); //(Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 8;
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

            if (timeAfterLaunch > 30 && timeAfterLaunch < 55)
            {
                Main.spriteBatch.Draw(eye, Projectile.Center - Main.screenPosition + eyePos, eye.Frame(1, 1, 0, 0), Color.OrangeRed with { A = 0 }, Projectile.rotation + eyeRot, eye.Size() / 2, eyeScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(eye, Projectile.Center - Main.screenPosition + eyePos, eye.Frame(1, 1, 0, 0), Color.OrangeRed with { A = 0 }, Projectile.rotation + eyeRot * -1, eye.Size() / 2, eyeScale, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(eye, Projectile.Center - Main.screenPosition + eyePos, eye.Frame(1, 1, 0, 0), Color.White with { A = 0 }, Projectile.rotation + eyeRot, eye.Size() / 2, eyeScale * 0.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(eye, Projectile.Center - Main.screenPosition + eyePos, eye.Frame(1, 1, 0, 0), Color.White with { A = 0 }, Projectile.rotation + eyeRot * -1, eye.Size() / 2, eyeScale * 0.5f, SpriteEffects.None, 0f);
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
            Projectile.damage *= 2;
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            if (timeAfterLaunch >= 55)
            {
                hitStop = 3;
                storedVel = Projectile.velocity;

                SoundStyle style = new SoundStyle("JadeFables/Sounds/BurnBurst") with { Pitch = .56f, PitchVariance = .14f, Volume = 0.7f, MaxInstances = 1 };
                SoundEngine.PlaySound(style, target.Center);

                SoundStyle style2 = new SoundStyle("JadeFables/Sounds/FireAttack_Med") with { Pitch = .13f, PitchVariance = .24f, MaxInstances = 1, Volume = 0.7f };
                SoundEngine.PlaySound(style2, target.Center);

                /*
                int directUpward = Projectile.velocity.X > 0 ? -1 : 1;
                for (int i = 0; i < 14; i++)
                {
                    Dust d = Dust.NewDustPerfect(target.Center, DustType<Glow>(),
                        (Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Math.PI / 2 * directUpward)).RotatedByRandom(0.25f) * Main.rand.NextFloat(1f, 5f),
                        newColor: Main.rand.NextBool() ? Color.OrangeRed : Color.DarkOrange, Scale: 0.5f);
                    d.position += d.velocity * 2;
                }

                for (int i = 0; i < 11; i++)
                {
                    int dust = Dust.NewDust(target.position, target.width, target.height, DustType<GlowFastDecelerate>(), newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.8f, 1.2f));
                    Main.dust[dust].velocity = Vector2.Normalize(target.Center - Main.dust[dust].position) * -1 * Main.rand.NextFloat(2f, 6f);

                }
                */

                //Impact
                for (int i = 0; i < 12; i++)
                {
                    Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustType<RoaParticle>(), randomStart, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.75f, 1.15f));

                    dust.noLight = false;

                }

                for (int i = 0; i < 20; i++)
                {
                    Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.DarkOrange;
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 9f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(1f, 1.75f));
                    d.fadeIn = Main.rand.Next(0, 4);
                    d.alpha = Main.rand.Next(0, 2);
                    d.noLight = false;

                }

                target.AddBuff(BuffID.OnFire, 120);
            }
            else if (timeAfterLaunch == 0)
            {
                Vector2 toTarget = (target.Center - owner.Center).SafeNormalize(Vector2.UnitX);

                for (int i = 0; i < 13; i++)
                {
                    Dust d = Dust.NewDustPerfect(target.Center, DustType<RoaParticle>(),
                        toTarget.RotatedByRandom(0.75f) * Main.rand.NextFloat(1f, 4f),
                        newColor: Main.rand.NextBool() ? Color.OrangeRed : Color.DarkOrange, Scale: 0.85f);
                    d.position += d.velocity * 2;
                }
            }

            //base.OnHitNPC(target, damage, knockback, crit);
        }
    }

    internal class JadeStaffFirePulse : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
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

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_betsy_fireball_shot_1") with { Pitch = -.53f, PitchVariance = 0.35f, Volume = 0.5f };
            SoundEngine.PlaySound(style, target.Center);


            target.AddBuff(BuffID.OnFire, 120);
        }
    }

    // OK TO DELETE, just used for testing so delete this if I forgot
    internal class JadeStaffPortal : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

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
            Projectile.timeLeft = 450;
        }

        private int timer = 0;
        private bool maxSizeRunOnce = false;


        public override void AI()
        {

            scale = Math.Clamp(MathHelper.Lerp(scale, 1.2f, 0.1f), 0, 1);

            if (scale == 1 && !maxSizeRunOnce)
            {
                maxSizeRunOnce = true;

                //Fart Dust heheh
            }

            if (timer % 6 == 0)
            {
                if (portalFrame == 3)
                    portalFrame = 0;
                else
                    portalFrame++;
            }
            timer++;
        }

        float scale = 0f;
        float alpha = 1f;
        int portalFrame = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Portal = Mod.Assets.Request<Texture2D>("Items/Jade/JadeStaff/JadeFirePortal").Value;
            Texture2D White = Mod.Assets.Request<Texture2D>("Items/Jade/JadeStaff/JadeFirePortalWhite").Value;
            Texture2D Glorb = Mod.Assets.Request<Texture2D>("Assets/Projectile_540").Value;

            Vector2 scaleVec2 = new Vector2(scale * 0.5f, scale * 1.2f);
            Color glorbCol = Color.OrangeRed;
            glorbCol.A = 0;
            Main.spriteBatch.Draw(Glorb, Projectile.Center - Main.screenPosition + new Vector2(1f, 10), null, glorbCol * 0.4f, 0f, Glorb.Size() / 2, scaleVec2 * 1.1f, SpriteEffects.None, 0f);

            Vector2 scalePortalVec2 = new Vector2(scale, 1f);
            Rectangle sFrame = new Rectangle(0, Portal.Height / 4 * portalFrame, Portal.Width, (Portal.Height / 4));
            Vector2 spawnPos = (Projectile.Center - Main.screenPosition);
            Main.spriteBatch.Draw(Portal, new Vector2((int)spawnPos.X, (int)spawnPos.Y), sFrame, Color.White, 0f, sFrame.Size() / 2, scalePortalVec2 * 0.7f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(White, new Vector2((int)spawnPos.X, (int)spawnPos.Y), sFrame, Color.White * (1 - scale), 0f, sFrame.Size() / 2, scalePortalVec2 * 0.7f, SpriteEffects.None, 0f);

            return false;
        }
    }

    internal class NewJadeStaffPulse : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        int timer = 0;
        public float opacity = 1f;
        public float size = 1f;
        public bool maxPower = false;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
            Projectile.tileCollide = false;
            Projectile.scale = 0.1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; 
        }

        Vector2 startingCenter;
        public override void AI()
        {
            if (timer == 0)
            {
                startingCenter = Projectile.Center;
                timer = Main.rand.Next(0, 200);
            }

            timer++;

            Projectile.scale = MathHelper.Clamp(MathHelper.Lerp(Projectile.scale, 1.25f * size, 0.08f), 0f, 1.25f * size);

            if (Projectile.scale >= 0.8f * size)
                opacity = MathHelper.Clamp(MathHelper.Lerp(opacity, -0.2f, 0.15f), 0, 2);

            if (opacity <= 0)
                Projectile.active = false;

            Projectile.width = (int)(425 * Projectile.scale);
            Projectile.height = (int)(425 * Projectile.scale);
            Projectile.Center = startingCenter;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Items/Jade/JadeStaff/ElectricPopDA").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Items/Jade/JadeStaff/ElectricPopE").Value;

            float scale = Projectile.scale * 0.25f;
            float timeFade = 1f - (0.25f * (Projectile.scale / size));

            float timeA = timer * 0.045f * timeFade;
            float timeB = timer * -0.07f * timeFade;


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.DarkOrange with { A = 0 } * opacity * 0.35f, timeA, Tex.Size() / 2, scale * 1.65f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.DarkOrange with { A = 0 } * opacity * 0.35f, timeB, Tex.Size() / 2, scale * 1.65f + (0.15f * scale), SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.Red * opacity, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.OrangeRed * opacity, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity * 2f, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), Color.OrangeRed * opacity * 2f, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity * 2f, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), Color.OrangeRed * opacity * 2f, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 170);
            Projectile.damage = (int)(Projectile.damage * 0.95f);
        }
    }

}