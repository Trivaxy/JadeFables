//TODO:
//Sellprice
//Balance
//Make it work with manual targetting
//Obtainability
//Add More accurate firing
//Add push pull mechanic
using JadeFables.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hwacha");
			Tooltip.SetDefault("update later");
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.QueenSpiderStaff);
			Item.damage = 20;
			Item.mana = 12;
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
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
            var proj = Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, player.whoAmI);
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
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hwacha");
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
				arrowTimer++;

            Projectile.velocity.Y = 5;
            Projectile.frame = (5 - arrows);

			NPC target = Main.npc.Where(n => n.active && n.CanBeChasedBy(Projectile) && n.Distance(Projectile.Center) < 800).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();

			if (target != default)
			{
				Vector2 directionVec = (Projectile.Center - new Vector2(0, 24)).DirectionTo(target.Center);
                float rotDifference = ((((directionVec.ToRotation() - Projectile.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

				Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.rotation + rotDifference, 0.03f);

				direction = Math.Sign(Projectile.rotation.ToRotationVector2().X);
            }

            if (arrows >= 1)
            {
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    Projectile proj = Main.projectile[i];

                    if (proj == null || !proj.active || proj.damage == 0 || !ProjectileID.Sets.IsAWhip[proj.type])
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
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center - new Vector2(0, 20) + Main.rand.NextVector2Circular(4, 4), Projectile.rotation.ToRotationVector2().RotatedByRandom(0.15f) * Main.rand.NextFloat(7,9), ModContent.ProjectileType<HwachaArrow>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        }
                        arrowTimer = 0;
                        break;
                    }
                }
            }
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
            Texture2D frontWheel = ModContent.Request<Texture2D>(Texture + "_Frontwheel").Value;
            Texture2D backWheel = ModContent.Request<Texture2D>(Texture + "_Backwheel").Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];

			Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			Vector2 origin = new Vector2(22, 42);
			if (direction == 1)
				origin.X = tex.Width - origin.X;

            Main.spriteBatch.Draw(backWheel, Projectile.position + origin - Main.screenPosition, frame, lightColor, 0, origin, Projectile.scale, direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, Projectile.position + origin - Main.screenPosition, frame, lightColor, Projectile.rotation - (direction == -1 ? 3.14f : 0), origin, Projectile.scale, direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(frontWheel, Projectile.position + origin - Main.screenPosition, frame, lightColor, 0, origin, Projectile.scale, direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            return false;
        }
    }

    public class HwachaArrow : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hwacha");
        }

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
