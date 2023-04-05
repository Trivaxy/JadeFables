using JadeFables.Tiles.SpringChest;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace JadeFables.Tiles.WarriorStatue
{
	public class WarriorStatue : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileMergeDirt[Type] = true;

			TileObjectData.newTile.Width = 5;
			TileObjectData.newTile.Height = 7;
			TileObjectData.newTile.Origin = new Point16(0, 6);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16};
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.addTile(Type); 
			DustType = DustID.Stone;

			LocalizedText name = CreateMapEntryName();
            AddMapEntry(jadeStoneGray, name);
		}

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconText = "";
            player.cursorItemIconID = ItemID.GoldCoin;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
		public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new EntitySource_TileBreak(i, j), (i + 3) * 16, (j + 5) * 16, 16, 32, ModContent.ItemType<WarriorStatueItem>());

        public override bool RightClick(int i, int j) {
            if (Main.tile[i, j].TileFrameY >= 126)
                return false;
            int gillsCost = Item.gold;
            Player player = Main.LocalPlayer;

            if (!player.CanBuyItem(gillsCost)) {
                return false;
            }

            player.BuyItem(gillsCost);
            // 15 minute buff
            player.AddBuff(BuffID.Gills, 60 * 60 * 15, false);

            SoundEngine.PlaySound(SoundID.CoinPickup);
            for (int k = 0; k < 3; k++)
            {
                int dust = Dust.NewDust(player.position, player.width, player.height, DustID.GoldCoin, 0, -2);
                Main.dust[dust].velocity.X = 0;
                Main.dust[dust].velocity.Y *= 0.3f;
            }
            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen);
            //SoundEngine.PlaySound(SoundID.Item155 with {PitchVariance = 0.125f});

            Tile tile = Framing.GetTileSafely(i, j); //Selects current tile

            int newX = i; //Here to line 67 adjusts the tile position so we get the top-left of the multitile
            int newY = j;

            int tries = 0;
            while (Main.tile[newX, newY].TileFrameX > 0 && tries++ < 99)
            {
                newX--;
            }

            while (Main.tile[newX, newY].TileFrameY > 0 && tries++ < 99)
            {
                newY--;
            }

            Projectile.NewProjectile(new EntitySource_TileInteraction(null, newX, newY), new Vector2(newX, newY) * 16, Vector2.Zero, ModContent.ProjectileType<WarriorStatueProj>(), 0, 0);
            for (int k = 0; k < 5; k++)
            {
                for (int l = 0; l < 7; ++l)
                    Main.tile[newX + k, newY + l].TileFrameY += 126; 
            }
            return true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 126 && !Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<WarriorStatueProj>() && n.Center == new Vector2(i,j) * 16))
            {
                for (int k = 0; k < 5; k++)
                {
                    for (int l = 0; l < 7; ++l)
                        Main.tile[i + k, j+ l].TileFrameY -= 126;
                }
            }
        }
    }
    public class WarriorStatueProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center + new Vector2(40, 50), Color.Teal.ToVector3() * 1.6f);
            if (Main.rand.NextBool(7))
            {
                Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.Next(10, 70), 0);
                Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.GlowLineFast>(), Vector2.UnitY * -4, 0, Color.Teal, 1.0f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawOffset = new Vector2(-8, -4);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            for (float i = 0; i < 6.28f; i += 0.78f)
            {
                float sin = MathF.Sin((float)Main.timeForVisualEffects * 0.04f) + 1;
                Vector2 offset = i.ToRotationVector2() * (2 * sin);
                Main.spriteBatch.Draw(tex, Projectile.Center + drawOffset + offset - Main.screenPosition, null, Color.White * (0.4f * (((sin + 0.3f) * 0.5f))), 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }

    internal class WarriorStatueItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<WarriorStatue>();
            Item.width = 10;
            Item.height = 24;


            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.White;
        }
    }
}