﻿//TODO:
//Better map color

using JadeFables.Core;
using JadeFables.Tiles.JadeSand;
using Terraria.Localization;

namespace JadeFables.Tiles.HardenedJadeSand
{
    public class HardenedJadeSandTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            MinPick = 10;
            DustType = DustType<Dusts.JadeSandstoneDust>();
            HitSound = SoundID.Dig;
            //ItemDrop = ItemType<HardenedJadeSandItem>();
            Main.tileMerge[TileType<JadeSandTile>()][Type] = true;
            Main.tileBrick[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.Conversion.HardenedSand[Type] = true;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(jadeSandMid, name);
        }
    }

    public class HardenedJadeSandItem : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = Item.CommonMaxStack;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = TileType<HardenedJadeSandTile>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }
    }
}
