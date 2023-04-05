//TODO:
//eventual player sheet

using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using System.IO;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.Audio;
using JadeFables.Core;
using JadeFables.Helpers;
using JadeFables.Helpers.FastNoise;

namespace JadeFables.Items.BullfrogTree.BullfrogLegs
{
    [AutoloadEquip(EquipType.Shoes)]
    public class BullfrogLegs : ModItem
    {
        public bool jumping = false;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Bullfrog Legs");
            // Tooltip.SetDefault("Jumping while moving boosts you forward");
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.accessory = true;
            Item.hasVanityEffects = true;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.controlJump && player.velocity.Y == 0 && !jumping)
            {
                if (!player.controlLeft && !player.controlRight)
                    return;

                SoundEngine.PlaySound(SoundID.Run, player.Center);
                if (player.controlLeft)
                    player.velocity.X = -8;
                else if (player.controlRight)
                    player.velocity.X = 8;

                Projectile.NewProjectileDirect(player.GetSource_Accessory(Item), player.Bottom, Vector2.Zero, ModContent.ProjectileType<BullfrogLegRingAlt>(), 0, 0, player.whoAmI).rotation = 1.57f - (0.78f * Math.Sign(player.velocity.X));
                for (int i = 0; i < 6; i++)
                {
                    //Dust.NewDustPerfect(player.Bottom, ModContent.DustType<BullfrogLegDust>(), new Vector2(-Math.Sign(player.velocity.X), 1).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.5f, 0.75f), 0, Color.White, Main.rand.NextFloat(0.4f, 0.7f));
                }
            }

            if (player.controlJump)
            {
                jumping = true;
            }
            else
            {
                jumping = false;
            }
        }
    }
    public class BullfrogLegRing : ModProjectile
    {
        private FastNoise noise;
        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        public int timeLeftStart = 25;

        private float Progress => 1 - Projectile.timeLeft / (float)timeLeftStart;

        private float Radius => Projectile.ai[0] * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = timeLeftStart;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Ring");
        }

        public override void AI()
        {
            noise = noise ?? new FastNoise(Main.rand.Next(0, 1000000));
            noise.Frequency = 1f;
            Projectile.velocity *= 0.95f;

            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            DrawPrimitives();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            float radius = Radius;

            for (int i = 0; i < 65; i++) //TODO: Cache offsets, to improve performance
            {
                double rad = i / 34f * 6.28f;
                var offset = new Vector2((float)Math.Sin(rad) * 0.4f, (float)Math.Cos(rad));
                offset *= radius;
                offset = offset.RotatedBy(Projectile.ai[1]);
                cache.Add(Projectile.Center + offset);
            }

            while (cache.Count > 65)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail ??= new Trail(Main.instance.GraphicsDevice, 65, new TriangularTip(1), factor => 65 * (1 - Progress) * noise.GetSimplex(1 + (float)Math.Sin((factor + (Progress * 0.3f)) * 6.28f), 1 + (float)Math.Cos((factor + (Progress * 0.3f)) * 6.28f)), factor => new Color(107, 78, 50).MultiplyRGB(Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16)));

            trail2 ??= new Trail(Main.instance.GraphicsDevice, 65, new TriangularTip(1), factor => 15 * (1 - Progress) * noise.GetSimplex(1 + (float)Math.Sin((factor + (Progress * 0.3f)) * 6.28f), 1 + (float)Math.Cos((factor + (Progress * 0.3f)) * 6.28f)), factor => Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16));
            float nextplace = 65f / 64f;
            var offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
            offset *= Radius;

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + offset;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = Projectile.Center + offset;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["RingTrail"].GetShader().Shader;

            var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("JadeFables/Assets/GlowTrail").Value);
            effect.Parameters["alpha"].SetValue(1);

            trail?.Render(effect);
            trail2?.Render(effect);
        }
    }

    internal class BullfrogLegRingAlt : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Ring");
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.timeLeft = 999;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 4 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.active = false;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            SpriteEffects spriteEffects = Projectile.rotation > 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frameBox, lightColor * 0.7f, Projectile.rotation, new Vector2(tex.Width / 2, frameHeight / 2), Projectile.scale, spriteEffects, 0f);
            return false;
        }
    }
}
