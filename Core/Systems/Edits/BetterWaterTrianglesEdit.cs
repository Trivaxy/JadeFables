using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace JadeFables.Core.Systems.Edits;

public sealed class BetterWaterTrianglesEdit : RuntimeDetourModSystem
{
    public override void OnModLoad() {
        base.OnModLoad();

        IL.Terraria.Main.DrawBlack += InjectExtraDrawBlockConditions;
    }

    private void InjectExtraDrawBlockConditions(ILContext il) {
        var c = new ILCursor(il);

        #region Collect locals

        if (!c.TryGotoNext(x => x.MatchLdsflda<Main>(nameof(Main.tile)))) {
            AddOpCodeError("Terraria.Main", "DrawBlack", "ldsfld", "Terraria.Tilemap Terraria.Main::tile");
            return;
        }

        int jIndex = -1;
        int iIndex = -1;

        if (!c.TryGotoNext(x => x.MatchLdloc(out jIndex)) || jIndex == -1) {
            AddOpCodeError("Terraria.Main", "DrawBlack", "ldloc.s", iteration: 1);
            return;
        }

        if (!c.TryGotoNext(x => x.MatchLdloc(out iIndex)) || iIndex == -1) {
            AddOpCodeError("Terraria.Main", "DrawBlack", "ldloc.s", iteration: 1);
            return;
        }

        #endregion

        #region Inject conditions

        c.Index = c.Instrs.Count - 1;

        // Jump to middle of if condition we're injecting into.
        if (!c.TryGotoPrev(x => x.MatchLdsfld<Main>(nameof(Main.drawToScreen)))) {
            AddOpCodeError("Terraria.Main", "DrawBlack", "ldsfld", "bool Terraria.Main::drawToScreen");
            return;
        }

        // Get the jump label so we know where to jump to upon failure, as well as to jump to the end of the expression when anchored to.
        ILLabel? label = null;
        if (!c.TryGotoNext(MoveType.After, x => x.MatchBrtrue(out label)) || label is null) {
            AddOpCodeError("Terraria.Main", "DrawBlack", "brtrue.s");
            return;
        }

        c.Emit(OpCodes.Ldloc, jIndex);
        c.Emit(OpCodes.Ldloc, iIndex);
        c.EmitDelegate(InjectedCondition);
        c.Emit(OpCodes.Brfalse_S, label); // Jump is condition fails.

        #endregion
    }

    private static bool InjectedCondition(int x, int y) {
        return Framing.GetTileSafely(x, y - 1).LiquidAmount > 0
               || Framing.GetTileSafely(x, y + 1).LiquidAmount > 0
               || Framing.GetTileSafely(x + 1, y).LiquidAmount > 0
               || Framing.GetTileSafely(x - 1, y).LiquidAmount > 0
               || (Framing.GetTileSafely(x, y).IsHalfBlock && Framing.GetTileSafely(x - 1, y).LiquidAmount > 0 && Lighting.Brightness(x, y) > 0f);
    }
}