using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace JadeFables.Items.Potions.Heartbeat
{
    public class HeartbeatBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }
    }

    public class HeartGNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public Player hurter = null;

        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            hurter = player;
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            hurter = Main.player[projectile.owner];
        }
    }

    public class HeartGItem : GlobalItem
    {

        public override void OnSpawn(Item item, IEntitySource source)
        {
            if (source is not EntitySource_Loot)
                return;

            var lootsource = source as EntitySource_Loot;
            Entity entity = lootsource.Entity;

            if (entity is not NPC)
                return;

            NPC npc = entity as NPC;
            Player player = npc.GetGlobalNPC<HeartGNPC>().hurter;

            if (player == null || !player.HasBuff(BuffType<HeartbeatBuff>()))
                return;

            if (item.type == ItemID.Heart || item.type == ItemID.CandyCane || item.type == ItemID.CandyApple)
            {
                Item.NewItem(source, item.Center, ModContent.ItemType<BigHeartItem>(), 1);
                item.TurnToAir();
            }
        }
    }
}
