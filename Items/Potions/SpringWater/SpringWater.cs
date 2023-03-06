using JadeFables.Items.Potions.Heartbeat;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Recipe;
using JadeFables.Biomes.JadeLake;
using Terraria.Localization;

namespace JadeFables.Items.Potions.SpringWater
{
	public class SpringWater : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spring Water");
			Tooltip.SetDefault("Clears almost all debuffs\nCauses Potion Sickness for 15 seconds");
            ItemID.Sets.DrinkParticleColors[Item.type] = new Color[1] { Color.Cyan};
		}

		public override void SetDefaults()
		{
            Item.width = 24;
            Item.height = 32;
            Item.maxStack = 30; //Change this when Labor of Love drops?

            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;

            Item.consumable = true;

            Item.UseSound = SoundID.Item3;
        }
		public override bool CanUseItem(Player player) => player.FindBuffIndex(BuffID.PotionSickness) < 0;

		public override bool? UseItem(Player player)
		{
			Item.healLife = 0; //set item's heal life to 0 when actually used, so it doesnt heal player
			if (!player.pStone)
				player.AddBuff(BuffID.PotionSickness, 900);
			else
				player.AddBuff(BuffID.PotionSickness, 600);

			player.AddBuff(ModContent.BuffType<SpringWaterBuff>(), 600);
			return true;
		}

        public override void AddRecipes()
        {
            Condition nearJadeWater = new Condition(NetworkText.FromKey("RecipeConditions.NearWater"), n => Main.LocalPlayer.adjWater && Main.LocalPlayer.InModBiome<JadeLakeBiome>());
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Bottle);
            recipe.AddCondition(nearJadeWater);
            recipe.Register();
        }
    }

    public class SpringWaterBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spring Water");
            Description.SetDefault("Debuffs are cured");
            Main.buffNoSave[Type] = true;
        }

		public override void Update(Player player, ref int buffIndex)
		{
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int type = player.buffType[i];
                if (Main.debuff[type] && !BuffID.Sets.NurseCannotRemoveDebuff[type] && type != BuffID.TheTongue)
                    player.DelBuff(i);
            }
            base.Update(player, ref buffIndex);
		}
	}
}
