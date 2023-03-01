using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using JadeFables.Core;
using ReLogic.Content;
using JadeFables.Helpers;
using Terraria.Graphics.Effects;
using SteelSeries.GameSense;
using IL.Terraria.Audio;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using static Humanizer.In;
using static System.Net.Mime.MediaTypeNames;
using Terraria.Enums;

namespace JadeFables.Items.SpringChestLoot.Chopsticks
{
	public class Chopsticks : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Chopsticks");
			Tooltip.SetDefault("Non projectile swords have more range\nEnemies drop food more often");
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ChopstickPlayer>().equipped = true;
		}
	}

	public class ChopstickPlayer : ModPlayer
	{
		public bool equipped = false;

        public override void ResetEffects()
        {
            equipped = false;
        }

        public override bool CanUseItem(Item item)
        {
            if (!equipped)
                return base.CanUseItem(item);

            if (item != Player.HeldItem)
                return true;

            if (item.DamageType.Type == DamageClass.Melee.Type && item.pick <= 0 && item.axe <= 0 && item.hammer <= 0 && item.shoot <= ProjectileID.None && item.useStyle == Terraria.ID.ItemUseStyleID.Swing && !item.noMelee)
            {
                if (Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<ChopstickProj>() && n.owner == Player.whoAmI))
                    return false;

                int i = Projectile.NewProjectile(Player.GetSource_ItemUse(item), Player.Center, Vector2.Zero, ModContent.ProjectileType<ChopstickProj>(), item.damage, item.knockBack, Player.whoAmI);
                Projectile proj = Main.projectile[i];

                proj.timeLeft = item.useAnimation;
                proj.scale = item.scale;

                if (proj.ModProjectile is ChopstickProj)
                {
                    var modProj = proj.ModProjectile as ChopstickProj;
                    modProj.swordTexture = TextureAssets.Item[item.type].Value;
                    modProj.length = ((float)Math.Sqrt(Math.Pow(modProj.swordTexture.Width, 2) + Math.Pow(modProj.swordTexture.Width, 2)) * item.scale) + 40;
                    modProj.lifeSpan = item.useAnimation;
                    modProj.baseAngle = (Main.MouseWorld - Player.Center).ToRotation();
                    modProj.itemScale = item.scale;
                }

                if (item.UseSound.HasValue)
                    Terraria.Audio.SoundEngine.PlaySound(item.UseSound.Value, Player.Center);

                return false;
            }

			return true;
		}
    }

    public class ChopstickProj : ModProjectile
    {
        public readonly float HALFSWINGARC = 1.57f;

        private List<NPC> alreadyHit = new List<NPC>();

        public float length;
        public Texture2D swordTexture;
        public float lifeSpan;
        public float baseAngle;
        public float itemScale;

        public float Progress => 1 - Projectile.timeLeft / (float)lifeSpan;
        public int Direction => (Math.Abs(baseAngle) < Math.PI / 2f) ? 1 : -1;
        public Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.penetrate = 1;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Owner.direction = Direction;
            Owner.heldProj = Projectile.whoAmI;

            Projectile.rotation = baseAngle + MathHelper.Lerp(-HALFSWINGARC * Owner.direction, HALFSWINGARC * Owner.direction, Progress);

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
            Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Projectile.Center;
            Vector2 end = Projectile.Center + (Projectile.rotation.ToRotationVector2() * length);
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15, ref collisionPoint);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D stickTex = ModContent.Request<Texture2D>(Texture).Value;

            if (swordTexture == null)
                return false;
            Main.spriteBatch.Draw(swordTexture, Projectile.Center + new Vector2(0, Owner.gfxOffY) + (29 * Projectile.rotation.ToRotationVector2()) - Main.screenPosition, null, lightColor, (Projectile.rotation + 0.78f) + (Owner.direction == 1 ? 0f : 1.57f), new Vector2(Owner.direction == 1 ? 0 : swordTexture.Width, swordTexture.Height), Projectile.scale * itemScale, Owner.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(stickTex, Projectile.Center + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, null, lightColor, Projectile.rotation + 1.47f, stickTex.Bounds.Bottom(), Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(stickTex, Projectile.Center + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, null, lightColor, Projectile.rotation + 1.67f, stickTex.Bounds.Bottom(), Projectile.scale, SpriteEffects.None, 0f);
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

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            hitDirection = Math.Sign(target.Center.X - Owner.Center.X);
        }

        public override bool? CanCutTiles()
        {
            return true;
        }

        public override void CutTiles()
        {
            Vector2 start = Projectile.Center;
            Vector2 end = Projectile.Center + (Projectile.rotation.ToRotationVector2() * length);
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.PlotTileLine(start, end, 15, DelegateMethods.CutTiles);
        }
        }
    public class ChopsticksNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool destroyNonFood = false;
        readonly int FOODMULT = 100;

        public override void Load()
        {
            On.Terraria.Item.NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool += ItemDetour1; //covering ALL my bases here
            On.Terraria.Item.NewItem_IEntitySource_Rectangle_int_int_bool_int_bool_bool += ItemDetour2;
            On.Terraria.Item.NewItem_IEntitySource_Vector2_int_int_bool_int_bool_bool += ItemDetour3;
            On.Terraria.Item.NewItem_IEntitySource_Vector2_int_int_int_int_bool_int_bool_bool += ItemDetour4;
            On.Terraria.Item.NewItem_IEntitySource_Vector2_Vector2_int_int_bool_int_bool_bool += ItemDetour5;
            On.Terraria.NPC.NPCLoot_DropItems += NPC_NPCLoot_DropItems;
        }

        private void NPC_NPCLoot_DropItems(On.Terraria.NPC.orig_NPCLoot_DropItems orig, NPC self, Player closestPlayer)
        {
            if (closestPlayer.GetModPlayer<ChopstickPlayer>().equipped)
            {
                self.GetGlobalNPC<ChopsticksNPC>().destroyNonFood = true;
                for (int i = 0; i < FOODMULT; i++)
                {
                   // orig(self, closestPlayer);
                }
                self.GetGlobalNPC<ChopsticksNPC>().destroyNonFood = false;
            }
            orig(self, closestPlayer);
        }

        private int ItemDetour5(On.Terraria.Item.orig_NewItem_IEntitySource_Vector2_Vector2_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, Vector2 pos, Vector2 randomBox, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, pos, randomBox, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
        }

        private int ItemDetour4(On.Terraria.Item.orig_NewItem_IEntitySource_Vector2_int_int_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, Vector2 pos, int Width, int Height, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, pos, Width, Height, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
        }

        private int ItemDetour3(On.Terraria.Item.orig_NewItem_IEntitySource_Vector2_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, Vector2 position, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, position, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
        }

        private int ItemDetour2(On.Terraria.Item.orig_NewItem_IEntitySource_Rectangle_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, Rectangle rectangle, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, rectangle, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
        }

        private int ItemDetour1(On.Terraria.Item.orig_NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool orig, Terraria.DataStructures.IEntitySource source, int X, int Y, int Width, int Height, int Type, int Stack, bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
        {
            if (DestroyNonFood(source, Type))
                return -1;
            return orig(source, X, Y, Width, Height, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
        }

        public static bool DestroyNonFood(IEntitySource source, int Type)
        {
            if (source is EntitySource_Loot lootSource && lootSource.Entity is NPC npc)
            {
                if (npc.GetGlobalNPC<ChopsticksNPC>().destroyNonFood && !IsFood(Type))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsFood(int type)
        {
            Item item = new Item(type);
            item.SetDefaults(type);
            if (item.buffType == BuffID.WellFed || item.buffType == BuffID.WellFed2 || item.buffType == BuffID.WellFed3)
                return true;
            return false;
        }
    }
}
