using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace JadeFables.Tiles.JasmineFlower
{
    public class JasmineFlowerTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
            TileID.Sets.SwaysInWindBasic[Type] = true;

            DustType = DustID.Plantera_Green;
            HitSound = SoundID.Grass;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(175, 190, 135), name);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 2;
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            height = 20;
            offsetY = 2;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<JasmineFlower>());
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);
            if (!tileBelow.HasTile || tileBelow.IsHalfBlock || tileBelow.TopSlope || (tileBelow.TileType != ModContent.TileType<JadeSand.JadeSandTile>() && tileBelow.TileType != ModContent.TileType<OvergrownJadeSand.OvergrownJadeSandTile>()))
                WorldGen.KillTile(i, j);
            return true;
        }

        public override bool CanPlace(int i, int j)
        {
            Tile baseTile = Framing.GetTileSafely(i, j + 1);
            if (baseTile.HasTile && baseTile.BlockType == BlockType.Solid && (baseTile.TileType == ModContent.TileType<JadeSand.JadeSandTile>() || baseTile.TileType == ModContent.TileType<OvergrownJadeSand.OvergrownJadeSandTile>()))
                return true;
            return false;
        }
    }

    public class JasmineFlower : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = Item.CommonMaxStack;

            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.Blue;

            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<JasmineFlowerTile>();
        }
    }
}