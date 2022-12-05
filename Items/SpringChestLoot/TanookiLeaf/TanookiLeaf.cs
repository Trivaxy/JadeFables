using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace JadeFables.Items.SpringChestLoot.TanookiLeaf
{
	public class TanookiLeaf : ModItem
	{
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("Tanooki Leaf");
			Tooltip.SetDefault("Jump higher while running faster \nPress jump in the air to slow your fall");
		}

		int cooldown = 0;
		bool pressedJump = false;
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.jumpSpeedBoost += Math.Min(Math.Abs(player.velocity.X), 6);

			if (cooldown-- < 0 && player.controlJump && player.velocity.Y > 0 && !pressedJump)
			{
				pressedJump = true;
				cooldown = 10;
				player.velocity.Y *= 0.1f;
			}
			if (!player.controlJump)
				pressedJump = false;

        }

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
	}
}
