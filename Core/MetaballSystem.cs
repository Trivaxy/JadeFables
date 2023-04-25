using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Enums;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace JadeFables.Core
{
    public class MetaballSystem : ILoadable
    {
        public static int oldScreenWidth = 0;
        public static int oldScreenHeight = 0;

        public static List<MetaballActor> Actors = new List<MetaballActor>();

        public float Priority => 1;

        public void Load(Mod mod)
        {
            if (Main.dedServ)
                return;

            Terraria.On_Main.DrawNPCs += DrawTargets;
            Terraria.On_Main.CheckMonoliths += BuildTargets;
        }

        public void Unload()
        {
            Terraria.On_Main.DrawNPCs -= DrawTargets;
            Terraria.On_Main.CheckMonoliths -= BuildTargets;

            Actors = null;
        }

        public void UpdateWindowSize(int width, int height)
        {
            Main.QueueMainThreadAction(() =>
            {
                Actors.ForEach(n => n.ResizeTarget(width, height));
            });

            oldScreenWidth = width;
            oldScreenHeight = height;
        }

        private void DrawTargets(Terraria.On_Main.orig_DrawNPCs orig, Main self, bool behindTiles = false)
        {

            if (behindTiles && !Main.gameMenu)
                Actors.ForEach(a => a.DrawTarget(Main.spriteBatch));

            orig(self, behindTiles);
        }

        private void BuildTargets(Terraria.On_Main.orig_CheckMonoliths orig)
        {
            if (!Main.gameMenu)
            {
                if (Main.graphics.GraphicsDevice != null)
                {
                    if (Main.screenWidth != oldScreenWidth || Main.screenHeight != oldScreenHeight)
                        UpdateWindowSize(Main.screenWidth, Main.screenHeight);
                }

                if (Main.spriteBatch != null && Main.graphics.GraphicsDevice != null)
                    Actors.ForEach(a => a.DrawToTarget(Main.spriteBatch, Main.graphics.GraphicsDevice));
            }
            orig();
        }
    }
}
