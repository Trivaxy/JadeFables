using JadeFables.Core.Boids;

namespace JadeFables.Items.Critters
{
    public class KoiItem : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 24;

            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 3);
            Item.rare = ItemRarityID.Blue;
        }
        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ItemID.CookedFish);
            recipe.AddIngredient<Critters.KoiItem>(2);
            recipe.AddTile(TileID.CookingPots);
            recipe.Register();
        }
    }

    public class NetGItem : GlobalItem
    {
        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (Main.myPlayer == player.whoAmI && (item.type == ItemID.BugNet || item.type == ItemID.GoldenBugNet || item.type == ItemID.FireproofBugNet))
            {
                foreach (Flock fishflock in ModContent.GetInstance<BoidHost>().Flocks)
                {
                    fishflock.TryCatchFish(hitbox, out var caughtFish);
                    if (caughtFish.Any()) OnCatchBoidFish(player, caughtFish);
                }
            }
        }
        internal void OnCatchBoidFish(Player player, List<Fish> caughtFish)
        {
            player.QuickSpawnItem(NPC.GetSource_NaturalSpawn(), ItemType<KoiItem>(), caughtFish.Count);
        }
    }
}
