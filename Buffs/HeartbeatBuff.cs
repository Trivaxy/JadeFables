using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace JadeFables.Buffs
{
    public class HeartbeatBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Heartbeat");
            Description.SetDefault("Increases life restored by heart pickups");
            Main.buffNoSave[Type] = true;
        }
    }

    public class HeartGItem : GlobalItem
    {    
        public override bool OnPickup(Item item, Player player)
        {
            if (player.HasBuff(BuffType<HeartbeatBuff>()))
            {
                if (item.type == ItemID.Heart || item.type == ItemID.CandyCane || item.type == ItemID.CandyApple)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, player.Center);

                    const int healAmount = 40;

                    player.HealEffect(healAmount, true);
                    player.statLife += Math.Min(healAmount, player.statLifeMax2 - player.statLife);

                    item.active = false;
                    return false;
                }
            }
            return true;
        }
    }
}
