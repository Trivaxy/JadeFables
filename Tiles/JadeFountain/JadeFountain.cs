using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace JadeFables.Tiles.JadeFountain
{
    public class JadeFountain : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Origin = new Point16(0, 2); // Todo: make less annoying.
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);
            AnimationFrameHeight = 72;
            AddMapEntry(new Color(200, 200, 200));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            SoundEngine.PlaySound(SoundID.Item27, new Vector2(i * 16, j * 16));
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter >= 9)
            {
                frameCounter = 0;
                if (++frame >= 4)
                {
                    frame = 1;
                }
            }
        }
    }
    internal class JadeFountainItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Fountain");
            Tooltip.SetDefault("Makes nearby water sparkly");
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<JadeFountain>();
            Item.width = 10;
            Item.height = 24;
            Item.value = 500;
        }
    }
}