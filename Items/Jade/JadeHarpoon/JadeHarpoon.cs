//TODO on jade harpoon:
//Balance
//Sellprice
//Rarity
//Description
//Chain drawing
//Disable mount while flipping
//Make the player's hand point towards the harpoon
//Move the origin up while spiking in
//Implement more safeguards
//Visuals

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
using JadeFables.Core.Systems;

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
            Item.damage = 25;
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
        private bool launching = false;

        private bool spinning = false;

        private NPC hookTarget;
        private Vector2 hookOffset;
        private float playerSpeed = 1.0f;

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
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (hooked && !spinning)
            {
                if (!hookTarget.active)
                {
                    Projectile.active = false;
                    return;
                }
                Projectile.Center = hookTarget.Center + hookOffset;

                if (!owner.channel)
                {
                    launching = true;
                }
            }
            if (spinning)
            {
                Projectile.rotation += 0.3f * owner.direction;

                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 0.78f);
                Projectile.velocity = Vector2.Zero;
                Vector2 posToBe = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 0.78f);
                    Projectile.Center = posToBe;

                Projectile.timeLeft = 2;
                if (!owner.GetModPlayer<JadeHarpoonPlayer>().flipping)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX, ModContent.ProjectileType<JadeHarpoonShockwave>(), (int)(Projectile.damage * 2f), 0, owner.whoAmI, 0, 10);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX * -1, ModContent.ProjectileType<JadeHarpoonShockwave>(), (int)(Projectile.damage * 2f), 0, owner.whoAmI, 0, -10);
                    Projectile.active = false;
                }
                return;
            }
            owner.itemAnimation = owner.itemTime = 2;
            owner.direction = Math.Sign(owner.DirectionTo(Main.MouseWorld).X);
            Projectile.rotation = Projectile.DirectionFrom(owner.Center).ToRotation() + 0.78f;

            if (launching)
            {
                owner.velocity = owner.DirectionTo(Projectile.Center) * playerSpeed;

                if (playerSpeed < 20)
                {
                    playerSpeed += 0.15f;
                    playerSpeed *= 1.15f;
                }
                return;
            }

           if (Projectile.timeLeft == 40)
           {
                startPos = Projectile.Center;
           }
           if (retracting)
           {
                Projectile.extraUpdates = 1;
                Projectile.Center = Vector2.Lerp(owner.Center, startPos, EaseFunction.EaseCircularOut.Ease(Projectile.timeLeft / 40f));
           }
           else
           {
                Projectile.velocity *= 0.935f;
           }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D spinTex = ModContent.Request<Texture2D>(Texture + "_Spin").Value;
            Texture2D chaintex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
            Vector2 origin = new Vector2(0, tex.Height);
            if (!spinning)
            {
                origin = new Vector2(tex.Width, 0);

                Vector2 pointToDrawFrom = Projectile.Center + new Vector2(-tex.Width, tex.Height).RotatedBy(Projectile.rotation);

                float length = MathHelper.Max((pointToDrawFrom - owner.Center).Length(), 10000);
                if (length > chaintex.Height * 3)
                {
                    for (float i = 0; i < length; i += chaintex.Height)
                    {
                        Vector2 pointToDraw = Vector2.Lerp(pointToDrawFrom, owner.Center, i / length);
                        Color chainColor = Lighting.GetColor((int)(pointToDraw.X / 16), (int)(pointToDraw.Y / 16));
                        Main.spriteBatch.Draw(chaintex, pointToDraw - Main.screenPosition, null, chainColor, pointToDrawFrom.DirectionFrom(owner.Center).ToRotation() + 1.57f, chaintex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
                    }
                }
            }
            Main.spriteBatch.Draw(spinning ? spinTex : tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (retracting && !spinning)
                return false;
            if (hooked && !launching)
                return false;
            return base.CanHitNPC(target);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0;
            if (spinning)
                return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), owner.Center, owner.Center + (Projectile.rotation.ToRotationVector2() * 40), 10, ref point);

            Rectangle playerHitbox = owner.Hitbox;
            playerHitbox.Inflate(20, 20);
            if (launching)
                return Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), playerHitbox.TopLeft(), playerHitbox.Size());

            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0)
                return;
            if (launching && !spinning)
            {
                CameraSystem.Shake += 7;
                startPos = Projectile.Center;
                owner.velocity = owner.DirectionFrom(Projectile.Center).RotatedBy(-Math.Sign(owner.Center.X - Projectile.Center.X) * 0.55f) * 16;
                spinning = true;
                Projectile.rotation = 0;
                owner.GetModPlayer<JadeHarpoonPlayer>().flipping = true;
                return;
            }
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1000;
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

    class JadeHarpoonShockwave : ModProjectile
    { 

        public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");
        private int TileType => (int)Projectile.ai[0];
        private int ShockwavesLeft => (int)Projectile.ai[1];//Positive and Negitive

        private bool createdLight = false;

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 1060;
            Projectile.tileCollide = true;
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.idStaticNPCHitCooldown = 20;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.extraUpdates = 5;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 1000)
            {
                if (Projectile.timeLeft < 1002 && Projectile.timeLeft > 80)
                    Projectile.Kill();

                Projectile.velocity.Y = 4f;
            }
            else
            {
                Projectile.velocity.Y = Projectile.timeLeft <= 10 ? 1f : -1f;

                if (Projectile.timeLeft == 19 && Math.Abs(ShockwavesLeft) > 0)
                {
                    Projectile proj = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), new Vector2((int)Projectile.Center.X / 16 * 16 + 16 * Math.Sign(ShockwavesLeft)
                    , (int)Projectile.Center.Y / 16 * 16 - 32),
                    Vector2.Zero, Projectile.type, Projectile.damage, 0, Main.myPlayer, TileType, Projectile.ai[1] - Math.Sign(ShockwavesLeft));
                    proj.extraUpdates = (int)(Math.Abs(ShockwavesLeft) / 3f);
                }

            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft < 21)
                Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.Tile[TileType].Value, Projectile.position - Main.screenPosition, new Rectangle(18, 0, 16, 16), lightColor);

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > 800)
            {
                Point16 point = new Point16((int)((Projectile.Center.X + Projectile.width / 3f * Projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)(Projectile.Center.Y / 16) + 1));
                Tile tile = Framing.GetTileSafely(point.X, point.Y);

                if (!createdLight)
                {
                    createdLight = true;
                    Dust.NewDustPerfect(point.ToVector2() * 16, ModContent.DustType<JadeHarpoonLight>(), Vector2.Zero, 0, Color.Green, 1);
                }
                if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    Projectile.timeLeft = 20;
                    Projectile.ai[0] = tile.TileType;
                    Projectile.tileCollide = false;
                    Projectile.position.Y += 16;

                    for (float num315 = 0.50f; num315 < 3; num315 += 0.25f)
                    {
                        float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
                        Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 2f;
                        int dustID = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, (int)(Projectile.height / 2f), ModContent.DustType<JadeHarpoonGlow>(), 0f, 0f, 50, Color.Green, Main.rand.NextFloat(0.45f, 0.95f));
                        Main.dust[dustID].velocity = vecangle;
                    }
                }
            }
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
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
            if (flipping)
                return false;
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
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

    public class JadeHarpoonGlow : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.frame = new Rectangle(0, 0, 64, 64);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(JadeFables.Instance.Assets.Request<Effect>("Effects/GlowingDust", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value), "GlowingDustPass");
            int a = 1;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= Vector2.One * 32 * dust.scale;
                dust.customData = true;
            }

            Vector2 currentCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 32 * dust.scale;

            dust.scale *= 0.95f;
            Vector2 nextCenter = dust.position + Vector2.One.RotatedBy(dust.rotation + 0.06f) * 32 * dust.scale;

            dust.rotation += 0.06f;
            dust.position += currentCenter - nextCenter;

            dust.shader.UseColor(dust.color);

            dust.position += dust.velocity;

            if (!dust.noGravity)
                dust.velocity.Y += 0.1f;

            dust.velocity *= 0.99f;
            dust.color *= 0.95f;

            if (!dust.noLight)
                Lighting.AddLight(dust.position, dust.color.ToVector3());

            if (dust.scale < 0.05f)
                dust.active = false;

            return false;
        }
    }

    public class JadeHarpoonLight : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.scale *= 0.96f;
            if (dust.scale < 0.05f)
                dust.active = false;
            Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 2);
            return false;
        }
    }
}
