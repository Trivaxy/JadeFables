using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using JadeFables.Dusts;
using JadeFables.Core;
using Terraria.Audio;
using Microsoft.CodeAnalysis;

namespace JadeFables.Items.Jade.JadeArmor
{
    [AutoloadEquip(EquipType.Head)]
    public class JadeHat : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Blue; // The rarity of the item
            Item.defense = 7; // The amount of defense the item will give when equipped
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<JadeRobe>();
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.1f;
            player.GetCritChance(DamageClass.Generic) += 10;
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Allows the ability to dash"; // This is the setbonus tooltip
            player.GetModPlayer<JadeArmorPlayer>().equipped = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<JadeChunk.JadeChunk>(15);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class JadeRobe : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Blue; // The rarity of the item
            Item.defense = 10; // The amount of defense the item will give when equipped
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.07f;
            player.runAcceleration += 0.07f;
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            robes = true;
            equipSlot = EquipLoader.GetEquipSlot(Mod, "JadeRobe_Legs", EquipType.Legs);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<JadeChunk.JadeChunk>(20);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    class JadeArmorPlayer : ModPlayer
    {
        public const float dashVelo = 30f;

        public Vector2 dashDirection = new(0f);
        public Vector2 oldDashDirection;
        public List<Vector2> oldPositions = new List<Vector2>();

        public int dashTimer;
        public int dashCooldown;

        public bool equipped;

        public override void Load()
        {
            Terraria.On_Main.DrawPlayers_AfterProjectiles += Main_DrawPlayers_AfterProjectiles;
        }

        private void Main_DrawPlayers_AfterProjectiles(Terraria.On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
        {
            if (PlayerTarget.canUseTarget)
            {
                Main.spriteBatch.Begin(default, blendState: BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.outOfRange && !player.dead && player.GetModPlayer<JadeArmorPlayer>().oldPositions.Count > 0)
                    {
                        for (int x = player.GetModPlayer<JadeArmorPlayer>().oldPositions.Count - 1; x > 0; x--)
                        {
                            
                            //Drawing the after image would be messed up while zooming, so we have to do an absolute bullshit fucky wucky fix for it
                            //This fix is slightly offset for some Zoom values (specifically scale I think), but it is perfect the for the two that matter the most: min and max
                            //dm me (Linty) if you want to know what is happening (you dont) or if you have a similar zoom related problem

                            float scale = 1f + ((1f - Main.GameZoomTarget) * 0.5f);

                            float xZoomOffset = 40f * (Main.GameZoomTarget - 1);
                            float yZoomOffset = -21f * (Main.GameZoomTarget - 1); 

                            Vector2 offset = new Vector2(-110 + xZoomOffset, -171 + yZoomOffset) * (1f + (1f - Main.GameZoomTarget) * 0.5f);

                            // If you're curious how I got the specific values above, it was by manually testing values until it was right very fun very happy :) :) :) :)

                            Main.spriteBatch.Draw(PlayerTarget.Target, player.GetModPlayer<JadeArmorPlayer>().oldPositions[x] - Main.screenPosition + offset,
                                PlayerTarget.getPlayerTargetSourceRectangle(player.whoAmI), Color.Green * ((x / 10f) * 2f), player.fullRotation, Vector2.Zero, scale, 0f, 0f);
                        }
                    }
                }

                Main.spriteBatch.End();
            }

            orig.Invoke(self);

        }

        public override void ResetEffects()
        {
            equipped = false;

            if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[0] < 15)
                dashDirection = new(0f, 1f);
            else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[1] < 15)
                dashDirection = new(0f, -1f);
            else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[2] < 15)
                dashDirection = new(1f, 0.01f);
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[3] < 15)
                dashDirection = new(-1f, 0.01f);
            else
                dashDirection = new();

        }

        public override void PreUpdateMovement()
        {
            if (dashCooldown > 0)
                dashCooldown--;


            if (CanUseDash() && dashDirection.Length() > 0f && dashCooldown <= 0)
            {
                Player.maxFallSpeed = 2000f;
                Player.velocity = dashDirection * 15f;
                oldDashDirection = dashDirection;

                dashCooldown = 60;
                dashTimer = 25;

                SoundStyle style1 = new SoundStyle("JadeFables/Sounds/glaive_shot_01") with { Pitch = .53f, PitchVariance = .09f, Volume = 0.6f }; 
                SoundEngine.PlaySound(style1, Player.Center);

                SoundStyle style2 = new SoundStyle("JadeFables/Sounds/SwooshySwoosh") with { Pitch = .62f, PitchVariance = .1f, Volume = 0.2f }; 
                SoundEngine.PlaySound(style2, Player.Center);
            }

            if (dashTimer > 0)
            {
                oldPositions.Add(Player.Center);

                dashTimer--;
                if (dashTimer == 1)
                    Player.velocity *= 0.65f;

                if (Main.rand.NextBool(5))
                    Dust.NewDustDirect(Player.position, Player.width, Player.height, ModContent.DustType<Dusts.JadeSparkle>(), 0f, 0f).velocity = Vector2.Zero;

                float sin = (float)Math.Sin((dashTimer / 25f) * Math.PI);
                if (oldDashDirection.X > 0 || oldDashDirection.X < 0)
                {
                    Vector2 pos = new Vector2(0f, 35f) * sin - Vector2.One + new Vector2(0f, -10);
                    Dust.NewDustPerfect(Player.Center + Player.velocity * 2f + pos, ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(0f, 255, 0f), 0.45f);
                    Dust.NewDustPerfect(Player.Center + pos, ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(0f, 255, 0f), 0.45f);
                    pos = new Vector2(0f, -35f) * sin - Vector2.One + new Vector2(0f, 10);
                    Dust.NewDustPerfect(Player.Center + Player.velocity * 2f + pos, ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(0f, 255, 0f), 0.45f);
                    Dust.NewDustPerfect(Player.Center + pos, ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(0f, 255, 0f), 0.45f);
                }
                else
                {
                    Vector2 pos = new Vector2(15f, 0f) * sin - Vector2.One + new Vector2(0f, -5);
                    Dust.NewDustPerfect(Player.Center + Player.velocity * 2f + pos, ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(0f, 255, 0f), 0.45f);
                    Dust.NewDustPerfect(Player.Center + pos, ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(0f, 255, 0f), 0.45f);
                    pos = new Vector2(-15f, 0f) * sin - Vector2.One + new Vector2(0f, -5);
                    Dust.NewDustPerfect(Player.Center + Player.velocity * 2f + pos, ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(0f, 255, 0f), 0.45f);
                    Dust.NewDustPerfect(Player.Center + pos, ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(0f, 255, 0f), 0.45f);
                }
            }

            if (oldPositions.Count > 10 || (oldPositions.Count > 0 && dashTimer == 0))
                oldPositions.RemoveAt(0);
        }

        public override void PostUpdateMiscEffects()
        {
            if (dashTimer > 0)
            {
                Player.endurance += 0.5f;

                Player.noKnockback = true;
            }
        }

        private bool CanUseDash()
        {
            return equipped
                && Player.dashType == 0
                && !Player.setSolar
                && !Player.mount.Active;
        }
    }
}
