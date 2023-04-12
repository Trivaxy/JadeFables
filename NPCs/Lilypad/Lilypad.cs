﻿//TODO on lilypads:
//Make them naturally spawn
using JadeFables.Biomes.JadeLake;
using JadeFables.Core;
using JadeFables.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace JadeFables.NPCs.Lilypad
{
    internal class Lilypad : MovingPlatform
    {

        public Vector2 floatingPos = Vector2.Zero;

        public bool falling = false;

        private float fallSpeed = 0;

        private float shakeTimer = 0;
        public override void SafeSetDefaults()
        {
            NPC.width = 96;
            NPC.height = 24;
            NPC.gfxOffY = -6;
        }

        public override void SafeAI()
        {
            floatingPos = NPC.Center;
            Tile aboveTile = Framing.GetTileSafely((int)floatingPos.X / 16, (int)(floatingPos.Y / 16) - 1);
            while (aboveTile.LiquidAmount > 0)
            {
                floatingPos.Y -= 16;
                floatingPos.Y -= (floatingPos.Y % 16);

                Tile currentTile = Framing.GetTileSafely((int)floatingPos.X / 16, (int)(floatingPos.Y / 16));
                floatingPos.Y += 16 * (1 - (currentTile.LiquidAmount / 255f));

                aboveTile = Framing.GetTileSafely((int)floatingPos.X / 16, (int)(floatingPos.Y / 16) - 1);
            }

            if (aboveTile.HasTile && Main.tileSolid[aboveTile.TileType])
                NPC.active = false;

            if (beingStoodOn)
            {
                if (fallSpeed < 1)
                    fallSpeed += 0.01f;

                if (NPC.velocity.Y < 4)
                    NPC.velocity.Y += fallSpeed;

                if (shakeTimer == -1)
                {
                    shakeTimer = 1;
                }
                if (shakeTimer > 0)
                {
                    shakeTimer -= 0.04f;
                }
                else
                    shakeTimer= 0;
                NPC.rotation = shakeTimer * 0.17f * MathF.Sin(shakeTimer * 6.28f);
            }    
            else
            {
                if (shakeTimer != -1)
                {
                    Player nearest = Main.player.Where(n => n.active && !n.dead).OrderBy(n => n.Distance(NPC.Center)).FirstOrDefault();
                    if (nearest != default)
                    {
                        NPC.rotation = Math.Sign(nearest.Center.X - NPC.Center.X) * 0.4f;
                    }
                    shakeTimer = -1;
                }
                NPC.rotation *= 0.93f;
                fallSpeed = 0;
                if (NPC.wet)
                {
                    if (floatingPos.Y < NPC.Center.Y)
                    {
                        NPC.velocity.Y = -3;
                    }
                    else
                    {
                        NPC.Center = floatingPos;
                        NPC.velocity.Y = 0;
                    }
                }
                else
                    NPC.velocity.Y += 0.1f;
            }
        }
    }
}