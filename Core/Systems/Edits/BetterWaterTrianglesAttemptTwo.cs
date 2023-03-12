﻿using System.Reflection;
using System.Transactions;
using JadeFables.Biomes.JadeLake;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.GameContent.Tile_Entities;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;
using Terraria.Graphics.Effects;
using Terraria.Utilities;

namespace JadeFables.Core.Systems.Edits;

public sealed class BetterWaterTrianglesAttmptTwo : RuntimeDetourModSystem
{
    public static bool drawWater = false;

    public static MethodInfo? DrawTile_LiquidBehindTile_Info;
    public static Action<TileDrawing, bool, int, Vector2, Vector2, int, int, TileDrawInfo>? DrawTile_LiquidBehindTile;

    public static RenderTarget2D waterSlopeTarget;

    public override void OnModLoad() 
    {
        if (Main.dedServ)
            return;

        DrawTile_LiquidBehindTile_Info = typeof(TileDrawing).GetMethod("DrawTile_LiquidBehindTile", BindingFlags.NonPublic | BindingFlags.Instance);
        //Here we cache this method for performance
        DrawTile_LiquidBehindTile = (Action<TileDrawing, bool, int, Vector2, Vector2, int, int, TileDrawInfo>)Delegate.CreateDelegate(
            typeof(Action<TileDrawing, bool, int, Vector2, Vector2, int, int, TileDrawInfo>), DrawTile_LiquidBehindTile_Info);

        //On.Terraria.GameContent.Liquid.LiquidRenderer.InternalDraw += LiquidRenderer_InternalDraw;
        On.Terraria.Main.CheckMonoliths += DrawToRT;
        On.Terraria.GameContent.Drawing.TileDrawing.DrawTile_LiquidBehindTile += TileDrawing_DrawTile_LiquidBehindTile;
        On.Terraria.GameContent.Drawing.TileDrawing.DrawPartialLiquid += TileDrawing_DrawPartialLiquid;
        On.Terraria.Main.DoDraw_Tiles_Solid += Main_DoDraw_Tiles_Solid;
    }

    private void TileDrawing_DrawPartialLiquid(On.Terraria.GameContent.Drawing.TileDrawing.orig_DrawPartialLiquid orig, TileDrawing self, Tile tileCache, Vector2 position, Rectangle liquidSize, int liquidType, Color aColor)
    {
        if (!Main.LocalPlayer.InModBiome<JadeLakeBiome>() || liquidType != LiquidID.Water)
        {
            orig(self, tileCache, position, liquidSize, liquidType, aColor);
        }
        else
        {
            int num = (int)tileCache.BlockType;
            if (!TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType] || num == 0)
            {
                Main.spriteBatch.Draw(ModContent.Request<Texture2D>("JadeFables/Biomes/JadeLake/JadeLakeWaterStyle_Block").Value, position, liquidSize, aColor * 0.7f, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                return;
            }
            liquidSize.X += 18 * (num - 1);
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>("JadeFables/Biomes/JadeLake/JadeLakeWaterStyle_Block").Value, position, liquidSize, aColor * 0.7f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }

    private void Main_DoDraw_Tiles_Solid(On.Terraria.Main.orig_DoDraw_Tiles_Solid orig, Main self)
    {
        if (Main.LocalPlayer.InModBiome<JadeLakeBiome>())
        {
            var effect = Filters.Scene["JadeLakeWater"].GetShader().Shader;

            //var a = Vector2.Normalize(Helpers.Helper.ScreenSize);
            //effect.Parameters["offset"].SetValue(Main.screenPosition - HotspringMapTarget.oldScreenPos);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.Transform);
            Main.spriteBatch.Draw(waterSlopeTarget, -new Vector2(Main.offScreenRange, Main.offScreenRange), null, Color.White);
            Main.spriteBatch.End();
        }
        orig(self);
    }

    private void TileDrawing_DrawTile_LiquidBehindTile(On.Terraria.GameContent.Drawing.TileDrawing.orig_DrawTile_LiquidBehindTile orig, TileDrawing self, bool solidLayer, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY, TileDrawInfo drawData)
    {
        if (drawWater)
        {
            orig(self, solidLayer, waterStyleOverride, screenPosition, screenOffset, tileX, tileY, drawData);
        }
    }

    private void DrawToRT(On.Terraria.Main.orig_CheckMonoliths orig)
    {
        orig();
        var graphics = Main.graphics.GraphicsDevice;

        int RTwidth = Main.waterTarget.Width;
        int RTheight = Main.waterTarget.Height;
        if (waterSlopeTarget is null || waterSlopeTarget.Size() != new Vector2(RTwidth, RTheight))
            waterSlopeTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);

        if (Main.gameMenu)
            return;

        if (Main.LocalPlayer.InModBiome<JadeLakeBiome>())
        {
            Vector2 drawOffset = (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange)) - Main.screenPosition;
            drawWater = true;

            Main.graphics.GraphicsDevice.SetRenderTarget(waterSlopeTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin();

            for (int i = (int)(Main.screenPosition.X / 16) - 12; i < (int)((Main.screenPosition.X + Main.screenWidth) / 16) + 16; i++)
            {
                for (int j = (int)(Main.screenPosition.Y / 16) - 12; j < (int)((Main.screenPosition.Y + Main.screenHeight) / 16) + 16; j++)
                {
                    TileDrawInfo drawInfo = new TileDrawInfo();
                    drawInfo.tileCache = Framing.GetTileSafely(i, j);
                    Vector3[] colorSlices = new Vector3[9];
                    Lighting.GetColor9Slice(i, j, ref colorSlices);
                    drawInfo.colorSlices = colorSlices;
                    if (drawInfo.tileCache.HasTile && Main.tileSolid[drawInfo.tileCache.TileType])
                        DrawTile_LiquidBehindTile(Main.instance.TilesRenderer, true, 0, Main.screenPosition, new Vector2(Main.offScreenRange, Main.offScreenRange), i, j, drawInfo);
                }
            }
            Main.spriteBatch.End();
            Main.graphics.GraphicsDevice.SetRenderTarget(null);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

        }
        drawWater = !Main.LocalPlayer.InModBiome<JadeLakeBiome>();
    }
}