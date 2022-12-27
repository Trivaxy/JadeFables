
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace JadeFables.Items.Potions.Dumpling
{
	public class Dumpling : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dumpling");
			Tooltip.SetDefault("Medium improvements to all stats\n'Almost unrecognizable'");
		}


		public override void SetDefaults()
		{
			Item.width = Item.height = 20;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 99;
			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.EatFood;
			Item.useTime = Item.useAnimation = 30;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.buffType = BuffID.WellFed2;
			Item.buffTime = 54000;
			Item.noMelee = true;
			Item.consumable = true;
			Item.UseSound = SoundID.Item2;
			Item.autoReuse = false;
		}
	}
}
