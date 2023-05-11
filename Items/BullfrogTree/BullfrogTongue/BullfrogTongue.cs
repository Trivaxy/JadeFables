using System.Collections.Generic;
using Terraria.GameContent.Creative;
using Terraria.ID;
using JadeFables.Core;

namespace JadeFables.Items.BullfrogTree.BullfrogTongue
{
    public class BullfrogTongue : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<BullfrogTongueProj>(), 12, 1.2f, 5f, 25);
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
    }

    public class BullfrogTongueProj : BaseWhip
    {
        public BullfrogTongueProj() : base("Bullfrog Tongue", 15, 0.47f, new Color(153, 122, 97)) { }

        public override int SegmentVariant(int segment)
        {
            int variant = segment switch
            {
                5 or 6 or 7 or 8 => 2,
                9 or 10 or 11 or 12 or 13 => 3,
                _ => 1,
            };
            return variant;
        }

        public override bool ShouldDrawSegment(int segment)
        {
            return true;// segment % 2 == 0;
        }

        public override void ArcAI()
        {

        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

            int item = target.catchItem;
            if (target.CountsAsACritter && item > 0)
            {

                Player owner = Main.player[Projectile.owner];
                target.immortal = true;
                Item.NewItem(target.GetSource_CatchEntity(target), owner.Center, item);
                target.active = false;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BullfrogTongueTag>(), 240);
            Projectile.damage = (int)(Projectile.damage * .6f);
            if (Projectile.damage < 1)
            {
                Projectile.damage = 1;
            }

            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color minLight = lightColor;
            var minColor = new Color(10, 25, 33);

            if (minLight.R < minColor.R)
                minLight.R = minColor.R;

            if (minLight.G < minColor.G)
                minLight.G = minColor.G;

            if (minLight.B < minColor.B)
                minLight.B = minColor.B;

            return minLight;
        }
    }

    public class BullfrogTongueTag : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC NPC, ref int buffIndex)
        {
            NPC.GetGlobalNPC<BullfrogTongueGlobalNPC>().tagType = Type;
        }
    }

    //Should be rolled into a larger GlobalNPC class later, realistically
    public class BullfrogTongueGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int tagType;

        public override void ResetEffects(NPC npc)
        {
            tagType = -1;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!projectile.trap && (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type]))
            {
                if (tagType == BuffType<BullfrogTongueTag>())
                {
                    projectile.damage += 6;
                }
            }
        }
    }
}