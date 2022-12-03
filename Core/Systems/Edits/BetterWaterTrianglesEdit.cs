using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;

namespace JadeFables.Core.Systems.Edits;

public sealed class BetterWaterTrianglesEdit : RuntimeDetourModSystem
{
    public override void OnModLoad() {
        base.OnModLoad();

        IL.Terraria.Main.DrawBlack += InjectExtraDrawBlockConditions;
        IL.Terraria.GameContent.Liquid.LiquidRenderer.InternalDraw += DrawWouldBePartials;
        On.Terraria.GameContent.Drawing.TileDrawing.DrawPartialLiquid += SkipPartialLiquidDrawing;
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
    
    private void DrawWouldBePartials(ILContext il) {
        var c = new ILCursor(il);
        
        if (!c.TryGotoNext(MoveType.Before, x => x.MatchCallvirt<TileBatch>("Draw"))) {
            AddOpCodeError("Terraria.GameContent.Liquid.LiquidRenderer", "InternalDraw", "callvirt", "void Terraria.Graphics.TileBatch::Draw(Texture2D,Vector2,Rectangle?,VertexColors,Vector2,float,SpriteEffects)");
            return;
        }

        // Remove the call to Draw.
        // We should typically avoid this, but oh well - tough luck.
        c.Remove();

        c.Emit(OpCodes.Ldarg_2); // push draw offset
        
        // Use our injected Draw method instead.
        // We could do this without removing the original call, but it should probably be fine?
        c.EmitDelegate(InjectedDraw);
    }

    private static void InjectedDraw(
        LiquidRenderer self,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        VertexColors colors,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        Vector2 drawOffset
    ) {
        var tileCoords = (position - drawOffset).ToTileCoordinates();
        // var tile = Framing.GetTileSafely(tileCoords);

        void DrawLiquidAtRelativeCoordinate(int x, int y) {
            var relativeTile = Framing.GetTileSafely(tileCoords.X + x, tileCoords.Y + y);
            var relativeOffset = new Vector2(x * 16f, y * 16f);
            Lighting.GetCornerColors(tileCoords.X + x, tileCoords.Y + y, out var relativeColors);
            
            // Don't draw if it's blocked.
            if (TileID.Sets.BlocksWaterDrawingBehindSelf[relativeTile.TileType]) return;

            switch (relativeTile.Slope) {
                case SlopeType.Solid:
                    // Do nothing.
                    //break;
                
                case SlopeType.SlopeDownLeft:
                case SlopeType.SlopeDownRight:
                case SlopeType.SlopeUpLeft:
                case SlopeType.SlopeUpRight:
                    Main.tileBatch.Draw(texture, position + relativeOffset, sourceRectangle /* TODO */, relativeColors, origin, scale, effects);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), "Relative tile coordinate from position had invalid slope type.");
            }
        }
        
        DrawLiquidAtRelativeCoordinate(1, 0);
        DrawLiquidAtRelativeCoordinate(0, 1);
        DrawLiquidAtRelativeCoordinate(-1, 0);
        DrawLiquidAtRelativeCoordinate(0, -1);


        // Render original liquid.
        Main.tileBatch.Draw(texture, position, sourceRectangle, colors, origin, scale, effects);
    }
    
    private static void SkipPartialLiquidDrawing(On.Terraria.GameContent.Drawing.TileDrawing.orig_DrawPartialLiquid orig, Terraria.GameContent.Drawing.TileDrawing self, Tile tileCache, Vector2 position, Rectangle liquidSize, int liquidType, Color aColor) {
        // orig(self, tileCache, position, liquidSize, liquidType, aColor);
    }
}