using JadeFables.Dusts;
using JadeFables.Tiles.JadeGrassShort;
using JadeFables.Tiles.JadeSandstone;
using JadeFables.Tiles.JasmineFlower;

namespace JadeFables.Tiles.JadeSand
{
    public class JadeSandTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            MinPick = 10;
            DustType = DustType<Dusts.JadeSandDust>();
            HitSound = SoundID.Dig;
            ItemDrop = ItemType<JadeSandItem>();
            Main.tileMerge[TileID.Stone][Type] = true;
            Main.tileBrick[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileBlockLight[Type] = true;
            //Main.tileSand[Type] = true;
            TileID.Sets.TouchDamageSands[Type] = 15;
            TileID.Sets.Conversion.Sand[Type] = true;
            TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
            //TileID.Sets.Falling[Type] = true;

            TileSets.CanGrowBamboo[Type] = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Spring Sand");
            AddMapEntry(new Color(207, 160, 118), name);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {

            return true;

            if (WorldGen.noTileActions)
                return true;

            Tile above = Main.tile[i, j - 1];
            Tile below = Main.tile[i, j + 1];
            bool canFall = true;

            if (below == null || below.HasTile)
                canFall = false;

            if (above.HasTile && (TileID.Sets.BasicChest[above.TileType] || TileID.Sets.BasicChestFake[above.TileType] || above.TileType == TileID.PalmTree || TileID.Sets.BasicDresser[above.TileType]))
                canFall = false;

            if (canFall)
            {
                int projectileType = ProjectileType<JadeSandProjectile>();
                float positionX = i * 16 + 8;
                float positionY = j * 16 + 8;

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.tile[i, j].ClearTile();
                    int proj = Projectile.NewProjectile(null, positionX, positionY, 0f, 0.41f, projectileType, 10, 0f, Main.myPlayer);
                    Main.projectile[proj].ai[0] = 1f;
                    WorldGen.SquareTileFrame(i, j);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    Main.tile[i, j].ClearTile();
                    bool spawnProj = true;

                    for (int k = 0; k < 1000; k++)
                    {
                        Projectile otherProj = Main.projectile[k];

                        if (otherProj.active && otherProj.owner == Main.myPlayer && otherProj.type == projectileType && Math.Abs(otherProj.timeLeft - 3600) < 60 && otherProj.Distance(new Vector2(positionX, positionY)) < 4f)
                        {
                            spawnProj = false;
                            break;
                        }
                    }

                    if (spawnProj)
                    {
                        int proj = Projectile.NewProjectile(null, positionX, positionY, 0f, 2.5f, projectileType, 10, 0f, Main.myPlayer);
                        Main.projectile[proj].velocity.Y = 0.5f;
                        Main.projectile[proj].position.Y += 2f;
                        Main.projectile[proj].netUpdate = true;
                    }

                    NetMessage.SendTileSquare(-1, i, j, 1);
                    WorldGen.SquareTileFrame(i, j);
                }
                return false;
            }
            return true;
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            Tile tileBelow = Framing.GetTileSafely(i, j + 1);
            Tile tileAbove = Framing.GetTileSafely(i, j - 1);
            Tile tileAbove2 = Framing.GetTileSafely(i, j - 2);

            //try place foliage
            if (WorldGen.genRand.NextBool(25) && !tileAbove.HasTile && !(tileBelow.LiquidType == LiquidID.Lava))
            {
                if (!tile.BottomSlope && !tile.TopSlope && !tile.IsHalfBlock && !tile.TopSlope)
                {
                    tileAbove.HasTile = true;
                    tileAbove.TileFrameY = 0;
                    if (Main.rand.NextBool(50))
                    {
                        tileAbove.TileType = (ushort)ModContent.TileType<JasmineFlowerTile>();
                        tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(3) * 18);
                    }
                    else if (!tileAbove2.HasTile && Main.rand.NextBool(2))
                    {
                        short tileFrame = (short)(WorldGen.genRand.Next(6) * 18);
                        WorldGen.PlaceTile(i, j - 1, ModContent.TileType<JadeGrassTall>());
                        tileAbove.TileFrameX = tileFrame;
                        tileAbove2.TileFrameX = tileFrame;
                    }
                    else
                    {
                        tileAbove.TileType = (ushort)ModContent.TileType<JadeGrassShort.JadeGrassShort>();
                        tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(6) * 18);
                    }
                    WorldGen.SquareTileFrame(i, j + 1, true);
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
                }
            }
        }
    }

    public class JadeSandItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spring Sand");
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
            Item.createTile = TileType<JadeSandTile>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }
    }

    class JadeSandProjectile : ModProjectile
    {
        protected bool falling = true;
        protected int tileType;
        protected int dustType;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Example Sand Ball");
            ProjectileID.Sets.ForcePlateDetection[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.knockBack = 6f;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.penetrate = -1;

            tileType = TileType<JadeSandTile>();
            dustType = DustType<JadeSandDust>();
        }

        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
                Main.dust[dust].velocity.X *= 0.4f;
            }

            Projectile.tileCollide = true;
            Projectile.localAI[1] = 0f;

            if (Projectile.ai[0] == 1f)
            {
                if (!falling)
                {
                    Projectile.ai[1] += 1f;

                    if (Projectile.ai[1] >= 60f)
                    {
                        Projectile.ai[1] = 60f;
                        Projectile.velocity.Y += 0.2f;
                    }
                }
                else
                    Projectile.velocity.Y += 0.41f;
            }
            else if (Projectile.ai[0] == 2f)
            {
                Projectile.velocity.Y += 0.2f;

                if (Projectile.velocity.X < -0.04f)
                    Projectile.velocity.X += 0.04f;
                else if (Projectile.velocity.X > 0.04f)
                    Projectile.velocity.X -= 0.04f;
                else
                    Projectile.velocity.X = 0f;
            }

            Projectile.rotation += 0.1f;

            if (Projectile.velocity.Y > 10f)
                Projectile.velocity.Y = 10f;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            if (falling)
                Projectile.velocity = Collision.AnyCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true);
            else
                Projectile.velocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, fallThrough, fallThrough, 1);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer && !Projectile.noDropItem)
            {
                int tileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
                int tileY = (int)(Projectile.position.Y + Projectile.width / 2) / 16;

                Tile tile = Main.tile[tileX, tileY];
                Tile tileBelow = Main.tile[tileX, tileY + 1];

                if (tile.BlockType == BlockType.HalfBlock && Projectile.velocity.Y > 0f && System.Math.Abs(Projectile.velocity.Y) > System.Math.Abs(Projectile.velocity.X))
                    tileY--;

                if (!tile.HasTile)
                {
                    bool onMinecartTrack = tileY < Main.maxTilesY - 2 && tileBelow != null && tileBelow.HasTile && tileBelow.TileType == TileID.MinecartTrack;

                    if (!onMinecartTrack)
                        WorldGen.PlaceTile(tileX, tileY, tileType, false, true);

                    if (!onMinecartTrack && tile.HasTile && tile.TileType == tileType)
                    {
                        if (tileBelow.BlockType == BlockType.HalfBlock || tileBelow.Slope != 0)
                        {
                            WorldGen.SlopeTile(tileX, tileY + 1, 0);

                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, tileX, tileY + 1);
                        }

                        if (Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, tileX, tileY, tileType);
                    }
                }
            }
        }

        public override bool? CanDamage() => Projectile.localAI[1] != -1f;
    }
}
