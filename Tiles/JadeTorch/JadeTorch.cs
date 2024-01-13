using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.Localization;
using Terraria.ModLoader;
using JadeFables.Items.Jade.JadeChunk;
using ReLogic.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using JadeFables.Biomes.JadeLake;

namespace JadeFables.Tiles.JadeTorch
{
    public class JadeTorch : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;

            ItemID.Sets.Torches[Type] = true;
            ItemID.Sets.WaterTorches[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.flame = true;
            Item.noWet = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.holdStyle = ItemHoldStyleID.HoldFront;
            Item.autoReuse = true;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<JadeTorchTile>();
            Item.width = 10;
            Item.height = 12;
            Item.value = 50;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        { // Overrides the default sorting method of this Item.
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Torches; // Vanilla usually matches sorting methods with the right type of item, but sometimes, like with torches, it doesn't. Make sure to set whichever items manually if need be.
        }

        public static Color jadeTorchColor => Color.Lerp(new Color(20, 210, 60), new Color(0, 255, 200), Main.demonTorch);

        public override void HoldItem(Player player)
        {

            // Create a white (1.0, 1.0, 1.0) light at the torch's approximate position, when the item is held.
            Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

            Lighting.AddLight(position, jadeTorchColor.ToVector3());
        }

        public override void PostUpdate()
        {
            // Create a white (1.0, 1.0, 1.0) light when the item is in world, and isn't underwater.
            if (!Item.wet)
            {
                Lighting.AddLight(Item.Center, jadeTorchColor.ToVector3());
            }
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes()
        {
            CreateRecipe(5)
                .AddIngredient(ItemID.Torch, 5)
                .AddIngredient<JadeChunk>(1)
                .Register();
        }
    }

    public class JadeTorchTile : ModTile
    {
        private Asset<Texture2D> flameTexture;

        public override void SetStaticDefaults()
        {
            // Properties
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileWaterDeath[Type] = false;
            TileID.Sets.FramesOnKillWall[Type] = false;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.Torch[Type] = true;
            TileSets.TorchThatTriggersTorchGod[Type] = true;

            //ItemDrop = ModContent.ItemType<JadeTorch>();
            //DustType = ModContent.DustType<Sparkle>();
            AdjTiles = new int[] { TileID.Torches };

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

            // Placement
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newTile = AllowPlacingInLiquid(TileObjectData.newTile);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.newAlternate = AllowPlacingInLiquid(TileObjectData.newAlternate);
            TileObjectData.addAlternate(1);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.newAlternate = AllowPlacingInLiquid(TileObjectData.newAlternate);
            TileObjectData.addAlternate(2);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            TileObjectData.newAlternate.AnchorWall = true;
            TileObjectData.newAlternate = AllowPlacingInLiquid(TileObjectData.newAlternate);
            TileObjectData.addAlternate(0);
            TileObjectData.addTile(Type);

            // Etc
            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(0, 170, 140), name);

            // Assets
            if (!Main.dedServ)
            {
                flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
            }
        }

        public static TileObjectData AllowPlacingInLiquid(TileObjectData tileObjectData)
        {
            tileObjectData.WaterDeath = false;
            tileObjectData.LavaDeath = false;
            tileObjectData.WaterPlacement = LiquidPlacement.Allowed;
            tileObjectData.LavaPlacement = LiquidPlacement.Allowed;

            return tileObjectData;
        }

        public override float GetTorchLuck(Player player)
        {
            // GetTorchLuck is called when there is an ExampleTorch nearby the client player
            // In most use-cases you should return 1f for a good luck torch, or -1f for a bad luck torch.
            // You can also add a smaller amount (eg 0.5) for a smaller postive/negative luck impact.
            // Remember that the overall torch luck is decided by every torch around the player, so it may be wise to have a smaller amount of luck impact.
            // Multiple example torches on screen will have no additional effect.

            // Positive and negative luck are accumulated separately and then compared to some fixed limits in vanilla to determine overall torch luck.
            // Postive luck is capped at 1, any value higher won't make any difference and negative luck is capped at 2.
            // A negative luck of 2 will cancel out all torch luck bonuses.

            // The influence positive torch luck can have overall is 0.1 (if positive luck is any number less than 1) or 0.2 (if positive luck is greater than or equal to 1)

            bool inJadeBiome = player.InModBiome<JadeLakeBiome>();
            return inJadeBiome ? 1f : 0f; //gives no negative luck since the torch looks good in a variety of biomes
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = Main.rand.Next(1, 3);

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];

            // If the torch is on
            if (tile.TileFrameX < 66)
            {
                // Make it emit the following light.
                r = JadeTorch.jadeTorchColor.R / 255f;
                g = JadeTorch.jadeTorchColor.G / 255f;
                b = JadeTorch.jadeTorchColor.B / 255f;
            }
        }
        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            offsetY = 0;

            if (WorldGen.SolidTile(i, j - 1))
            {
                offsetY = 2;

                if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
                {
                    offsetY = 4;
                }
            }
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // The following code draws multiple flames on top our placed torch.

            int offsetY = 0;

            if (WorldGen.SolidTile(i, j - 1))
            {
                offsetY = 2;

                if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
                {
                    offsetY = 4;
                }
            }

            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);

            if (Main.drawToScreen)
            {
                zero = Vector2.Zero;
            }

            ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i); // Don't remove any casts.
            Color color = new Color(100, 100, 100, 0);
            int width = 20;
            int height = 20;
            var tile = Main.tile[i, j];
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;

            for (int k = 0; k < 7; k++)
            {
                float xx = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
                float yy = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;

                spriteBatch.Draw(flameTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + xx, j * 16 - (int)Main.screenPosition.Y + offsetY + yy) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}
