﻿//TO ALL WHO OPEN THIS FILE:
//This snail is cursed. Spare yourself. Do not go further
//When I wrote this code, only I understood it. It has been months, I no longer understand it.
//If you do venture forward, good fucking luck
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System.Collections.Generic;
using static Terraria.Utils;

using Terraria.DataStructures;
using Terraria.GameContent;

using Terraria.Audio;
using Terraria.Utilities;

using System;
using System.Linq;
using static Terraria.ModLoader.ModContent;
using JadeFables.Core;
using JadeFables.Helpers;
using static System.Formats.Asn1.AsnWriter;
using JadeFables.Biomes.JadeLake;
using Terraria.GameContent.Bestiary;
using JadeFables.Tiles.Banners;
using Terraria.GameContent.ItemDropRules;

namespace JadeFables.NPCs.GiantSnail
{
    internal class GiantSnail : ModNPC
    {
        private readonly int size = 75;
        private readonly int NUMPOINTS = 100;

        private float SPEED = 1;

        protected Vector2 oldVelocity = Vector2.Zero;
        protected Vector2 moveDirection;
        protected Vector2 newVelocity = Vector2.Zero;
        protected int initialDirection = 0;

        protected Vector2 uncollidedVel = Vector2.Zero;

        private float segmentRotation = 0;

        private List<Vector2> cache;
        private List<float> oldRotation;
        private Trail trail;
        private Player target => Main.player[NPC.target];

        private Vector2 climbCenter = Vector2.Zero;

        private int climbHalfSize = 2;

        private bool scared => false;

        private bool fullyScared = false;

        private bool lastChangedY = false;

        private bool flippedOnce = false;

        private int flipCountdown = 0;

        public override bool IsLoadingEnabled(Mod mod)
        {
            //Since this NPC is just about to be loaded and assigned its type, the current count BEFORE the load will be its type, which is why we can do this
            int npcType = NPCLoader.NPCCount;

            DefaultNPCBanner.AddBannerAndItemForNPC(mod, npcType, "GiantSnail", out int bannerType);
            Banner = npcType;
            BannerItem = bannerType;

            return true;
        }

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            for (int j = 1; j <= 4; j++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "JadeFables/NPCs/GiantSnail/GiantSnailGore" + j);

        }

        public override void SetDefaults()
        {
            NPC.width = size;
            NPC.height = size;
            NPC.damage = 30;
            NPC.defense = 5;
            NPC.lifeMax = 150;
            NPC.value = 10f;
            NPC.knockBackResist = 2.6f;
            NPC.HitSound = SoundID.NPCHit33 with { Pitch = -0.6f };
            NPC.DeathSound = SoundID.NPCDeath16 with { Pitch = -0.6f };
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.behindTiles = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                JadeSpawnConditions.JadeSprings,
                new FlavorTextBestiaryInfoElement("This mythical bottom feeder scoops up its prey using its wide, gaping jaws, retreating to the safety of its shell when provoked. Best to avoid getting too close, or it might be the last thing you do…")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            initialDirection = (Main.rand.Next(2) * 2) - 1;
            moveDirection = new Vector2(initialDirection, 0);
            climbCenter = NPC.Center + new Vector2(0, (size / 2) - climbHalfSize).RotatedBy(initialDirection == -1 ? 3.14f : 0);
        }

        public override void AI()
        {
            if (flipCountdown-- == 1)
                initialDirection *= -1;
            if (scared)
            {
                if (SPEED < 0.05f && !fullyScared)
                {
                    fullyScared = true;
                    Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GiantSnailDamager>(), 55, 3, NPC.target);
                    (proj.ModProjectile as GiantSnailDamager).parent = NPC;
                    NPC.velocity = Vector2.Zero;
                }
                SPEED *= 0.92f;
            }

            if (!fullyScared)
                Crawl();
            ManageCache();
            ManageTrail();

            if (!fullyScared)
            {
                segmentRotation = ExperimentalAverage(oldRotation);
                climbCenter += NPC.velocity;
                NPC.Center = climbCenter - new Vector2(0, (size / 2) - climbHalfSize).RotatedBy(segmentRotation + (initialDirection == -1 ? 3.14f : 0));
            }
            else
            {
                segmentRotation += NPC.velocity.Length() * 0.005f * Math.Sign(NPC.velocity.X);
                climbCenter = NPC.Center + new Vector2(0, (size / 2) - climbHalfSize).RotatedBy(segmentRotation + (initialDirection == -1 ? 3.14f : 0));
            }
        }
        protected void Crawl()
        {
            newVelocity = Collide(1, uncollidedVel, 2f);

            if (newVelocity.X != uncollidedVel.X)
                NPC.collideX = true;
            else
                NPC.collideX = false;
            if (newVelocity.Y != uncollidedVel.Y)
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
                    lastChangedY = false;
                    moveDirection.X = -moveDirection.X;
                    NPC.ai[1] = 1f;
                    NPC.ai[0] = 1f;
                }
                if (NPC.collideX)
                {
                    lastChangedY = true;
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
                    lastChangedY = true;
                    moveDirection.Y = -moveDirection.Y;
                    NPC.ai[1] = 0f;
                    NPC.ai[0] = 1f;
                }
                if (NPC.collideY)
                {
                    lastChangedY = false;
                    moveDirection.X = -moveDirection.X;
                    NPC.ai[1] = 0f;
                }
            }

            if (!NPC.collideX && !NPC.collideY)
            {
                if (lastChangedY)
                {
                    moveDirection.Y *= -1;
                }
                else
                {
                    if (flipCountdown > 1)
                    {
                        flipCountdown = 0;
                    }
                    else
                        flipCountdown = 5;
                    moveDirection.X *= -1;
                }
            }
            NPC.velocity = SPEED * moveDirection;
            uncollidedVel = NPC.velocity;
            NPC.velocity = Collide(1, NPC.velocity, 2);
            //NPC.velocity = Vector2.Normalize(NPC.velocity) * SPEED;
        }

        protected Vector2 Collide(float speedMult, Vector2 vel, float hitboxMult) => Collision.noSlopeCollision(climbCenter - (new Vector2(climbHalfSize, climbHalfSize) * hitboxMult), vel * speedMult, (int)(climbHalfSize * 2 * hitboxMult), (int)(climbHalfSize * 2 * hitboxMult), true, true);

        protected void RotateCrawl()
        {
            float rotDifference = ((((NPC.velocity.ToRotation() - NPC.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
            if (Math.Abs(rotDifference) < 0.15f)
            {
                NPC.rotation = NPC.velocity.ToRotation();
                return;
            }
            float increment = Math.Sign(rotDifference) * 0.025f;
            if (Math.Abs(rotDifference) > 1)
                increment *= 3;
            NPC.rotation += increment;
        }

        private void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                oldRotation = new List<float>();
                for (int i = 0; i < NUMPOINTS; i++)
                {
                    cache.Add(climbCenter);
                }
                for (int i = 0; i < NUMPOINTS / 2; i++)
                    oldRotation.Add(0);
            }
            float directionRotationOffset = (initialDirection == -1 ? 3.14f : 0);
            cache.Add(climbCenter + ((NPC.rotation + 1.57f + directionRotationOffset).ToRotationVector2() * climbHalfSize * SPEED));
            oldRotation.Add(NPC.rotation);
            while (cache.Count > NUMPOINTS)
            {
                cache.RemoveAt(0);
                oldRotation.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUMPOINTS * 2, new TriangularTip(40), factor => 31, factor =>
            {
                return Lighting.GetColor((int)(climbCenter.X / 16), (int)(climbCenter.Y / 16));
            });

            List<Vector2> newCache = new List<Vector2>();
            cache.ForEach(n => newCache.Add(n));

            Vector2 point = cache.Last();
            Vector2 vel = segmentRotation.ToRotationVector2() * SPEED;
            for (int i = 0; i < NUMPOINTS; i++)
            {
                float v = vel.ToRotation().AngleLerp(segmentRotation - (1.57f * initialDirection), 0.04f);
                vel = v.ToRotationVector2() * SPEED;
                point += vel;
                newCache.Add(point);
            }

            List<Vector2> newerCache = SmoothBezierPointRetreivalFunction(newCache, NUMPOINTS * 2, 5);
            trail.Positions = newerCache.ToArray();
            trail.NextPosition = newerCache.Last() + ((segmentRotation - (1.57f * initialDirection)).ToRotationVector2() * 40);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Potions.Dumpling.Dumpling>(), 40));
            npcLoot.Add(ItemDropRule.Common(ItemID.IceCream, 40));
            npcLoot.Add(ItemDropRule.Common(ItemID.Compass, 60));
            npcLoot.Add(ItemDropRule.Common(ItemID.DepthMeter, 60));
        }

        public override void OnKill()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            for (int i = 1; i <= 4; i++)
                Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("GiantSnailGore" + i).Type);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                Texture2D previewTex = ModContent.Request<Texture2D>(Texture + "_Preview").Value;
                Main.spriteBatch.Draw(previewTex, NPC.Center - screenPos, null, Color.White, 0, previewTex.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
                return false;
            }

            if (!fullyScared)
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

            //Texture2D headTex = ModContent.Request<Texture2D>(Texture + "_Head").Value;
            //Vector2 headOrigin = headTex.Size() * new Vector2(0f, 0.71f);

            //if (initialDirection == -1)
            //    headOrigin.X = headTex.Width - headOrigin.X;
            // Vector2 headPos = (NPC.Center) - ((NPC.rotation).ToRotationVector2() * 8);
            //Main.spriteBatch.Draw(headTex, headPos - screenPos, null, Lighting.GetColor((int)(headPos.X / 16), (int)(headPos.Y / 16)), rotation + (initialDirection == -1 ? 3.14f : 0), headOrigin, NPC.scale, initialDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

            float offsetLength = tex.Height * MathHelper.Lerp(0.35f, 0.5f, SPEED);
            Vector2 shellPos = (pos) - ((rotation + 1.57f + directionRotationOffset).ToRotationVector2() * offsetLength);

            if (scared)
                shellPos = Vector2.Lerp(shellPos, NPC.Center, 1 - SPEED) + Main.rand.NextVector2Circular(2 - SPEED, 2 - SPEED);
            Main.spriteBatch.Draw(tex, shellPos - screenPos, null, Lighting.GetColor((int)(shellPos.X / 16), (int)(shellPos.Y / 16)), rotation + (initialDirection == -1 ? 3.14f : 0), tex.Size() * new Vector2(0.5f, 0.5f), NPC.scale, initialDirection != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.InModBiome(ModContent.GetInstance<JadeLakeBiome>()) && !spawnInfo.PlayerSafe && Main.npc.Count(n => n.active && n.type == ModContent.NPCType<GiantSnail>()) < 3 ? 25f : 0f;

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position = climbCenter + new Vector2(0, 16).RotatedBy(segmentRotation);
            return true;
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

        private float ExperimentalAverage(List<float> rots)
        {
            Vector2 retVec = Vector2.Zero;
            float mult = 1;
            for (int i = 0; i < rots.Count; i++)
            {
                float rot = rots[i];
                Vector2 newVec = rot.ToRotationVector2() * mult;
                mult += 0.1f;
                retVec += newVec;
            }
            float firstHalf = (cache[cache.Count - 1] - cache[cache.Count / 2]).ToRotation();
            float secondHalf = (cache[cache.Count / 2] - cache[0]).ToRotation();
            float ret2 = (firstHalf.ToRotationVector2() + secondHalf.ToRotationVector2() + Vector2.Normalize(retVec)).ToRotation();
            return ret2;
        }

        public static List<Vector2> SmoothBezierPointRetreivalFunction(IEnumerable<Vector2> originalPositions, int totalTrailPoints, int divider)
        {
            List<Vector2> controlPoints = new List<Vector2>();
            for (int i = 0; i < originalPositions.Count(); i++)
            {
                // Don't incorporate points that are zeroed out.
                // They are almost certainly a result of incomplete oldPos arrays.
                if (originalPositions.ElementAt(i) == Vector2.Zero)
                    continue;

                if (i % divider != 0)
                    continue;
                controlPoints.Add(originalPositions.ElementAt(i));
            }

            BezierCurve bezierCurve = new BezierCurve(controlPoints.ToArray());
            return controlPoints.Count <= 1 ? controlPoints : bezierCurve.GetEvenlySpacedPoints(totalTrailPoints);
        }
    }

    internal class GiantSnailDamager : ModProjectile
    {
        public NPC parent;

        public override void SetDefaults()
        {
            Projectile.width = 75;
            Projectile.height = 75;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (parent.active)
            {
                Projectile.timeLeft = 2;
                Projectile.Center = parent.Center;
            }
            else
                Projectile.active = false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (parent.velocity.Length() < 4 || target == parent)
                return false;
            return base.CanHitNPC(target);
        }
    }
}