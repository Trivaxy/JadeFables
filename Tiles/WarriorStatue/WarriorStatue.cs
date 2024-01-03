using JadeFables.Helpers;
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
        public const int CONST_TileWidth = 5;
        public const int CONST_TileHeight = 7;
        public const short CONST_HeightPixels = CONST_TileHeight * 18; // 126

        public static bool IsGlowing(int i, int j) => Main.tile[i, j].TileFrameY >= CONST_HeightPixels;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.Width = CONST_TileWidth;
            TileObjectData.newTile.Height = CONST_TileHeight;
            TileObjectData.newTile.Origin = new Point16(0, 6);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16 };
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
            if (player.HasBuff(BuffID.Gills))
                return;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconText = "";
            player.cursorItemIconID = ItemID.GoldCoin;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            //for some reason this drops the item twice, the multitile is dropping it somewhere else for some reason
            //Item.NewItem(new EntitySource_TileBreak(i, j), (i + 3) * 16, (j + 5) * 16, 16, 32, ModContent.ItemType<WarriorStatueItem>());
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (player.HasBuff(BuffID.Gills))
                return false;
            int gillsCost = Item.gold;

            if (!player.CanAfford(gillsCost))
            {
                return false;
            }

            player.BuyItem(gillsCost);
            // 15 minute buff
            player.AddBuff(BuffID.Gills, 60 * 60 * 15, false);

            SoundEngine.PlaySound(SoundID.CoinPickup);
            for (int k = 0; k < 3; k++)
            {
                int dust = Dust.NewDust(player.HandPosition - new Vector2(8,0) ?? player.position, 16, 0, DustID.GoldCoin, 0, -2);
                Main.dust[dust].velocity.X = 0;
                Main.dust[dust].velocity.Y *= 0.3f;
            }
            ActivateGlow(i, j);
            return true;
        }
        public override void HitWire(int i, int j)
        {
            int newX;
            int newY;
            GetMultitileTopLeft(i, j, out newX, out newY);
            // Here we run SkipWire on all tiles so wire overlapping multiple tiles of the multitile doesn't cancel out
            Helper.ForTilesInRect(CONST_TileWidth, CONST_TileHeight, newX, newY, Wiring.SkipWire, topLeft: true);

            if (!IsGlowing(i, j))
                ActivateGlow(i, j, true);
            else
                DeactivateGlow(i, j);
        }
        public static void ActivateGlow(int i, int j, bool immortal = false)
        {
            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen);
            //SoundEngine.PlaySound(SoundID.Item155 with {PitchVariance = 0.125f});

            if (IsGlowing(i, j))
                return;
            Player player = Main.LocalPlayer;

            int newX;
            int newY;
            GetMultitileTopLeft(i, j, out newX, out newY);

            int glow = Projectile.NewProjectile(new EntitySource_TileInteraction(null, newX, newY), new Vector2(newX, newY) * 16, Vector2.Zero, ProjectileType<WarriorStatueProj>(), 0, 0);
            (Main.projectile[glow].ModProjectile as WarriorStatueProj).immortal = immortal;
            Helper.ForTilesInRect(CONST_TileWidth, CONST_TileHeight, newX, newY, (x, y) => Main.tile[x, y].TileFrameY += CONST_HeightPixels, topLeft: true); //might not work in MP?
        }
        public static void DeactivateGlow(int i, int j)
        {
            if (!IsGlowing(i, j))
                return;

            int newX;
            int newY;
            GetMultitileTopLeft(i, j, out newX, out newY);

            Projectile? glow = Main.projectile.FirstOrDefault(n => n.active && n.type == ProjectileType<WarriorStatueProj>() && n.Center == new Vector2(newX, newY) * 16);
            if (glow is not null) glow.timeLeft = 0;
            Helper.ForTilesInRect(CONST_TileWidth, CONST_TileHeight, newX, newY, (x, y) => Main.tile[x, y].TileFrameY -= CONST_HeightPixels, topLeft: true);
        }
        public static void GetMultitileTopLeft(int i, int j, out int newX, out int newY)
        {
            newX = i;
            newY = j;

            int tries = 0;
            while (Main.tile[newX, newY].TileFrameX > 0 && tries++ < 99)
            {
                newX--;
            }

            while (Main.tile[newX, newY].TileFrameY % CONST_HeightPixels > 0 && tries++ < 99)
            {
                newY--;
            }
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == CONST_HeightPixels && !Main.projectile.Any(n => n.active && n.type == ProjectileType<WarriorStatueProj>() && n.Center == new Vector2(i, j) * 16))
            {
                int glow = Projectile.NewProjectile(new EntitySource_TileInteraction(null, i, j), new Vector2(i, j) * 16, Vector2.Zero, ProjectileType<WarriorStatueProj>(), 0, 0);
                (Main.projectile[glow].ModProjectile as WarriorStatueProj).immortal = true; //This means if the player for some reason saves and exits within 5 seconds of activating the statue manually it would stay active the same as hitting a wire. This is lazy but it's only cosmetic and should rarely happen
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
        public bool immortal = false;

        public override void AI()
        {
            if (immortal && Projectile.timeLeft > 0) Projectile.timeLeft = 300;
            Lighting.AddLight(Projectile.Center + new Vector2(40, 50), Color.Teal.ToVector3() * 1.6f);
            if (Main.rand.NextBool(7))
            {
                Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.Next(10, 70), 0);
                Dust.NewDustPerfect(dustPos, DustType<Dusts.GlowLineFast>(), Vector2.UnitY * -4, 0, Color.Teal, 1.0f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            int i = (int)(Projectile.Center.X / 16);
            int j = (int)(Projectile.Center.Y / 16);
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileType == TileType<WarriorStatue>()) WarriorStatue.DeactivateGlow(i, j);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Request<Texture2D>(Texture).Value;
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
            Item.createTile = TileType<WarriorStatue>();
            Item.width = 10;
            Item.height = 24;


            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.White;
        }
    }
}