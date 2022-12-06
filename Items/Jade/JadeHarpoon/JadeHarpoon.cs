using JadeFables.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.In;
using static Terraria.ModLoader.ModContent;
using JadeFables.Core;

namespace JadeFables.Items.Jade.JadeHarpoon
{
    class JadeHarpoon : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Harpoon");
            Tooltip.SetDefault("update later");
        }

        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.DamageType = DamageClass.Melee;
            Item.width = 16;
            Item.height = 64;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
            Item.channel = true;
            Item.shoot = ProjectileType<JadeHarpoonHook>();
            Item.shootSpeed = 30f;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.channel = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<JadeHarpoonHook>());
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ProjectileType<JadeHarpoonHook>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<JadeChunk.JadeChunk>(12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    internal class JadeHarpoonHook : ModProjectile
    {
        Player owner => Main.player[Projectile.owner];

        Vector2 startPos;
        private bool retracting => Projectile.timeLeft < 40;

        private bool hooked = false;

        private bool swinging = false;

        private NPC hookTarget;
        private Vector2 hookOffset;
        private float playerSpeed;

        private int progress;
        private float playerRotation = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Harpoon");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 80;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            if (swinging)
            {
                Projectile.rotation += 0.3f * owner.direction;

                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 0.78f);
                Projectile.velocity = Vector2.Zero;
                Vector2 posToBe = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 0.78f);
                    Projectile.Center = posToBe;

                Projectile.timeLeft = 2;
                if (!owner.GetModPlayer<JadeHarpoonPlayer>().flipping)
                    Projectile.active = false;
                return;
            }
            owner.itemAnimation = owner.itemTime = 2;
            owner.direction = Math.Sign(owner.DirectionTo(Main.MouseWorld).X);
            Projectile.rotation = Projectile.DirectionFrom(owner.Center).ToRotation() + 0.78f;

            if (hooked)
            {
                if (!hookTarget.active)
                {
                    Projectile.active = false;
                    return;
                }
                Projectile.timeLeft = 80;
                Projectile.Center = hookTarget.Center + hookOffset;
                owner.velocity = owner.DirectionTo(Projectile.Center) * playerSpeed;

                if (playerSpeed < 20)
                {
                    playerSpeed += 0.15f;
                    playerSpeed *= 1.15f;
                }
                if (!owner.channel && Main.mouseLeft && owner.Distance(Projectile.Center) < 70)
                {
                    startPos = Projectile.Center;
                    owner.velocity = owner.DirectionFrom(Projectile.Center) * 16;
                    swinging = true;
                    Projectile.rotation = 0;
                    owner.GetModPlayer<JadeHarpoonPlayer>().flipping = true;
                }
                return;
            }

           if (Projectile.timeLeft == 40)
           {
                startPos = Projectile.Center;
           }
           if (retracting)
           {
                Projectile.Center = Vector2.Lerp(owner.Center, startPos, EaseFunction.EaseCircularOut.Ease(Projectile.timeLeft / 40f));
           }
           else
           {
                Projectile.velocity *= 0.92f;
           }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new Vector2(0, tex.Height);
            if (!swinging)
                origin = new Vector2(tex.Width / 2, tex.Height / 2);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (retracting)
                return false;
            return base.CanHitNPC(target);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.friendly = false;
            Projectile.penetrate++;
            hooked = true;
            hookTarget = target;
            hookOffset = Projectile.Center - target.Center;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > 40)
                Projectile.timeLeft = 41;
            return false;
        }
    }

    public class JadeHarpoonPlayer : ModPlayer
    {
        public bool flipping;
        public Vector2 jumpVelocity = Vector2.Zero;

        public float storedBodyRotation = 0f;
        public override void PreUpdate()
        {
            if (flipping)
                Player.maxFallSpeed = 2000f;

        }

        public override void PostUpdate()
        {
            if (flipping)
            {
                storedBodyRotation += 0.3f * Player.direction;
                Player.fullRotation = storedBodyRotation;
                Player.fullRotationOrigin = Player.Size / 2;
            }
            if (Player.velocity.Y == 0)
            {
                storedBodyRotation = 0;
                Player.fullRotation = 0;
                flipping = false;
            }
            else
                jumpVelocity = Player.velocity;
        }

    }
}
