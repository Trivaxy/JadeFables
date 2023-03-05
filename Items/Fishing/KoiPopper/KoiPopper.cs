//TODO:
//Obtainability
//Sfx for item use Lclick
//Sfx for item use Rclick
//Sfx for bubble pop
//Holdout offset
//Bubble death effects
//Deathring
//Better visuals

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
using static Terraria.ModLoader.ModContent;

namespace JadeFables.Items.Fishing.KoiPopper
{
    class KoiPopper : ModItem
    {
        public bool popping;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Koi Popper");
            Tooltip.SetDefault("Shoot bubles out in a spread \nRight click to pop all bubbles, dealing damage to nearby enemies");
        }

        public override void SetDefaults()
        {
            Item.damage = 16;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 16;
            Item.height = 64;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1;
            Item.shoot = ProjectileType<KoiPopperBubble>();
            Item.shootSpeed = 11f;
            Item.autoReuse = true;
            Item.useTurn = false;

            Item.value = Item.sellPrice(silver: 45);
            Item.rare = ItemRarityID.Blue;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (popping)
            {
                var nearestBubble = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<KoiPopperBubble>() && n.owner == player.whoAmI).OrderBy(n => n.Distance(player.Center)).FirstOrDefault();
                if (nearestBubble == default)
                {
                    popping = false;
                }
                else
                {
                    (nearestBubble.ModProjectile as KoiPopperBubble).Pop();
                    player.itemAnimation = player.itemTime = 2;
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                popping = true;
                return false;
            }
            for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.9f, 1.1f), type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }

    internal class KoiPopperBubble : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bubble");
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.tileCollide = true;
            Projectile.friendly = false;
            Projectile.timeLeft = 300;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(Main.projFrames[Projectile.type]);
        }

        public override void AI()
        {
            Projectile.velocity *= 0.96f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frameBox, lightColor, Projectile.rotation, new Vector2(tex.Width / 2, frameHeight / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            return false;
        }

        public void Pop()
        {
            Projectile.active = false;
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<KoiPop>(), Projectile.damage, Projectile.knockBack, owner.whoAmI);
            for (int i = 0; i < 12; i++)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(3,3);
                Dust.NewDustPerfect(Projectile.Center + (dir * 12), ModContent.DustType<GlowLineFast>(), dir, 0, Color.Pink, Main.rand.NextFloat(0.5f,0.7f));
            }
        }
    }
    internal class KoiPop : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bubble");
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 9;
            Projectile.hide = true;
        }

        public override void AI()
        {
            
        }

    }
}