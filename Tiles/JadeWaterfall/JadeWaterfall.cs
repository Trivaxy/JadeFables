using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

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
            DisplayName.SetDefault("Spring Waterfall");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = TileType<JadeWaterfallTile>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }
    }

    public class JadeWaterfallProj : ModProjectile
    {
        Tile originLeft => Main.tile[(int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)];
        Tile originRight => Main.tile[(int)(Projectile.Center.X / 16) + 1, (int)(Projectile.Center.Y / 16)];

        int length = 0;

        int frame = 0;
        int yFrames = 3;
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
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            for (int i = 0; i < length; i++)
            {
                Vector2 pos = Projectile.Center + (Vector2.UnitY * 16 * i);
                int frameHeight = tex.Height / yFrames;
                Rectangle frameBox = new Rectangle(0, frameHeight * ((i + frame) % yFrames), tex.Width, frameHeight);
                Color color = Lighting.GetColor((int)(pos.X / 16), (int)(pos.Y / 16)) * 0.75f;
                Main.spriteBatch.Draw(tex, pos - Main.screenPosition, frameBox, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void AI()
        {
            if (Projectile.frameCounter++ % 4 == 0)
                frame++;
            int i = 0;
            for (i = 0; i < 60; i++)
            {
                bool foundWater = false;
                for (int j = 0; j < 2; j++)
                {
                    Tile tile = Main.tile[(int)(Projectile.Center.X / 16) + j, (int)(Projectile.Center.Y / 16) + i];
                    if (tile.LiquidAmount > 0 && !tile.HasTile)
                        foundWater = true;
                }
                if (foundWater)
                    break;
            }
            length = i;
            if (originLeft.HasTile)
                Projectile.timeLeft = 2;
        }
    }
}
