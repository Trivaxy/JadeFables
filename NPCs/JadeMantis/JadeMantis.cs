//TODO:
//Balance
//Prevent it from clipping into blocks
//Make it go straight to flying animation during pop up

//SOUND EFFECTS:
//Pulse sound

//SPRITE DEPENDANT:
//Make foliage cover the mantis when its hiding
//Gores
//Banner
//Swoop animations
//Throwing animation and offset

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
using JadeFables.Biomes.JadeLake;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;
using Humanizer;
using System.Reflection;
using JadeFables.Tiles.Banners;
using static Humanizer.In;
using JadeFables.Dusts;

namespace JadeFables.NPCs.JadeMantis
{
    internal class JadeMantis : ModNPC
    {
        private enum AttackPhase
        {
            JustSpawned = 0,
            PoppingOut = 1,
            Hiding = 2,
            Idle = 3,
            Swooping = 4,
            Throwing = 5,
        }

        private AttackPhase attackPhase = AttackPhase.JustSpawned;
        private Player target => Main.player[NPC.target];

        private int XFRAMES = 1;
        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;

        private float bobCounter = 0f;

        int attackTimer = 0;

        private Vector2 movementTarget = Vector2.Zero;
        public Vector2 oldPosition = Vector2.Zero;

        private Vector2 spearVel = Vector2.Zero;

        private int swoopDirection = 0;
        private float swoopCounter = 0;
        private float swoopSpeed = 0;
        private float swoopPulse = 1;
        private int swoopPauseTimer = 20;
        private bool swoopStopped = false;
        private bool swoopEnded = false;

        private int popoutTimer = 0;

        private Vector2 knockBackPos = Vector2.Zero;
        private int knockBackTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            DisplayName.SetDefault("Artisan Mantis");
        }

        public override void SetDefaults()
        {
            NPC.width = 64;
            NPC.height = 64;
            NPC.damage = 0;
            NPC.defense = 5;
            NPC.lifeMax = 300;
            NPC.value = 10f;
            NPC.knockBackResist = 1.2f;
            NPC.HitSound = SoundID.NPCHit32;
            NPC.DeathSound = SoundID.NPCDeath35;
            NPC.noGravity = true;
            NPC.behindTiles = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                JadeSpawnConditions.JadeSprings,
                new FlavorTextBestiaryInfoElement("Flight has its perks, and has allowed the Jade Mantis tribe to erect large monuments efficiently. But beware; the Mantises are very protective of their designs, and will pluck away anyone who dares to enter the Jade Pagodas.")
            });
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (attackPhase == AttackPhase.Hiding)
            {
                attackPhase = AttackPhase.PoppingOut;
            }
            if (attackPhase == AttackPhase.Idle && hit.Knockback > 0)
            {
                knockBackPos = NPC.Center;
                knockBackTimer = 30;
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (attackPhase == AttackPhase.Hiding)
            {
                attackPhase = AttackPhase.PoppingOut;
                if (attackPhase == AttackPhase.Idle && hit.Knockback > 0)
                {
                    knockBackPos = NPC.Center;
                    knockBackTimer = 30;
                }
            }
        }

        public override void AI()
        {
            NPC.TargetClosest(true);

            switch (attackPhase)
            {
                case AttackPhase.JustSpawned:
                    NPC.Center = FindSpot();
                    attackPhase = AttackPhase.Hiding;
                    break;
                case AttackPhase.PoppingOut:
                    PoppingOutBehavior();
                    break;
                case AttackPhase.Hiding:
                    HidingBehavior();
                    break;
                case AttackPhase.Idle:
                    IdleBehavior();
                    break;
                case AttackPhase.Swooping:
                    SwoopingBehavior();
                    break;
                case AttackPhase.Throwing:
                    ThrowingBehavior();
                    break;
            }
        }

        private Vector2 FindSpot()
        {
            Tile tile = Framing.GetTileSafely((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);
            int j = 0;
            for (; j < 100; j++)
            {
                tile = Framing.GetTileSafely((int)NPC.Center.X / 16, (int)(NPC.Center.Y / 16) + j);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    break;
                }
            }
            Vector2 ret = NPC.Center + new Vector2(0, j * 16);
            ret += new Vector2(0, 10);
            return ret;
        }

        private void PoppingOutBehavior()
        {
            if (popoutTimer == 0)
            {
                SoundEngine.PlaySound(SoundID.WormDig, NPC.Center);
                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustPos = NPC.Center - new Vector2(0, NPC.height / 2);
                    Vector2 dustVel = Main.rand.NextVector2Circular(4, 8);
                    dustVel.Y *= -Math.Sign(dustVel.Y);
                    Dust.NewDustPerfect(dustPos, ModContent.DustType<JadeMantisPopupDust>(), dustVel);
                }
                NPC.velocity.Y = -20;
            }
            NPC.velocity.Y *= 0.85f;
            NPC.noTileCollide = true;
            popoutTimer++;

            if (popoutTimer > 30)
            {
                NPC.noTileCollide = false;
                attackPhase = AttackPhase.Idle;
            }
        }

        private void HidingBehavior()
        {
            NPC.velocity = Vector2.Zero;

            if (NPC.Distance(target.Center) < 200)
            {
                attackPhase = AttackPhase.PoppingOut;
            }
        }

        private void IdleBehavior()
        {
            bobCounter += 0.2f;
            if (GoToPos(movementTarget, oldPosition) || movementTarget == Vector2.Zero || attackTimer == 0)
            {
                oldPosition = NPC.Center;
                movementTarget = Main.rand.NextVector2Circular(500, 400);
                movementTarget.Y *= -Math.Sign(movementTarget.Y);
                movementTarget += target.Center;
                if (attackTimer > 200)
                {
                    attackTimer = 0;
                    if (Main.rand.NextBool())
                        PrepareThrow();
                    else
                        PrepareSwoop();
                    return;
                }
            }
            attackTimer++;
        }

        private void SwoopingBehavior()
        {
            NPC.spriteDirection = swoopDirection;
            if (swoopEnded)
            {
                NPC.velocity *= 0.9f;
                if (NPC.velocity.Length() < 2)
                {
                    attackPhase = AttackPhase.Idle;
                }
                return;
            }
            float yDiff = (target.Center.Y - 32) - NPC.Center.Y;
            float yMult = 0.3f;

            if (swoopPulse < 1)
                swoopPulse += 0.06f;

            if (swoopStopped)
            {
                swoopSpeed += 0.7f;
                NPC.velocity *= 0.95f;
                swoopPauseTimer--;
                if (swoopPauseTimer <= 0)
                    swoopStopped = false;
            }
            else if (swoopCounter < 4.71f)
            {
                swoopCounter += 0.06f;
                if (swoopPulse == 1 && swoopCounter > 3.14f)
                {
                    swoopStopped = true;
                    swoopPulse = 0;
                }
            }
            else
            {
                yMult = 0.15f;
                if (NPC.collideX)
                {
                    attackPhase = AttackPhase.Idle;
                }
                if (Math.Sign(target.Center.X - NPC.Center.X) != swoopDirection && Math.Abs(target.Center.X - NPC.Center.X) > 550)
                {
                    swoopEnded = true;
                }
            }

            if ((swoopCounter > 3.14f || swoopStopped) && swoopSpeed < 20)
            {
                swoopSpeed += 0.7f;
            }
            NPC.velocity.Y = Math.Sign(yDiff) * MathF.Sqrt(MathF.Abs(yDiff)) * yMult;

            if (!swoopStopped)
                NPC.velocity.X = swoopDirection * -MathF.Sin(swoopCounter) * swoopSpeed;


        }

        private void ThrowingBehavior()
        {
            NPC.velocity *= 0.97f;
            if (frameCounter == 0 && yFrame == 3)
            {
                SoundEngine.PlaySound(SoundID.Item19, NPC.Center);
                Vector2 pos = NPC.Center;
                Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), pos, spearVel, ModContent.ProjectileType<JadeMantisSpear>(), (int)(NPC.damage * (Main.expertMode ? 0.5f : 1f)), 3);
            }
        }

        private void PrepareThrow()
        {
            xFrame = 0;
            yFrame = 0;
            frameCounter = 0;
            Vector2 pos = NPC.Center;
            spearVel = pos.DirectionTo(target.Center) * 40;
            attackPhase = AttackPhase.Throwing;
        }

        private void PrepareSwoop()
        {
            swoopEnded = false;
            swoopStopped = false;
            swoopPauseTimer = 20;
            swoopPulse = 1;
            xFrame = 0;
            yFrame = 0;
            frameCounter = 0;
            swoopDirection = Math.Sign(target.Center.X - NPC.Center.X);
            attackPhase = AttackPhase.Swooping;
            swoopCounter = 0;
            swoopSpeed = 5;
        }

        /// <summary>
		/// Attempts to navigate to the given position 
		/// </summary>
		/// <param name="pos"> the destination for the NPC </param>
		/// <param name="oldPos"> the point at which the NPC started it's journey </param>
		/// <returns> If the enemy has reached it's destination </returns>
		private bool GoToPos(Vector2 pos, Vector2 oldPos)
        {
            float distance = pos.X - oldPos.X;
            float progress = MathHelper.Clamp((NPC.Center.X - oldPos.X) / distance, 0, 1);

            Vector2 dir = NPC.DirectionTo(pos);

            if (NPC.Distance(pos) > 7 && !NPC.collideY && !NPC.collideX)
            {
                if (knockBackTimer-- <= 0)
                {
                    NPC.velocity = dir * ((float)Math.Sin(progress * 3.14f) + 0.1f) * 5;
                    NPC.velocity.Y += (float)Math.Cos(bobCounter) * 0.45f;
                }
                else
                {
                    NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(pos) * 6, 0.05f);
                }
                return false;
            }

            NPC.velocity.Y = (float)Math.Cos(bobCounter) * 0.45f;
            return true;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (attackPhase == AttackPhase.Swooping && swoopCounter > 4.71f)
                return base.CanHitPlayer(target, ref cooldownSlot);
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;

            int frameWidth = mainTex.Width / XFRAMES;
            int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];
            Rectangle frameBox = new Rectangle(xFrame * frameWidth, yFrame * frameHeight, frameWidth, frameHeight);

            SpriteEffects effects = SpriteEffects.None;
            Vector2 origin = new Vector2(frameWidth, frameHeight) / 2;

            if (NPC.spriteDirection != 1)
            {
                effects = SpriteEffects.FlipHorizontally;
                origin.X = frameWidth - origin.X;
            }

            Vector2 scaleVec = Vector2.One; 
            if (attackPhase == AttackPhase.PoppingOut)
            {
                float squash = MathHelper.Max(1 - (NPC.velocity.Length() / 80f), 0.25f);
                float stretch = 1 + NPC.velocity.Length() / 30f;
                scaleVec = new Vector2(squash, stretch);
            }
            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);

            if (NPC.IsABestiaryIconDummy)
                drawColor = Color.White;
            Main.spriteBatch.Draw(mainTex, slopeOffset + NPC.Center - screenPos, frameBox, drawColor * (1 - swoopPulse), NPC.rotation, origin, NPC.scale + swoopPulse, effects, 0f);
            Main.spriteBatch.Draw(mainTex, slopeOffset + NPC.Center - screenPos, frameBox, drawColor, NPC.rotation, origin, NPC.scale * scaleVec, effects, 0f);
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.IsABestiaryIconDummy || attackPhase == AttackPhase.Idle)
            {
                xFrame = 0;
                frameCounter++;
                if (frameCounter > 4)
                {
                    frameCounter = 0;
                    yFrame++;
                }
                yFrame %= 4;
            }

            if (attackPhase == AttackPhase.Throwing)
            {
                xFrame = 0;
                frameCounter++;
                if (frameCounter > 30)
                {
                    frameCounter = 0;
                    yFrame++;
                }
                if (yFrame >= 4)
                {
                    yFrame = 0;
                    xFrame = 0;
                    attackPhase = AttackPhase.Idle;
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Potions.Dumpling.Dumpling>(), 40));
            npcLoot.Add(ItemDropRule.Common(ItemID.IceCream, 40));
        }


        public override float SpawnChance(NPCSpawnInfo spawnInfo) => !spawnInfo.Water && !spawnInfo.PlayerSafe && spawnInfo.Player.InModBiome(ModContent.GetInstance<JadeLakeBiome>()) ? 4f : 0f;
    }

    public class JadeMantisSpear : ModProjectile
    {
        public bool stuck = false;

        public float opacity = 1;

        public float shakeTimer = 1;

        float oldRot = 0;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override void AI()
        {
            if (stuck)
            {
                opacity -= 0.01f;
                if (opacity <= 0)
                    Projectile.active = false;

                if (shakeTimer > 0)
                    shakeTimer -= 0.05f;
                Projectile.rotation = oldRot + (0.5f * MathF.Sin(shakeTimer * 6.28f * 2) * shakeTimer);
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Lighting.AddLight(Projectile.Center, Color.LimeGreen.ToVector3() * opacity);
            if (!stuck)
            {
                SoundEngine.PlaySound(SoundID.Item89 with { Pitch = -0.1f }, Projectile.Center);
                oldRot = Projectile.rotation;
                Projectile.velocity = Vector2.Zero;
                stuck = true;
                Projectile.hostile = false;
                Projectile.position += oldVelocity;
                Core.Systems.CameraSystem.Shake += 6;
                for (int i = 0; i < 8; i++)
                {
                    Vector2 dir = Main.rand.NextVector2Circular(3, 3);
                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<JadeSparkle>(), dir);
                }
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            Vector2 origin = new Vector2(tex.Width / 2, 0);
            Vector2 glowOrigin = new Vector2(glowTex.Width / 2, 6);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, new Color(255,255,255,0) * opacity * 0.3f, Projectile.rotation, glowOrigin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}