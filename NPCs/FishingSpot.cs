using JadeFables.Biomes.JadeLake;
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

namespace JadeFables.NPCs
{
    internal class FishingSpot : ModNPC
    {

        public float time;

        public bool firstFish = false;

        public int deathCounter = 500;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glistening Pool");
        }

        public override void SetDefaults()
        {
            NPC.width = 128;
            NPC.height = 32;
            NPC.damage = 0;
            NPC.lifeMax = 100;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.immortal = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                JadeSpawnConditions.JadeSprings,
                new FlavorTextBestiaryInfoElement("These rare, mysterious pools appear in the springs, and fishing in them can give you all sorts of rewards! Nobody knows how they work.")
            });
        }

        public override void AI()
        {
            Tile aboveTile = Framing.GetTileSafely((int)NPC.Center.X / 16, (int)(NPC.Center.Y / 16) - 2);
            while (aboveTile.LiquidAmount > 0)
            {
                NPC.position.Y -= 16;
                NPC.position.Y -= (NPC.position.Y % 16);
                aboveTile = Framing.GetTileSafely((int)NPC.Center.X / 16, (int)(NPC.Center.Y / 16) - 2);
            }

            if (aboveTile.HasTile && Main.tileSolid[aboveTile.TileType])
                NPC.active = false;

            if (Main.rand.NextBool(5))
            {
                Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(64, 8), ModContent.DustType<JadeSparkle>(), -Vector2.UnitY * 0.3f);
            }

            if (!firstFish)
                deathCounter = 500;
            NPC.velocity = Vector2.Zero;

            Projectile bobber = Main.projectile.Where(n => n.active && n.bobber && n.Colliding(n.Hitbox, NPC.Hitbox)).FirstOrDefault();
            if (bobber != default)
            {
                Player owner = Main.player[bobber.owner];
                owner.fishingSkill = 9999;
                firstFish = true;
            }

            if (deathCounter-- < 0)
            {
                Main.BestiaryTracker.Kills.RegisterKill(NPC);
                NPC.active = false;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            time += 0.004f;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Water && spawnInfo.Player.InModBiome(ModContent.GetInstance<JadeLakeBiome>()) ? 1f : 0f;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Effect effect = Terraria.Graphics.Effects.Filters.Scene["FishingSpot"].GetShader().Shader;

            effect.Parameters["time"].SetValue(time);

            Color color = Color.Lerp(Color.Cyan, Color.Green, 0.5f);
            effect.Parameters["inputColor"].SetValue(color.ToVector4() * (1.3f * MathHelper.Min(deathCounter / 20f, 1)));
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(tex, (NPC.Center - screenPos) + new Vector2(0, NPC.IsABestiaryIconDummy ? -4 : 4), null, Color.Green, 3.14f, NPC.IsABestiaryIconDummy ? tex.Size() / 2 : tex.Bounds.Top(), new Vector2(0.5f, 0.3f), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}