using MonoMod.Cil;

namespace JadeFables.Core.Edits;

public sealed class ModifyTreeShakeResultsEdit : RuntimeDetourModSystem
{
    public override void OnModLoad() {
        base.OnModLoad();
        
        IL.Terraria.WorldGen.ShakeTree += RewriteShakeDropRates;
    }

    private void RewriteShakeDropRates(ILContext il) {
    }
}