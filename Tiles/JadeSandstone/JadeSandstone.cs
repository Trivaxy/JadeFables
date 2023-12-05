using JadeFables.Core;
using JadeFables.Tiles.JadeSand;
using Terraria.Localization;

namespace JadeFables.Tiles.JadeSandstone
{
    public class JadeSandstoneTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            MinPick = 10;
            DustType = DustType<Dusts.JadeSandstoneDust>();
            HitSound = SoundID.Dig;
            //ItemDrop = ItemType<JadeSandstoneItem>();
            Main.tileMerge[TileType<JadeSandTile>()][Type] = true;
            Main.tileBrick[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.Conversion.Sandstone[Type] = true;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(jadeSandDark, name);
        }
    }

    public class JadeSandstoneItem : ModItem
    {

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
            Item.createTile = TileType<JadeSandstoneTile>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<JadeSandItem>(2).
                AddTile(TileID.Furnaces).
                Register();
        }
    }
}
