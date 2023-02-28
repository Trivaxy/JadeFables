using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using JadeFables.Dusts;
using System.Security.Cryptography.X509Certificates;

namespace JadeFables.Tiles.JadeWaterfall
{
    public class JadeWaterfallTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0); // Todo: make less annoying.
            //TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16};
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);
            MinPick = 999;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Spring Waterfall");
            AddMapEntry(new Color(207, 160, 118), name);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            var existingGrass = Main.projectile.Where(n => n.active && n.Center == new Vector2(i, j) * 16 && n.type == ModContent.ProjectileType<JadeWaterfallProj>()).FirstOrDefault();
            if (existingGrass == default)
            {
                Projectile.NewProjectile(new EntitySource_Misc("Jade Waterfall"), new Vector2(i, j) * 16, Vector2.Zero, ModContent.ProjectileType<JadeWaterfallProj>(), 0, 0);
            }
        }
    }

    public class JadeWaterfallItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Waterfall Bucket");
            Tooltip.SetDefault("Contains a small amount of waterfall\nCan be poured out");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = TileType<JadeWaterfallTile>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }

        public override bool CanUseItem(Player player)
        {
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile || !player.InInteractionRange(Player.tileTargetX, Player.tileTargetY))
                return false;
            return base.CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            if (Item.stack == 1)
            {
                Item.type = ItemID.EmptyBucket;
                Item.SetDefaults();
                return false;
            }
            else
            {
                Item.NewItem(Item.GetSource_ItemUse(Item), player.Center, ItemID.EmptyBucket);
            }
            return base.UseItem(player);
        }
    }

    public class JadeWaterfallProj : ModProjectile
    {
        Tile originLeft => Main.tile[(int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)];
        Tile originRight => Main.tile[(int)(Projectile.Center.X / 16) + 1, (int)(Projectile.Center.Y / 16)];

        public readonly int FADEOUTLENGTH = 7;

        int length = 0;

        int frame = 0;
        int yFrames = 6;

        public bool foundWater = false;

        public override void Load()
        {
            On.Terraria.Main.DrawPlayers_AfterProjectiles += Main_DrawPlayers_AfterProjectiles;
        }

        private void Main_DrawPlayers_AfterProjectiles(On.Terraria.Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
        {
            orig(self);
            {
                Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
                var toDraw = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<JadeWaterfallProj>()).ToList();
                toDraw.ForEach(n => (n.ModProjectile as JadeWaterfallProj).Draw());
                Main.spriteBatch.End();
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Waterfall");
        }

        public override void SetDefaults()
        {
            Projectile.knockBack = 6f;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 10;
            Projectile.hide = true;
        }


        public void Draw()
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D topTex = ModContent.Request<Texture2D>(Texture + "_Top").Value;
            Texture2D bottomTex = ModContent.Request<Texture2D>(Texture + "_Bottom").Value;
            int topFrameHeight = topTex.Height / yFrames;
            Rectangle topFrameBox = new Rectangle(0, (frame % yFrames) * topFrameHeight, topTex.Width, topFrameHeight);
            Color topColor = Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            Main.spriteBatch.Draw(topTex, Projectile.Center - Main.screenPosition, topFrameBox, topColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
            int i;
            for (i = 1; i < length; i++)
            {
                int tileHeight = 4;
                Vector2 pos = Projectile.Center + (Vector2.UnitY * 16 * i);
                int frameHeight = (tex.Height / yFrames) / tileHeight;
                Rectangle frameBox = new Rectangle(0, (tileHeight * frameHeight * ((((i - 1) / tileHeight) + frame) % yFrames)) + (frameHeight * ((i - 1) % tileHeight)), tex.Width, frameHeight);
                Color color = Lighting.GetColor((int)(pos.X / 16), (int)(pos.Y / 16));
                Main.spriteBatch.Draw(tex, pos - Main.screenPosition, frameBox, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }

            if (length == 120)
            {
                for (i = length; i < length + FADEOUTLENGTH; i++)
                {
                    int tileHeight = 4;
                    Vector2 pos = Projectile.Center + (Vector2.UnitY * 16 * i);
                    int frameHeight = (tex.Height / yFrames) / tileHeight;
                    Rectangle frameBox = new Rectangle(0, (tileHeight * frameHeight * ((((i - 1) / tileHeight) + frame) % yFrames)) + (frameHeight * ((i - 1) % tileHeight)), tex.Width, frameHeight);
                    Color color = Lighting.GetColor((int)(pos.X / 16), (int)(pos.Y / 16)) * (1 - ((i - length) / (float)FADEOUTLENGTH));
                    Main.spriteBatch.Draw(tex, pos - Main.screenPosition, frameBox, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
                }
            }

            if (!foundWater)
                return;
            int bottomFrameHeight = bottomTex.Height / yFrames;
            Rectangle bottomFrameBox = new Rectangle(0, ((i + frame) % yFrames) * bottomFrameHeight, bottomTex.Width, bottomFrameHeight);
            Vector2 bottomPos = Projectile.Center + (Vector2.UnitY * 16 * i) - new Vector2(16, 16);
            Color bottomColor = Lighting.GetColor((int)(bottomPos.X / 16), (int)(bottomPos.Y / 16));
            Main.spriteBatch.Draw(bottomTex, bottomPos - Main.screenPosition, bottomFrameBox, bottomColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
            return;
        }

        public override void AI()
        {
            if (Projectile.frameCounter++ % 4 == 0)
                frame++;
            int i = 0;
            for (i = 0; i < 120; i++)
            {
                foundWater = false;
                for (int j = 0; j < 2; j++)
                {
                    int x = (int)(Projectile.Center.X / 16) + j;
                    int y = (int)(Projectile.Center.Y / 16) + i;
                    Tile tile = Main.tile[x, y];
                    Lighting.AddLight(new Vector2(x * 16, y * 16), new Vector3(0, 220, 200) * 0.0030f);
                    if (tile.LiquidAmount == 255 && !tile.HasTile)
                    {
                        Vector2 velocity = Vector2.UnitY.RotatedByRandom(0.1f) * -Main.rand.NextFloat(1f, 1.5f);
                        Vector2 pos = new Vector2(Projectile.Center.X + (j * 16), Projectile.Center.Y + (i * 16)) + new Vector2(8,8) + Main.rand.NextVector2Circular(4, 12);
                        Dust.NewDustPerfect(pos, ModContent.DustType<WaterfallMist>(), velocity, 0, Color.White, Main.rand.NextFloat(0.025f, 0.225f));
                        foundWater = true;
                    }
                }
                if (foundWater)
                    break;
            }

            length = i;

            if (length == 120)
            {
                for (i = length; i < length + FADEOUTLENGTH; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int x = (int)(Projectile.Center.X / 16) + j;
                        int y = (int)(Projectile.Center.Y / 16) + i;
                        Tile tile = Main.tile[x, y];
                        Lighting.AddLight(new Vector2(x * 16, y * 16), new Vector3(0, 220, 200) * 0.0030f * (1 - ((i - length) / (float)FADEOUTLENGTH)));
                    }
                }
            }

            if (originLeft.HasTile && originLeft.TileType == ModContent.TileType<JadeWaterfallTile>())
                Projectile.timeLeft = 2;
        }
    }

    public class BucketObtainability : GlobalItem
    {
        public override bool? UseItem(Item item, Player player)
        {
            Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
            if (player.itemAnimation == item.useAnimation - 1 && item.type == ItemID.EmptyBucket && player.InInteractionRange(Player.tileTargetX, Player.tileTargetY) && tile.HasTile && tile.TileType == ModContent.TileType<JadeWaterfallTile>())
            {
                tile.HasTile = false;
                item.stack--;
                Item.NewItem(item.GetSource_ItemUse(item), player.Center, ModContent.ItemType<JadeWaterfallItem>());
            }
            
            return base.UseItem(item, player);
        }
    }
}
