//TODO on eel:
//Balance
//Hitsound
//Killsound
//Drops
//Reduce lag
//Sprite implementation
//Bestiary
//Gore

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
using System.Threading;
using System.Net.PeerToPeer;
using rail;

namespace JadeFables.NPCs.Eel
{
    internal class Eel : ModNPC
    {
        private readonly int NUMPOINTS = 40;

        private List<Vector2> cache;
        private float velStack = 0;
        private Trail trail;

        private Player target => Main.player[NPC.target];

        private Astar astar;

        private int timer;

        private Vector2 corner = Vector2.Zero;

        private Vector2 posToBe = Vector2.Zero;
        public override void SetDefaults()
        {
            NPC.width = 16;
            NPC.height = 16;
            NPC.damage = 30;
            NPC.defense = 5;
            NPC.lifeMax = 100;
            NPC.value = 80f;
            NPC.knockBackResist = 1.2f;
            NPC.HitSound = SoundID.Item111 with { PitchVariance = 0.2f, Pitch = 0.4f };
            NPC.DeathSound = SoundID.NPCDeath26;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            AIType = NPCID.Goldfish;
        }

        public override void AI()
        {
            int height = 50 * Node.NODE_SIZE;
            int width = 50 * Node.NODE_SIZE;

            if (timer++ % 60 == 0)
            {
                corner = new Vector2((int)(NPC.position.X / 16), (int)(NPC.position.Y / 16)) - (new Vector2(width, height) / 2);
                astar = new Astar(GetGrid(new Vector2(width, height) / 2, corner));
            }

            if (astar != null && timer % 3 == 0)
            {
                Vector2 targetPos = (target.position / 16) - corner;
                targetPos.X = MathHelper.Clamp(targetPos.X, 0, width - 1);
                targetPos.Y = MathHelper.Clamp(targetPos.Y, 0, height - 1);
                Stack<Node> path = astar.FindPath((NPC.position / 16) - corner, targetPos);
                if (path == null)
                {
                    NPC.aiStyle = 16;
                    AIType = NPCID.Goldfish;
                    return;
                }
                AIType = 0;
                NPC.aiStyle = -1;
                if (path.Count <= 3)
                {
                    posToBe = target.position;
                    return;
                }
                if (NPC.velocity != Vector2.Zero)
                {
                    path.Pop();
                }
                posToBe = (path.First<Node>().Center * 16) + (corner * 16);
            }


            if (NPC.aiStyle == 16)
                return;
            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.position.DirectionTo(posToBe) * 10, 0.05f);
        }

        public override void PostAI()
        {
            velStack += NPC.velocity.Length();
            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawPrimitives();
            return false;
        }

        private static List<List<Node>> GetGrid(Vector2 start, Vector2 corner)
        {
            int height = 50 * Node.NODE_SIZE;
            int width = 50 * Node.NODE_SIZE;

            List<List<Node>> ret = new List<List<Node>>();
            for (int i = 0; i < width; i += Node.NODE_SIZE)
            {
                List<Node> col = new List<Node>();
                for (int j = 0; j < height; j += Node.NODE_SIZE)
                {
                    Vector2 pos = new Vector2(i, j);
                    if (i == width / 2 && j == height / 2)
                    {
                        col.Add(new Node(pos, true, 1));
                    }
                    else
                        col.Add(new Node(pos, WalkableTile(pos + corner), 1));
                }
                ret.Add(col);
            }
            return ret;
        }

        private static bool WalkableTile(Vector2 pos)
        {
            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    int i = (int)pos.X + x;
                    int j = (int)pos.Y + y;
                    Tile tile = Framing.GetTileSafely(i, j);
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                        return false;
                    if (tile.LiquidAmount < 255)
                        return false;
                }
            }
            return true;
        }

        private void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < NUMPOINTS; i++)
                {
                    cache.Add(NPC.Center);
                }


            }
            if (velStack > 5)
            {
                cache.Add(NPC.Center);
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
            Color goldEnd = Color.Gold;
            goldEnd.A = 0;
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUMPOINTS, new TriangularTip(2), factor => 24, factor =>
            {
                return Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = NPC.Center;
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
            effect.Parameters["flip"].SetValue(Math.Sign(NPC.velocity.X) == -1);

            trail.Render(effect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}