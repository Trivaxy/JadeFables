using JadeFables.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
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
            Item.UseSound = SoundID.Item111;
            Item.value = Item.sellPrice(silver: 45);
            Item.rare = ItemRarityID.Blue;
        }

        public override bool AltFunctionUse(Player player)
        {
            return false;
        }

        public override void UpdateInventory(Player player)
        {
            if (Main.mouseRight)
                popping = true;
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

        public override Vector2? HoldoutOffset() => new Vector2(-4, 0);
    }

    internal class KoiPopperBubble : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        private float scale;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bubble");
            Main.projFrames[Projectile.type] = 1;
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
            Projectile.frame = 0;
            scale = Main.rand.NextFloat(1, 1.65f);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.96f;
            Projectile.scale = MathHelper.Min(scale, Projectile.timeLeft / 12f);
            Lighting.AddLight(Projectile.Center, new Color(48, 213, 200).ToVector3() * 0.76f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            float squash = MathHelper.Max(1 - (Projectile.velocity.Length() / 50f), 0.25f);
            float stretch = 1 + Projectile.velocity.Length() / 30f;

            Effect effect = Filters.Scene["ManualRotation"].GetShader().Shader;
            float rotation = -Projectile.rotation;
            effect.Parameters["uTime"].SetValue(rotation);
            effect.Parameters["cosine"].SetValue(MathF.Cos(rotation));
            effect.Parameters["uColor"].SetValue(lightColor.ToVector3());
            effect.Parameters["uOpacity"].SetValue(0.7f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, new Vector2(tex.Width / 2, frameHeight / 2), Projectile.scale * new Vector2(stretch, squash), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
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
            SoundEngine.PlaySound(SoundID.Item54, Projectile.Center);
            Projectile.active = false;
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<KoiPop>(), Projectile.damage, Projectile.knockBack, owner.whoAmI);
            proj.scale = scale;
            /*for (int i = 0; i < 6; i++)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(3, 3);
                Dust.NewDustPerfect(Projectile.Center + (dir * 12), ModContent.DustType<GlowLineFast>(), dir, 0, Color.Pink, Main.rand.NextFloat(0.5f, 0.7f));
            }

            for (int i = 0; i < 12; i++)
            {
                int dustType = Main.rand.NextBool() ? 176 : 177;
                Vector2 dir = Main.rand.NextVector2Circular(3, 3);
                Dust.NewDustPerfect(Projectile.Center, dustType, dir, default, default, 1.3f).noGravity = true;
            }*/
        }
    }
    internal class KoiPop : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        private List<NPC> alreadyHit = new List<NPC>();

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bubble");
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 16;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 3 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame >= 3)
                    Projectile.active = false;
            }

            Lighting.AddLight(Projectile.Center, new Color(48, 213, 200).ToVector3() * 1.1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frameBox, lightColor * 0.7f, Projectile.rotation, new Vector2(tex.Width / 2, frameHeight / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.penetrate++;
            alreadyHit.Add(target);
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (alreadyHit.Contains(target))
                return false;
            return base.CanHitNPC(target);
        }

    }
}