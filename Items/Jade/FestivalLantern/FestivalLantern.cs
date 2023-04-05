using JadeFables.Items.Potions.Heartbeat;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Recipe;
using JadeFables.Biomes.JadeLake;
using Terraria.Localization;
using Terraria.DataStructures;
using JadeFables.Items.SpringChestLoot.FireworkPack;
using Terraria.GameContent.Events;
using Terraria.Audio;

namespace JadeFables.Items.Jade.FestivalLantern
{
    public class FestivalLantern : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 32;
            Item.maxStack = 30; //Change this when Labor of Love drops?

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;

            Item.value = Item.sellPrice(silver: 54);
            Item.rare = ItemRarityID.Blue;

            Item.consumable = true;

            Item.UseSound = SoundID.Item1;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<FestivalLanternProj>();
            Item.shootSpeed = 6;
        }

        public override bool CanUseItem(Player player)
        {
            if (LanternNight.GenuineLanterns || LanternNight.NextNightIsLanternNight || Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<FestivalLanternProj>()))
                return false;
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 4; i++)
            {
                Projectile.NewProjectile(source, position, ((i * -0.78f) - 0.4f).ToRotationVector2().RotatedByRandom(0.3f) * Main.rand.NextFloat(1.5f, 3f), type, damage, knockback, player.whoAmI, 50 + (15 * i));
            }
            return false;
        }
    }

    internal class FestivalLanternProj : ModProjectile
    {
        bool launching => timer > Projectile.ai[0];

        int timer = 0;

        Color color = Color.Orange;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 160;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            timer++;
            Lighting.AddLight(Projectile.Center, color.ToVector3());
            if (!launching)
            {
                Projectile.velocity.Y -= 0.01f;
                Projectile.velocity *= 0.98f;
                if (timer == Projectile.ai[0])
                {
                    Projectile.velocity = -Vector2.UnitY * 4;
                    SoundEngine.PlaySound(SoundID.Item110, Projectile.Center);
                }
            }
            else
            {
                Projectile.velocity = Projectile.velocity.RotatedByRandom(0.07f);
                Projectile.velocity *= 1.05f;
                for (int i = 0; i < 4; i++)
                {
                    var pos = (Projectile.Center) + (Projectile.velocity * Main.rand.NextFloat(2));
                    Dust dust = Dust.NewDustPerfect(pos, ModContent.DustType<FireworkPackDust1>(), Vector2.Normalize(-Projectile.velocity).RotatedByRandom(0.6f) * Main.rand.NextFloat(3.5f), 0, AdjacentColor());
                    dust.scale = Main.rand.NextFloat(0.15f, 0.35f) * 0.75f;
                    dust.alpha = Main.rand.Next(50);
                    dust.rotation = Main.rand.NextFloatDirection();
                }

                for (int j = 0; j < 1; j++)
                {
                    var pos = (Projectile.Center) + (Projectile.velocity * Main.rand.NextFloat(2));
                    Dust dust2 = Dust.NewDustPerfect(pos, ModContent.DustType<FireworkPackDust2>(), Vector2.Normalize(-Projectile.velocity).RotatedByRandom(3.0f) * Main.rand.NextFloat(5.5f), 0, AdjacentColor());
                    dust2.scale = Main.rand.NextFloat(0.1f, 0.25f) * 0.75f;
                    dust2.alpha = Main.rand.Next(50);
                    dust2.rotation = Main.rand.NextFloatDirection();
                }

                var pos2 = (Projectile.Center) + (Projectile.velocity * Main.rand.NextFloat(2));
                Dust dust3 = Dust.NewDustPerfect(pos2, ModContent.DustType<FireworkPackGlowDust>(), Vector2.Normalize(-Projectile.velocity).RotatedByRandom(2.6f) * Main.rand.NextFloat(3.5f), 0, AdjacentColor());
                dust3.scale = Main.rand.NextFloat(0.25f, 1f) * 0.5f;
                dust3.alpha = Main.rand.Next(50);
            }
        }

        public override void Kill(int timeLeft)
        {
            if (Projectile.ai[0] == 95)
            {
                if (Main.dayTime)
                {
                    Main.NewText("The festival awaits!", Color.Orange);
                    LanternNight.NextNightIsLanternNight = true;
                }
                else
                {
                    Main.NewText("Let the festival begin!", Color.Orange);
                    LanternNight.GenuineLanterns = true;
                }
            }
        }
        private Color AdjacentColor()
        {
            int r = color.R + Main.rand.Next(-20, 20);
            int g = color.G + Main.rand.Next(-20, 20);
            int b = color.B + Main.rand.Next(-20, 20);
            return new Color(r, g, b);
        }
    }
}
