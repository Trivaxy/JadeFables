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
using rail;
using Terraria.GameContent;
using JadeFables.Dusts;
using static tModPorter.ProgressUpdate;

namespace JadeFables.Biomes.JadeLake
{
	class JadeLakeAddon : WaterAddon
	{
		public override bool Visible => Main.LocalPlayer.InModBiome<JadeLakeBiome>();

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
		}

		public override void SpritebatchChangeBack()
		{
            JadeLakeMapTarget.oldScreenPos = Main.screenPosition;
            var effect = Filters.Scene["JadeLakeWater"].GetShader().Shader;

			//the multiply by 1.3 and 1.5 seem to fix the jittering when moving, seems to be tied to the 2 magic numbers in Visuals.HotspringMapTarget.cs
			//effect.Parameters["offset"].SetValue(Main.screenPosition - HotspringMapTarget.oldScreenPos);

			Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);
		}
	}

    class JadeLakeMapTarget : ILoadable
    {
        public static RenderTarget2D jadelakeMapTarget;
        public static RenderTarget2D jadelakeShineTarget;

        public static Vector2 oldScreenPos;

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

            if (Main.gameMenu || !Main.LocalPlayer.InModBiome<JadeLakeBiome>())
                return;


            Vector2 RTratio = Main.ScreenSize.ToVector2() / Main.waterTarget.Size();
            var effect = Terraria.Graphics.Effects.Filters.Scene["JadeLakeWater"].GetShader().Shader;
            effect.Parameters["offset"].SetValue(((Main.screenPosition - oldScreenPos) * -1));
            effect.Parameters["sampleTexture2"].SetValue(JadeLakeMapTarget.jadelakeMapTarget);
            effect.Parameters["sampleTexture3"].SetValue(JadeLakeMapTarget.jadelakeShineTarget);
            effect.Parameters["time"].SetValue(Main.GameUpdateCount / 20f);

            var graphics = Main.graphics.GraphicsDevice;

            int RTwidth = Main.waterTarget.Width;
            int RTheight = Main.waterTarget.Height;
            if (jadelakeMapTarget is null || jadelakeMapTarget.Size() != new Vector2(RTwidth, RTheight))
                jadelakeMapTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);

            if (jadelakeShineTarget is null || jadelakeShineTarget.Size() != new Vector2(RTwidth, RTheight))
                jadelakeShineTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);

            graphics.SetRenderTarget(jadelakeMapTarget);

            graphics.Clear(Color.Transparent);
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default);

            DrawGradients();

            Main.spriteBatch.End();
            graphics.SetRenderTarget(null);


            //if (Main.renderCount == 3)//
            //{
            Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default);

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

            Main.graphics.GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawGradients()
        {
            Texture2D tex = ModContent.Request<Texture2D>("JadeFables/Assets/WaterGradient").Value;

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                if (new Vector2(x * 16f, Main.LocalPlayer.Center.Y).Distance(Main.LocalPlayer.Center) < Main.screenWidth / 2)
                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.LiquidAmount > 0)
                        {
                            if (tile.LiquidAmount > 0 && Main.tile[x, y - 1].LiquidAmount <= 0 && !Main.tile[x, y - 1].HasTile)
                            {
                                Vector2 pos = (new Vector2(x, y) * 16);
                                Main.spriteBatch.Draw(tex, pos - Main.sceneWaterPos, null, Color.White, 0, Vector2.Zero, new Vector2(1, 0.5f), SpriteEffects.None, 0f);
                            }
                        }
                    }
            }

        }
    }
}
