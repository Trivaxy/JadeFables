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

namespace JadeFables.Items.Fishing.Arowana
{
    class Arowana : ModItem
    {
        public bool popping;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flying Arowana");
            Tooltip.SetDefault("Flies around your cursor, distracting nearby enemies");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.shoot = ProjectileType<ArowanaProj>();
            Item.shootSpeed = 8f;
            Item.autoReuse = false;
            Item.useTurn = false;
            Item.UseSound = SoundID.Item1;
            Item.value = Item.sellPrice(silver: 45);
            Item.rare = ItemRarityID.Blue;
            Item.consumable = true;
            Item.maxStack = 30;
            Item.noUseGraphic = true;
        }
    }

    internal class ArowanaProj : ModProjectile
    {
        private readonly int NUMPOINTS = 30;

        public Player owner => Main.player[Projectile.owner];
        private List<Vector2> cache;
        private Trail trail;

        private float velStack = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Arowana");
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Main.MouseWorld) * 7, 0.05f);

            if (Main.rand.NextBool(20))
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), ModContent.DustType<GoldSparkle>(), Vector2.Zero);

            velStack += Projectile.velocity.Length();
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
                    cache.Add(Projectile.Center);
                }


            }
            if (velStack > 5)
            {
                cache.Add(Projectile.Center);
            }
            while (velStack > 5)
            {
                velStack -= 5;
                
            }
            while (cache.Count > NUMPOINTS)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUMPOINTS, new TriangularTip(2), factor => 24, factor =>
            {
                return Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center;
        }

        private void DrawPrimitives()
        {
            if (trail == null || trail == default)
                return;

            Main.spriteBatch.End();
            Effect effect = Terraria.Graphics.Effects.Filters.Scene["SnailBody"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture).Value);
            effect.Parameters["flip"].SetValue(Math.Sign(Projectile.velocity.X) == -1);

            trail.Render(effect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    public class ArowanaSystem : ModSystem
    {
        public override void PostUpdateNPCs()
        {
            foreach(NPC npc in Main.npc)
            {
                npc.TryGetGlobalNPC<ArowanaGNPC>(out ArowanaGNPC gnpc);
                if (gnpc != null)
                {
                    if (gnpc.target != default)
                    {
                        gnpc.target.Center = gnpc.oldTargetPos;
                        gnpc.target = default;
                    }
                }
            }
        }
    }

    public class ArowanaGNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public Player target;

        public Vector2 oldTargetPos = Vector2.Zero;

        public override bool PreAI(NPC npc)
        {
            Projectile arowana = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<ArowanaProj>()).OrderBy(n => n.Distance(npc.Center)).FirstOrDefault();
            if (arowana != default)
            {
                target = (arowana.ModProjectile as ArowanaProj).owner;

                oldTargetPos = target.Center;
                target.Center = arowana.Center;
            }
            else
            {
                target = default;
            }
            return true;
        }

        public override void PostAI(NPC npc)
        {
            if (target != default)
            {
                target.Center = oldTargetPos;
                target = default;
            }
        }
    }
}