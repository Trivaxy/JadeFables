using JadeFables.Tiles;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace JadeFables.Core.Systems.Edits
{

    /// <summary>
    /// Edit that allows tiles that are marked as true in <seealso cref="TileSets.TorchThatTriggersTorchGod"/>
    /// to actually trigger the Torch God event.
    /// </summary>
    internal class TorchGodEdit : ILoadable
    {
        public void Load(Mod mod)
        {
            Terraria.IL_Player.TryRecalculatingTorchLuck += Player_TryRecalculatingTorchLuck;
            Terraria.IL_Player.RelightTorches += Player_RelightTorches;

            Terraria.IL_Player.TorchAttack += Player_TorchAttack;
        }

        public void Unload() { }

        private void Player_TryRecalculatingTorchLuck(ILContext il)
        {
            // For this method specifically, we first need to remove an "else" branch instruction THEN do our swap, so that our new
            // check is properly called.
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(i => i.MatchCall<Player>("get_NearbyModTorch")))
            {
                return;
            }

            // Move to else branch instruction
            c.Index += 6;
            // Remove it
            c.Remove();

            DoSwap(c);
        }

        private void Player_RelightTorches(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            DoSwap(c);
        }

        private void Player_TorchAttack(ILContext il)
        {
            // There's 2 checks we need to swap here
            ILCursor c = new ILCursor(il);

            for (int i = 0; i < 2; i++)
            {
                DoSwap(c);
            }
        }

        private void DoSwap(ILCursor c)
        {
            // Swaps out the hardcoded Tile.type == 4 to TileSets.TorchThatTriggersTorchGod[Tile.type]
            if (!c.TryGotoNext(i => i.MatchCall<Tile>("get_type")) || !c.TryGotoNext(i => i.MatchLdcI4(4)))
            {
                return;
            }

            c.EmitDelegate<Func<int, int>>(SwapDelegate);
        }

        private int SwapDelegate(int tileType) => TileSets.TorchThatTriggersTorchGod[tileType] ? TileID.Torches : tileType;
    }
}
