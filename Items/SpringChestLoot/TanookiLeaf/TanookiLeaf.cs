using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using System.IO;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace JadeFables.Items.SpringChestLoot.TanookiLeaf
{
	public class TanookiLeaf : ModItem
	{
		public override void SetStaticDefaults() {
            // DisplayName.SetDefault("Tanooki Leaf");
			// Tooltip.SetDefault("Jump higher while running faster \nPress jump in the air to slow your fall");
		}

		int cooldown = 0;
		bool pressedJump = false;
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;
            Item.hasVanityEffects = true;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
		{
            player.noFallDmg = true;
            player.GetModPlayer<TanookiLeafPlayer>().equipped = true;
            player.jumpSpeedBoost += Math.Min(Math.Abs(player.velocity.X) * 0.4f, 5);
            player.GetModPlayer<TanookiLeafPlayer>().frameCounter--;
            if (cooldown-- < 0 && player.controlJump && player.velocity.Y > 0 && !pressedJump)
			{
				pressedJump = true;
                player.GetModPlayer<TanookiLeafPlayer>().frameCounter = 7;

                Helpers.Helper.PlayPitched("TanookiSpin", 0.1f, 0, player.Center);
                cooldown = 12;
                cooldown = 12;
				player.velocity.Y *= 0.1f;
			}

            if (cooldown > 3)
                player.velocity.Y *= 0.75f;
			if (!player.controlJump)
				pressedJump = false;

        }

        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<TanookiLeafPlayer>().equipped = true;
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
    }

    public class TanookiLeafPlayer : ModPlayer
    {
        public bool equipped = false;

        public int frameCounter = 0;
        public override void ResetEffects()
        {
            equipped = false;
        }
    }
    public class TanookiLeafDrawLayer : PlayerDrawLayer
    {
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.GetModPlayer<TanookiLeafPlayer>().equipped && !drawInfo.drawPlayer.dead;
        }

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Torso);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player armorOwner = drawInfo.drawPlayer;

            TanookiLeafPlayer modPlayer = armorOwner.GetModPlayer<TanookiLeafPlayer>();
            drawInfo.armorHidesArms = true;
            Texture2D tex = ModContent.Request<Texture2D>("JadeFables/Items/SpringChestLoot/TanookiLeaf/TanookiLeaf_Sheet").Value;

            Vector2 drawPos = (armorOwner.MountedCenter - Main.screenPosition) - new Vector2(0, 6 - armorOwner.gfxOffY);

            int xFrame = 0;
            int yFrame = 0;
            if (modPlayer.frameCounter > 0)
            {
                xFrame = 1;
                yFrame = 11 - modPlayer.frameCounter;
            }
            else
                xFrame = yFrame = 0;

            Rectangle frame = new Rectangle(40 * xFrame, 56 * yFrame, 40, 56);

            DrawData drawData = new DrawData(
                tex,
                new Vector2((int)(drawPos.X), (int)(drawPos.Y)),
                frame,
                Lighting.GetColor((int)armorOwner.Center.X / 16, (int)armorOwner.Center.Y / 16),
                0f,
                frame.Size() / 2,
                1,
                armorOwner.direction != -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0
            );

            drawInfo.DrawDataCache.Add(drawData);
        }
    }

}
