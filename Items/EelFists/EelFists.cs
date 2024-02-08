//TODO:
//Balance
//Sellprice
//Rarity
//Right click
//Stab VFX
//Some sort of electrical aura
//Obtainment
//Description
//Grass Collision
//Make sure damage is transferred correctly
//Better SFX
//Make it farther up the arm
//Snappier movement
//Rename to eel arms

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
using static tModPorter.ProgressUpdate;
using JadeFables.Helpers;
using static Humanizer.In;
using ReLogic.Content;

namespace JadeFables.Items.EelFists
{
    public class EelFists : ModItem
    {

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.width = 9;
            Item.height = 15;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item1;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<EelFistProj>();
            Item.useAnimation = 31;
            Item.useTime = 31;
            Item.shootSpeed = 45.5f;
            Item.damage = 13;
            Item.knockBack = 1.5f;
            Item.crit = 8;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = false;
            Item.channel = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Main.projectile.Where(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<EelFistProj>()).ToList().ForEach(n => (n.ModProjectile as EelFistProj).Attack());
            return false;
        }

        public override void HoldItem(Player player)
        {
            int ownedArm = Main.projectile.Count(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<EelFistProj>());
            if (ownedArm == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    var proj = Projectile.NewProjectileDirect(new EntitySource_ItemUse(player, Item), player.Center, Vector2.Zero, ModContent.ProjectileType<EelFistProj>(), Item.damage, Item.knockBack, player.whoAmI);
                    var MP = (proj.ModProjectile as EelFistProj);
                    MP.rightArm = i == 0;
                    MP.timer = i * 60;
                }
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
    }

    internal class EelFistProj : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        public override void Load()
        {
            Terraria.On_Main.DrawDust += On_Main_DrawDust;
        }

        public override void Unload()
        {
            Terraria.On_Main.DrawDust -= On_Main_DrawDust;
        }

        public bool rightArm = false;

        private Trail trail;
        private List<Vector2> cache;

        private readonly int NUMPOINTS = 20;

        private float length = 120;

        private Vector2 startPos = Vector2.Zero;

        public int timer;

        private float curveMult = 43;

        private int attackTimer = 0;

        private float attackLength = 30;

        private float armRotationVar = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.timeLeft = 500;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
        }

        private float SignPow(float inp, float exp)
        {
            return MathF.Sign(inp) * MathF.Pow(MathF.Abs(inp), exp);
        }
        public override void AI()
        {
            Vector2 dir = Main.MouseWorld - startPos;
            dir.Normalize();
            dir *= length;
            owner.ChangeDir(MathF.Sign(dir.X));
            dir += (Vector2.One.RotatedBy(timer * 0.03f) * 10);
            float rotationAmt = MathF.Pow(Math.Clamp(attackTimer / attackLength, 0, 1), 1.2f);
            armRotationVar = MathHelper.Lerp(armRotationVar, rotationAmt, 0.1f);
            float angleOffset = -0.8f * owner.direction;
            float armRotation = MathHelper.Lerp(-angleOffset, angleOffset, armRotationVar) + (dir.ToRotation() - (owner.direction * 0.6f));
            if (rightArm)
            {
                owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation - 1.57f);
                startPos = owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, armRotation - 1.57f);
            }
            else
            {
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation - 1.57f);
                startPos = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, armRotation - 1.57f);
            }
            timer++;

            dir = dir.RotatedBy(-0.4f * rotationAmt);
            FillCache(dir);
            ManageTrail();

            if (owner.HeldItem.type == ModContent.ItemType<EelFists>())
                Projectile.timeLeft = 2;
            Projectile.Center = owner.Center;

            if (attackTimer > 0)
            {

                if (attackTimer <= attackLength)
                {
                    curveMult = 30;

                    float progress = Math.Clamp(attackTimer / attackLength, 0, 1);
                    float lengthExtraMult = SignPow(MathF.Sin((progress * 9.42f) - 3.14f), 2.5f);
                    if (lengthExtraMult < 0)
                    {
                        if (progress > 0.5f)
                            lengthExtraMult = -MathF.Pow(MathF.Abs(lengthExtraMult) * 0.98f, 1.0f - (lengthExtraMult * 1.5f));
                        else
                            lengthExtraMult = SignPow(lengthExtraMult, 0.6f);
                    }
                    length = 120 + (70 * MathF.Pow(Math.Clamp(progress * 3.0f, 0, 1), 2.0f) * lengthExtraMult);
                    curveMult = 45 - (63 * MathF.Pow(Math.Clamp(progress * 3.0f, 0, 1), 5.35f) * SignPow(MathF.Sin((progress * 9.42f) - 3.14f), 1.25f));
                
                    if (Main.rand.NextBool(1))
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            Vector2 dustPos = cache[(int)((NUMPOINTS - 1) * (MathF.Pow(Main.rand.NextFloat(0.6f, 0.8f), (1.0f - SignPow(lengthExtraMult, 0.3f)) * 2.0f)))];
                            var dust = Dust.NewDustPerfect(dustPos, ModContent.DustType<EelFistDust>(), Main.rand.NextVector2Circular(0.15f, 0.15f), 0, rightArm ? new Color(0.5f, 0.5f, 0f, 1f) : new Color(0.5f, 0.0f, 0.5f, 1f), Main.rand.NextFloat(0.35f, 0.7f));
                            dust.rotation = Main.rand.NextFloat(6.28f);
                        }
                    }
                }
                attackTimer--;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        private void FillCache(Vector2 endPoint)
        {
            Vector2 midPoint = Vector2.Lerp(startPos, startPos + endPoint, 0.5f) + (Vector2.Normalize(endPoint).RotatedBy(-1.57f * owner.direction) * curveMult);
            BezierCurve curve = new BezierCurve(startPos, midPoint, startPos + endPoint);
            cache = curve.GetPoints(NUMPOINTS - 1);
            //cache.Add(endPoint);
        }

        private void ManageTrail()
        {
            trail ??= new Trail(Main.instance.GraphicsDevice, NUMPOINTS, new TriangularTip(1), factor => 6, factor => Lighting.GetColor((int)(cache[(int)Math.Floor((factor.X * 0.99f) * NUMPOINTS)].X / 16), (int)(cache[(int)Math.Floor((factor.X * 0.99f) * NUMPOINTS)].Y / 16)));
            trail.Positions = cache.ToArray();
            trail.NextPosition = cache[NUMPOINTS - 1];
        }

        private void On_Main_DrawDust(On_Main.orig_DrawDust orig, Main self)
        {  
            Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<EelFistProj>()).ToList().ForEach(n => (n.ModProjectile as EelFistProj).DrawTrail());
            orig(self);
        }

        public void DrawTrail()
        {
            Effect effect = Filters.Scene["EelFistShader"].GetShader().Shader;

            var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("JadeFables/Items/EelFists/EelFistProj").Value);

            trail?.Render(effect);
        }

        public void Attack()
        {
            if (attackTimer <= 0)
            {
                attackTimer = (int)attackLength;
                if (rightArm)
                {
                    attackTimer += (int)(attackLength / 5.5f);
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (length < 160)
                return false;
            foreach (Vector2 point in cache)
            {
                if (targetHitbox.Contains((int)point.X, (int)point.Y))
                {
                    return true;
                }
            }
            return false;
        }
    }

    class EelFistDust : ModDust
    {
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 24, 24);
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(ModContent.Request<Effect>("JadeFables/Effects/GlowingDust", AssetRequestMode.ImmediateLoad).Value), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.fadeIn % 3 == 2)
                dust.frame.Y += 6;
            if (dust.fadeIn == 0)
                dust.position -= Vector2.One.RotatedBy(dust.rotation) * 12 * dust.scale;
            dust.position += dust.velocity;
            dust.velocity *= 0.86f;
            dust.shader.UseColor(dust.color);
            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 1.1f);

            if (dust.fadeIn > 12)
                dust.active = false;

            return false;
        }
    }
}