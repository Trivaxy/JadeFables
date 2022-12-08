using JadeFables.Core.Boids;

namespace JadeFables.Items.Critters
{
    public class Koi : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Koi");
            Tooltip.SetDefault("'Don't you play koi with me'");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 24;

            Item.maxStack = 999; //change after labor of love
            Item.value = Item.sellPrice(silver: 3);
            Item.rare = ItemRarityID.Blue;
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
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int itemWhoAmI = Item.NewItem(NPC.GetSource_NaturalSpawn(), (int)player.Center.X, (int)player.Center.Y, 0, 0, ItemType<Koi>(), caughtFish.Count, noBroadcast: true, 0, noGrabDelay: true);
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemWhoAmI, 1f);
            }
        }
    }
}
