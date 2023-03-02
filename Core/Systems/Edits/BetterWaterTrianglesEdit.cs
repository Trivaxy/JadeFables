using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;

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

        IL.Terraria.GameContent.Liquid.LiquidRenderer.InternalPrepareDraw += PrepSlopeTiles;
        IL.Terraria.GameContent.Liquid.LiquidRenderer.InternalDraw += DrawSlopeTiles;

    }

    private void PrepSlopeTiles(ILContext il) {
        // Comments and documentation coming soon (TM) - Mutant
        ILCursor c = new ILCursor(il);

        const byte thisTileCacheVar = 4;
        const byte num2LocalVar = 6;
        const byte aboveTileCacheVar = 11;
        const byte leftTileCacheVar = 13;
        const byte rightTileCacheVar = 14;

        if (!c.TryGotoNext(i => i.MatchCall(typeof(Tile), "get_liquid"))) {
            return;
        }

        c.Index += 5;

        FieldReference liquidLevelRef = (FieldReference)c.Next.Operand;

        if (!c.TryGotoNext(i => i.MatchCall(typeof(Tile), "get_type"))) {
            return;
        }

        c.Index -= 4;

        FieldReference hasLiquidRef = (FieldReference)c.Next.Operand;

        if (!c.TryGotoNext(i => i.MatchCall(typeof(Tile), "liquidType"))) {
            return;
        }

        c.Index++;

        FieldReference liquidTypeRef = (FieldReference)c.Next.Operand;

        if (!c.TryGotoPrev(i => i.MatchCall<WorldGen>(nameof(WorldGen.SolidOrSlopedTile)))) {
            return;
        }

        c.Index -= 3;
        FieldReference isHalfBlockRef = (FieldReference)c.Next.Operand;
        c.Index++;
        c.Emit(OpCodes.Ldloc_S, thisTileCacheVar);
        c.Emit(OpCodes.Ldloc_S, thisTileCacheVar);
        c.Emit(OpCodes.Ldfld, isHalfBlockRef);
        c.Emit(OpCodes.Ldloc_1);
        c.EmitDelegate<Func<bool, Tile, bool>>((isHalfBrick, tile) => isHalfBrick || tile.Slope is not SlopeType.Solid);
        c.Emit(OpCodes.Stfld, isHalfBlockRef);

        if (!c.TryGotoNext(i => i.MatchLdcR4(0.5f))) {
            return;
        }

        c.Index += 3;
        ILLabel endConditionLabel = (ILLabel)c.Next.Operand;
        
        if (!c.TryGotoPrev(i => i.OpCode == OpCodes.Stloc_S && i.Operand is VariableDefinition { Index: aboveTileCacheVar })) {
            return;
        }

        c.Index -= 7;
        List<Instruction> stolenInstructions = new List<Instruction>();

        while (c.Next.OpCode != OpCodes.Stloc_S || c.Next.Operand is not VariableDefinition { Index: rightTileCacheVar }) {
            stolenInstructions.Add(c.Next);
            c.Index++;
        }
        stolenInstructions.Add(c.Next);
        c.Index++;

        if (!c.TryGotoPrev(i => i.MatchLdcR4(0f))) {
            return;
        }

        c.Index += 2;
        foreach (Instruction instruction in stolenInstructions) {
            if (instruction.Operand is null) {
                c.Emit(instruction.OpCode);
            }
            else {
                c.Emit(instruction.OpCode, instruction.Operand);
            }
        }

        c.Emit(OpCodes.Ldloc_S, thisTileCacheVar);
        c.Emit(OpCodes.Ldfld, hasLiquidRef);

        c.Emit(OpCodes.Ldloc_S, aboveTileCacheVar);
        c.Emit(OpCodes.Ldfld, hasLiquidRef);

        c.Emit(OpCodes.Ldloc_S, leftTileCacheVar);
        c.Emit(OpCodes.Ldfld, hasLiquidRef);

        c.Emit(OpCodes.Ldloc_S, rightTileCacheVar);
        c.Emit(OpCodes.Ldfld, hasLiquidRef);

        c.Emit(OpCodes.Ldloc_S, thisTileCacheVar);
        c.Emit(OpCodes.Ldfld, liquidLevelRef);

        c.Emit(OpCodes.Ldloc_S, leftTileCacheVar);
        c.Emit(OpCodes.Ldfld, liquidLevelRef);

        c.Emit(OpCodes.Ldloc_S, rightTileCacheVar);
        c.Emit(OpCodes.Ldfld, liquidLevelRef);

        c.EmitDelegate<Func<bool, bool, bool, bool, float, float, float, float>>((thisTileHasLiquid, aboveTileHasLiquid, leftTileHasLiquid, rightTileHasLiquid, thisTileLevel, leftTileLevel, rightTileLevel) => {
            float finalLevel = 0f;
            
            if (!thisTileHasLiquid) {
                if (aboveTileHasLiquid) {
                    finalLevel = 1f;
                }
                else if (leftTileHasLiquid || rightTileHasLiquid) {
                    finalLevel = leftTileLevel > rightTileLevel ? leftTileLevel : rightTileLevel;
                }
            }
            else {
                finalLevel = thisTileLevel;
            }

            return finalLevel;
        });
        c.Emit(OpCodes.Stloc_S, num2LocalVar);

        c.Emit(OpCodes.Ldloc_S, thisTileCacheVar);

        c.Emit(OpCodes.Ldloc_S, thisTileCacheVar);
        c.Emit(OpCodes.Ldfld, hasLiquidRef);

        c.Emit(OpCodes.Ldloc_S, aboveTileCacheVar);
        c.Emit(OpCodes.Ldfld, hasLiquidRef);

        c.Emit(OpCodes.Ldloc_S, leftTileCacheVar);
        c.Emit(OpCodes.Ldfld, hasLiquidRef);

        c.Emit(OpCodes.Ldloc_S, rightTileCacheVar);
        c.Emit(OpCodes.Ldfld, hasLiquidRef);

        c.Emit(OpCodes.Ldloc_S, thisTileCacheVar);
        c.Emit(OpCodes.Ldfld, liquidTypeRef);

        c.Emit(OpCodes.Ldloc_S, aboveTileCacheVar);
        c.Emit(OpCodes.Ldfld, liquidTypeRef);

        c.Emit(OpCodes.Ldloc_S, leftTileCacheVar);
        c.Emit(OpCodes.Ldfld, liquidTypeRef);

        c.Emit(OpCodes.Ldloc_S, rightTileCacheVar);
        c.Emit(OpCodes.Ldfld, liquidTypeRef);

        c.Emit(OpCodes.Ldloc_S, leftTileCacheVar);
        c.Emit(OpCodes.Ldfld, liquidLevelRef);

        c.Emit(OpCodes.Ldloc_S, rightTileCacheVar);
        c.Emit(OpCodes.Ldfld, liquidLevelRef);

        c.EmitDelegate<Func<bool, bool, bool, bool, byte, byte, byte, byte, float, float, byte>>((thisTileHasLiquid, aboveTileHasLiquid, leftTileHasLiquid, rightTileHasLiquid, thisTileType, aboveTileType, leftTileType, rightTileType, leftTileLevel, rightTileLevel) => {
            if (thisTileHasLiquid) {
                return thisTileType;
            }

            if (aboveTileHasLiquid) {
                return aboveTileType;
            }
            if (leftTileHasLiquid || rightTileHasLiquid) {
                 return leftTileLevel > rightTileLevel ? leftTileType : rightTileType;
            }

            return thisTileType;
        });
        c.Emit(OpCodes.Stfld, liquidTypeRef);

        c.Emit(OpCodes.Br_S, endConditionLabel);
    }

    private void DrawSlopeTiles(ILContext il) {
        // Comments and documentation coming soon (TM) - Mutant
        ILCursor c = new ILCursor(il);

        const byte drawOffsetArg = 2;

        const byte iVar = 3;
        const byte jVar = 4;
        const byte liquidOffsetVar = 6;
        const byte liquidTypeVar = 8;
        const byte liquidColorVar = 9;

        if (!c.TryGotoNext(MoveType.After, i => i.MatchCall<Main>(nameof(Main.DrawTileInWater)))) {
            return;
        }

        ILLabel nonSlopeLabel = c.MarkLabel();
        int oldIndex = c.Index;

        if (!c.TryGotoNext(MoveType.After, i => i.MatchCallvirt<TileBatch>(nameof(TileBatch.Draw)))) {
            return;
        }

        ILLabel endLabel = c.MarkLabel();

        c.Index = oldIndex;

        c.Emit(OpCodes.Ldloc_S, iVar);
        c.Emit(OpCodes.Ldloc_S, jVar);
        c.EmitDelegate<Func<int, int, bool>>((i, j) => Framing.GetTileSafely(i, j).Slope == SlopeType.Solid);
        c.Emit(OpCodes.Brtrue_S, nonSlopeLabel);

        //num2 (liquidType), i, j, drawOffset, liquidOffset, vertices (liquidColorVar)
        c.Emit(OpCodes.Ldloc_S, liquidTypeVar);
        c.Emit(OpCodes.Ldloc_S, iVar);
        c.Emit(OpCodes.Ldloc_S, jVar);
        c.Emit(OpCodes.Ldarg_S, drawOffsetArg);
        c.Emit(OpCodes.Ldloc_S, liquidOffsetVar);
        c.Emit(OpCodes.Ldloc_S, liquidColorVar);
        c.EmitDelegate<Action<int, int, int, Vector2, Vector2, VertexColors>>((liquidType, i, j, drawOffset, liquidOffset, vertices) => {
            Rectangle liquidSize = new Rectangle(18 * (int)(Framing.GetTileSafely(i, j).Slope - 1), 4, 16, 16);
            Main.tileBatch.Draw(TextureAssets.LiquidSlope[liquidType < 12 ? liquidType : 0].Value, new Vector2(i << 4, j << 4) + drawOffset + liquidOffset, liquidSize, vertices, Vector2.Zero, 1f, SpriteEffects.None);
        });
        c.Emit(OpCodes.Br_S, endLabel);
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