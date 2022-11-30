//TODO
//Hit and death sounds
//Bestiary
//Banners
//Balance
//Launches out projectiles
//Gores
//Spawning

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
            NPC.HitSound = SoundID.Item27 with
            {
                Pitch = -0.3f
            };
            NPC.DeathSound = SoundID.Shatter;
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
    }
}