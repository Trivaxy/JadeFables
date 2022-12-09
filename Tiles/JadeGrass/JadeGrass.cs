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

namespace JadeFables.Tiles.JadeGrass
{
    public class JadeGrassTile : ModTile
    {
        public override void SetStaticDefaults()
        {
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
            name.SetDefault("Spring Grass");
            AddMapEntry(new Color(207, 160, 118), name);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            var existingGrass = Main.projectile.Where(n => n.active && n.Center == new Vector2(i, j) * 16 && n.type == ModContent.ProjectileType<JadeGrassProj>()).FirstOrDefault();
            if (existingGrass == default)
            {
                Projectile.NewProjectile(new EntitySource_Misc("Jade Grass"), new Vector2(i, j) * 16, Vector2.Zero, ModContent.ProjectileType<JadeGrassProj>(), 0, 0);
            }
        }
    }

    public class JadeGrassItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spring Grass");
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
            Item.createTile = TileType<JadeGrassTile>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }
    }

    public class JadeGrassProj : ModProjectile
    {
        public class JadeGrassBlade
        {
            public float originalRotation;

            public float rotation = -1.57f;

            public string texturePath;

            public int height;

            public Vector2 scale = Vector2.One;

            public Vector2 offset;

            public void Draw(SpriteBatch spriteBatch, Color lightColor, Vector2 position)
            {
                Texture2D tex = ModContent.Request<Texture2D>(texturePath).Value;
                spriteBatch.Draw(tex, position + offset, null, lightColor, rotation + 1.57f, new Vector2(tex.Width / 2, tex.Height), scale, SpriteEffects.None, 0f);
            }

            public void Update(Vector2 startPoint)
            {
                startPoint += offset;
                Vector2 endPoint = startPoint + (rotation.ToRotationVector2() * height * scale.Y);
                var collider = Main.player.Where(n => n.active && !n.dead && Collision.CheckAABBvLineCollision(n.position, n.Hitbox.Size(), startPoint, endPoint)).FirstOrDefault();

                if (collider != default)
                {
                    rotation += collider.velocity.X * 0.019f * Main.rand.NextFloat(0.75f, 1.25f);
                    rotation = MathHelper.Clamp(rotation, -3.14f, 0f);
                }

                rotation = MathHelper.Lerp(rotation, originalRotation, 0.02f);
            }

            public JadeGrassBlade() { }
        }

        public List<JadeGrassBlade> blades = new List<JadeGrassBlade>();

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Grass");
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
                JadeGrassBlade blade = new JadeGrassBlade();
                blade.originalRotation = Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f;
                blade.texturePath = Texture;
                blade.height = 32;
                blade.offset = Main.rand.NextVector2Circular(16, 4) + new Vector2(8, 20);
                blade.scale = new Vector2(Main.rand.NextFloat(0.85f, 1.15f), Main.rand.NextFloat(0.3f, 1.6f));
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
            
            if (Main.tile[(int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)].HasTile)
                Projectile.timeLeft = 2;
        }
    }
}
