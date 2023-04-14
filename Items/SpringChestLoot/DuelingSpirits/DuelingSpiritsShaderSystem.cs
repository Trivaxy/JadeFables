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
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace JadeFables.Items.SpringChestLoot.DuelingSpirits
{
    class DuelingSpiritsShaderSystem : ILoadable
    {
        public static RenderTarget2D yinTarget;
        public static RenderTarget2D yangTarget;


        public static Vector2 oldScreenPos;

        public void Load(Mod mod)
        {
            if (Main.dedServ)
                return;

            Terraria.On_Main.CheckMonoliths += DrawToTargets;
            Terraria.On_Main.DrawProjectiles += DrawTargets;
        }

        private void DrawTargets(On_Main.orig_DrawProjectiles orig, Main self)
        {
            var validProjs = Main.projectile.Where(n => n.active && n.ModProjectile is Ying);
            if (Main.gameMenu || validProjs.Count() == 0)
            {
                orig(self);
                return;
            }

            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            if (Main.dedServ || spriteBatch == null || yinTarget == null || yangTarget == null || gD == null)
            {
                orig(self);
                return;
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect effect = Filters.Scene["YingOutline"].GetShader().Shader;
            effect.Parameters["uImageSize0"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["alpha"].SetValue(1);
            effect.Parameters["outlineColor"].SetValue(Color.Black.ToVector4());

            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(yinTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            effect.Parameters["outlineColor"].SetValue(Color.White.ToVector4());

            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(yangTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            spriteBatch.End();
            orig(self);
        }

        public void Unload()
        {
            yinTarget = null;
            yangTarget = null;
        }

        private void DrawToTargets(Terraria.On_Main.orig_CheckMonoliths orig)
        {
            orig();

            var validProjs = Main.projectile.Where(n => n.active && n.ModProjectile is Ying);
            if (Main.gameMenu || validProjs.Count() == 0)
                return;

            var graphics = Main.graphics.GraphicsDevice;

            int RTwidth = Main.screenWidth;
            int RTheight = Main.screenHeight;
            if (yinTarget is null || yinTarget.Size() != new Vector2(RTwidth, RTheight))
                yinTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);

            if (yangTarget is null || yangTarget.Size() != new Vector2(RTwidth, RTheight))
                yangTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);


            graphics.SetRenderTarget(yinTarget);
            graphics.Clear(Color.Transparent);

            DrawTargets(validProjs.ToList(), false);

            graphics.SetRenderTarget(yangTarget);
            graphics.Clear(Color.Transparent);

            DrawTargets(validProjs.ToList(), true);

            graphics.SetRenderTarget(null);
        }

        private void DrawTargets(List<Projectile> validProjs, bool yang)
        {
            foreach (Projectile proj in validProjs)
            {
                if (yang == (proj.type == ModContent.ProjectileType<Yang>()))
                {
                    (proj.ModProjectile as Ying).DrawPrimitives();
                }
            }
        }
    }
}
