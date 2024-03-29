﻿using JadeFables.Dusts;
using JadeFables.Items.SpringChestLoot.FireworkPack;
using JadeFables.Tiles.JadeLantern;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace JadeFables.Items.Jade.JadeBow
{
    class JadeBow : ModItem
    {

        public override void SetDefaults()
        {
            Item.damage = 11;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 16;
            Item.height = 64;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1;
            Item.channel = true;
            Item.shoot = ProjectileType<JadeBowProj>();
            Item.shootSpeed = 0f;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Arrow;
            Item.useTurn = true;
            Item.channel = true;

            Item.value = Item.sellPrice(silver: 45);
            Item.rare = ItemRarityID.Blue;
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
            Projectile.Center = owner.MountedCenter;

            owner.itemRotation = Projectile.rotation;

            if (owner.direction != 1)
                owner.itemRotation -= 3.14f;

            owner.heldProj = Projectile.whoAmI;

            Projectile.frameCounter++;
            if (Projectile.frameCounter % 6 == 5)
            {
                Projectile.frame++;
                if (Projectile.frame == 5)
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
            owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);

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
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + (Projectile.rotation.ToRotationVector2() * 5), Projectile.rotation.ToRotationVector2() * (speed + 12), type, damage, knockBack, owner.whoAmI);
            proj.GetGlobalProjectile<JadeBowGProj>().shotFromBow = true;
        }
    }

    internal class JadeBowHitbox : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        NPC parent => Main.npc[(int)Projectile.ai[0]];

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

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            Player owner = Main.player[projectile.owner];
            if (!shotFromBow)
                return;
            if (target.CountsAsACritter)
            {
                float mult = 1;
                var alreadyBuffed = Main.npc.Where(n => n.active && n.CountsAsACritter && n.GetGlobalNPC<JadeBowGNPC>().timer > 0);

                for (int i = 0; i < alreadyBuffed.Count(); i++)
                    mult *= 0.5f;
                if (target.type != NPCID.ExplosiveBunny)
                    target.immortal = true;
                if (target.GetGlobalNPC<JadeBowGNPC>().timer <= 0)
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<JadeBowHitbox>(), (int)(12 * owner.GetDamage(DamageClass.Ranged).Multiplicative), 0, projectile.owner, target.whoAmI);

                target.GetGlobalNPC<JadeBowGNPC>().timer = (int)(900 * mult);
                target.GetGlobalNPC<JadeBowGNPC>().owner = Main.player[projectile.owner];
                modifiers.SetMaxDamage(1);

                SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_death_1") with { Volume = 0.75f, Pitch = .6f, PitchVariance = .2f, MaxInstances = -1 };
                SoundEngine.PlaySound(style, projectile.Center);

                for (int i = 0; i < Main.rand.Next(10, 22); i++)
                {
                    Vector2 randomStart = Main.rand.NextVector2Circular(2.5f, 2.5f) * 2f;

                    Dust dust;
                    if (Main.rand.NextBool())
                        dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), randomStart, newColor: Color.Green, Scale: Main.rand.NextFloat(0.5f, 0.65f));
                    else
                        dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFastDecelerate>(), randomStart * 0.5f, newColor: new Color(0, 255, 0, 150), Scale: Main.rand.NextFloat(0.75f, 1f));

                    dust.noGravity = true;
                    dust.noLight = false;
                }
            }
            else if (target.townNPC)
            {
                target.immortal = true;
                target.GetGlobalNPC<JadeBowGNPC>().timer = 3000;
                modifiers.SetMaxDamage(1);

                SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_death_1") with { Volume = 0.75f, Pitch = .6f, PitchVariance = .2f, MaxInstances = -1 };
                SoundEngine.PlaySound(style, projectile.Center);

                for (int i = 0; i < Main.rand.Next(10, 22); i++)
                {
                    Vector2 randomStart = Main.rand.NextVector2Circular(2.5f, 2.5f) * 2f;

                    Dust dust;
                    if (Main.rand.NextBool())
                        dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), randomStart, newColor: Color.Green, Scale: Main.rand.NextFloat(0.5f, 0.65f));
                    else
                        dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFastDecelerate>(), randomStart * 0.5f, newColor: new Color(0, 255, 0, 150), Scale: Main.rand.NextFloat(0.75f, 1f));

                    dust.noGravity = true;
                    dust.noLight = false;
                }
            }
            else
            {
                Player player = Main.player[projectile.owner];
                player.MinionAttackTargetNPC = target.whoAmI;
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

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (shotFromBow)
            {
                SoundStyle style4 = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_crystal_impact_1") with { Pitch = 0f, PitchVariance = .25f, MaxInstances = -1, Volume = 0.5f };
                SoundEngine.PlaySound(style4, projectile.Center);

                for (int i = 0; i < Main.rand.Next(5, 12); i++)
                {
                    Vector2 randomStart = Main.rand.NextVector2Circular(2.5f, 2.5f);
                    Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFastDecelerate>(), randomStart, newColor: Color.Green, Scale: Main.rand.NextFloat(0.5f, 0.65f));
                    dust.velocity += projectile.velocity * 0.25f;
                    dust.noGravity = true;
                    dust.noLight = false;
                }

            }

            base.OnKill(projectile, timeLeft);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (shotFromBow)
            {
                Texture2D glow = Mod.Assets.Request<Texture2D>("Items/Jade/JadeStaff/DragonEyeAlt").Value;

                Vector2 scale = new Vector2(1f, 1f) * projectile.scale;
                float rot = projectile.rotation + MathHelper.PiOver2 + +(float)(Main.timeForVisualEffects * 0.02f * projectile.direction);
                float rot2 = projectile.rotation + (float)(Main.timeForVisualEffects * 0.02f * projectile.direction);

                //Main.EntitySpriteDraw(glow, projectile.Center - Main.screenPosition, null, Color.ForestGreen with { A = 0 } * 0.35f, rot, glow.Size() / 2, scale, SpriteEffects.None);
                //Main.EntitySpriteDraw(glow, projectile.Center - Main.screenPosition, null, Color.Green with { A = 0 } * 0.25f, rot, glow.Size() / 2, scale, SpriteEffects.None);
                //Main.EntitySpriteDraw(glow, projectile.Center - Main.screenPosition, null, Color.LawnGreen with { A = 0 } * .75f, rot, glow.Size() / 2, scale * 0.75f, SpriteEffects.None);

                //Main.EntitySpriteDraw(glow, projectile.Center - Main.screenPosition, null, Color.ForestGreen with { A = 0 } * 0.35f, rot2, glow.Size() / 2, scale, SpriteEffects.None);
                //Main.EntitySpriteDraw(glow, projectile.Center - Main.screenPosition, null, Color.Green with { A = 0 } * 0.25f, rot2, glow.Size() / 2, scale, SpriteEffects.None);
                //Main.EntitySpriteDraw(glow, projectile.Center - Main.screenPosition, null, Color.LawnGreen with { A = 0 } * .75f, rot2, glow.Size() / 2, scale * 0.75f, SpriteEffects.None);
                return false;
            }

            return base.PreDraw(projectile, ref lightColor);
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            
            if (shotFromBow)
            {
                Texture2D arrowTex = TextureAssets.Projectile[projectile.type].Value;
 
                //Need to do this and not just use lightcolor to account for arrows with 0 alpha values (Jester and Holy)
                Color originalColor = projectile.GetAlpha(Color.White);

                // Original projectile's alpha from 0-1
                float oringinalAlpha = originalColor.A / 255f;

                //Want to draw the under glow effect at lower intensity for low alpha values so it looks better
                float clampedAlpha = Math.Clamp(oringinalAlpha, 0.25f, 1f);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(1f, 0f).RotatedBy((i * MathHelper.PiOver2) + (Main.timeForVisualEffects * 0.03f));
                    Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition + offset, null, Color.ForestGreen with { A = 0 } * clampedAlpha * 1.25f, projectile.rotation, arrowTex.Size() / 2, projectile.scale * 1.08f, SpriteEffects.None);
                }


                // If the original color is Alpha0 then also be 0Alpha
                Color colToUse = lightColor;
                if (oringinalAlpha == 0)
                    colToUse.A = 0;

                Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition, null, colToUse, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);
                Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition, null, Color.ForestGreen with { A = 0 } * 0.5f, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);


            }
            base.PostDraw(projectile, lightColor);
        }
    }

    public class JadeBowGNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int timer = -1;

        int jumpTimer = 0;

        public Player owner;

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
                NPC target = Main.npc.Where(n => n.active && n.CanBeChasedBy() && Helpers.Helper.ClearSightline(n.Center, npc.Center) && n.Distance(npc.Center) < 900).OrderBy(n => n.Distance(npc.Center)).FirstOrDefault();
                if (owner.HasMinionAttackTargetNPC)
                {
                    target = Main.npc[owner.MinionAttackTargetNPC];
                }
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