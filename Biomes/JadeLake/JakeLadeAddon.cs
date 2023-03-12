﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using JadeFables.Core;
using JadeFables.NPCs;
using rail;
using Terraria.GameContent;
using JadeFables.Dusts;
using static tModPorter.ProgressUpdate;

namespace JadeFables.Biomes.JadeLake
{
	class JadeLakeAddon : WaterAddon
	{
		public override bool Visible => Main.LocalPlayer.InModBiome<JadeLakeBiome>() || GetInstance<JadeLakeSystem>().forceLakeAesthetic;

		public override Texture2D BlockTexture(Texture2D normal, int x, int y)
		{
			return normal;
		}

		public override void SpritebatchChange()
		{
            /*Main.spriteBatch.Begin();
			Main.spriteBatch.Draw(HotspringMapTarget.hotspringShineTarget, Microsoft.Xna.Framework.Vector2.Zero, Microsoft.Xna.Framework.Color.White);
			Main.spriteBatch.End();*/
            var effect = Filters.Scene["JadeLakeWater"].GetShader().Shader;

            //var a = Vector2.Normalize(Helpers.Helper.ScreenSize);
            //effect.Parameters["offset"].SetValue(Main.screenPosition - HotspringMapTarget.oldScreenPos);
            Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            // Main.spriteBatch.Draw(Core.Systems.Edits.BetterWaterTrianglesAttmptTwo.waterSlopeTarget, -new Vector2(Main.offScreenRange, Main.offScreenRange), null, Color.White * 0.3f);
        }

		public override void SpritebatchChangeBack()
		{
            //JadeLakeMapTarget.oldScreenPos = Main.screenPosition;
            var effect = Filters.Scene["JadeLakeWater"].GetShader().Shader;
            //the multiply by 1.3 and 1.5 seem to fix the jittering when moving, seems to be tied to the 2 magic numbers in Visuals.HotspringMapTarget.cs
            //effect.Parameters["offset"].SetValue(Main.screenPosition - HotspringMapTarget.oldScreenPos);
            Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            //Main.spriteBatch.Draw(Core.Systems.Edits.BetterWaterTrianglesAttmptTwo.waterSlopeTarget, -new Vector2(Main.offScreenRange, Main.offScreenRange), null, Color.White);
		}
	}

    class JadeLakeMapTarget : ILoadable
    {
        public static RenderTarget2D jadelakeMapTarget;
        public static RenderTarget2D jadelakeShineTarget;
        public static RenderTarget2D jadelakeBubbleTarget;

        public static bool UpdateOffset = false;

        public static Vector2 oldScreenPos;
        public static Vector2 oldTranslation;

        public static Vector2 cameraDiff = Vector2.Zero;
        public static Vector2 jerk = Vector2.Zero;

        public static int framesSinceDraw = 0;

        public void Load(Mod mod)
        {
            if (Main.dedServ)
                return;

            On.Terraria.Main.CheckMonoliths += HotspringTarget;
        }

        public void Unload()
        {
            jadelakeMapTarget = null;
        }

        private void HotspringTarget(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.gameMenu || !GetInstance<JadeLakeAddon>().Visible)
                return;

            var translation = Main.screenPosition - oldScreenPos;
            oldScreenPos = Main.screenPosition;

            var effect = Terraria.Graphics.Effects.Filters.Scene["JadeLakeWater"].GetShader().Shader;
            jerk += (oldTranslation / 4) - translation;
            cameraDiff = translation;
            translation *= 4;

            if (Main.renderCount == 1)
            {
                effect.Parameters["offset"].SetValue(Vector2.Zero);
            }

            effect.Parameters["sampleTexture2"].SetValue(JadeLakeMapTarget.jadelakeMapTarget);
            effect.Parameters["time"].SetValue(Main.GameUpdateCount / 20f);
            effect.Parameters["bubbleTex"].SetValue(JadeLakeMapTarget.jadelakeBubbleTarget);
            effect.Parameters["sampleTexture3"].SetValue(JadeLakeMapTarget.jadelakeShineTarget);
            if (Main.renderCount != 1)
            {
                framesSinceDraw++;
                Vector2 offset = (jerk / -(Main.waterTarget.Size()));
                effect.Parameters["offset"].SetValue(offset);
                return;
                //effect.Parameters["sampleTexture2"].SetValue(TextureAssets.MagicPixel.Value);
            }
            framesSinceDraw = 0;
            oldTranslation = translation;
            cameraDiff = Vector2.Zero;
            oldScreenPos = Main.screenPosition;
            jerk = Vector2.Zero;
            Vector2 RTratio = Main.ScreenSize.ToVector2() / Main.waterTarget.Size();

            var graphics = Main.graphics.GraphicsDevice;

            int RTwidth = Main.waterTarget.Width;
            int RTheight = Main.waterTarget.Height;
            if (jadelakeMapTarget is null || jadelakeMapTarget.Size() != new Vector2(RTwidth, RTheight))
                jadelakeMapTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);

            if (jadelakeShineTarget is null || jadelakeShineTarget.Size() != new Vector2(RTwidth, RTheight))
                jadelakeShineTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);

            if (jadelakeBubbleTarget is null || jadelakeBubbleTarget.Size() != new Vector2(RTwidth, RTheight))
                jadelakeBubbleTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);

            graphics.SetRenderTarget(jadelakeMapTarget);

            graphics.Clear(Color.Transparent);
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Matrix.CreateTranslation(new Vector3(-translation.X, -translation.Y, 0)));

            DrawGradients();

            Main.spriteBatch.End();
            graphics.SetRenderTarget(null);


            //if (Main.renderCount == 3)//
            //{
            Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Matrix.CreateTranslation(new Vector3(-translation.X, -translation.Y, 0)));

            Main.graphics.GraphicsDevice.SetRenderTarget(jadelakeShineTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Texture2D tex2 = Terraria.ModLoader.ModContent.Request<Texture2D>("JadeFables/Assets/WaterMap", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            //The seam issue is not in this file, See StarlightRiver.cs and enable the commented out PostDrawInterface hook to view RTs
            for (int i = -tex2.Width; i <= RTwidth + tex2.Width; i += tex2.Width)
                for (int j = -tex2.Height; j <= RTheight + tex2.Height; j += tex2.Height)
                {
                    //the divide by 1.3 and 1.5 are what keep the tile tied to the world location, seems to be tied to the 2 magic numbers in HotspringAddon.cs
                    Vector2 pos = (new Vector2(i, j));
                    Main.spriteBatch.Draw(tex2, pos - new Vector2(Main.sceneWaterPos.X % tex2.Width, Main.sceneWaterPos.Y % tex2.Height), null, Color.White);

                    //Vector2 debugSize = new Vector2(32, 156);

                    //float posOffY = (float)Math.Sin(Main.GameUpdateCount / 15f) * (Main.screenHeight / 2.2f);
                    //Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle(0, Main.screenHeight / 2 + (int)posOffY, (int)debugSize.X, (int)debugSize.Y), Color.Red);

                    //float posOffY2 = (float)Math.Sin(Main.GameUpdateCount / 13.33f) * (Main.screenHeight / 2.2f);
                    //Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle(Main.screenWidth - (int)debugSize.X, Main.screenHeight / 2 + (int)posOffY2, (int)debugSize.X, (int)debugSize.Y), Color.Red);
                }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.AlphaBlend, SamplerState.PointWrap, default, default, default, Matrix.CreateTranslation(new Vector3(-translation.X, -translation.Y, 0)));

            Main.graphics.GraphicsDevice.SetRenderTarget(jadelakeBubbleTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            var bubbles = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<AirBubble>());
            foreach (Projectile proj in bubbles)
            {
                (proj.ModProjectile as AirBubble).DrawBubble(Main.sceneWaterPos);
            }

            Main.spriteBatch.End();
            Main.graphics.GraphicsDevice.SetRenderTarget(null);
        }

        private bool ShouldDrawGradient(int i, int j)
        {
            Tile tile = Main.tile[i, j];

            if (tile.LiquidType != LiquidID.Water)
                return false;

            if (tile.LiquidAmount == 0)
                return false;

            Tile tileAbove = Main.tile[i, j - 1];
            if (tileAbove.LiquidAmount > 0 || (tileAbove.HasTile && Main.tileSolid[tileAbove.TileType]))
                return false;
            return true;
        }

        private int WaterDepthPixels(int i, int j)
        {
            int depth = 0;
            //depth = -16 + (Main.tile[i, j].LiquidAmount / 16); unnecessary precision
            for (int y = j; y < j + 10; y++)
            {
                Tile tile = Framing.GetTileSafely(i, y);

                if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Water)
                {
                    depth += 16;
                }
                else
                {
                    break;
                }
            }
            return depth;
        }
        private void DrawGradients()
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("JadeFables/Assets/WaterGradient").Value;

            for (int i = 0; i < Main.maxTilesX; i++)
            {
                if (new Vector2(i * 16f, Main.LocalPlayer.Center.Y).Distance(Main.LocalPlayer.Center) < Main.screenWidth / 2)
                    for (int j = 0; j < Main.maxTilesY; j++)
                    {
                        Tile tile = Main.tile[i, j];
                        if (ShouldDrawGradient(i, j))
                        {
                            Main.spriteBatch.Draw(glowTex, (new Vector2(i, j) * 16) - Main.sceneWaterPos, new Rectangle(0, 0, glowTex.Width, Math.Min(WaterDepthPixels(i, j) / 2, glowTex.Height)), Color.White * 0.4f, 0, new Vector2(glowTex.Width / 2, 0), new Vector2(1, 2), SpriteEffects.None, 0f);
                            /*float heightScale = ((float)Math.Sin(Main.GameUpdateCount * 0.025f) / 8) + 1;

                            Color overlayColor = Color.White;
                            bool emptyLeft;
                            bool emptyRight;

                            emptyLeft = !ShouldDrawGradient(i - 1, j);
                            emptyRight = !ShouldDrawGradient(i + 1, j);

                            Vector2 offset2 = new Vector2(0, -8);
                            if (emptyLeft)
                                if (emptyRight) //solo
                                    Main.spriteBatch.Draw(Request<Texture2D>("JadeFables/Assets/GlowSolo").Value, offset2 + new Vector2(i, j) - Main.sceneWaterPos, null, overlayColor, 0, Vector2.Zero, new Vector2(1, 2), SpriteEffects.FlipVertically, 0f);
                                else            //left
                                    Main.spriteBatch.Draw(Request<Texture2D>("JadeFables/Assets/GlowLeft").Value, offset2 + (new Vector2(i, j) * 16) - Main.sceneWaterPos, null, overlayColor, 0, Vector2.Zero, new Vector2(1, 2), SpriteEffects.FlipVertically, 0f);
                            else if (emptyRight)//right
                                Main.spriteBatch.Draw(Request<Texture2D>("JadeFables/Assets/GlowRight").Value, offset2 + (new Vector2(i, j) * 16) - Main.sceneWaterPos, null, overlayColor, 0, Vector2.Zero, new Vector2(1, 2), SpriteEffects.FlipVertically, 0f);
                            else                //both
                                Main.spriteBatch.Draw(Request<Texture2D>("JadeFables/Assets/GlowMid").Value, offset2 + (new Vector2(i, j) * 16) - Main.sceneWaterPos, null, overlayColor, 0, Vector2.Zero, new Vector2(1,2), SpriteEffects.FlipVertically, 0f);


                            Texture2D glowLines = Request<Texture2D>("JadeFables/Assets/GlowLines").Value;
                            int realX = i * 16;
                            int realY = j * 16;
                            int realWidth = glowLines.Width - 1;//1 pixel offset since the texture has a empty row of pixels on the side, this is also accounted for elsewhere below
                            Color drawColor = overlayColor * 0.35f;

                            realWidth = (int)MathHelper.Max(1, realWidth);
                            float val = (((Main.GameUpdateCount * 0.3333f) + realY) % realWidth);
                            int offset = (int)(val + (realX % realWidth) - realWidth);

                            Main.spriteBatch.Draw(glowLines, new Rectangle((int)offset2.X + realX - (int)Main.sceneWaterPos.X, (int)offset2.Y + realY - (int)Main.sceneWaterPos.Y, 16, glowLines.Height * 2), new Rectangle(offset + 1, 0, 16, (int)(glowLines.Height * heightScale)), drawColor, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0f);

                            if (offset < 0)
                            {
                                int rectWidth = Math.Min(-offset, 16);
                                Main.spriteBatch.Draw(glowLines, new Rectangle(realX - (int)Main.sceneWaterPos.X, realY - (int)Main.sceneWaterPos.Y, rectWidth, glowLines.Height * 5), new Rectangle(offset + 1 + realWidth, 0, rectWidth, (int)(glowLines.Height * heightScale)), drawColor, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
                            }*/
                        }
                    }
            }

        }
    }
}
