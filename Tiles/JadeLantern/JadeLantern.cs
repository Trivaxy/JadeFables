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
using JadeFables.Core;
using Steamworks;
using JadeFables.Tiles.JadeTorch;

namespace JadeFables.Tiles.JadeLantern
{
    public class JadeLantern : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0); // Todo: make less annoying.
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Spring Lantern");
            AddMapEntry(new Color(207, 160, 118), name);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            var existingLantern = Main.projectile.Where(n => n.active && n.Center == new Vector2(i, j) * 16 && n.type == ModContent.ProjectileType<JadeLanternProj>()).FirstOrDefault();
            if (existingLantern == default)
            {
                Projectile.NewProjectile(new EntitySource_Misc("Jade Lantern"), new Vector2(i, j) * 16, Vector2.Zero, ModContent.ProjectileType<JadeLanternProj>(), 0, 0);
            }
        }
    }

    public class JadeLanternFurniture : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0); // Todo: make less annoying.
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);
            ItemDrop = ModContent.ItemType<JadeLanternItem>();
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Spring Lantern");
            AddMapEntry(new Color(207, 160, 118), name);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            var existingLantern = Main.projectile.Where(n => n.active && n.Center == new Vector2(i, j) * 16 && n.type == ModContent.ProjectileType<JadeLanternProjFurniture>()).FirstOrDefault();
            if (existingLantern == default)
            {
                Projectile.NewProjectile(new EntitySource_Misc("Jade Lantern"), new Vector2(i, j) * 16, Vector2.Zero, ModContent.ProjectileType<JadeLanternProjFurniture>(), 0, 0);
            }
        }
    }

    public class JadeLanternItemDebug : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spring Lantern DEBUG");
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
            Item.createTile = TileType<JadeLantern>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }
    }

    public class JadeLanternItem : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spring Lantern");
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
            Item.createTile = TileType<JadeLanternFurniture>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }
    }

    public class JadeLanternProj : ModProjectile
    {

        public bool burning = false;

        public bool burnable => chainFrame.Y == 0;

        public int burnTimer = 0;

        public int burnedSegments = 0;
        public VerletChain chain;

        public Rectangle hitbox { get
            {
                RopeSegment seg = chain.ropeSegments[chain.segmentCount - 1];

                return new Rectangle((int)seg.posNow.X - 16, (int)seg.posNow.Y - 16, 32, 32);
            } 
        }

        private Rectangle chainFrame;

        private Rectangle lanternFrame;

        private Rectangle pivotFrame;

        public override void Load()
        {
            for (int j = 1; j <= 3; j++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "JadeFables/Tiles/JadeLantern/JadeChainGore" + j);

            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "JadeFables/Tiles/JadeLantern/Gores/lantern" + i + "gore" + j);
                }
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Lantern");
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
            chain = new VerletChain(Main.rand.Next(10, 15), true, Projectile.Center, 14, true);
            chain.Start();
            chain.forceGravity = new Vector2(0, 0.4f);

            int chainMat = Main.rand.Next(3);
            chainFrame = new Rectangle(0, 22 * chainMat, 12, 22);
            pivotFrame = new Rectangle(0, 10 * chainMat, 14, 10);
            lanternFrame = new Rectangle(0, 32 * Main.rand.Next(4), 32, 32);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (chain == null)
                return false;
            Texture2D chainTex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            Texture2D pivotTex = ModContent.Request<Texture2D>(Texture + "_Pivot").Value;
            Texture2D lanternTex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D backTex = ModContent.Request<Texture2D>(Texture + "_BackGlow").Value;

            Color glowColor = Color.Orange;
            glowColor.A = 0;

            RopeSegment seg = chain.ropeSegments[chain.segmentCount - 2];
            RopeSegment nextSeg = chain.ropeSegments[chain.segmentCount - 1];

            if (!burning)
            {
                Main.spriteBatch.Draw(backTex, nextSeg.posNow - Main.screenPosition, null, glowColor * 0.4f, 0, backTex.Size() / 2, 0.7f, SpriteEffects.None, 0f);
            }

            for (int i = (chain.segmentCount - 2) - burnedSegments; i >= 0; i--)
            {
                RopeSegment segInner = chain.ropeSegments[i];
                RopeSegment nextSegInner = chain.ropeSegments[i + 1];
                Main.spriteBatch.Draw(chainTex, segInner.posNow - Main.screenPosition, chainFrame, Lighting.GetColor((int)(segInner.posNow.X / 16), (int)(segInner.posNow.Y / 16)), segInner.posNow.DirectionTo(nextSegInner.posNow).ToRotation() + 1.57f, chainFrame.Size() / 2, 1, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(pivotTex, (Projectile.Center - new Vector2(0, 6)) - Main.screenPosition, pivotFrame, lightColor, 0, pivotFrame.Size() / 2, 1, SpriteEffects.None, 0f);

            if (!burning)
            {
                Main.spriteBatch.Draw(lanternTex, nextSeg.posNow - Main.screenPosition, lanternFrame, Lighting.GetColor((int)(nextSeg.posNow.X / 16), (int)(nextSeg.posNow.Y / 16)), seg.posNow.DirectionTo(nextSeg.posNow).ToRotation() - 1.57f, lanternFrame.Size() / 2, 1, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glowTex, nextSeg.posNow - Main.screenPosition, lanternFrame, glowColor * 0.6f, seg.posNow.DirectionTo(nextSeg.posNow).ToRotation() - 1.57f, lanternFrame.Size() / 2, 1, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void AI()
        {
            Tile tile = Main.tile[(int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)];
            if (tile.HasTile && tile.TileType == ModContent.TileType<JadeLantern>())
                Projectile.timeLeft = 2;

            chain.UpdateChain();
            for (int i = 0; i < chain.segmentCount - burnedSegments; i++)
            {
                RopeSegment segment = chain.ropeSegments[i];
                if (Collision.CheckAABBvAABBCollision(Main.LocalPlayer.TopLeft, Main.LocalPlayer.Hitbox.Size(), segment.posNow - new Vector2(6,6), new Vector2(12,12)))
                {
                    segment.posNow.X += Main.LocalPlayer.velocity.X * 0.2f;
                }
            }

            if (!burning)
            {
                RopeSegment seg = chain.ropeSegments[chain.segmentCount - 1];

                Lighting.AddLight(seg.posNow, Color.Orange.ToVector3() * 0.6f);
                Rectangle hitbox = new Rectangle((int)seg.posNow.X - 16, (int)seg.posNow.Y - 16, 32, 32);
                if (Main.projectile.Any(n => n.active && n.friendly && n.Hitbox.Intersects(hitbox)))
                {
                    Break();
                }

                if (Main.rand.NextBool(60))
                {
                    Dust.NewDustPerfect(seg.posNow + Main.rand.NextVector2Circular(6, 6), ModContent.DustType<LanternGlow>(), Main.rand.NextVector2Circular(1, 1), 0, Color.OrangeRed, Main.rand.NextFloat(0.35f, 0.55f));
                }
            }
            else
            {
                burnTimer++;
                if (burnTimer > 3)
                {
                    burnTimer = 0;
                    RopeSegment burnSegment = chain.ropeSegments[(chain.segmentCount - 1) - burnedSegments];

                    burnedSegments++;
                    if (burnedSegments >= chain.segmentCount - 1)
                    {
                        tile.HasTile = false;
                        Projectile.active = false;
                    }

                    for (int i = 0; i < 2; i++)
                        Dust.NewDustPerfect(burnSegment.posNow + new Vector2(0, -4) + Main.rand.NextVector2Circular(6, 6), ModContent.DustType<LanternGlow>(), Main.rand.NextVector2Circular(1.3f, 1.3f), 0, Color.OrangeRed, Main.rand.NextFloat(0.45f, 0.55f));
                }
            }

        }

        public virtual void Break()
        {
            Tile tile = Main.tile[(int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)];
            RopeSegment seg = chain.ropeSegments[chain.segmentCount - 1];
            if (!burnable)
            {
                foreach (RopeSegment segment in chain.ropeSegments)
                {
                    Gore.NewGoreDirect(Projectile.GetSource_Death(), segment.posNow, Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("JadeChainGore" + ((chainFrame.Y / 22) + 1)).Type);
                }
                Projectile.active = false;
                tile.HasTile = false;
            }
            else
                burning = true;

            Helpers.Helper.PlayPitched("LanternBreak", 0.6f, Main.rand.NextFloat(-0.1f, 0.1f), seg.posNow);

            for (int i = 1; i <= 4; i++)
            {
                Gore.NewGoreDirect(Projectile.GetSource_Death(), seg.posNow, Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("lantern" + ((lanternFrame.Y / 32) + 1) + "gore" + i).Type);
            }
            for (int i = 0; i < 5; i++)
                Dust.NewDustPerfect(seg.posNow + Main.rand.NextVector2Circular(12, 12), ModContent.DustType<LanternGlow>(), Main.rand.NextVector2Circular(3, 3), 0, Color.OrangeRed, Main.rand.NextFloat(0.85f, 1.15f));

            for (int i = 0; i < 13; i++)
                Dust.NewDustPerfect(seg.posNow + Main.rand.NextVector2Circular(12, 12), DustID.Torch, Main.rand.NextVector2Circular(3, 3), default, default, Main.rand.NextFloat(1,1.3f));

            Loot();
        }

        public void Loot()
        {
            RopeSegment seg = chain.ropeSegments[chain.segmentCount - 1];
            int i = (int)(seg.posNow.X / 16);
            int j = (int)(seg.posNow.Y / 16);
            EntitySource_TileBreak source = new((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            if (Main.rand.NextBool(250))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(source, (i + 1.5f) * 16f, j * 16f, 0f, 0f, ProjectileID.CoinPortal, 0, 0, Main.myPlayer);
            }
            else
            {
                if (Main.expertMode ? Main.rand.Next(45) < 2 : Main.rand.NextBool(45))
                {
                    switch (Main.rand.Next(10))
                    {
                        case 0:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.BattlePotion);
                            break;
                        case 1:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.EndurancePotion);
                            break;
                        case 2:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.InvisibilityPotion);
                            break;
                        case 3:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.ManaRegenerationPotion);
                            break;
                        case 4:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.MagicPowerPotion);
                            break;
                        case 5:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.TitanPotion);
                            break;
                        case 6:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.WrathPotion);
                            break;
                    }
                }
                else
                {
                    switch (Main.rand.Next(7))
                    {
                        case 0:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.Heart);
                            if (Main.rand.NextBool(2))
                                Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.Heart);
                            break;
                        case 1:
                            if (Main.tile[i, j].LiquidAmount == 255 && Main.tile[i, j].LiquidType == LiquidID.Water)
                                Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.SpelunkerGlowstick, Main.rand.Next(Main.expertMode ? 5 : 4, Main.expertMode ? 18 : 12));
                            else
                                Item.NewItem(source, i * 16, j * 16, 32, 16, ModContent.ItemType<Tiles.JadeTorch.JadeTorch>(), Main.rand.Next(Main.expertMode ? 5 : 4, Main.expertMode ? 18 : 12));
                            break;
                        case 2:
                            goto case 5;
                        case 3:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.HealingPotion);
                            if (Main.rand.NextBool(3)) { Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.HealingPotion); }
                            break;
                        case 4:
                            Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.Bomb, Main.rand.Next(1, Main.expertMode ? 7 : 4));
                            break;
                        case 5:
                            for (int k = 0; k < Main.rand.Next(1, 4); k++)
                            {
                                if (Main.rand.NextBool(2)) { Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.CopperCoin, Main.rand.Next(1, 99)); }
                            }
                            for (int k = 0; k < Main.rand.Next(1, 4); k++)
                            {
                                if (Main.rand.NextBool(2)) { Item.NewItem(source, i * 16, j * 16, 32, 16, ItemID.SilverCoin, Main.rand.Next(1, 3)); }
                            }
                            break;
                        case 6:
                            goto case 5;
                    }
                }
            }
        }
    }

    public class JadeLanternProjFurniture : JadeLanternProj
    {
        public override string Texture => "JadeFables/Tiles/JadeLantern/JadeLanternProj";

        public override void Load()
        {
            
        }

        public override void Break()
        {
            
        }
    }

    public class BreakJadeLanterns : GlobalItem
    {
        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (item.damage > 0 && !noHitbox)
            {
                Rectangle hitboxLocal = hitbox;
                var breakable = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<JadeLanternProj>() && hitboxLocal.Intersects((n.ModProjectile as JadeLanternProj).hitbox) && !(n.ModProjectile as JadeLanternProj).burning).ToArray();
                foreach (Projectile proj in breakable)
                {
                    (proj.ModProjectile as JadeLanternProj).Break();
                }
            }
        }
    }

    class SkeletonMerchantSellsLanterns : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.SkeletonMerchant && Main.moonPhase > 2 && Main.moonPhase < 5)
            {
                shop.item[nextSlot].SetDefaults(ModContent.ItemType<JadeLanternItem>());
                nextSlot++;
            }
        }
    }
}
