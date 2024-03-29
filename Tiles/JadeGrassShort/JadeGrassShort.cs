using JadeFables.Tiles.OvergrownJadeSand;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using JadeFables.Items.Lotus.LotusFiber;

namespace JadeFables.Tiles.JadeGrassShort
{
    public class JadeGrassShort : ModTile
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

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(jadeGrassLime);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 2;
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            offsetY = 2;

        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);
            if (!tileBelow.HasTile || tileBelow.IsHalfBlock || tileBelow.TopSlope || (tileBelow.TileType != ModContent.TileType<Tiles.JadeSand.JadeSandTile>() && tileBelow.TileType != ModContent.TileType<Tiles.OvergrownJadeSand.OvergrownJadeSandTile>()))
                WorldGen.KillTile(i, j);
            return true;
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail && Main.rand.NextBool(20)) Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ItemType<JadeGrassSeeds>());
            if (!fail && Main.rand.NextBool(10)) Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ItemType<LotusFiber>());
        }
    }

    public class JadeGrassTall : JadeGrassShort
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
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(jadeGrassLime);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);
            if (tileBelow.HasTile && tileBelow.TileType == ModContent.TileType<JadeGrassTall>())
                return true;
            if (!tileBelow.HasTile || tileBelow.IsHalfBlock || tileBelow.TopSlope || (tileBelow.TileType != ModContent.TileType<Tiles.JadeSand.JadeSandTile>() && tileBelow.TileType != ModContent.TileType<Tiles.OvergrownJadeSand.OvergrownJadeSandTile>()))
                WorldGen.KillTile(i, j);
            return true;
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail && Main.rand.NextBool(20)) Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ItemType<JadeGrassSeeds>());
            if (!fail && Main.rand.NextBool(10)) Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ItemType<LotusFiber>());
        }
    }
}