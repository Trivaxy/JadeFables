using JadeFables.Biomes.JadeLake;
using JadeFables.Helpers;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.ModLoader;

namespace JadeFables.NPCs
{
    public class JadeHappiness : ILoadable
    {
        public void Load(Mod mod)
        {
            Terraria.GameContent.On_ShopHelper.GetShoppingSettings += ShopHelper_GetShoppingSettings;
        }

        public void Unload()
        {
            Terraria.GameContent.On_ShopHelper.GetShoppingSettings -= ShopHelper_GetShoppingSettings;
        }

        private static ShoppingSettings ShopHelper_GetShoppingSettings(Terraria.GameContent.On_ShopHelper.orig_GetShoppingSettings orig, ShopHelper self, Player player, NPC npc)
        {
            var val = orig(self, player, npc);
            if (player.InModBiome<JadeLakeBiome>())
                ModifyShoppingSettings(player, npc, ref val, self);
            return val;
        }
        public static void ModifyShoppingSettings(Player player, NPC npc, ref ShoppingSettings settings, ShopHelper shopHelper)
        {
            string happinessQuote = ""; //TODO: Localize
            string oldHappinessQuote = "[LikeBiomeQuote]";
            switch (npc.type)
            {
                case 633: //Zoologist doesn't have an NPCID for some reason
                    oldHappinessQuote = "I love animals, so naturally TownNPCMoodBiomes.The Springs is like, the best place ever! Yas!";
                    happinessQuote = "Ohmigosh, the baby mantises here in the Springs are sooo cute!";
                    break;
                case NPCID.Demolitionist:
                    oldHappinessQuote = "Dwarves are naturally drawn to the Underground, it's in our blood!";
                    happinessQuote = "There’s too much water in the Springs. I cannae blow things up proper. Give me solid rock any day!";
                    break;
                case NPCID.Angler:
                    oldHappinessQuote = "Why is TownNPCMoodBiomes.The Springs my favorite place to go? It has tons of cool fish, duh!";
                    happinessQuote = "Muahaha, the Springs are awesome! Giving me access to an endless supply of pufferfish is about to be everyone’s problem!";
                    break;
                case NPCID.WitchDoctor:
                    oldHappinessQuote = "I cannot fathom existing elsewhere, TownNPCMoodBiomes.The Springs is the center of my voodoo spirits.";
                    happinessQuote = "A tree that has taken root in foreign turf may grow tallest of all. My heart rests in the Jungle, but the Spring’s energies bring peace to my soul.";
                    break;
                case NPCID.Cyborg:
                    oldHappinessQuote = "The flora and fauna are invading my synthetic exoskeleton: TownNPCMoodBiomes.The Springs is an undesired location for myself.";
                    happinessQuote = "Humidity levels in the Springs interfering with transistor flow. Initializing relocation request.";
                    break;
                case NPCID.Truffle:
                    oldHappinessQuote = "I dislike TownNPCMoodBiomes.The Springs.";
                    happinessQuote = "I keep getting attacked by the bullfrogs here in the Springs. I think I smell like food to them. I hate it!";
                    break;
                case NPCID.Steampunker:
                    oldHappinessQuote = "The shrubbery and mess of TownNPCMoodBiomes.The Springs really grinds my gears!";
                    happinessQuote = "Great Scott! The air’s proper drenched with moisture in the Springs. My gizmos and gadgets have practically rusted over.";
                    break;
                case NPCID.Mechanic:
                    oldHappinessQuote = "I don't like the Underground, it reminds me of a traumatic experience!";
                    happinessQuote = "What do I think about the Springs? The humidity is corroding the contacts in my wiring, it’s eroding the circuits— Oh, forget it. I hate it!";
                    break;
                case NPCID.Dryad:
                    oldHappinessQuote = "I kissed a tree in TownNPCMoodBiomes.The Springs and I liked it.";
                    happinessQuote = "The Springs bring me a sense of relief and tranquility. I like it here.";
                    break;
            }
            Helper.ReplaceText(ref settings.HappinessReport, oldHappinessQuote, happinessQuote);
        }
    }

    public class JadeGNPC : GlobalNPC
    {
        public override void SetStaticDefaults()
        {
            NPCHappiness.Get(633).SetBiomeAffection<JadeLakeBiome>(AffectionLevel.Like);
            NPCHappiness.Get(NPCID.Demolitionist).SetBiomeAffection<JadeLakeBiome>(AffectionLevel.Dislike);
            NPCHappiness.Get(NPCID.Angler).SetBiomeAffection<JadeLakeBiome>(AffectionLevel.Like);
            NPCHappiness.Get(NPCID.WitchDoctor).SetBiomeAffection<JadeLakeBiome>(AffectionLevel.Like);
            NPCHappiness.Get(NPCID.Cyborg).SetBiomeAffection<JadeLakeBiome>(AffectionLevel.Dislike);
            NPCHappiness.Get(NPCID.Truffle).SetBiomeAffection<JadeLakeBiome>(AffectionLevel.Dislike);
            NPCHappiness.Get(NPCID.Steampunker).SetBiomeAffection<JadeLakeBiome>(AffectionLevel.Dislike);
            NPCHappiness.Get(NPCID.Mechanic).SetBiomeAffection<JadeLakeBiome>(AffectionLevel.Dislike);
            NPCHappiness.Get(NPCID.Dryad).SetBiomeAffection<JadeLakeBiome>(AffectionLevel.Like);
        }
    }
}
