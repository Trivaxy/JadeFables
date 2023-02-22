//TODO
//Bestiary
//Balance
//Gores
//Banner
//Sound effects

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

namespace JadeFables.NPCs.Bullfrog
{
    internal class Bullfrog : ModNPC
    {
        public static Vector2 tongueOffset = new Vector2(10, 10);
        private Entity target;

        private int XFRAMES = 3;
        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;

        private int jumpCounter = 0;

        private bool tongueing = false;

        private int oldDirection;

        private Projectile tongue = null;

        /*public override bool IsLoadingEnabled(Mod mod) {
            //Since this NPC is just about to be loaded and assigned its type, the current count BEFORE the load will be its type, which is why we can do this
            int npcType = NPCLoader.NPCCount;
           
            DefaultNPCBanner.AddBannerAndItemForNPC(mod, npcType, "Pufferfish", out int bannerType);
            Banner = npcType;
            BannerItem = bannerType;

            return true;
        }*/

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bullfrog");
            Main.npcFrameCount[NPC.type] = 8;
        }

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 96;
            NPC.damage = 0;
            NPC.defense = 5;
            NPC.lifeMax = 300;
            NPC.value = 10f;
            NPC.knockBackResist = 0.4f;
            NPC.HitSound = SoundID.Item111 with { PitchVariance = 0.2f, Pitch = 0.4f};
            NPC.DeathSound = SoundID.NPCDeath26;
            NPC.noGravity = false;
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            int[] dragonflies = new int[] { NPCID.BlackDragonfly, NPCID.BlueDragonfly, NPCID.GoldDragonfly, NPCID.GreenDragonfly, NPCID.OrangeDragonfly, NPCID.RedDragonfly, NPCID.YellowDragonfly };
            var nearbyDragonfly = Main.npc.Where(n => n.active && dragonflies.Contains(n.type) && n.Distance(NPC.Center) < 300 && MathF.Abs(MathF.Sin((n.Center - NPC.Center).ToRotation())) < 0.35f).OrderBy(n => n.Distance(NPC.Center)).FirstOrDefault();

            if (nearbyDragonfly == default)
            {
                target = Main.player[NPC.target];
            }
            else
            {
                target = nearbyDragonfly;
                NPC.spriteDirection = MathF.Sign(target.Center.X - NPC.Center.X);
            }
            if (NPC.collideY || NPC.velocity.Y == 0)
            {
                NPC.velocity.X *= 0.9f;
                Vector2 dir = target.Center - NPC.Center;
                if (jumpCounter == 0 && Math.Abs(dir.X) < 200 && MathF.Abs(MathF.Sin(dir.ToRotation())) < 0.35f)
                {
                    oldDirection = NPC.spriteDirection;
                    tongueing = true;
                }
                if (tongueing)
                {
                    NPC.spriteDirection = oldDirection;
                    xFrame = 2;
                    frameCounter++;
                    if (yFrame != 3)
                    {
                        if (frameCounter % 4 == 0)
                        {
                            yFrame++;
                            if (yFrame >= Main.npcFrameCount[NPC.type])
                            {
                                yFrame = 0;
                                xFrame = 0;
                                tongueing = false;
                                frameCounter = 0;
                            }
                        }

                    }
                    else
                    {
                        if (tongue == null)
                        {
                            tongue = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + tongueOffset * new Vector2(NPC.spriteDirection, 1), (NPC.Center + tongueOffset * new Vector2(NPC.spriteDirection, 1)).DirectionTo(target.Center) * 15, ModContent.ProjectileType<Bullfrog_Tongue>(), 30, 0, NPC.target);
                            (tongue.ModProjectile as Bullfrog_Tongue).parent = NPC;
                        }
                        else if (!tongue.active)
                        {
                            tongue = null;
                            yFrame++;
                        }
                    }
                }
                else
                {
                    jumpCounter++;
                    xFrame = 0;
                    yFrame = 0;
                }
                if (jumpCounter > 90 && !tongueing)
                {
                    NPC.velocity.X = Math.Sign(target.Center.X - NPC.Center.X) * 9;
                    NPC.velocity.Y = -4;
                    jumpCounter = 0;
                }
            }
            else
            {
                NPC.velocity.X *= 1.03f;
                xFrame = 1;
                yFrame = 0;
            }
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
            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);
            Main.spriteBatch.Draw(mainTex, slopeOffset + NPC.Center - screenPos, frameBox, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => !spawnInfo.Water && spawnInfo.Player.InModBiome(ModContent.GetInstance<JadeLakeBiome>()) ? 150f : 0f;

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Potions.Dumpling.Dumpling>(), 40));
        }
    }
    internal class Bullfrog_Tongue : ModProjectile
    {
        public NPC parent;

        private bool retracting => Projectile.timeLeft < 40;

        private Vector2 startPos;

        private Vector2 mouthPos => parent.Center + Bullfrog.tongueOffset * new Vector2(parent.spriteDirection, 1);

        private NPC dragonfly = default;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 80;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bullfrog");
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 40)
            {
                startPos = Projectile.Center;
            }
            if (retracting)
            {
                Projectile.extraUpdates = 1;
                Projectile.Center = Vector2.Lerp(mouthPos, startPos, EaseFunction.EaseCircularOut.Ease(Projectile.timeLeft / 40f));
            }
            else
            {
                Projectile.velocity *= 0.935f;
            }
            if (dragonfly != default)
            {
                dragonfly.Center = Projectile.Center;
                if (Projectile.timeLeft == 2)
                {
                    dragonfly.immortal = false;
                    dragonfly.StrikeNPC(Main.rand.Next(500, 1000), 0, 0, true);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D startTex = ModContent.Request<Texture2D>(Texture + "Start").Value;
            Texture2D middleTex = ModContent.Request<Texture2D>(Texture + "Middle").Value;
            Texture2D endTex = ModContent.Request<Texture2D>(Texture + "End").Value;

            float length = (mouthPos - Projectile.Center).Length();

            Vector2 origin = new Vector2(0, middleTex.Height / 2);

            float rot = (mouthPos - Projectile.Center).ToRotation() + 3.14f;
            Main.spriteBatch.Draw(startTex, mouthPos - Main.screenPosition, null, Lighting.GetColor((int)(mouthPos.X / 16), (int)(mouthPos.Y / 16)), rot, origin, Projectile.scale, SpriteEffects.None, 0f);

            for (int i = startTex.Width; i < length; i+= middleTex.Width)
            {
                float lerper = i / length;
                Vector2 posToDraw = Vector2.Lerp(mouthPos, Projectile.Center, lerper);
                Color drawColor = Lighting.GetColor((int)(posToDraw.X / 16), (int)(posToDraw.Y / 16));
                Main.spriteBatch.Draw(middleTex, posToDraw - Main.screenPosition, null, drawColor, rot, origin, Projectile.scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(endTex, Projectile.Center - Main.screenPosition, null, lightColor, rot, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > 40)
                Projectile.timeLeft = 41;
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Projectile.timeLeft > 40)
                Projectile.timeLeft = 41;
            damage = 1;
            target.immortal = true;
            dragonfly = target;
        }

        public override bool? CanHitNPC(NPC target)
        {
            int[] dragonflies = new int[] { NPCID.BlackDragonfly, NPCID.BlueDragonfly, NPCID.GoldDragonfly, NPCID.GreenDragonfly, NPCID.OrangeDragonfly, NPCID.RedDragonfly, NPCID.YellowDragonfly };
            if (dragonflies.Contains(target.type) && !target.immortal && dragonfly == null)
                return true;
            return false;
        }
    }
}