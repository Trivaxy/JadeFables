using JadeFables.Items.SpringChestLoot.FireworkPack;
using JadeFables.Items.SpringChestLoot.Gong;
using JadeFables.Items.SpringChestLoot.TanookiLeaf;
using JadeFables.Items.SpringChestLoot.Chopsticks;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;
using Terraria.GameContent.ItemDropRules;
using JadeFables.Biomes.JadeLake;
using static JadeFables.Items.Fishing.Crates.CrateDropRules;
using JadeFables.Items.SpringChestLoot.Hwacha;
using JadeFables.Tiles.JadeFountain;
using JadeFables.Items.Fishing.KoiPopper;

namespace JadeFables.Items.Fishing.Crates
{
    public class SpringFishingPlayer : ModPlayer
    {
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            if (attempt.rare && Player.InModBiome<JadeLakeBiome>())
            {
                if (attempt.crate)
                {
                    if (Main.hardMode)
                        itemDrop = ModContent.ItemType<Crates.DragonCrate>();
                    else
                        itemDrop = ModContent.ItemType<SpringCrate>();
                }
                else if (Main.rand.NextBool(4))
                    itemDrop = ModContent.ItemType<KoiPopper.KoiPopper>();
            }
        }
    }
}
