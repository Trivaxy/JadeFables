using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;

namespace JadeFables.Core.Systems.Edits;

public sealed class BetterWaterTrianglesEdit : RuntimeDetourModSystem
{
    private FieldInfo? animationFrameField;

    public override void OnModLoad() {
        base.OnModLoad();

        IL.Terraria.Main.DrawBlack += InjectExtraDrawBlockConditions;

        animationFrameField = typeof(LiquidRenderer).GetField("_animationFrame", BindingFlags.Instance | BindingFlags.NonPublic);
        if (animationFrameField is null) {
            AddMissingMemberError(typeof(LiquidRenderer).FullName + "::_animationFrame");
            return;
        }

        On.Terraria.GameContent.Drawing.TileDrawing.DrawPartialLiquid += RenderRealLiquidInPlaceOfPartial;
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

    private void RenderRealLiquidInPlaceOfPartial(
        On.Terraria.GameContent.Drawing.TileDrawing.orig_DrawPartialLiquid orig,
        Terraria.GameContent.Drawing.TileDrawing self,
        Tile tileCache,
        Vector2 position,
        Rectangle liquidSize,
        int liquidType,
        Color aColor
    ) {
        if (TileID.Sets.BlocksWaterDrawingBehindSelf[(int) tileCache.BlockType] || tileCache.Slope == SlopeType.Solid) {
            orig(self, tileCache, position, liquidSize, liquidType, aColor);
            return;
        }

        var drawPos = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        var drawOffset = drawPos - Main.screenPosition;
        var tileCoords = (position + Main.screenPosition - drawPos).ToTileCoordinates();

        Main.tileBatch.Begin();

        var tile = Framing.GetTileSafely(tileCoords);
        int liquidStyle = tile.LiquidType;
        float opacity = tile.LiquidType switch
        {
            LiquidID.Water => 0.6f,
            LiquidID.Lava => 0.95f,
            LiquidID.Honey => 0.95f,
            _ => 1f
        };

        switch (tile.LiquidType) {
            case LiquidID.Water:
                liquidStyle = Main.waterStyle;
                opacity *= 0.75f;
                break;

            case LiquidID.Honey:
                liquidStyle = WaterStyleID.Honey;
                break;
        }

        opacity = Math.Min(opacity, 1f);
        if (tile.WallType != 0) opacity = 0.5f;

        Lighting.GetCornerColors(tileCoords.X, tileCoords.Y, out var vertices);
        vertices.BottomLeftColor *= opacity;
        vertices.BottomRightColor *= opacity;
        vertices.TopLeftColor *= opacity;
        vertices.TopRightColor *= opacity;

        int ySlice = tile.IsHalfBlock ? 8 : 0;
        int frameYOffset = tile.LiquidAmount == 0 && !tile.HasTile ? 0 : 48;
        var srcRect = new Rectangle(
            16,
            frameYOffset + (int) (animationFrameField!.GetValue(LiquidRenderer.Instance) ?? 0) * 80,
            16,
            16 - ySlice
        );

        Main.tileBatch.Draw(
            TextureAssets.Liquid[liquidStyle].Value,
            position + new Vector2(0f, ySlice),
            srcRect,
            vertices,
            Vector2.Zero,
            1f,
            SpriteEffects.None
        );

        Main.tileBatch.End();
    }
}