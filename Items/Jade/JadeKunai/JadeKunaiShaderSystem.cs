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

namespace JadeFables.Items.Jade.JadeKunai
{
    class JadeKunaiShaderSystem : ILoadable
    {
        public static RenderTarget2D jadeBowRT;

        public float opacity;

        public static Vector2 oldScreenPos;

        public void Load(Mod mod)
        {
            if (Main.dedServ)
                return;

            On.Terraria.Main.CheckMonoliths += JadeBowTarget;
            On.Terraria.Main.DrawNPCs += DrawTarget;
        }

        private void DrawTarget(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            orig(self, behindTiles);

            var validNPCs = Main.npc.Where(n => n.active && n.GetGlobalNPC<JadeKunaiStackNPC>().flashOpacity > 0);
            if (Main.gameMenu || validNPCs.Count() == 0)
                return;

            NPC opacityNPC = validNPCs.FirstOrDefault();
            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            if (Main.dedServ || spriteBatch == null || jadeBowRT == null || gD == null)
                return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color color = new Color(0, 255, 100);
            Effect effect = Filters.Scene["JadeKunaiFlash"].GetShader().Shader;
            effect.Parameters["uImageSize0"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight) / 1.5f);
            effect.Parameters["alpha"].SetValue(opacityNPC.GetGlobalNPC<JadeKunaiStackNPC>().flashOpacity);
            effect.Parameters["drawColor"].SetValue(color.ToVector4());

            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(jadeBowRT, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }

        public void Unload()
        {
            jadeBowRT = null;
        }

        private void JadeBowTarget(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            orig();

            var validNPCs = Main.npc.Where(n => n.active && n.GetGlobalNPC<JadeKunaiStackNPC>().flashOpacity > 0);
            if (Main.gameMenu || validNPCs.Count() == 0)
                return;



            var graphics = Main.graphics.GraphicsDevice;

            int RTwidth = Main.screenWidth;
            int RTheight = Main.screenHeight;
            if (jadeBowRT is null || jadeBowRT.Size() != new Vector2(RTwidth, RTheight))
                jadeBowRT = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);


            graphics.SetRenderTarget(jadeBowRT);

            graphics.Clear(Color.Transparent);
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default);

            DrawTargets(validNPCs.ToList());

            Main.spriteBatch.End();
            graphics.SetRenderTarget(null);
        }

        private void DrawTargets(List<NPC> validNPCs)
        {
            foreach (NPC NPC in validNPCs)
            {
                if (NPC.ModNPC != null)
                {
                    if (NPC.ModNPC != null && NPC.ModNPC is ModNPC ModNPC)
                    {
                        if (ModNPC.PreDraw(Main.spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White)))
                            Main.instance.DrawNPC(NPC.whoAmI, false);

                        ModNPC.PostDraw(Main.spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White));
                    }
                }
                else
                    Main.instance.DrawNPC(NPC.whoAmI, false);
            }
        }
    }
}
