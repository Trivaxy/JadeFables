using Terraria.ID;

namespace JadeFables.Items.Potions.Heartbeat
{
    public class HeartbeatPotion : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Heartbeat Potion");
            Tooltip.SetDefault("Heart pickups turn into larger hearts, which heal more");
            ItemID.Sets.DrinkParticleColors[Item.type] = new Color[3] { new Color(150, 0, 0), new Color(100, 0, 0), new Color(50, 0, 0) }; //change the color when the item sprite is done
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
            Item.buffType = (ModContent.BuffType<HeartbeatBuff>());
            Item.buffTime = 21600;

            Item.UseSound = SoundID.Item3;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.RegenerationPotion);
            recipe.AddIngredient(ItemID.Fireblossom);
            recipe.AddIngredient<Critters.Koi>(1);
            recipe.AddTile(TileID.Bottles);
            recipe.Register();
        }
    }
}
