using JadeFables.Tiles.Banners;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent.Drawing;

namespace JadeFables.Core.Systems.Edits
{
    /// <summary>
    /// Edit that allows for our NPC banners to properly sway like vanilla banners.
    /// </summary>
    /// <remarks>
    /// Ideally, tML would have some method to allow copying of this functionality,
    /// but it does not (as of yet), so IL it is!
    /// </remarks>
    public class BannerWindSwayEdit : ILoadable
    {
        public delegate void RefSizeModification(int x, int y, ref int sizeX, ref int sizeY);

        public void Load(Mod mod)
        {
            IL_TileDrawing.DrawMultiTileVines += CheckForModdedBanners;
        }

        public void Unload() { }

        private void CheckForModdedBanners(ILContext il)
        {
            ILCursor c = new(il);

            //Navigate to DrawMultiTileVinesInWind call
            if (!c.TryGotoNext(i => i.MatchCall<Terraria.GameContent.Drawing.TileDrawing>("DrawMultiTileVinesInWind")))
            {
                JadeFables.Instance.Logger.Warn("BannerWindSway Edit failed; Mod banners may act weird in the wind.");
                return;
            }

            byte xLocalVar = 5;
            byte yLocalVar = 6;
            byte sizeYLocalVar = 8;

            c.Index -= 6;
            //Keep the original instruction so we don't have to mess with labels
            c.Emit(OpCodes.Pop);
            //Load x, y, and sizeY
            c.Emit(OpCodes.Ldloc_S, xLocalVar);
            c.Emit(OpCodes.Ldloc_S, yLocalVar);
            c.Emit(OpCodes.Ldloc_S, sizeYLocalVar);
            //Luckily, since sizeX is already 1 by default, we only need to change sizeY to 3 (instead of default 1)
            c.EmitDelegate<Func<int, int, int, int>>((x, y, sizeY) =>
            {
                if (TileLoader.GetTile(Main.tile[x, y].TileType) is DefaultNPCBanner)
                {
                    sizeY = 3;
                }

                return sizeY;
            });
            c.Emit(OpCodes.Stloc_S, sizeYLocalVar);
            //Add the ldarg.0 that we popped at the beginning (so the next function call works)
            c.Emit(OpCodes.Ldarg_0);
        }
    }
}