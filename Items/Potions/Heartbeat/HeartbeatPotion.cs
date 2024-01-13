using Terraria.ID;

namespace JadeFables.Items.Potions.Heartbeat
{
    public class HeartbeatPotion : ModItem
    {

        public override void SetStaticDefaults()
        {
            ItemID.Sets.DrinkParticleColors[Item.type] = new Color[3] { new Color(179, 20, 20), new Color(232, 33, 33), new Color(255, 135, 182) };
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 32;
            Item.maxStack = Item.CommonMaxStack;

            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;

            Item.consumable = true;
            Item.buffType = (ModContent.BuffType<HeartbeatBuff>());
            Item.buffTime = 21600;

            Item.UseSound = SoundID.Item3;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.RegenerationPotion);
            recipe.AddIngredient(ItemID.Fireblossom);
            recipe.AddIngredient<Critters.KoiItem>(10);
            recipe.AddTile(TileID.Bottles);
            recipe.Register();
        }
    }
}
