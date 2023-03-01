using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using JadeFables.Core;
using ReLogic.Content;
using JadeFables.Helpers;
using Terraria.Graphics.Effects;
using SteelSeries.GameSense;
using IL.Terraria.Audio;
using Terraria.Audio;
using Terraria.DataStructures;

namespace JadeFables.Items.SpringChestLoot.Chopsticks
{
	public class Chopsticks : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Chopsticks");
			Tooltip.SetDefault("Swords have more range and radius \nEnemies drop food more often");
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ChopstickPlayer>().equipped = true;
		}
	}

	public class ChopstickPlayer : ModPlayer
	{
		public bool equipped = false;

        public override void ResetEffects()
        {
            equipped = false;
        }
    }

    public class ChopsticksNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool destroyNonFood = false;
        readonly int FOODMULT = 100;

        public override void Load()
        {
            On.Terraria.Item.NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool += ItemDetour1; //covering ALL my bases here
            On.Terraria.Item.NewItem_IEntitySource_Rectangle_int_int_bool_int_bool_bool += ItemDetour2;
            On.Terraria.Item.NewItem_IEntitySource_Vector2_int_int_bool_int_bool_bool += ItemDetour3;
            On.Terraria.Item.NewItem_IEntitySource_Vector2_int_int_int_int_bool_int_bool_bool += ItemDetour4;
            On.Terraria.Item.NewItem_IEntitySource_Vector2_Vector2_int_int_bool_int_bool_bool += ItemDetour5;
            On.Terraria.NPC.NPCLoot_DropItems += NPC_NPCLoot_DropItems;
        }

        private void NPC_NPCLoot_DropItems(On.Terraria.NPC.orig_NPCLoot_DropItems orig, NPC self, Player closestPlayer)
        {
            if (closestPlayer.GetModPlayer<ChopstickPlayer>().equipped)
            {
                self.GetGlobalNPC<ChopsticksNPC>().destroyNonFood = true;
                for (int i = 0; i < FOODMULT; i++)
                {
                   // orig(self, closestPlayer);
                }
                self.GetGlobalNPC<ChopsticksNPC>().destroyNonFood = false;
            }
            orig(self, closestPlayer);
        }

        private int ItemDetour5(On.Terraria.Item.orig_NewItem_IEntitySource_Vector2_Vector2_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, Vector2 pos, Vector2 randomBox, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, pos, randomBox, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
        }

        private int ItemDetour4(On.Terraria.Item.orig_NewItem_IEntitySource_Vector2_int_int_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, Vector2 pos, int Width, int Height, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, pos, Width, Height, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
        }

        private int ItemDetour3(On.Terraria.Item.orig_NewItem_IEntitySource_Vector2_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, Vector2 position, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, position, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
        }

        private int ItemDetour2(On.Terraria.Item.orig_NewItem_IEntitySource_Rectangle_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, Rectangle rectangle, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, rectangle, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
        }

        private int ItemDetour1(On.Terraria.Item.orig_NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, int X, int Y, int Width, int Height, int Type, int Stack, bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, X, Y, Width, Height, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
        }

        public static bool DestroyNonFood(IEntitySource source, int Type)
        {
            if (source is EntitySource_Loot lootSource && lootSource.Entity is NPC npc)
            {
                if (npc.GetGlobalNPC<ChopsticksNPC>().destroyNonFood && !IsFood(Type))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsFood(int type)
        {
            Item item = new Item(type);
            item.SetDefaults(type);
            if (item.buffType == BuffID.WellFed || item.buffType == BuffID.WellFed2 || item.buffType == BuffID.WellFed3)
                return true;
            return false;
        }
    }
}
