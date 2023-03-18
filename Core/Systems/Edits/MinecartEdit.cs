using JadeFables.Biomes.JadeLake;
using JadeFables.Tiles;
using MonoMod.Cil;

namespace JadeFables.Core.Systems.Edits {
    /// <summary>
    /// IL Edit that allows for other tiles to grow bamboo other than just Jungle Grass.
    /// </summary>
    public class MinecartEdit : ILoadable {
        public void Load(Mod mod) {
            On.Terraria.GameContent.Generation.TrackGenerator.IsLocationInvalid += TrackGenerator_IsLocationInvalid;
        }

        private bool TrackGenerator_IsLocationInvalid(On.Terraria.GameContent.Generation.TrackGenerator.orig_IsLocationInvalid orig, int x, int y)
        {
            if (ValidPlacement(x, y))
                return orig(x, y);
            return false;
        }

        public void Unload() 
        {
            On.Terraria.GameContent.Generation.TrackGenerator.IsLocationInvalid -= TrackGenerator_IsLocationInvalid;
        }

        public bool ValidPlacement(int x, int y)
        {
            foreach(Rectangle rect in JadeLakeWorldGen.LowerIslandRects)
            {
                if (ContainsCoordinates(rect, x, y))
                    return false;
            }

            foreach (Rectangle rect in JadeLakeWorldGen.PagodaRects)
            {
                if (ContainsCoordinates(rect, x, y))
                    return false;
            }
            return true;
        }

        public bool ContainsCoordinates(Rectangle rect, int x, int y)
        {
            if (x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom)
                return true;
            return false;
        }
    }
}
