using System.Reflection;
using JadeFables.Biomes.JadeLake;
using JadeFables.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace JadeFables.Tiles.JadeFountain
{
    public class JadeFountain : ModTile
    {
        public static Delegate SetActiveFountain;

        public override void SetStaticDefaults()
        {
            //Mostly everything in SceneMetrics is public, EXCEPT the one thing I need to use :(
            SetActiveFountain = typeof(SceneMetrics).GetProperty(nameof(SceneMetrics.ActiveFountainColor), BindingFlags.Instance | BindingFlags.Public)!.GetSetMethod(true)!.CreateDelegate<Action<int>>(Main.SceneMetrics);

            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Origin = new Point16(1, 2); // Todo: make less annoying.
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);
            AnimationFrameHeight = 72;

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(jadeStoneGray, name);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            SoundEngine.PlaySound(SoundID.Item27, new Vector2(i * 16, j * 16));

            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 3 * 16, 4 * 16, ItemType<JadeFountainItem>());
        }

        public override bool RightClick(int i, int j)
        {
            short pixelWidthOfFullTile = 3 * 18;
            Tile tile = Main.tile[i, j];
            int frameXDisplacement = tile.TileFrameX >= pixelWidthOfFullTile ? (int)(tile.TileFrameX / (float)pixelWidthOfFullTile) * pixelWidthOfFullTile : 0;
            Point topLeft = new Point(i - (tile.TileFrameX - frameXDisplacement) / 18, j - tile.TileFrameY / 18);

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    Tile newTile = Main.tile[topLeft.X + x, topLeft.Y + y];
                    newTile.TileFrameX += (short)(newTile.TileFrameX < 18 * 3 ? pixelWidthOfFullTile : -pixelWidthOfFullTile);
                }
            }
            Main.tileFrame[Type] = Main.tileFrameCounter[Type] = 0;

            SoundEngine.PlaySound(SoundID.MenuTick, new Vector2(i * 16, j * 16));
            return true;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            Tile tile = Main.tile[i, j];

            //Sparkle every now and then
            if (tile.TileFrameX % 3 * 18 == 0 && tile.TileFrameY % 4 * 18 == 0 && (Lighting.NotRetro ? Main.rand.NextBool(200) : Main.rand.NextBool(200 * 4)))
            {
                Dust.NewDust(new Vector2(i * 16, (j + 1) * 16), 3, 3, DustID.TreasureSparkle, Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
            }

            //Only animate when activated
            if (tile.TileFrameX == 3 * 18 && tile.TileFrameY % 4 * 18 == 0)
            {
                ref int frame = ref Main.tileFrame[type];
                ref int frameCounter = ref Main.tileFrameCounter[Type];

                frameCounter++;
                if (frameCounter >= 9)
                {
                    frameCounter = 0;
                    if (++frame >= 3)
                    {
                        frame = 0;
                    }
                }

            }
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer || Main.tile[i, j].TileFrameX < 18 * 3)
            {
                return;
            }

            GetInstance<JadeLakeSystem>().forceLakeAesthetic = true;
            SetActiveFountain.DynamicInvoke(GetInstance<JadeLakeWaterStyle>().Slot);
        }
    }
    internal class JadeFountainItem : ModItem
    {

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<JadeFountain>();
            Item.width = 10;
            Item.height = 24;

            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.Blue;
        }
    }
}