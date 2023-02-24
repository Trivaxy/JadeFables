﻿using JadeFables.Dusts;
using JadeFables.Tiles.JadeSandstone;
using JadeFables.Tiles.JadeSand;

namespace JadeFables.Tiles.JadeOre
{
    public class JadeOre : ModTile
    {
        public override void SetStaticDefaults()
        {
            MinPick = 65;
            DustType = DustType<Dusts.JadeSandDust>();
            HitSound = SoundID.Dig;
            ItemDrop = ItemType<Items.Jade.JadeChunk.JadeChunk>();
            Main.tileMerge[Type][ModContent.TileType<JadeSandTile>()] = true;
            Main.tileBrick[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
            //TileID.Sets.Falling[Type] = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Jade Ore");
            AddMapEntry(Color.Green, name);
        }
    }
}