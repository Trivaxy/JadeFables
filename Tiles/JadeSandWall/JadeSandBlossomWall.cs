using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using JadeFables.Biomes.JadeLake;
using JadeFables.Helpers;

namespace JadeFables.Tiles.JadeSandWall
{
    public class JadeSandBlossomWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = false;
            DustType = DustID.Dirt;
            HitSound = SoundID.Dig;
            ItemDrop = ModContent.ItemType<JadeSandWallItem>();
            AddMapEntry(jadeSandDarkWall);
        }

        public static Dictionary<UniversalVariationKey, WhateverPaintRenderTargetHolder> _paintRenders = new();
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (i > (Main.screenPosition.X / 16) - 2 && i < (Main.screenPosition.X / 16 + Main.screenWidth / 16) + 2 && j > (Main.screenPosition.Y / 16) - 2 && j < (Main.screenPosition.Y / 16 + Main.screenHeight / 16) + 2)
            {
                string texPath = "JadeFables/Tiles/BlossomWall/BlossomWallFlow";
                var rand = new Random(i + (j * 1000000));

                if (rand.Next(80) == 2)
                {
                    float offset = i * j % 6.28f + (float)rand.NextDouble() / 8f;
                    float sin = (float)Math.Sin(Main.GameUpdateCount / 45f + offset);


                    Texture2D paintedTexture = _paintRenders.TryGetTexturePaintAndRequestIfNotReady(Type, Main.tile[i, j].WallColor, texPath, -1);
                    if (paintedTexture is not null) PaintHelper.DrawWithCoating(Main.tile[i, j].IsWallFullbright, Main.tile[i, j].IsWallInvisible, paintedTexture, (new Vector2(i + 0.5f, j + 0.5f) + BlossomWall.BlossomWall.TileAdj) * 16 + new Vector2(1, 0.5f) * sin * 2.2f - Main.screenPosition,
                    new Rectangle(rand.Next(4) * 26, 0, 24, 24), Lighting.GetColor(i, j), MathF.Sin(offset) + sin * 0.09f, new Vector2(12, 12), 1 + sin / 14f, 0, 0);
                }
            }
        }

    }
}