using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using JadeFables.Tiles.JadeSand;
using JadeFables.Core;
using Steamworks;
using JadeFables.Tiles.JadeTorch;
using Terraria.ModLoader.IO;

namespace JadeFables.Tiles.JadeLantern
{
    public class JadeLanternFurnitureTileEntity : JadeLanternTileEntity
    {
        protected override int StartLength => 4;

        public override void Load()
        {

        }

        public override void Break()
        {

        }
    }
    public class JadeLanternTileEntity : ModTileEntity
    {
        public bool initialized = false;
        public bool burning = false;

        public bool burnable => chainFrame.Y == 0;

        public int burnTimer = 0;

        public int burnedSegments = 0;
        public VerletChain chain;

        public Rectangle hitbox
        {
            get
            {
                RopeSegment seg = chain.ropeSegments[chain.segmentCount - 1];

                return new Rectangle((int)seg.posNow.X - 16, (int)seg.posNow.Y - 16, 32, 32);
            }
        }

        public int length;

        private int chainFrameY;
        private Rectangle chainFrame;

        private int lanternFrameY;
        private Rectangle lanternFrame;

        private Rectangle pivotFrame;

        public Vector2 WorldPosition => Position.ToVector2() * 16;

        protected virtual int StartLength => Main.rand.Next(10, 15);

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(length)] = length;
            tag[nameof(chainFrameY)] = chainFrameY;
            tag[nameof(lanternFrameY)] = lanternFrameY;
        }

        public override void LoadData(TagCompound tag)
        {
            try
            {
                length = tag.GetInt(nameof(length));
                chainFrameY = tag.GetInt(nameof(chainFrameY));
                lanternFrameY = tag.GetInt(nameof(lanternFrameY));
            }
            catch (Exception e)
            {

            }
        }

        public override void Load()
        {
            for (int j = 1; j <= 3; j++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "JadeFables/Tiles/JadeLantern/JadeChainGore" + j);

            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "JadeFables/Tiles/JadeLantern/Gores/lantern" + i + "gore" + j);
                }
            }
            On.Terraria.Main.DrawProjectiles += Main_DrawProjectiles;
        }

        private void Main_DrawProjectiles(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            foreach (KeyValuePair<int, TileEntity> item in TileEntity.ByID)
            {
                if (item.Value is JadeLanternTileEntity te)
                    te.Draw();
            }

            Main.spriteBatch.End();
        }

        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.HasTile && (tile.TileType == ModContent.TileType<JadeLantern>() || tile.TileType == ModContent.TileType<JadeLanternFurniture>());
        }

        public override void Update()
        {
            if (!initialized)
            {
                initialized = true;
                SpawnIn();
            }
            Tile tile = Main.tile[Position.X, Position.Y];
            if (!tile.HasTile || !(tile.TileType == ModContent.TileType<JadeLantern>() || tile.TileType == ModContent.TileType<JadeLanternFurniture>()))
                Kill(Position.X, Position.Y);

            if (chain == null)
                return;
            chain.UpdateChain();
            for (int i = 0; i < chain.segmentCount - burnedSegments; i++)
            {
                RopeSegment segment = chain.ropeSegments[i];
                if (Collision.CheckAABBvAABBCollision(Main.LocalPlayer.TopLeft, Main.LocalPlayer.Hitbox.Size(), segment.posNow - new Vector2(6, 6), new Vector2(12, 12)))
                {
                    segment.posNow.X += Main.LocalPlayer.velocity.X * 0.2f;
                }
            }

            if (!burning)
            {
                RopeSegment seg = chain.ropeSegments[chain.segmentCount - 1];

                Lighting.AddLight(seg.posNow, Color.Orange.ToVector3() * 0.6f);
                Rectangle hitbox = new Rectangle((int)seg.posNow.X - 16, (int)seg.posNow.Y - 16, 32, 32);
                if (Main.projectile.Any(n => n.active && n.friendly && n.Colliding(n.Hitbox, hitbox)))
                {
                    Break();
                }

                if (Main.rand.NextBool(60))
                {
                    Dust.NewDustPerfect(seg.posNow + Main.rand.NextVector2Circular(6, 6), ModContent.DustType<LanternGlow>(), Main.rand.NextVector2Circular(1, 1), 0, Color.OrangeRed, Main.rand.NextFloat(0.35f, 0.55f));
                }
            }
            else
            {
                burnTimer++;
                if (burnTimer > 3)
                {
                    burnTimer = 0;
                    RopeSegment burnSegment = chain.ropeSegments[(chain.segmentCount - 1) - burnedSegments];

                    burnedSegments++;
                    if (burnedSegments >= chain.segmentCount - 1)
                    {
                        tile.HasTile = false;
                    }

                    for (int i = 0; i < 2; i++)
                        Dust.NewDustPerfect(burnSegment.posNow + new Vector2(0, -4) + Main.rand.NextVector2Circular(6, 6), ModContent.DustType<LanternGlow>(), Main.rand.NextVector2Circular(1.3f, 1.3f), 0, Color.OrangeRed, Main.rand.NextFloat(0.45f, 0.55f));
                }
            }

        }

        public virtual void Break()
        {
            EntitySource_TileBreak source = new((int)(WorldPosition.X / 16), (int)(WorldPosition.Y / 16));
            Tile tile = Main.tile[(int)(WorldPosition.X / 16), (int)(WorldPosition.Y / 16)];
            RopeSegment seg = chain.ropeSegments[chain.segmentCount - 1];
            if (!burnable)
            {
                foreach (RopeSegment segment in chain.ropeSegments)
                {
                    Gore.NewGoreDirect(source, segment.posNow, Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("JadeChainGore" + ((chainFrame.Y / 22) + 1)).Type);
                }
                tile.HasTile = false;
            }
            else
                burning = true;

            Helpers.Helper.PlayPitched("LanternBreak", 0.6f, Main.rand.NextFloat(-0.1f, 0.1f), seg.posNow);

            for (int i = 1; i <= 4; i++)
            {
                Gore.NewGoreDirect(source, seg.posNow, Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("lantern" + ((lanternFrame.Y / 32) + 1) + "gore" + i).Type);
            }
            for (int i = 0; i < 5; i++)
                Dust.NewDustPerfect(seg.posNow + Main.rand.NextVector2Circular(12, 12), ModContent.DustType<LanternGlow>(), Main.rand.NextVector2Circular(3, 3), 0, Color.OrangeRed, Main.rand.NextFloat(0.85f, 1.15f));

            for (int i = 0; i < 13; i++)
                Dust.NewDustPerfect(seg.posNow + Main.rand.NextVector2Circular(12, 12), DustID.Torch, Main.rand.NextVector2Circular(3, 3), default, default, Main.rand.NextFloat(1, 1.3f));

            Loot();
        }

        public void Loot()
        {
            RopeSegment seg = chain.ropeSegments[chain.segmentCount - 1];
            int i = (int)(seg.posNow.X / 16);
            int j = (int)(seg.posNow.Y / 16);
            EntitySource_TileBreak source = new((int)(WorldPosition.X / 16), (int)(WorldPosition.Y / 16));
            if (Main.rand.NextBool(250))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(source, (i + 1.5f) * 16f, j * 16f, 0f, 0f, ProjectileID.CoinPortal, 0, 0, Main.myPlayer);
            }
            else
            {
                if (Main.expertMode ? Main.rand.Next(45) < 2 : Main.rand.NextBool(45))
                {
                    switch (Main.rand.Next(10))
                    {
                        case 0:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.BattlePotion);
                            break;
                        case 1:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.EndurancePotion);
                            break;
                        case 2:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.InvisibilityPotion);
                            break;
                        case 3:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.ManaRegenerationPotion);
                            break;
                        case 4:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.MagicPowerPotion);
                            break;
                        case 5:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.TitanPotion);
                            break;
                        case 6:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.WrathPotion);
                            break;
                    }
                }
                else
                {
                    switch (Main.rand.Next(7))
                    {
                        case 0:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.Heart);
                            if (Main.rand.NextBool(2))
                                Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.Heart);
                            break;
                        case 1:
                            if (Main.tile[i, j].LiquidAmount == 255 && Main.tile[i, j].LiquidType == LiquidID.Water)
                                Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.SpelunkerGlowstick, Main.rand.Next(Main.expertMode ? 5 : 4, Main.expertMode ? 18 : 12));
                            else
                                Item.NewItem(source, i * 16, j * 16, 32, 16, ModContent.ItemType<Tiles.JadeTorch.JadeTorch>(), Main.rand.Next(Main.expertMode ? 5 : 4, Main.expertMode ? 18 : 12));
                            break;
                        case 2:
                            goto case 5;
                        case 3:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.HealingPotion);
                            if (Main.rand.NextBool(3)) { Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.HealingPotion); }
                            break;
                        case 4:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.Bomb, Main.rand.Next(1, Main.expertMode ? 7 : 4));
                            break;
                        case 5:
                            for (int k = 0; k < Main.rand.Next(1, 4); k++)
                            {
                                if (Main.rand.NextBool(2)) { Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.CopperCoin, Main.rand.Next(1, 99)); }
                            }
                            for (int k = 0; k < Main.rand.Next(1, 4); k++)
                            {
                                if (Main.rand.NextBool(2)) { Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.SilverCoin, Main.rand.Next(1, 3)); }
                            }
                            break;
                        case 6:
                            goto case 5;
                    }
                }
            }
        }

        public int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
        { 
            return Place(i, j);
        }

        public void SpawnIn()
        {
            if (length == 0)
            {
                length = StartLength;
                chainFrameY = Main.rand.Next(3);
                lanternFrameY = Main.rand.Next(4);
            }
            chain = new VerletChain(length, true, WorldPosition + new Vector2(8, 0), 14, true);
            chain.Start();
            chain.forceGravity = new Vector2(0, 0.4f);

            chainFrame = new Rectangle(0, 22 * chainFrameY, 12, 22);
            pivotFrame = new Rectangle(0, 10 * chainFrameY, 14, 10);
            lanternFrame = new Rectangle(0, 32 * lanternFrameY, 32, 32);
        }

        public void Draw()
        {
            if (chain == null)
                return;
            string texPath = "JadeFables/Tiles/JadeLantern/JadeLanternProj";
            Texture2D chainTex = ModContent.Request<Texture2D>(texPath + "_Chain").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(texPath + "_Glow").Value;
            Texture2D pivotTex = ModContent.Request<Texture2D>(texPath + "_Pivot").Value;
            Texture2D lanternTex = ModContent.Request<Texture2D>(texPath).Value;
            Texture2D backTex = ModContent.Request<Texture2D>(texPath + "_BackGlow").Value;

            Color glowColor = Color.Orange;
            glowColor.A = 0;

            RopeSegment seg = chain.ropeSegments[chain.segmentCount - 2];
            RopeSegment nextSeg = chain.ropeSegments[chain.segmentCount - 1];

            if (!burning)
            {
                Main.spriteBatch.Draw(backTex, nextSeg.posNow - Main.screenPosition, null, glowColor * 0.4f, 0, backTex.Size() / 2, 0.7f, SpriteEffects.None, 0f);
            }

            for (int i = (chain.segmentCount - 2) - burnedSegments; i >= 0; i--)
            {
                RopeSegment segInner = chain.ropeSegments[i];
                RopeSegment nextSegInner = chain.ropeSegments[i + 1];
                Main.spriteBatch.Draw(chainTex, segInner.posNow - Main.screenPosition, chainFrame, Lighting.GetColor((int)(segInner.posNow.X / 16), (int)(segInner.posNow.Y / 16)), segInner.posNow.DirectionTo(nextSegInner.posNow).ToRotation() + 1.57f, chainFrame.Size() / 2, 1, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(pivotTex, (WorldPosition - new Vector2(-8, 6)) - Main.screenPosition, pivotFrame, Lighting.GetColor((int)(WorldPosition.X / 16), (int)(WorldPosition.Y / 16)), 0, pivotFrame.Size() / 2, 1, SpriteEffects.None, 0f);

            if (!burning)
            {
                Main.spriteBatch.Draw(lanternTex, nextSeg.posNow - Main.screenPosition, lanternFrame, Lighting.GetColor((int)(nextSeg.posNow.X / 16), (int)(nextSeg.posNow.Y / 16)), seg.posNow.DirectionTo(nextSeg.posNow).ToRotation() - 1.57f, lanternFrame.Size() / 2, 1, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glowTex, nextSeg.posNow - Main.screenPosition, lanternFrame, glowColor * 0.6f, seg.posNow.DirectionTo(nextSeg.posNow).ToRotation() - 1.57f, lanternFrame.Size() / 2, 1, SpriteEffects.None, 0f);
            }
        }

    }
}
