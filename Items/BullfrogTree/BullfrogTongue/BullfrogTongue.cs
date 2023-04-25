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
}