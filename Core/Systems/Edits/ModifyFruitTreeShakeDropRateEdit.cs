using JadeFables.Core.Systems.TileHits;
using JadeFables.Items.Jade.JadeAxe;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.Utilities;

namespace JadeFables.Core.Systems.Edits;

public sealed class ModifyFruitTreeShakeDropRateEdit : RuntimeDetourModSystem
{
    public override void OnModLoad()
    {
        base.OnModLoad();

        Terraria.IL_WorldGen.ShakeTree += il =>
        {
            int[] fruits =
            {
                // apple, apricot, grapefruit, lemon, and peach
                ItemID.Apple,

                // cherry, plum
                ItemID.Cherry,

                // blackcurrant, elderberry
                ItemID.BlackCurrant,

                // blood orange, rambutan
                ItemID.BloodOrange,

                // mango, pineapple
                ItemID.Mango,

                // banana, coconut
                ItemID.Banana,

                // dragonfruit, star fruit
                ItemID.Dragonfruit
            };

            var nextInt32 = typeof(UnifiedRandom).GetMethod("Next", new[] { typeof(int) });
            if (nextInt32 is null)
            {
                AddMissingMemberError(typeof(UnifiedRandom).FullName + "::Next(int32)");
                return;
            }

            var c = new ILCursor(il);
            int urIterations = 0;

            foreach (int fruit in fruits)
            {
                c.Index = 0;

                if (!c.TryGotoNext(x => x.MatchLdcI4(fruit)))
                {
                    AddOpCodeError("Terraria.WorldGen", "ShakeTree", "ldc.i4", fruit);
                    return;
                }

                // Twice since fruits always have an extra rand call.
                for (int i = 0; i < 2; i++)
                    if (!c.TryGotoPrev(x => x.MatchCallvirt(nextInt32)))
                    {
                        AddOpCodeError("Terraria.WorldGen", "ShakeTree", "callvirt", nextInt32!.DeclaringType + "::" + nextInt32.Name, ++urIterations);
                        return;
                    }

                // Objective: multiply rate (already pushed to stack) by given multiplier

                // c.Emit(OpCodes.Conv_R4); // convert rate to float
                // c.Emit(OpCodes.Ldc_R4, 0.7f); // push multiplier
                // c.Emit(OpCodes.Mul); // multiply
                // c.Emit(OpCodes.Conv_I4); // convert back to int

                c.Emit(OpCodes.Ldarg_0); // int x
                c.Emit(OpCodes.Ldarg_1); // int y
                c.EmitDelegate((int rate, int x, int y) =>
                {
                    if (!ModContent.GetInstance<TileHitInfoSystem>().TryGetHitTileContext(new Point(x, y), out var hitContext)) return rate;

                    // TODO: Use an ID set?
                    return hitContext.Player.HeldItem.type == ModContent.ItemType<JadeAxe>() ? (int)(rate * 0.7f) : rate;
                });
            }
        };
    }

    #region Scrapped approach

    //    private static readonly int[] VanillaFruits =
    //    {
    //        // Forest
    //        ItemID.Apple,
    //        ItemID.Apricot,
    //        ItemID.Grapefruit,
    //        ItemID.Lemon,
    //        ItemID.Peach,
    //
    //        // Boreal
    //        ItemID.Cherry,
    //        ItemID.Plum,
    //
    //        // Ebonwood
    //        ItemID.BlackCurrant,
    //        ItemID.Elderberry,
    //
    //        // Shadewood
    //        ItemID.BloodOrange,
    //        ItemID.Rambutan,
    //
    //        // Mahogany
    //        ItemID.Mango,
    //        ItemID.Pineapple,
    //
    //        // Palm
    //        ItemID.Banana,
    //        ItemID.Coconut,
    //
    //        // Pearlwood
    //        ItemID.Dragonfruit,
    //        ItemID.Starfruit,
    //
    //        // Ash (1.4.4)
    //        // ItemID.Pomegranate,
    //        // ItemID.SpicyPepper
    //    };
    //
    //    public delegate void TreeShakeRateModifier(int rate, ILCursor c);
    //
    //    public TreeShakeRateModifier MultiplicativeModifier(float multiplier) {
    //        return (_, c) =>
    //        {
    //            // Objective: multiply rate (already pushed to stack) by given multiplier
    //
    //            c.Emit(OpCodes.Conv_R4); // convert rate to float
    //            c.Emit(OpCodes.Ldc_R4, multiplier); // push multiplier
    //            c.Emit(OpCodes.Mul); // multiply
    //            c.Emit(OpCodes.Conv_I4); // convert back to int
    //        };
    //    }
    //
    //    public Dictionary<int, TreeShakeRateModifier> RateModifiers { get; } = new();
    //
    //    public override void OnModLoad() {
    //        base.OnModLoad();
    //
    //        foreach (int fruit in VanillaFruits) RateModifiers.Add(fruit, MultiplicativeModifier(1.3f));
    //
    //        IL.Terraria.WorldGen.ShakeTree += RewriteShakeDropRates;
    //    }
    //
    //    public override void OnModUnload() {
    //        base.OnModUnload();
    //
    //        RateModifiers.Clear();
    //    }
    //
    //    private void RewriteShakeDropRates(ILContext il) {
    //        /* Objective: Arbitrarily modify the drop rates of tree shake results.
    //         *  All instances of the pattern: -----------------------------------> ldc.i4(.s) x
    //         *  represent a drop rate. After finding the drop rate,                UnifiedRandom::Next(int32)
    //         *  we can search for item IDs based on Item::NewItem calls            brtrue.s x
    //         *  (item ID is last value on stack before method invocation).
    //         */
    //        ILCursor c = new(il);
    //
    //        var nextInt32 = typeof(UnifiedRandom).GetMethod("Next", new[] {typeof(int)});
    //        if (nextInt32 is null) {
    //            AddMissingMemberError(typeof(UnifiedRandom).FullName + "::Next(int32)");
    //            return;
    //        }
    //
    //        // Go to each UnifiedRandom::Next(int32) call.
    //        while (c.TryGotoNext(x => x.MatchCallvirt(nextInt32))) {
    //            // If opcode immediately before isn't an ldc.i4(.s) instruction, skip.
    //            if (!c.Prev.MatchLdcI4(out int rate)) continue;
    //
    //            // If opcode immediately after isn't brtrue.s, skip.
    //            if (!c.Next.MatchBrtrue(out var label)) continue;
    //        }
    //    }

    #endregion
}