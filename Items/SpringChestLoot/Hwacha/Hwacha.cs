using JadeFables.Core;
using JadeFables.Helpers;
using JadeFables.Tiles.JadeWaterfall;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.In;

namespace JadeFables.Items.SpringChestLoot.Hwacha
{
    public class Hwacha : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.QueenSpiderStaff);
            Item.damage = 13;
            Item.mana = 12;
            Item.width = 40;
            Item.height = 40;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Blue;
            Item.knockBack = 2.5f;
            Item.UseSound = SoundID.Item25;
            Item.shoot = ModContent.ProjectileType<HwachaProj>();
            Item.shootSpeed = 0f;
        }

        public override bool CanUseItem(Player player)
        {
            player.FindSentryRestingSpot(Item.shoot, out int worldX, out int worldY, out _);
            worldX /= 16;
            worldY /= 16;
            worldY--;
            return !WorldGen.SolidTile(worldX, worldY);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.FindSentryRestingSpot(type, out int worldX, out int worldY, out int pushYUp);
            var proj = Projectile.NewProjectileDirect(source, new Vector2(worldX, worldY) - new Vector2(0, 34), velocity, type, damage, knockback, player.whoAmI);
            proj.originalDamage = Item.damage;
            player.UpdateMaxTurrets();
            return false;
        }
    }

    public class HwachaProj : ModProjectile
    {
        int arrowTimer = 0;

        int direction = 1;

        int arrows => arrowTimer / 60;

        bool pulling;
        float pullOffsetX;

        Vector2 arcDir = new Vector2(1, 0);

        Player owner => Main.player[Projectile.owner];

        private float shakeVal = 0;

        private float xDistanceTravelled;
        private int wheelFrame;

        private int specialArrowTimer = 0;

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
                for (int j = 1; j <= 5; j++)
                    GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "JadeFables/Items/SpringChestLoot/Hwacha/HwachaProj_Gore" + j);
            Terraria.On_Main.DrawPlayers_AfterProjectiles += DrawFrontWheels;
        }

        private void DrawFrontWheels(Terraria.On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
        {
            orig(self);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            var toDraw = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<HwachaProj>()).ToList();
            toDraw.ForEach(n => (n.ModProjectile as HwachaProj).DrawFrontWheel());
            Main.spriteBatch.End();
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 48;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.sentry = true;
            Projectile.ignoreWater = true;
            Projectile.sentry = true;
        }

        public override void AI()
        {
            if (arrows < 5)
            {
                arrowTimer++;
                Item fakeItem = new Item(ItemID.WoodenBow);
                owner.PickAmmo(fakeItem, out int projToShoot, out float speed, out int damage, out float knockBack, out int usedAmmoItemID, true);
                if (projToShoot == ProjectileID.HolyArrow || projToShoot == ProjectileID.HellfireArrow || projToShoot == ProjectileID.UnholyArrow || projToShoot == ProjectileID.JestersArrow)
                {
                    specialArrowTimer++;
                }
                if (specialArrowTimer >= 5)
                {
                    specialArrowTimer = 0;
                    arrowTimer--;
                }
            }

            if (shakeVal > 0)
            {
                shakeVal -= 0.05f;
            }
            else
            {
                shakeVal = 0;
            }

            Projectile.velocity.Y = 5;
            Projectile.frame = (5 - arrows);

            NPC target = Main.npc.Where(n => n.active && n.CanBeChasedBy(Projectile) && n.Distance(Projectile.Center) < 800).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();

            if (owner.HasMinionAttackTargetNPC)
                target = Main.npc[owner.MinionAttackTargetNPC];

            if (target != default)
            {
                arcDir = ArcVelocityHelper.GetArcVel(Projectile.Center - new Vector2(0, 20), target.Center, 0.08f, 0, 200, 12);
                float rotDifference = ((((arcDir.ToRotation() - Projectile.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.rotation + rotDifference, 0.08f);

                direction = Math.Sign(Projectile.rotation.ToRotationVector2().X);
            }

            if (owner == Main.LocalPlayer && Main.mouseRight && owner.Distance(Projectile.Center) < (pulling ? 60 : 30))
            {
                if (!pulling)
                {
                    pulling = true;
                    pullOffsetX = Projectile.position.X - owner.Center.X;
                }
                owner.velocity.X *= 0.9f;
                xDistanceTravelled += Math.Abs(Projectile.position.X - (owner.Center.X + pullOffsetX));
                if (xDistanceTravelled > 8)
                {
                    xDistanceTravelled = 0;
                    wheelFrame++;
                    wheelFrame %= 4;
                }
                Projectile.position.X = owner.Center.X + pullOffsetX;
                if (owner.itemAnimation <= 0)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, owner.DirectionTo(Projectile.Center).ToRotation() - 1.57f);
                owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, owner.DirectionTo(Projectile.Center).ToRotation() - 1.57f);
                float stepupSpeed = 5;
                Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref stepupSpeed, ref Projectile.gfxOffY);

                owner.RemoveAllGrapplingHooks();
            }
            else
                pulling = false;

            if (arrows >= 1)
            {
                shakeVal = 1;
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    Projectile proj = Main.projectile[i];

                    if (proj == null || !proj.active || proj.damage == 0 || !ProjectileID.Sets.IsAWhip[proj.type])
                        continue;

                    int timeToFlyOut = Main.player[proj.owner].itemAnimationMax * proj.MaxUpdates;
                    if (proj.ai[0] < timeToFlyOut / 2)
                        continue;

                    ModProjectile modProj = proj.ModProjectile;

                    bool colliding = false;

                    for (int n = 0; n < proj.WhipPointsForCollision.Count; n++)
                    {
                        var point = proj.WhipPointsForCollision[n].ToPoint();
                        var myRect = new Rectangle(0, 0, proj.width, proj.height);
                        myRect.Location = new Point(point.X - myRect.Width / 2, point.Y - myRect.Height / 2);

                        if (myRect.Intersects(Projectile.Hitbox))
                        {
                            colliding = true;
                            break;
                        }
                    }

                    if (colliding)
                    {
                        SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
                        for (int arr = 0; arr < arrows * 3; arr++)
                        {
                            Item fakeItem = new Item(ItemID.WoodenBow);
                            owner.PickAmmo(fakeItem, out int projToShoot, out float speed, out int damage, out float knockBack, out int usedAmmoItemID);
                            if (projToShoot <= 0)
                            {
                                damage = 6;
                                speed = 8;
                                projToShoot = ModContent.ProjectileType<HwachaArrow>();
                                knockBack = Projectile.knockBack;
                            }
                            Vector2 arrowVel = Vector2.Normalize(arcDir.RotatedByRandom(0.15f)) * Main.rand.NextFloat(0.85f, 1.15f) * MathHelper.Max(arcDir.Length(), speed);
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center - new Vector2(0, 20) + Main.rand.NextVector2Circular(4, 4), arrowVel, projToShoot, Projectile.damage + damage, knockBack, Projectile.owner);
                        }
                        arrowTimer = 0;
                        break;
                    }
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Helpers.Helper.PlayPitched("HwachaBreak", 0.6f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);
            for (int i = 1; i <= 5; i++)
                Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(1, 1), Mod.Find<ModGore>("HwachaProj_Gore" + i).Type);
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D backWheel = ModContent.Request<Texture2D>(Texture + "_Backwheel").Value;
            Texture2D arrowTex = ModContent.Request<Texture2D>(Texture + "_Arrows").Value;
            int frameHeight = tex.Height;

            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            Vector2 origin = new Vector2(22, 42);
            if (direction == 1)
                origin.X = tex.Width - origin.X;

            float rotation = Projectile.rotation - (direction == -1 ? 3.14f : 0) + (MathF.Sin(shakeVal * 12.56f) * 0.2f * (shakeVal));

            Main.spriteBatch.Draw(backWheel, Projectile.position + origin + new Vector2(0, Projectile.gfxOffY) - Main.screenPosition, new Rectangle(0, backWheel.Height / 4 * wheelFrame, backWheel.Width, backWheel.Height / 4), lightColor, 0, origin, Projectile.scale, direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            for (int i = arrows + 2; i >= 0; i--)
            {
                frame = new Rectangle(0, frameHeight * (Main.projFrames[Projectile.type] - i), tex.Width, frameHeight);
                float progress = 1;
                if (i == arrows + 2)
                {
                    progress = (arrowTimer % 60) / 60f;
                }
                Vector2 offset = rotation.ToRotationVector2() * -15 * direction * (1 - progress);
                Main.spriteBatch.Draw(arrowTex, Projectile.position + origin + offset + new Vector2(0, Projectile.gfxOffY) - Main.screenPosition, frame, lightColor * EaseFunction.EaseCircularIn.Ease(progress), rotation, origin, Projectile.scale, direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(tex, Projectile.position + origin + new Vector2(0, Projectile.gfxOffY) - Main.screenPosition, null, lightColor, rotation, origin, Projectile.scale, direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            return false;
        }

        public void DrawFrontWheel()
        {
            Color lightColor = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);
            Texture2D frontWheel = ModContent.Request<Texture2D>(Texture + "_Frontwheel").Value;
            int frameHeight = frontWheel.Height / 4;

            Rectangle frame = new Rectangle(0, frameHeight * wheelFrame, frontWheel.Width, frameHeight);

            Vector2 origin = new Vector2(22, 42);
            if (direction == 1)
                origin.X = frontWheel.Width - origin.X;

            Main.spriteBatch.Draw(frontWheel, Projectile.position + origin + new Vector2(0, Projectile.gfxOffY) - Main.screenPosition, frame, lightColor, 0, origin, Projectile.scale, direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
    }

    public class HwachaArrow : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.penetrate = 1;

            Projectile.aiStyle = 1;
            AIType = ProjectileID.WoodenArrowFriendly;

            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = true;
        }

        public override void AI()
        {

        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            for (int i = 0; i < 2; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture);
            }
        }
    }
}
