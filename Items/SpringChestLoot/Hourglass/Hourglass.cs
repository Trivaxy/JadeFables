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

namespace JadeFables.Items.SpringChestLoot.Hourglass
{
    public class Hourglass : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hourglass");
        }
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.width = 9;
            Item.height = 15;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item1;
            Item.DamageType = DamageClass.Ranged;
            Item.channel = true;
            Item.noMelee = true;
            Item.consumable = true;
            Item.maxStack = 999;
            Item.shoot = ModContent.ProjectileType<HourglassProj>();
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.shootSpeed = 7.5f;
            Item.damage = 0;
            Item.knockBack = 1.5f;
            Item.value = Item.sellPrice(0, 0, 1, 0);
            Item.crit = 8;
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = true;
            Item.maxStack = 999;
            Item.consumable = true;
        }
    }

    internal class HourglassProj : ModProjectile
    {

        public bool activated = false;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hourglass");
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 1500;
            Projectile.penetrate = 1;
            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override void AI()
        {
            base.AI();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (activated)
            {
                Texture2D radiusTex = ModContent.Request<Texture2D>(Texture + "_Radius").Value;
                Main.spriteBatch.Draw(radiusTex, Projectile.Center - Main.screenPosition, null, Color.White, 0, radiusTex.Size() / 2, 1, SpriteEffects.None, 0f);
            }
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            activated = true;
            return false;
        }
    }

    public class HourglassGNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public float waterMovementSpeedPublic = 0;

        public bool inRadius = false;

        public float aiCounter = 0;

        public float aiTicker = 0;

        public Vector2 oldPos = Vector2.Zero;

        public override void ResetEffects(NPC npc)
        {
            inRadius = false;
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            waterMovementSpeedPublic = (float)typeof(NPC).GetField("waterMovementSpeed", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(npc);
        }

        public override bool PreAI(NPC npc)
        {
            oldPos = npc.position;
            if (npc.boss)
                return true;
            var hourglass = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<HourglassProj>() && (n.ModProjectile as HourglassProj).activated && n.Distance(npc.Center) < 200).OrderBy(n => n.Distance(npc.Center)).FirstOrDefault();

            if (hourglass != default)
            {
                inRadius = true;
                aiTicker = MathHelper.Lerp(0.25f, 1f, hourglass.Distance(npc.Center) / 200f);
            }
            else
                return true;

            aiCounter += aiTicker;

            if (aiCounter > 1)
            {
                aiCounter -= 1;
                return true;
            }
            return false;
        }

        public override void PostAI(NPC npc)
        {
            float waterMult = npc.wet ? waterMovementSpeedPublic : 1;
            if (inRadius)
            {
                if (!npc.collideX)
                    npc.position.X -= npc.velocity.X * (1 - aiTicker) * waterMult;

                if (!npc.collideY)
                    npc.position.Y -= npc.velocity.Y * (1 - aiTicker) * waterMult;
            }
        }
    }

    public class HourglassGProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;


        public bool inRadius = false;

        public float aiCounter = 0;

        public float aiTicker = 0;

        public override bool PreAI(Projectile projectile)
        {
            inRadius = false;
            if (projectile.friendly || projectile.type == ModContent.ProjectileType<HourglassProj>())
                return true;

            var hourglass = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<HourglassProj>() && (n.ModProjectile as HourglassProj).activated && n.Distance(projectile.Center) < 200).OrderBy(n => n.Distance(projectile.Center)).FirstOrDefault();

            if (hourglass != default)
            {
                inRadius = true;
                aiTicker = MathHelper.Lerp(0.0f, 0.75f, hourglass.Distance(projectile.Center) / 200f);
            }
            else
                return true;

            aiCounter += aiTicker;

            if (aiCounter > 1)
            {
                aiCounter -= 1;
                return true;
            }
            projectile.timeLeft++;
            return false;
        }

        public override void PostAI(Projectile projectile)
        {
            float waterFactor = (projectile.wet && !projectile.ignoreWater) ? 0.5f : 1;
            if (inRadius)
            {
                projectile.position -= projectile.velocity * (1 - aiTicker) * waterFactor;
            }
        }
    }
}