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
using JadeFables.Tiles.JadeSand;
using JadeFables.Tiles.OvergrownJadeSand;
using JadeFables.Helpers;

namespace JadeFables.Tiles.JadeSeaweed
{
    public class JadeSeaweedTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            DustType = DustID.OasisCactus;
            HitSound = SoundID.Grass;
            MineResist = 0.9f;

            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0); // Todo: make less annoying.
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16};
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Spring Seaweed");
            AddMapEntry(jadeGrassLime, name);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            var existingSeaweed = Main.projectile.Where(n => n.active && n.Center == new Vector2(i, j) * 16 && n.type == ModContent.ProjectileType<JadeSeaweedProj>()).FirstOrDefault();
            if (existingSeaweed == default)
            {
                Projectile.NewProjectile(new EntitySource_Misc("Spring Seaweed"), new Vector2(i, j) * 16, Vector2.Zero, ModContent.ProjectileType<JadeSeaweedProj>(), 0, 0);
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);
            if (!tileBelow.HasTile || tileBelow.IsHalfBlock || tileBelow.TopSlope || (tileBelow.TileType != ModContent.TileType<Tiles.JadeSand.JadeSandTile>() && tileBelow.TileType != ModContent.TileType<Tiles.OvergrownJadeSand.OvergrownJadeSandTile>()))
                WorldGen.KillTile(i, j);
            return true;
        }
    }

    public class JadeSeaweedItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spring Seaweed");
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
            Item.createTile = TileType<JadeSeaweedTile>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }
    }

    public class JadeSeaweedProj : ModProjectile
    {
        public class JadeSeaweedBlade
        {
            public float originalRotation;

            public float rotation = -1.57f;

            public string texturePath;

            public float swaySpeed = 0.01f;

            public float sway;

            public int height;

            public float springiness = 0.02f;

            public Vector2 scale = Vector2.One;

            public Vector2 offset;

            public Tile origin;

            public void Draw(SpriteBatch spriteBatch, Color lightColor, Vector2 position)
            {
                Texture2D texRef = ModContent.Request<Texture2D>(texturePath).Value;
                PaintHelper.DrawWithPaint(origin.TileColor, texturePath, position + offset, null, lightColor, rotation + 1.57f, new Vector2(texRef.Width / 2, texRef.Height), scale, SpriteEffects.None, 0f);
            }

            public void Update(Vector2 startPoint)
            {
                sway += swaySpeed;
                startPoint += offset;
                Vector2 endPoint = startPoint + (rotation.ToRotationVector2() * height * scale.Y);
                var collider = Main.player.Where(n => n.active && !n.dead && Collision.CheckAABBvLineCollision(n.position, n.Hitbox.Size(), startPoint, endPoint)).FirstOrDefault();

                if (collider != default)
                {
                    rotation += collider.velocity.X * (collider.wet ? 0.5f : 1) * 0.019f * (1.57f - Math.Abs(originalRotation - rotation));
                    rotation = MathHelper.Clamp(rotation, -3.14f, 0f);
                }

                rotation = MathHelper.Lerp(rotation, originalRotation + (0.15f * (float)Math.Sin(sway)), springiness);
            }

            public JadeSeaweedBlade(Tile tile) { origin = tile; }
        }

        public List<JadeSeaweedBlade> blades = new List<JadeSeaweedBlade>();

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spring Seaweed");
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

        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 3; i++)
            {
                int texNum = Main.rand.Next(1, 9);
                JadeSeaweedBlade blade = new JadeSeaweedBlade(Main.tile[(int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)]);
                blade.originalRotation = Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f;
                blade.texturePath = Texture + texNum.ToString();
                switch (texNum)
                {
                    case 1:
                        blade.height = 40;
                        break;
                    case 2:
                        blade.height = 26;
                        break;
                    case 3:
                        blade.height = 14;
                        break;
                    case 4:
                        blade.height = 44;
                        break;
                    case 5:
                        blade.height = 32;
                        break;
                    case 6:
                        blade.height = 38;
                        break;
                    case 7:
                        blade.height = 18;
                        break;
                    case 8:
                        blade.height = 14;
                        break;
                }
                blade.springiness = Main.rand.NextFloat(0.01f, 0.03f);
                blade.offset = Main.rand.NextVector2Circular(16, 4) + new Vector2(8, 20);
                blade.scale = Vector2.One;
                blade.swaySpeed = Main.rand.NextFloat(0.01f, 0.03f);
                blades.Add(blade);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color color = lightColor;
            blades.ForEach(n => n.Draw(Main.spriteBatch, color, Projectile.Center - Main.screenPosition));
            return false;
        }

        public override void AI()
        {
            blades.ForEach(n => n.Update(Projectile.Center));
            Projectile.velocity = Vector2.Zero;

            Tile tile = Main.tile[(int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)];
            if (tile.HasTile && tile.TileType == ModContent.TileType<JadeSeaweedTile>())
                Projectile.timeLeft = 2;
        }
    }
}
