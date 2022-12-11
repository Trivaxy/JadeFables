//TODO
//Bestiary
//Banners
//Balance
//Gores
//dust on collision

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

namespace JadeFables.NPCs.Pufferfish
{
    internal class Pufferfish : ModNPC
    {

        private Player target => Main.player[NPC.target];

        private int XFRAMES = 2;
        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pufferfish");
            Main.npcFrameCount[NPC.type] = 10;
        }

        public override void SetDefaults()
        {
            NPC.width = 7;
            NPC.height = 7;
            NPC.damage = 30;
            NPC.defense = 5;
            NPC.lifeMax = 100;
            NPC.value = 10f;
            NPC.knockBackResist = 2.6f;
            NPC.HitSound = SoundID.Item111 with { PitchVariance = 0.2f, Pitch = 0.4f};
            NPC.DeathSound = SoundID.NPCDeath26;
            NPC.noGravity = true;
            NPC.aiStyle = 16;
            AIType = NPCID.Goldfish;
        }

        public override void AI()
        {
            NPC.spriteDirection = Math.Sign(-NPC.velocity.X);
            frameCounter++;

            int threshhold = 4;

            if (xFrame == 1)
            {
                NPC.velocity.X *= 0.95f;
                threshhold = 5;
                if (yFrame == 4)
                    threshhold = 25;
            }
            if (frameCounter >= threshhold)
            {
                frameCounter = 0;
                yFrame++;
                if (yFrame == 3 && xFrame == 1)
                {
                    float offset = Main.rand.NextFloat(6.28f);
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, ((i / 4f) * 6.28f).ToRotationVector2().RotatedBy(offset), ModContent.ProjectileType<PufferfishProj>(), Main.expertMode ? 15 : 30, 4);
                    }
                }
                if (yFrame >= Main.npcFrameCount[NPC.type])
                {
                    xFrame = 0;
                    yFrame = 0;
                }
            }
            //NPC.TargetClosest(true);
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            if (xFrame == 0)
            {
                yFrame = 0;
                frameCounter = 0;
                xFrame = 1;
            }
        }

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            if (xFrame == 0)
            {
                yFrame = 0;
                frameCounter = 0;
                xFrame = 1;
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

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return (xFrame == 1);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Poisoned, 200);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Water && spawnInfo.Player.InModBiome(ModContent.GetInstance<JadeLakeBiome>()) ? 150f : 0f;

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Potions.Spine.SpineItem>(), 2));
        }
    }

    internal class PufferfishProj : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 270;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spike");
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

            if (Projectile.velocity.Length() > 5)
                Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale * ((3 + (float)Math.Sin(Main.timeForVisualEffects * 0.1f)) * 0.5f), SpriteEffects.None, 0f);
            
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

            if (Projectile.velocity.Length() > 5)
                Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.velocity.Length() > 5;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.velocity.Length() < 8)
                Projectile.velocity *= 1.08f;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Poisoned, 200);
        }
    }
}