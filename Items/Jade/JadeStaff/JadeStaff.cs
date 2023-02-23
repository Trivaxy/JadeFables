//TODO:
//Description
//Balance
//Some sort of chargeup mechanic
//Visuals
using JadeFables.Core;
using JadeFables.Dusts;
using JadeFables.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        private Projectile dragon;

        private bool released = false;
        private bool thrownDragon = false;

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
        }

        public override void AI()
        {
            if (!thrownDragon)
            {
                if (dragon == null)
                {
                    Projectile.scale = 0;
                    dragon = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), owner.Center + rotation.ToRotationVector2() * 80, Vector2.Zero, ModContent.ProjectileType<JadeStaffDragon>(), Projectile.damage, Projectile.knockBack, owner.whoAmI);
                }
                if (dragon != null && dragon.active)
                {
                    dragon.timeLeft = 90;
                    dragon.Center = owner.Center + rotation.ToRotationVector2() * 80;
                    (dragon.ModProjectile as JadeStaffDragon).flip = owner.direction == -1;
                }
                if (soundTimer++ % 30 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item1, owner.Center);
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
                        dragon.velocity = dragon.DirectionTo(Main.MouseWorld) * 13;
                    }
                }
            }
                Projectile.timeLeft = 2;

            rotation += 0.15f * owner.direction;
            owner.itemAnimation = owner.itemTime = 2;
            Projectile.rotation = rotation;
            Projectile.velocity = Vector2.Zero;

            owner.itemRotation = Projectile.rotation;

            if (owner.direction != 1)
                owner.itemRotation -= 3.14f;

            owner.heldProj = Projectile.whoAmI;
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
            Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);

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
            Color glowColor = Color.OrangeRed;
            glowColor.A = 0;
            Vector2 glowPos = Projectile.Center + ((rotation - (0.1f * owner.direction)).ToRotationVector2() * 46 * Projectile.scale) + new Vector2(0, owner.gfxOffY);

            float glowScale = (Projectile.scale + (0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.25f))) * 0.7f;
            Main.spriteBatch.Draw(glowTex, glowPos - Main.screenPosition, null, glowColor, 0, glowTex.Size() / 2, glowScale, SpriteEffects.None, 0f);

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0, owner.gfxOffY) - Main.screenPosition, null, lightColor, (Projectile.rotation + 0.78f) + (owner.direction == -1 ? 0f : 1.57f), new Vector2(owner.direction == -1 ? 0 : tex.Width, tex.Height), Projectile.scale, owner.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
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

        private float burnProgress => EaseFunction.EaseCircularIn.Ease(1 - (Projectile.timeLeft / 90f));

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
        }

        public override void AI()
        {
            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawPrimitives();
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
}