//TODO
//Bestiary
//Banners
//Balance
//Gores
//Spawning
//Better hitbox
//Make it not geek out while turning at some angles
//Make it not turn upside down
//Reduce jank in general
//Animations
//Smoother collection of points
//Make them not occaisionally fly off into space

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System.Collections.Generic;

using Terraria.DataStructures;
using Terraria.GameContent;

using Terraria.Audio;

using System;
using System.Linq;
using static Terraria.ModLoader.ModContent;
using JadeFables.Core;
using JadeFables.Helpers;
using static System.Formats.Asn1.AsnWriter;

namespace JadeFables.NPCs.GiantSnail
{
    internal class GiantSnail : ModNPC
    {

        private readonly int NUMPOINTS = 100;

        protected Vector2 oldVelocity = Vector2.Zero;
        protected Vector2 moveDirection;
        protected Vector2 newVelocity = Vector2.Zero;
        protected int initialDirection = 0;

        private float segmentRotation = 0;

        private List<Vector2> cache;
        private List<float> oldRotation;
        private Trail trail;
        private Player target => Main.player[NPC.target];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Giant Snail");
        }

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 32;
            NPC.damage = 30;
            NPC.defense = 5;
            NPC.lifeMax = 1000;
            NPC.value = 10f;
            NPC.knockBackResist = 2.6f;
            NPC.HitSound = SoundID.NPCHit23;
            NPC.DeathSound = SoundID.NPCDeath26;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.behindTiles = true;
            initialDirection = (Main.rand.Next(2) * 2) - 1;
            moveDirection = new Vector2(initialDirection, 0);
        }

        public override void AI()
        {
            Crawl();
            ManageCache();
            ManageTrail();
            segmentRotation = oldRotation.Average();
        }
        protected void Crawl()
        {
            newVelocity = Collide();

            if (Math.Abs(newVelocity.X) < 0.5f)
                NPC.collideX = true;
            else
                NPC.collideX = false;
            if (Math.Abs(newVelocity.Y) < 0.5f)
                NPC.collideY = true;
            else
                NPC.collideY = false;

            RotateCrawl();

            if (NPC.ai[0] == 0f)
            {
                NPC.TargetClosest(true);
                moveDirection.Y = 1;
                NPC.ai[0] = 1f;
            }

            if (NPC.ai[1] == 0f)
            {
                if (NPC.collideY)
                    NPC.ai[0] = 2f;

                if (!NPC.collideY && NPC.ai[0] == 2f)
                {
                    moveDirection.X = -moveDirection.X;
                    NPC.ai[1] = 1f;
                    NPC.ai[0] = 1f;
                }
                if (NPC.collideX)
                {
                    moveDirection.Y = -moveDirection.Y;
                    NPC.ai[1] = 1f;
                }
            }
            else
            {
                if (NPC.collideX)
                    NPC.ai[0] = 2f;

                if (!NPC.collideX && NPC.ai[0] == 2f)
                {
                    moveDirection.Y = -moveDirection.Y;
                    NPC.ai[1] = 0f;
                    NPC.ai[0] = 1f;
                }
                if (NPC.collideY)
                {
                    moveDirection.X = -moveDirection.X;
                    NPC.ai[1] = 0f;
                }
            }
            NPC.velocity = 1 * moveDirection;
            NPC.velocity = Collide();
        }

        protected Vector2 Collide() => Collision.noSlopeCollision(NPC.position, NPC.velocity, 32, 32, true, true);

        protected void RotateCrawl()
        {
            float rotDifference = ((((NPC.velocity.ToRotation() - NPC.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
            if (Math.Abs(rotDifference) < 0.15f)
            {
                NPC.rotation = NPC.velocity.ToRotation();
                return;
            }
            NPC.rotation += Math.Sign(rotDifference) * 0.025f;
        }

        private void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                oldRotation = new List<float>();
                for (int i = 0; i < NUMPOINTS; i++)
                {
                    cache.Add(NPC.Center);
                }
                for (int i = 0; i < NUMPOINTS / 2; i++)
                    oldRotation.Add(0);
            }
            float directionRotationOffset = (initialDirection == -1 ? 3.14f : 0);
            cache.Add(NPC.Center + ((NPC.rotation + 1.57f + directionRotationOffset).ToRotationVector2() * 10));
            oldRotation.Add(NPC.rotation);
            while (cache.Count > NUMPOINTS)
            {
                cache.RemoveAt(0);
                oldRotation.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUMPOINTS, new TriangularTip(40), factor => 21, factor =>
            {
                return Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16));
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = NPC.Center + (segmentRotation.ToRotationVector2() * 40);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawBody();

            float directionRotationOffset = (initialDirection == -1 ? 3.14f : 0);
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            if (cache == null)
                return false;

            Vector2 pos = Vector2.Zero;
            foreach (Vector2 oldPos in cache)
            {
                pos += oldPos;
            }
            pos /= NUMPOINTS;

            float rotation = segmentRotation;

            Texture2D headTex = ModContent.Request<Texture2D>(Texture + "_Head").Value;
            Vector2 headOrigin = headTex.Size() * new Vector2(0f, 0.71f);

            if (initialDirection == -1)
                headOrigin.X = headTex.Width - headOrigin.X;
            Vector2 headPos = (NPC.Center) - ((NPC.rotation).ToRotationVector2() * 8);
            Main.spriteBatch.Draw(headTex, headPos - screenPos, null, Lighting.GetColor((int)(headPos.X / 16), (int)(headPos.Y / 16)), rotation + (initialDirection == -1 ? 3.14f : 0), headOrigin, NPC.scale, initialDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

            Vector2 shellPos = (pos) - ((rotation + 1.57f + directionRotationOffset).ToRotationVector2() * tex.Height * 0.5f);
            Main.spriteBatch.Draw(tex, shellPos - screenPos, null, Lighting.GetColor((int)(shellPos.X / 16), (int)(shellPos.Y / 16)), rotation + (initialDirection == -1 ? 3.14f : 0), tex.Size() * new Vector2(0.5f, 0.5f), NPC.scale, initialDirection != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }

        private void DrawBody()
        {
            if (trail == null || trail == default)
                return;

            Main.spriteBatch.End();
            Effect effect = Terraria.Graphics.Effects.Filters.Scene["SnailBody"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_Body").Value);
            effect.Parameters["flip"].SetValue(initialDirection == -1);

            trail.Render(effect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}