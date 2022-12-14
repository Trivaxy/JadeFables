//TODO on gong:
//Sound effects
//Balance
//Sellprice
//Rarity
//Sprites
//Visuals
//Screenshake
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
using System.Reflection;
using Terraria.GameContent;
using Terraria.DataStructures;
using CsvHelper.TypeConversion;
using JadeFables.Core;
using System.Reflection.Metadata;

namespace JadeFables.Items.SpringChestLoot.Gong
{
    public class GongItem : ModItem
    {
        public int gongCooldown = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gong and Ringer");
        }
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.width = 9;
            Item.height = 15;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item1;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<GongProj>();
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.shootSpeed = 45.5f;
            Item.damage = 15;
            Item.knockBack = 1.5f;
            Item.value = Item.sellPrice(0, 0, 1, 0);
            Item.crit = 8;
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = true;
        }

        public override void HoldItem(Player player)
        {
            gongCooldown--;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.shoot = ModContent.ProjectileType<RingerProj>();
                Item.shootSpeed = 0f;
            }
            else
            {
                Item.useAnimation = 6;
                Item.useTime = 6;
                Item.useStyle = ItemUseStyleID.Swing;
                Item.shoot = ModContent.ProjectileType<GongProj>();
                Item.shootSpeed = 45.5f;
                if (Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == Item.shoot) || gongCooldown > 0)
                    return false;
                gongCooldown = 20;
            }
            return base.CanUseItem(player);
        }
    }

    internal class GongProj : ModProjectile
    {

        readonly int MAXHITS = 3;
        int timesHit = 0;

        private bool onLastHit => timesHit >= MAXHITS;

        private bool embedded = false;
        private NPC embedTarget = default;
        private Vector2 embedOffset = Vector2.Zero;
        private float opacity = 1;

        private Player owner => Main.player[Projectile.owner];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gong");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 500;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            if (onLastHit)
            {
                if (embedded)
                {
                    Projectile.velocity = Vector2.Zero;
                    if (embedTarget != default)
                    {
                        if (!embedTarget.active)
                        {
                            Projectile.active = false;
                            return;
                        }
                        Projectile.Center = embedTarget.Center + embedOffset;
                    }
                    opacity -= 0.025f;
                    if (opacity <= 0)
                        Projectile.active = false;
                }
                return;
            }
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(owner.Center + new Vector2(1,1)) * 15, 0.06f);
            if (Projectile.timeLeft < 430)
            {
                Projectile.tileCollide = false;
            }
            if (Projectile.Distance(owner.Center) < 20 && Projectile.timeLeft < 460)
            {
                Projectile.active = false;
            }

            Projectile ringer = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<RingerProj>() && n.Hitbox.Intersects(Projectile.Hitbox)).FirstOrDefault();
            if (ringer != default && Projectile.timeLeft < 480)
            {
                Core.Systems.CameraSystem.Shake += 5;
                Helpers.Helper.PlayPitched("GongRing", 1, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);
                Projectile.tileCollide = true;
                Projectile.damage = (int)(Projectile.damage * 1.4f);
                Projectile.friendly = true;
                Projectile.timeLeft = 500;
                Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * MathHelper.Lerp(45.5f, 60f, timesHit++ / (float)MAXHITS);
                if (onLastHit)
                    Projectile.velocity *= 0.4f;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (onLastHit)
            {
                embedded = true;
                embedTarget = target;
                embedOffset = Projectile.Center - target.Center;
            }
            Projectile.friendly = false;
            Projectile.penetrate++;
            Projectile.velocity *= -0.5f;
            Projectile.timeLeft = 390;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (onLastHit)
            {
                embedded = true;
                return false;
            }
            Projectile.timeLeft = 390;
            Projectile.velocity *= -0.5f;
            return false;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new Vector2(tex.Width, tex.Height) / 2;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }

    internal class RingerProj : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        private float angle;

        private float startAngle = 0;
        private float endAngle = 0;

        private float progress = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ringer");
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 100;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            startAngle = Projectile.DirectionTo(Main.MouseWorld).ToRotation() - (1 * owner.direction);
            endAngle = Projectile.DirectionTo(Main.MouseWorld).ToRotation() + (1 * owner.direction);
        }

        public override void AI()
        {
            progress += 0.1f;
            float easedProgress = EaseFunction.EaseCircularInOut.Ease(progress);

            if (progress > 1)
            {
                Projectile.active = false;
                return;
            }
            angle = MathHelper.Lerp(startAngle, endAngle, easedProgress);
            owner.itemTime = owner.itemAnimation = 5;
            owner.heldProj = Projectile.whoAmI;

            owner.direction = Math.Sign(angle.ToRotationVector2().X);

            owner.itemRotation = angle;
            if (owner.direction != 1)
            {
                owner.itemRotation -= 3.14f;
            }
            owner.itemRotation = MathHelper.WrapAngle(owner.itemRotation);
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, angle - 1.57f);
            Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, angle - 1.57f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new Vector2(0, tex.Height);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, angle, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}