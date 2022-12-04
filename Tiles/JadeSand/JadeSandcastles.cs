
using CsvHelper.TypeConversion;
using IL.Terraria.GameContent;
using Microsoft.Xna.Framework;
using On.Terraria.GameContent;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace JadeFables.Tiles.JadeSand
{
    public class JadeSandCastle1 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
            16,
            16
            };
            TileObjectData.addTile(Type);
            DustType = DustID.Sand;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Jade Sandcastle");
            AddMapEntry(new Color(207, 160, 118), name);
        }
        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            offsetY = 2;
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tileBelow = Framing.GetTileSafely(i, j + 2);
            if (!tileBelow.HasTile || tileBelow.IsHalfBlock || tileBelow.TopSlope)
            {
                WorldGen.KillTile(i, j);
            }

            return true;
        }
    }

    public class JadeSandCastle2 : JadeSandCastle1 { }

    public class JadeSandCastle3 : JadeSandCastle1
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
            16,
            16
            };
            TileObjectData.addTile(Type);
            DustType = DustID.Sand;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Jade Sandcastle");
            AddMapEntry(new Color(207, 160, 118), name);
        }
    }

    public class JadeSandCastle4 : JadeSandCastle1
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
            16,
            };
            TileObjectData.addTile(Type);
            DustType = DustID.Sand;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Jade Sandcastle");
            AddMapEntry(new Color(207, 160, 118), name);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);
            if (!tileBelow.HasTile || tileBelow.IsHalfBlock || tileBelow.TopSlope)
            {
                WorldGen.KillTile(i, j);
            }

            return true;
        }
    }
    public class JadeSandCastle5 : JadeSandCastle4
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
            16
            };
            TileObjectData.addTile(Type);
            DustType = DustID.Sand;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Jade Sandcastle");
            AddMapEntry(new Color(207, 160, 118), name);
        }
    }

    public class JadeSandcastleGItem : GlobalItem
    {
        public override void HoldItem(Item item, Player player)
        {
            if (item.type != ItemID.SandcastleBucket)
                return;

            int x = (int)Main.MouseWorld.X / 16;
            int y = (int)Main.MouseWorld.Y / 16;
            if (player.InInteractionRange(x, y) && Main.tile[x, y + 1].HasTile && Main.tile[x, y + 1].TileType == ModContent.TileType<JadeSandTile>())
            {
                item.tileWand = ModContent.ItemType<JadeSandItem>();
                item.createTile = ModContent.TileType<JadeSandCastle2>();
            }
            else
            {
                item.createTile = 552;
                item.tileWand = 169;
            }
        }
    }
}