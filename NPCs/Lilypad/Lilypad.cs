//TODO on lilypads:
//Make them naturally spawn
//Implement non placeholder sprite
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
        public override void SafeSetDefaults()
        {
            NPC.width = 64;
            NPC.height = 16;
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
            }    
            else
            {
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