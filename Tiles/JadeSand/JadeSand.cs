using JadeFables.Tiles.JadeSandstone;

namespace JadeFables.Tiles.JadeSand
{
    public class JadeSandTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            MinPick = 10;
            DustType = DustType<Dusts.JadeSandDust>();
            HitSound = SoundID.Dig;
            ItemDrop = ItemType<JadeSandItem>();
            Main.tileMerge[TileID.Stone][Type] = true;
            Main.tileBrick[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileBlockLight[Type] = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Spring Sand");
            AddMapEntry(new Color(207, 160, 118), name);
        }
    }

    public class JadeSandItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spring Sand");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = TileType<JadeSandTile>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }
    }
}
