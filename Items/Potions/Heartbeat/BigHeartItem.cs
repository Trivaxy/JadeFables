using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace JadeFables.Items.Potions.Heartbeat
{
    public class BigHeartItem : ModItem
    {

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 5));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
        }

        public override bool ItemSpace(Player Player) => true;
        public override bool OnPickup(Player Player)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, Player.Center);

            const int healAmount = 40;

            Player.HealEffect(healAmount, true);
            Player.statLife += Math.Min(healAmount, Player.statLifeMax2 - Player.statLife);

            Item.active = false;
            return false;
        }
    }
}
