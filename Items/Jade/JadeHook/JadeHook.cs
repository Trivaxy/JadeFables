//TODO:
//Explosion on tile grabbing
//Sound effects
//Visuals
//Sprites
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using JadeFables.Items.Jade.JadeChunk;

namespace JadeFables.Items.Jade.JadeHook
{
	internal class JadeHookItem : ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Jade Hook");
			// Tooltip.SetDefault("Creates an explosion when you hook onto a tile");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; 
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 18f; 
			Item.shoot = ModContent.ProjectileType<JadeHookProjectile>();
            Item.value = Item.sellPrice(silver: 45);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<JadeChunk.JadeChunk>(12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

	internal class JadeHookProjectile : ModProjectile
	{
		private static Asset<Texture2D> chainTexture;

		private bool pulling = false;

		private bool exploded = false;

		private int pullTimer = 0;

		private Player owner => Main.player[Projectile.owner];

		public override void Load() { 
			chainTexture = ModContent.Request<Texture2D>("JadeFables/Items/Jade/JadeHook/JadeHookChain");
		}

		public override void Unload() { 
			chainTexture = null;
		}

		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Jade Hook");
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst); 
		}

		// Use this hook for hooks that can have multiple hooks mid-flight: Dual Hook, Web Slinger, Fish Hook, Static Hook, Lunar Hook.
		public override bool? CanUseGrapple(Player player) {
			int hooksOut = 0;
			for (int l = 0; l < 1000; l++) {
				if (Main.projectile[l].active && Main.projectile[l].owner == Main.myPlayer && Main.projectile[l].type == Projectile.type) {
					hooksOut++;
				}
			}

			return hooksOut <= 2;
		}

        public override void AI()
        {
            if (pulling && !exploded && (owner.velocity.X == 0 || owner.velocity.Y == 0))
			{
				exploded = true;
				Main.NewText("Here");
			}
        }

        // Amethyst Hook is 300, Static Hook is 600.
        public override float GrappleRange() {
			return 300f;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks) {
			numHooks = 1; // The amount of hooks that can be shot out
		}

		// default is 11, Lunar is 24
		public override void GrappleRetreatSpeed(Player player, ref float speed) {
			speed = 18f; // How fast the grapple returns to you after meeting its max shoot distance
		}

		public override void GrapplePullSpeed(Player player, ref float speed) {
			pulling = true;
			pullTimer++;
			speed = 20; // How fast you get pulled to the grappling hook projectile's landing position
		}

		// Draws the grappling hook's chain.
		public override bool PreDrawExtras() {
			Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
			Vector2 center = Projectile.Center;
			Vector2 directionToPlayer = playerCenter - Projectile.Center;
			float chainRotation = directionToPlayer.ToRotation() - MathHelper.PiOver2;
			float distanceToPlayer = directionToPlayer.Length();

			while (distanceToPlayer > 20f && !float.IsNaN(distanceToPlayer)) {
				directionToPlayer /= distanceToPlayer; // get unit vector
				directionToPlayer *= chainTexture.Height(); // multiply by chain link length

				center += directionToPlayer; // update draw position
				directionToPlayer = playerCenter - center; // update distance
				distanceToPlayer = directionToPlayer.Length();

				Color drawColor = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16));

				// Draw chain
				Main.EntitySpriteDraw(chainTexture.Value, center - Main.screenPosition,
					chainTexture.Value.Bounds, drawColor, chainRotation,
					chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
			}
			// Stop vanilla from drawing the default chain.
			return false;
		}
	}
}
