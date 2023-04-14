//TODO on dueling spirits:
//Item sprite
//Obtainment
//Make it inherit weapon damage
//Localization
//Sound effects
//Description
//Some sort of synergy

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
using System.Reflection;
using Terraria.GameContent;
using Terraria.DataStructures;
using CsvHelper.TypeConversion;
using JadeFables.Core;
using System.Reflection.Metadata;
using Steamworks;
using JadeFables.Dusts;
using JadeFables.Helpers;

namespace JadeFables.Items.SpringChestLoot.DuelingSpirits
{
    internal class MiniYang : MiniYing
    {
        public override bool active => target.GetGlobalNPC<DuelingSpiritsGNPC>().yanged;
    }
    internal class MiniYing : ModProjectile
    {
        private readonly int NUMPOINTS = 30;

        public Player owner => Main.player[Projectile.owner];
        private List<Vector2> cache;
        private Trail trail;

        public NPC target;

        public float rotSpeed = 0.1f;

        public ref float rot => ref Projectile.ai[0];

        public ref float distance => ref Projectile.ai[1];

        public virtual bool active => target.GetGlobalNPC<DuelingSpiritsGNPC>().yinged;

        private List<Vector2> oldPos = new List<Vector2>();

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.timeLeft = 3;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            oldPos.Add(Projectile.Center);
            if (oldPos.Count > 2)
            {
                oldPos.RemoveAt(0);
            }
            if (active)
            {
                Projectile.timeLeft = 2;
            }

            distance = 20;
            rot += rotSpeed;
            if (!target.active)
            {
                Projectile.active = false;
                return;
            }
            Projectile.Center = target.Center + (rot.ToRotationVector2() * distance);

            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }
        }

        public override void Kill(int timeLeft)
        {

        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (oldPos.Count > 0)
            {
                Texture2D eyeTex = ModContent.Request<Texture2D>(Texture + "Eye").Value;
                Main.spriteBatch.Draw(eyeTex, oldPos[0] - Main.screenPosition, null, Color.White, rot + 1.57f, new Vector2(eyeTex.Width / 2, eyeTex.Height / 2), Projectile.scale * 0.6f, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.GetGlobalNPC<DuelingSpiritsGNPC>().yinged = true;
            modifiers.HitDirectionOverride = Math.Sign(target.Center.X - owner.Center.X);
        }

        private void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < NUMPOINTS; i++)
                {
                    cache.Add(Projectile.Center - target.Center);
                }
            }

            cache.Add(Projectile.Center - target.Center);
            while (cache.Count > NUMPOINTS)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUMPOINTS, new RoundedTip(20), factor => 5 * factor, factor =>
            {
                return Color.White;
            });

            List<Vector2> newCache = new List<Vector2>();
            foreach (Vector2 item in cache)
            {
                newCache.Add(item + target.Center);
            }
            trail.Positions = newCache.ToArray();
            trail.NextPosition = Projectile.Center;
        }

        public void DrawPrimitives()
        {
            if (trail == null || trail == default)
                return;

            //Main.spriteBatch.End();
            Effect effect = Filters.Scene["SnailBody"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture).Value);
            effect.Parameters["flip"].SetValue(true);

            trail.Render(effect);

            //Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}