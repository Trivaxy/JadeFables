using Microsoft.Xna.Framework;
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
using JadeFables.Helpers;
using Terraria.Graphics.Renderers;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Transactions;
using JadeFables.Core.Systems.LightingSystem;

namespace JadeFables.Biomes.JadeLake
{
	class JadeLakeFancyBG : IOrderedLoadable
	{
		public static List<Vector4> bgPoints; //w is width, z is height

        public static RenderTarget2D circle;

        public static RenderTarget2D background;

        public float Priority => 1.1f;
		public void Load()
		{
            if (Main.dedServ)
                return;

            On.Terraria.Main.DrawBlack += ForceDrawBlack;
            IL.Terraria.Main.DrawBlack += ChangeBlackThreshold;
            On.Terraria.Main.CheckMonoliths += DrawToTargets;
            On.Terraria.Main.DoDraw_WallsTilesNPCs += Main_DoDraw_WallsTilesNPCs;
        }

        private void Main_DoDraw_WallsTilesNPCs(On.Terraria.Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
        {
            if (Main.gameMenu || Main.dedServ)
            {
                orig(self);
                return;
            }

            if (circle == null || background == null)
            {
                orig(self);
                return;
            }

            if (!Main.LocalPlayer.InModBiome<JadeLakeBiome>())
            {
                orig(self);
                return;
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            DrawTilingBackground(Main.spriteBatch);
            var effect = Filters.Scene["BackgroundMask"].GetShader().Shader;
            effect.Parameters["mask"].SetValue(circle);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(background, Vector2.Zero, null, Color.White);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            orig(self);
        }

        public void Unload()
		{

		}

        private void ForceDrawBlack(On.Terraria.Main.orig_DrawBlack orig, Main self, bool force)
        {
            if (BackgroundOnScreen())
                orig(self, true);
            else
                orig(self, force);
        }

        private void ChangeBlackThreshold(ILContext il)
        {
            var c = new ILCursor(il);
            c.TryGotoNext(n => n.MatchLdloc(6), n => n.MatchStloc(12)); //beginning of the loop, local 11 is a looping variable
            c.Index++; //this is kinda goofy since I dont think you could actually ever write c# to compile to the resulting IL from emitting here.
            c.Emit(OpCodes.Ldloc, 3); //pass the original value so we can set that instead if we dont want to change the threshold
            c.EmitDelegate<Func<float, float>>(NewThreshold); //check if were in the biome to set, else set the original value
            c.Emit(OpCodes.Stloc, 3); //num2 in vanilla, controls minimum threshold to turn a tile black
        }

        private float NewThreshold(float orig)
        {
            if (BackgroundOnScreen())
                return 0.1f;
            else
                return orig;
        }

        private void DrawToTargets(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            orig();
            if (Main.gameMenu || !BackgroundOnScreen())
                return;

            var graphics = Main.graphics.GraphicsDevice;

            if (circle is null || circle.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
                circle = new RenderTarget2D(graphics, Main.screenWidth, Main.screenHeight, default, default, default, default, RenderTargetUsage.PreserveContents);

            if (background is null || background.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
                background = new RenderTarget2D(graphics, Main.screenWidth, Main.screenHeight, default, default, default, default, RenderTargetUsage.PreserveContents);

            graphics.SetRenderTarget(circle);

            graphics.Clear(Color.Transparent);
            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default);

            bgPoints = GetBgPoints();

            Texture2D circleTex = ModContent.Request<Texture2D>("JadeFables/Assets/BackgroundCircle").Value;
            foreach (Vector4 vec4 in bgPoints)
            {
                Main.spriteBatch.Draw(circleTex, vec4.XY() - Main.screenPosition, null, Color.White, 0f, circleTex.Size() / 2, new Vector2(vec4.W, vec4.Z) / 16, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();

            graphics.SetRenderTarget(background);
            graphics.Clear(Color.Transparent);
            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default);
            foreach (Vector4 vec4 in bgPoints)
            {
                DrawBGRT(vec4);
            }
            Main.spriteBatch.End();
            Main.graphics.GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawBGRT(Vector4 vec4)
        {
            Vector2 pos = vec4.XY();
            Vector2 size = new Vector2(vec4.W, vec4.Z) * 16;

            Vector2 basePoint = pos - (size * 0.5f);

            Texture2D gradiant = ModContent.Request<Texture2D>("JadeFables/Assets/Backgrounds/BigBG5").Value;
            Main.spriteBatch.Draw(gradiant, pos - Main.screenPosition, null, Color.White, 0, gradiant.Size() / 2, size / gradiant.Size(), SpriteEffects.None, 0f);

            for (int i = 4; i >= 0; i--)
            {
                DrawLayer(basePoint, Request<Texture2D>("JadeFables/Assets/Backgrounds/BigBG" + i).Value, i + 1, size.X, new Vector2(0, 0), Color.White, false);
            }
        }

        private static void DrawLayer(Vector2 basepoint, Texture2D texture, float parallax, float width, Vector2 off = default, Color color = default, bool flip = false)
        {
            if (color == default)
            {
                color = Color.White;

                byte a = color.A;

                color *= 0.8f + (Main.dayTime ? (float)Math.Sin(Main.time / Main.dayLength * 3.14f) * 0.35f : -(float)Math.Sin(Main.time / Main.nightLength * 3.14f) * 0.35f);
                color.A = a;
            }

            for (int k = 0; k <= 5; k++)
            {
                float x = (basepoint.X + off.X - (int)Main.screenPosition.X) + GetParallaxOffsetX(basepoint.X + width *0.5f, 0.1f);
                float y = basepoint.Y + off.Y + k * 739 * 4 + GetParallaxOffset(basepoint.Y, parallax * 0.1f) - (int)Main.screenPosition.Y;

                if (x > -texture.Width && x < Main.screenWidth + 30)
                    Main.spriteBatch.Draw(texture, new Vector2(x, y), null, color, 0f, Vector2.Zero, 1f, flip ? SpriteEffects.FlipHorizontally : 0, 0);
            }
        }

        private void DrawTilingBackground(SpriteBatch spriteBatch)
        {
            Texture2D tex = Request<Texture2D>("JadeFables/Assets/Backgrounds/JadeBGsmall").Value;
           /* Texture2D texBot = Request<Texture2D>("JadeFables/Assets/Backgrounds/JadeBGsmallBottom").Value;
            Texture2D texTop = Request<Texture2D>("JadeFables/Assets/Backgrounds/JadeBGsmallTop").Value;
            Texture2D texLeft = Request<Texture2D>("JadeFables/Assets/Backgrounds/JadeBGsmallLeft").Value;
            Texture2D texRight = Request<Texture2D>("JadeFables/Assets/Backgrounds/JadeBGsmallRight").Value;*/

            for (int x = -tex.Width; x <= Main.screenWidth + tex.Width; x += tex.Width)
            {
                for (int y = -tex.Height; y <= Main.screenHeight + tex.Height; y += tex.Height)
                {
                    Vector2 pos = new Vector2(x, y) - new Vector2(Main.screenPosition.X % tex.Width, Main.screenPosition.Y % tex.Height);
                    LightingBufferRenderer.DrawWithLighting(pos, tex);
                }
            }
        }

        private bool CheckBackground(Vector2 pos, Vector2 size, bool dontCheckScreen = false)
        {
            if (dontCheckScreen || Helper.OnScreen(new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y)))
            {
                if (!Main.BackgroundEnabled)
                    return true;
                else
                {
                    foreach (Vector4 vec in GetBgPoints())
                        if (PointInCircle(pos + Main.screenPosition, vec) && PointInCircle(pos + size + Main.screenPosition, vec))
                            return false;
                }
                return true;
            }

            return false;
        }

        private static List<Vector4> GetBgPoints()
        {
            List<Vector4> ret = new List<Vector4>();
            foreach (Rectangle rect in JadeLakeWorldGen.WholeBiomeRects)
            {
                Vector4 toAdd = new Vector4(rect.Center.X * 16, rect.Center.Y * 16, rect.Height * 0.75f, rect.Width * 0.75f);
                ret.Add(toAdd);
            }
            return ret;
        }

        private static bool BackgroundOnScreen()
        {
            return Main.LocalPlayer.InModBiome<JadeLakeBiome>();

        }

        private static bool PointInCircle(Vector2 point, Vector4 circle)
        {
            Vector2 dir = point - circle.XY();
            float heightMult = circle.W / circle.Z;
            if ((dir.X * dir.X) + (dir.Y * dir.Y * heightMult) > ((circle.W * circle.W) + (circle.Z * circle.Z)) * 16)
                return false;
            return true;
        }

        private static int GetParallaxOffset2(float startpoint, float factor)
        {
            float vanillaParallax = 1 - (Main.caveParallax - 0.8f) / 0.2f;
            return (int)((Main.screenHeight / 2 - startpoint) * factor * vanillaParallax);
        }

        private static int GetParallaxOffset(float startpoint, float factor)
        {
            float vanillaParallax = 1 - (Main.caveParallax - 0.8f) / 0.2f;
            return (int)((Main.screenPosition.Y + Main.screenHeight / 2 - startpoint) * factor * vanillaParallax);
        }

        private static int GetParallaxOffsetX(float startpoint, float factor)
        {
            return (int)((Main.screenPosition.X + Main.screenWidth / 2 - startpoint) * factor);
        }
    }
}