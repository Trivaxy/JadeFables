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

namespace JadeFables.Items.Jade.JadeBow
{
    class JadeBow : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Bow");
            Tooltip.SetDefault("Shoot critters to make them fight for you\nShoot town NPCs to give them a shield");
        }

        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 16;
            Item.height = 64;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
            Item.channel = true;
            Item.shoot = ProjectileType<JadeBowProj>();
            Item.shootSpeed = 0f;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Arrow;
            Item.useTurn = true;
            Item.channel = true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return player.itemTime == 2;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<JadeBowProj>());
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity / 4f, ProjectileType<JadeBowProj>(), damage, knockback, player.whoAmI);
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

    internal class JadeBowProj : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Bow");
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
        }

        public override void AI()
        {
            if (!owner.channel)
                Projectile.active = false;

            owner.itemAnimation = owner.itemTime = 2;
            owner.direction = Math.Sign(owner.DirectionTo(Main.MouseWorld).X);
            Projectile.rotation = owner.DirectionTo(Main.MouseWorld).ToRotation();
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = owner.Center;

            owner.itemRotation = Projectile.rotation;

            if (owner.direction != 1)
                owner.itemRotation -= 3.14f;

            owner.heldProj = Projectile.whoAmI;

            Projectile.frameCounter++;
            if (Projectile.frameCounter % 6 == 5)
            {
                Projectile.frame++;
                if (Projectile.frame == 4)
                    Shoot();
            }

            if (Projectile.frame >= 6)
            {
                Projectile.frame = 0;
                Projectile.frameCounter = 0;
            }

            Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
            if (Projectile.frame == 3)
                stretch = Player.CompositeArmStretchAmount.None;
            else if (Projectile.frame == 2)
                stretch = Player.CompositeArmStretchAmount.Quarter;
            else if (Projectile.frame == 1)
                stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
            else
                stretch = Player.CompositeArmStretchAmount.Full;
            owner.SetCompositeArmFront(true, stretch, Projectile.rotation - 1.57f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0, owner.gfxOffY) - Main.screenPosition, frameBox, lightColor, Projectile.rotation, new Vector2(tex.Width / 4, frameHeight / 2), Projectile.scale, owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
            return false;
        }

        private void Shoot()
        {
            if (!owner.PickAmmo(owner.HeldItem, out int type, out float speed, out int damage, out float knockBack, out int ammoItemID, false))
            {
                Projectile.active = false;
                return;
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.rotation.ToRotationVector2() * (speed + 12), type, damage, knockBack, owner.whoAmI);
            proj.GetGlobalProjectile<JadeBowGProj>().shotFromBow = true;
        }
    }

    internal class JadeBowHitbox : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        NPC parent => Main.npc[(int)Projectile.ai[0]];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Bow");
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.hide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (!parent.active || parent.GetGlobalNPC<JadeBowGNPC>().timer <= 0)
                Projectile.active = false;
            Projectile.Center = parent.Center;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target == parent || target.CountsAsACritter)
                return false;
            return base.CanHitNPC(target);
        }
    }

    public class JadeBowGProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool shotFromBow = false;

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (shotFromBow && target.CountsAsACritter)
            {
                float mult = 1;
                var alreadyBuffed = Main.npc.Where(n => n.active && n.CountsAsACritter && n.GetGlobalNPC<JadeBowGNPC>().timer > 0);

                for (int i = 0; i < alreadyBuffed.Count(); i++)
                    mult *= 0.9f;
                target.immortal = true;
                if (target.GetGlobalNPC<JadeBowGNPC>().timer <= 0)
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<JadeBowHitbox>(), damage, knockback, projectile.owner, target.whoAmI);

                target.GetGlobalNPC<JadeBowGNPC>().timer = (int)(900 * mult);
                damage = 0;
            }

            if (shotFromBow && target.townNPC)
            {
                target.immortal = true;
                target.GetGlobalNPC<JadeBowGNPC>().timer = 3000;
                damage = 0;
            }
        }

        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            if (shotFromBow && target.townNPC)
                return true;
            return base.CanHitNPC(projectile, target);
        }

        public override bool CanHitPlayer(Projectile projectile, Player target)
        {
            if (!shotFromBow)
                return base.CanHitPlayer(projectile, target);

            return false;
        }
    }

    public class JadeBowGNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int timer = -1;

        int jumpTimer = 0;

        public override bool PreAI(NPC npc)
        {
            //if (timer > 0)
            //    return false;
            return true;
        }

        public override void PostAI(NPC npc)
        {
            if (timer-- > 0 && npc.CountsAsACritter)
            {
                NPC target = Main.npc.Where(n => n.active && n.CanBeChasedBy() && n.Distance(npc.Center) < 900).OrderBy(n => n.Distance(npc.Center)).FirstOrDefault();
                if (target != default)
                {
                    npc.direction = npc.spriteDirection = Math.Sign(target.Center.X - npc.Center.X);
                    if (npc.noGravity)
                        npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(target.Center) * 4, 0.1f);
                    else
                    {
                        npc.velocity.X = MathHelper.Lerp(npc.velocity.X, Math.Sign(target.Center.X - npc.Center.X) * 4, 0.1f);
                        if (npc.collideY && target.Center.Y < npc.Center.Y - 20)
                        {
                            if (jumpTimer++ % 30 == 0)
                                npc.velocity.Y = -6;
                        }
                    }
                }

                if (Main.rand.NextBool(70))
                    Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(15, 15), ModContent.DustType<JadeSparkle>(), Vector2.Zero);
            }

            if (timer == 0)
                npc.immortal = false;
        }
    }
}