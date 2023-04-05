using JadeFables.Biomes.JadeLake;
using JadeFables.Tiles;
using MonoMod.Cil;

namespace JadeFables.Core.Systems.Edits {
    /// <summary>
    /// Detours that disallow certain structures from spawning in the biome
    /// </summary>
    public class WorldgenEdit : ILoadable {
        public void Load(Mod mod) {
            Terraria.GameContent.Generation.On_TrackGenerator.IsLocationInvalid += TrackGenerator_IsLocationInvalid;
            Terraria.GameContent.Biomes.On_MiningExplosivesBiome.Place += MiningExplosivesBiome_Place;
        }

        private bool MiningExplosivesBiome_Place(Terraria.GameContent.Biomes.On_MiningExplosivesBiome.orig_Place orig, Terraria.GameContent.Biomes.MiningExplosivesBiome self, Point origin, Terraria.WorldBuilding.StructureMap structures)
        {
            if (ValidPlacement_Explosives(origin.X,origin.Y))
                return orig(self, origin, structures);
            return false;
        }

        private bool TrackGenerator_IsLocationInvalid(Terraria.GameContent.Generation.On_TrackGenerator.orig_IsLocationInvalid orig, int x, int y)
        {
            if (ValidPlacement_Minecarts(x, y))
                return orig(x, y);
            return true;
        }

        public void Unload() 
        {
            Terraria.GameContent.Generation.On_TrackGenerator.IsLocationInvalid -= TrackGenerator_IsLocationInvalid;
            Terraria.GameContent.Biomes.On_MiningExplosivesBiome.Place -= MiningExplosivesBiome_Place;
        }

        public bool ValidPlacement_Minecarts(int x, int y)
        {
            foreach (Rectangle rect in JadeLakeWorldGen.LowerIslandRects)
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

        public bool ValidPlacement_Explosives(int x, int y)
        {

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
