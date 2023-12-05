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
using Steamworks;
using JadeFables.Dusts;
using JadeFables.Helpers;
using Terraria.Audio;

namespace JadeFables.Items.SpringChestLoot.DuelingSpirits
{
    public class DuelingSpirits : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Left click to throw Ying, pushing enemies away \nRight click to throw Yang, pulling enemies in\nAlternate strikes for guaranteed critical hits");
        }
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.width = 9;
            Item.height = 15;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.noMelee = true;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.damage = 21;
            Item.knockBack = 0f;
            Item.crit = 8;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool? UseItem(Player player)
        {
            int projType = player.altFunctionUse != 2 ? ModContent.ProjectileType<Ying>() : ModContent.ProjectileType<Yang>();
            int altProjType = player.altFunctionUse == 2 ? ModContent.ProjectileType<Ying>() : ModContent.ProjectileType<Yang>();
            Projectile toThrow = Main.projectile.Where(n => n.active && n.owner == player.whoAmI && n.type == projType).FirstOrDefault();
            Projectile toSpeedUp = Main.projectile.Where(n => n.active && n.owner == player.whoAmI && n.type == altProjType).FirstOrDefault();
            if (toThrow != default && toSpeedUp != default)
            {
                var mp = (toThrow.ModProjectile as Ying);
                var mp2 = toSpeedUp.ModProjectile as Ying;
                if (!mp.readyToStrike && !mp2.readyToStrike)
                {
                    mp.readyToStrike = true;
                    mp.rotSpeed = mp.GetStrikeSpeed();
                    mp2.rotSpeed = mp.GetStrikeSpeed();
                }
            }
            return base.UseItem(player);
        }

        public override void HoldItem(Player player)
        {
            if (!Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<Yang>()))
            {
                Projectile.NewProjectile(new EntitySource_ItemUse(player, Item), player.Center, Vector2.Zero, ModContent.ProjectileType<Yang>(), (int)(Item.damage * player.GetDamage(DamageClass.Magic).Additive), Item.knockBack * 2f, player.whoAmI, 0, 60);
            }

            if (!Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<Ying>()))
            {
                Projectile.NewProjectile(new EntitySource_ItemUse(player, Item), player.Center, Vector2.Zero, ModContent.ProjectileType<Ying>(), (int)(Item.damage * player.GetDamage(DamageClass.Magic).Additive), Item.knockBack, player.whoAmI, 3.14f, 60);
            }
        }
    }

    internal class Yang : Ying
    {
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!target.GetGlobalNPC<DuelingSpiritsGNPC>().yanged)
            {
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<MiniYang>(), 0, 0, owner.whoAmI);
                (proj.ModProjectile as MiniYing).target = target;
                target.GetGlobalNPC<DuelingSpiritsGNPC>().yanged = true;
            }
            if (target.GetGlobalNPC<DuelingSpiritsGNPC>().yinged)
            {
                SuperHit(target);
                modifiers.Knockback /= 1.4f;
                modifiers.SetCrit();
            }
            else modifiers.DisableCrit();
            ApplySpecialKnockback(target, -20);
            modifiers.Knockback *= 0;
        }
    }

    internal class Ying : ModProjectile
    {
        private readonly int NUMPOINTS = 30;

        public Player owner => Main.player[Projectile.owner];
        private List<Vector2> cache;
        private Trail trail;

        public bool expanding = false;

        public Vector2 posToCircle = Vector2.Zero;

        public bool readyToStrike = false;

        public float attackTimer = 0;

        public bool passive = true;

        public float rotSpeed = 0.1f;

        public Vector2 oldMousePos = Vector2.Zero;

        public ref float rot => ref Projectile.ai[0];

        public ref float distance => ref Projectile.ai[1];

        private List<Vector2> oldPos = new List<Vector2>();

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 1;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (passive)
            {
                return false;
            }
            return base.CanHitNPC(target);
        }

        public override void AI()
        {
            if (owner.dead)
                Projectile.active = false;
            oldPos.Add(Projectile.Center);
            if (oldPos.Count > 2)
            {
                oldPos.RemoveAt(0);
            }
            if (owner.HeldItem.type == ModContent.ItemType<DuelingSpirits>())
            {
                Projectile.timeLeft = 2;
            }

            if (passive)
            {
                float throwrot = owner.DirectionTo(Main.MouseWorld).ToRotation() - 1.57f;
                if (readyToStrike && MathF.Abs(MathHelper.WrapAngle(rot) - MathHelper.WrapAngle(throwrot)) < rotSpeed * 2)
                {
                    SoundEngine.PlaySound(SoundID.Item1, owner.Center);
                    passive = false;
                    Vector2 dir = Main.MouseWorld - owner.Center;
                    oldMousePos = owner.Center + (Vector2.Normalize(dir) * MathHelper.Clamp(dir.Length() - distance, -50, 350));
                    rotSpeed = 0.1f;
                    var otherProj = Main.projectile.Where(n => n.active && n.owner == owner.whoAmI && n.ModProjectile is Ying && n.type != Projectile.type).FirstOrDefault();
                    if (otherProj != default)
                    {
                        (otherProj.ModProjectile as Ying).rotSpeed = 0.1f;
                        (otherProj.ModProjectile as Ying).rot = rot - 3.14f;
                    }
                }

                posToCircle = owner.Center;
                distance = 60;
            }
            else
            {
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, owner.DirectionTo(oldMousePos).ToRotation() - 1.57f);
                owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, owner.DirectionTo(oldMousePos).ToRotation() - 1.57f);
                attackTimer += rotSpeed;
                posToCircle = Vector2.Lerp(owner.Center, oldMousePos, MathF.Sin(attackTimer));
                if (attackTimer >= 3f)
                {
                    attackTimer = 0;
                    passive = true;
                    readyToStrike = false;
                }
            }

            rot += rotSpeed;
            Projectile.Center = posToCircle + (rot.ToRotationVector2() * distance);

            if (!Main.dedServ)
            {
                ManageCache();
                ManageTrail();
            }
        }

        public void SuperHit(NPC target)
        {
            target.GetGlobalNPC<DuelingSpiritsGNPC>().yinged = false;
            target.GetGlobalNPC<DuelingSpiritsGNPC>().yanged = false;
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6, 6);
                Dust.NewDustPerfect(target.Center + (vel * 5), ModContent.DustType<GlowLineFast>(), vel, 0, Color.White, 1.2f);
            }
        }
        public void ApplySpecialKnockback(NPC target, float strength)
        {
            if (!target.boss && target.knockBackResist > 0)
            {
                strength *= target.knockBackResist;
                if (!target.collideY && strength < 0) strength *= 0.2f;
                target.velocity += owner.DirectionTo(target.Center) * strength;
            }
        }

        public float GetStrikeSpeed()
        {
            float throwrot = owner.DirectionTo(Main.MouseWorld).ToRotation() - 1.57f;
            float rotDifference = ((((throwrot - rot) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
            if (rotDifference < 0)
                rotDifference += 6.28f;

            return MathHelper.Max(rotDifference / 12, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {

        }

        public override bool? CanCutTiles()
        {
            return !passive;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (oldPos.Count > 0)
            {
                Texture2D eyeTex = ModContent.Request<Texture2D>(Texture + "Eye").Value;
                Main.spriteBatch.Draw(eyeTex, oldPos[0] - Main.screenPosition, null, Color.White, rot + 1.57f, new Vector2(eyeTex.Width / 2, eyeTex.Height / 2), Projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!target.GetGlobalNPC<DuelingSpiritsGNPC>().yinged)
            {
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<MiniYing>(), 0, 0, owner.whoAmI);
                (proj.ModProjectile as MiniYing).target = target;
                target.GetGlobalNPC<DuelingSpiritsGNPC>().yinged = true;
            }
            if (target.GetGlobalNPC<DuelingSpiritsGNPC>().yanged)
            {
                SuperHit(target);
                modifiers.SetCrit();
            }
            modifiers.Knockback *= 0;
            ApplySpecialKnockback(target, 5);
        }

        private void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < NUMPOINTS; i++)
                {
                    cache.Add(Projectile.Center - owner.Center);
                }
            }

            cache.Add(Projectile.Center - owner.Center);
            while (cache.Count > NUMPOINTS)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, NUMPOINTS, new RoundedTip(20), factor => 12 * factor, factor =>
            {
                return Color.White;
            });

            List<Vector2> newCache = new List<Vector2>();
            foreach (Vector2 item in cache)
            {
                newCache.Add(item + owner.Center);
            }
            trail.Positions = newCache.ToArray();
            trail.NextPosition = Projectile.Center;
        }

        public void DrawPrimitives()
        {
            if (trail == null || trail == default)
                return;

            //Main.spriteBatch.End();
            Effect effect = Filters.Scene["SnailBody"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture).Value);
            effect.Parameters["flip"].SetValue(true);

            trail.Render(effect);

            //Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    public class DuelingSpiritsGNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool yinged = false;
        public bool yanged = false;
    }
}