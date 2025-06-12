﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using JadeFables.Dusts;
using static JadeFables.Tiles.JadeWaterfall.WaterfallLight;
using static JadeFables.Tiles.JadeWaterfall.JadeWaterfallProj;
using System.Security.Cryptography.X509Certificates;
using JadeFables.Biomes.JadeLake;
using System.Diagnostics;
using JadeFables.Helpers;

namespace JadeFables.Tiles.JadeWaterfall
{
    public class JadeWaterfallTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0); // Todo: make less annoying.
            //TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);
            DustType = -1;
            MinPick = 999;

            /*LocalizedText name = CreateMapEntryName();

            AddMapEntry(new Color(9, 61, 191), name);*/ //Don't display since it'd be weird to have it only take up one tile
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            var existingGrass = Main.projectile.Where(n => n.active && n.Center == new Vector2(i, j) * 16 && n.type == ModContent.ProjectileType<JadeWaterfallProj>()).FirstOrDefault();
            if (existingGrass == default)
            {
                Projectile.NewProjectile(new EntitySource_Misc("Jade Waterfall"), new Vector2(i, j) * 16, Vector2.Zero, ModContent.ProjectileType<JadeWaterfallProj>(), 0, 0);
            }
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            fail = true;
            effectOnly = true;
            noItem = true;
        }
    }

    public class JadeWaterfallBucket : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.AlsoABuildingItem[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = Item.CommonMaxStack;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.White;
            Item.value = 0;
        }

        public override bool CanUseItem(Player player)
        {
            Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
            if (tile.HasTile && !Main.tileCut[tile.TileType] || !Helpers.Helper.TileInRange(player, Item))
                return false;
            return base.CanUseItem(player);
        }
        public override Nullable<bool> UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
        {
            if (!player.ItemTimeIsZero || player.itemAnimation != player.itemAnimationMax) return base.UseItem(player);

            SoundEngine.PlaySound(SoundID.SplashWeak);
            WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, TileType<JadeWaterfallTile>(), true);
            Item.stack--;
            player.PutItemInInventoryFromItemUsage(ItemID.EmptyBucket, player.selectedItem);
            return true;
        }
    }

    public class BucketObtainability : GlobalItem
    {
        public override Nullable<bool> UseItem(Item item, Player player)/* tModPorter Suggestion: Return null instead of false */
        {
            if (player.itemAnimation == item.useAnimation - 1 && item.type == ItemID.EmptyBucket && player.InInteractionRange(Player.tileTargetX, Player.tileTargetY, TileReachCheckSettings.Simple))
            {
                for (int j = 0; j < 2; j++)
                {
                    Tile tile = Main.tile[Player.tileTargetX - j, Player.tileTargetY];
                    if (tile.HasTile && tile.TileType == ModContent.TileType<JadeWaterfallTile>())
                    {
                        tile.TileColor = PaintID.None;
                        tile.HasTile = false;
                        SoundEngine.PlaySound(SoundID.SplashWeak, player.Center);
                        item.stack--;
                        player.PutItemInInventoryFromItemUsage(ModContent.ItemType<JadeWaterfallBucket>(), player.selectedItem);
                        return true;
                    }
                }
            }
            return base.UseItem(item, player);
        }
    }

    public class JadeWaterfallHashSetsReset : ModSystem
    {
        public override void PreUpdateProjectiles()
        {
            waterfallTiles.Clear();
            waterfallColumns.Clear();
            paintedWaterfalls.Clear();
        }
    }

    public class JadeWaterfallProj : ModProjectile
    {
        Tile originLeft => Main.tile[(int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)];
        Tile originRight => Main.tile[(int)(Projectile.Center.X / 16) + 1, (int)(Projectile.Center.Y / 16)];

        public static readonly int MAXLENGTH = 120;
        public static readonly int FADEOUTLENGTH = 7;

        int length = 0;

        int frame = 0;
        int yFrames = 6;

        public bool foundWater = false;

        int soundTimer = 0;

        private Microsoft.Xna.Framework.Audio.SoundEffectInstance sound;
        private ActiveSound soundInstance;

        public override void Load()
        {
            Terraria.On_Main.DrawPlayers_AfterProjectiles += Main_DrawPlayers_AfterProjectiles;
        }

        private void Main_DrawPlayers_AfterProjectiles(Terraria.On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
        {
            orig(self);
            {
                Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
                var toDraw = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<JadeWaterfallProj>()).ToList();
                toDraw.ForEach(n => (n.ModProjectile as JadeWaterfallProj).Draw());
                Main.spriteBatch.End();
            }
        }

        public override void SetDefaults()
        {
            Projectile.knockBack = 6f;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 10;
            Projectile.hide = true;
        }

        public static Dictionary<UniversalVariationKey, WhateverPaintRenderTargetHolder> _paintRenders = new();
        public void Draw()
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D topTex = ModContent.Request<Texture2D>(Texture + "_Top").Value;
            Texture2D bottomTex = ModContent.Request<Texture2D>(Texture + "_Bottom").Value;
            int topFrameHeight = topTex.Height / yFrames;
            Rectangle topFrameBox = new Rectangle(0, (frame % yFrames) * topFrameHeight, topTex.Width, topFrameHeight);
            Color topColor = Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));

            Texture2D? paintedTopTexture = _paintRenders.TryGetTexturePaintAndRequestIfNotReady(Projectile.type, originLeft.TileColor, Texture + "_Top", -1);
            if (paintedTopTexture is not null) PaintHelper.DrawWithCoating(originLeft.IsTileFullbright, originLeft.IsTileInvisible, paintedTopTexture, Projectile.Center - Main.screenPosition, topFrameBox, topColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);


            //PaintHelper.DrawWithPaint(originLeft.TileColor, Texture + "_Top", Projectile.Center - Main.screenPosition, topFrameBox, topColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
            int i;
            for (i = 1; i < length; i++)
            {
                int tileHeight = 4;
                Vector2 pos = Projectile.Center + (Vector2.UnitY * 16 * i);
                int frameHeight = (tex.Height / yFrames) / tileHeight;
                Rectangle frameBox = new Rectangle(0, (tileHeight * frameHeight * ((((i - 1) / tileHeight) + frame) % yFrames)) + (frameHeight * ((i - 1) % tileHeight)), tex.Width, frameHeight);
                Color color = Lighting.GetColor((int)(pos.X / 16), (int)(pos.Y / 16));

                Texture2D? paintedTexture = _paintRenders.TryGetTexturePaintAndRequestIfNotReady(Projectile.type + 1, originLeft.TileColor, Texture, -1);
                if (paintedTexture is not null) PaintHelper.DrawWithCoating(originLeft.IsTileFullbright, originLeft.IsTileInvisible, paintedTexture, pos - Main.screenPosition, frameBox, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
                //PaintHelper.DrawWithPaint(originLeft.TileColor, Texture, pos - Main.screenPosition, frameBox, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }

            if (length == MAXLENGTH)
            {
                for (i = length; i < length + FADEOUTLENGTH; i++)
                {
                    int tileHeight = 4;
                    Vector2 pos = Projectile.Center + (Vector2.UnitY * 16 * i);
                    int frameHeight = (tex.Height / yFrames) / tileHeight;
                    Rectangle frameBox = new Rectangle(0, (tileHeight * frameHeight * ((((i - 1) / tileHeight) + frame) % yFrames)) + (frameHeight * ((i - 1) % tileHeight)), tex.Width, frameHeight);
                    Color color = Lighting.GetColor((int)(pos.X / 16), (int)(pos.Y / 16)) * (1 - ((i - length) / (float)FADEOUTLENGTH));

                    Texture2D paintedTexture = _paintRenders.TryGetTexturePaintAndRequestIfNotReady(Projectile.type + 1, originLeft.TileColor, Texture, -1);
                    if (paintedTexture is not null) PaintHelper.DrawWithCoating(originLeft.IsTileFullbright, originLeft.IsTileInvisible, paintedTexture, pos - Main.screenPosition, frameBox, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
                    //PaintHelper.DrawWithPaint(originLeft.TileColor, Texture, pos - Main.screenPosition, frameBox, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
                }
            }

            if (!foundWater)
                return;
            int bottomFrameHeight = bottomTex.Height / yFrames;
            Rectangle bottomFrameBox = new Rectangle(0, ((i + frame) % yFrames) * bottomFrameHeight, bottomTex.Width, bottomFrameHeight);
            Vector2 bottomPos = Projectile.Center + (Vector2.UnitY * 16 * i) - new Vector2(16, 16);
            Color bottomColor = Lighting.GetColor((int)(bottomPos.X / 16), (int)(bottomPos.Y / 16));

            Texture2D paintedBottomTexture = _paintRenders.TryGetTexturePaintAndRequestIfNotReady(Projectile.type + 2, originLeft.TileColor, Texture + "_Bottom", -1);
            if (paintedBottomTexture is not null) PaintHelper.DrawWithCoating(originLeft.IsTileFullbright, originLeft.IsTileInvisible, paintedBottomTexture, bottomPos - Main.screenPosition, bottomFrameBox, bottomColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
            //PaintHelper.DrawWithPaint(originLeft.TileColor, Texture + "_Bottom", bottomPos - Main.screenPosition, bottomFrameBox, bottomColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);

            return;
        }

        public override void AI()
        {
            if (Projectile.frameCounter++ % 4 == 0)
                frame++;
            int i = 0;
            for (i = 0; i < MAXLENGTH; i++)
            {
                foundWater = false;
                for (int j = 0; j < 2; j++)
                {
                    int x = (int)(Projectile.Center.X / 16) + j;
                    int y = (int)(Projectile.Center.Y / 16) + i;
                    Tile tile = Main.tile[x, y];

                    if (i == 0)
                    {
                        waterfallColumns.Add(x);
                        waterfallColumns.Sort();
                    }
                    waterfallTiles.Add((x, y));
                    if (originLeft.TileColor != PaintID.None) paintedWaterfalls.Add((x, y), originLeft.TileColor);

                    if (tile.LiquidAmount == 255 && (!tile.HasTile || !Main.tileSolid[tile.TileType] || tile.IsActuated))
                    {
                        Vector2 velocity = Vector2.UnitY.RotatedByRandom(0.1f) * -Main.rand.NextFloat(1f, 1.5f);
                        Vector2 pos = new Vector2(Projectile.Center.X + (j * 16), Projectile.Center.Y + (i * 16)) + new Vector2(8, 8) + Main.rand.NextVector2Circular(4, 12);
                        Dust.NewDustPerfect(pos, ModContent.DustType<WaterfallMist>(), velocity, 0, Color.White, Main.rand.NextFloat(0.025f, 0.225f));
                        foundWater = true;
                    }
                }
                if (foundWater)
                    break;
            }

            length = i;

            if (length == MAXLENGTH)
            {
                for (i = length; i < length + FADEOUTLENGTH; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int x = (int)(Projectile.Center.X / 16) + j;
                        int y = (int)(Projectile.Center.Y / 16) + i;
                        Tile tile = Main.tile[x, y];

                        if ((i - length) < FADEOUTLENGTH / 2) waterfallTiles.Add((x, y));
                        if (originLeft.TileColor != PaintID.None) paintedWaterfalls.Add((x, y), originLeft.TileColor);
                    }
                }
            }
            else if (sound == null || !soundInstance.IsPlaying)
            {
                Vector2 soundPos = Projectile.Center + new Vector2(16, i * 16);
                ReLogic.Utilities.SlotId slot = SoundEngine.PlaySound(new SoundStyle($"{nameof(JadeFables)}/Sounds/Waterfall")
                {
                    Volume = 0.1f,
                    Pitch = 0,
                    MaxInstances = 5,
                    Type = SoundType.Ambient
                }, soundPos);
                SoundEngine.TryGetActiveSound(slot, out Terraria.Audio.ActiveSound soundInstanceLocal);
                soundInstance = soundInstanceLocal;
                sound = soundInstance?.Sound;
            }

            if (originLeft.HasTile && originLeft.TileType == ModContent.TileType<JadeWaterfallTile>() && Main.player.Where(n => n.active).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault().Distance(Projectile.Center) < 2500)
                Projectile.timeLeft = 2;
        }

        public override void OnKill(int timeLeft)
        {
            sound?.Stop(true);
        }
    }

    public class WaterfallLight : GlobalWall
    {
        public static HashSet<(int, int)> waterfallTiles = new();
        public static List<int> waterfallColumns = new();
        public static Dictionary<(int, int), byte> paintedWaterfalls = new();

        public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
        {
            if (waterfallTiles.Count <= 0) return;

            if (waterfallColumns[0] > i || waterfallColumns.Last() < i) return;
            if (!waterfallColumns.Contains(i)) return;

            if (waterfallTiles.Contains((i, j)))
            {
                Color color = new Color(0, 220, 200);
                byte paintType;
                if (paintedWaterfalls.TryGetValue((i, j), out paintType))
                {
                    if (paintType == PaintID.NegativePaint) color = new Color(255 - color.R, 255 - color.G, 255 - color.B);
                    else color = WorldGen.paintColor(paintType);
                }

                const float brightness = 0.9f;

                r += color.R / (255f / brightness);
                g += color.G / (255f / brightness);
                b += color.B / (255f / brightness);
            }
        }
    }
}
