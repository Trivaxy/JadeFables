using JadeFables.Tiles;
using MonoMod.Cil;

namespace JadeFables.Core.Systems.Edits {
    /// <summary>
    /// IL Edit that allows for other tiles to grow bamboo other than just Jungle Grass.
    /// </summary>
    public class BambooEdit : ILoadable {
        public void Load(Mod mod) {
            Terraria.IL_WorldGen.CheckBamboo += WorldGen_CheckBamboo;
            Terraria.IL_WorldGen.PlaceBamboo += WorldGen_PlaceBamboo;
            Terraria.IL_WorldGen.UpdateWorld_OvergroundTile += WorldGen_UpdateWorld_OvergroundTile;
        }

        public void Unload() { }

        private void WorldGen_CheckBamboo(ILContext il) {
            ILCursor c = new ILCursor(il);

            DoSwap(c);
        }

        private void WorldGen_PlaceBamboo(ILContext il) {
            ILCursor c = new ILCursor(il);

            DoSwap(c);
        }

        private void WorldGen_UpdateWorld_OvergroundTile(ILContext il) {
            // This method has two checks, but the second one is negated.
            ILCursor c = new ILCursor(il);

            DoSwap(c);

            if (!c.TryGotoNext(i => i.MatchCall<Tile>("get_type")) || !c.TryGotoNext(i => i.MatchLdcI4(TileID.JungleGrass))) {
                return;
            }

            c.EmitDelegate<Func<int, int>>(tileType => TileSets.CanGrowBamboo[tileType] ? tileType : TileID.JungleGrass);
        }

        private void DoSwap(ILCursor c) {
            // Swaps out the hardcoded Tile.type == 60 to TileSets.CanGrowBamboo[Tile.type]
            if (!c.TryGotoNext(i => i.MatchCall<Tile>("get_type")) || !c.TryGotoNext(i => i.MatchLdcI4(TileID.JungleGrass))) {
                return;
            }

            c.EmitDelegate<Func<int, int>>(SwapDelegate);
        }

        private int SwapDelegate(int tileType) => TileSets.CanGrowBamboo[tileType] ? TileID.JungleGrass : tileType;
    }
}
