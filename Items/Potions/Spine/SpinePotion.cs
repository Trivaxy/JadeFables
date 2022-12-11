using Terraria.ID;

// TODO : homing behavior, better vfx, some sort of trail?

namespace JadeFables.Items.Potions.Spine
{
    public class SpinePotion : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spine Potion");
            Tooltip.SetDefault("Taking any damage fires spines at nearby enemies");
            ItemID.Sets.DrinkParticleColors[Item.type] = new Color[3] { new Color(73, 130, 6), new Color(144, 169, 40), new Color(192, 212, 110) }; 
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 32;
            Item.maxStack = 30; //Change this when Labor of Love drops?

            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;

            Item.consumable = true;
            Item.buffType = (ModContent.BuffType<SpineBuff>());
            Item.buffTime = 21600;

            Item.UseSound = SoundID.Item3;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.BottledWater);//recipe.AddIngredient(ItemID.ThornsPotion); i think this is too expensive since the single target damage is worse than thorns
            recipe.AddIngredient(ItemID.Shiverthorn);
            recipe.AddIngredient<SpineItem>(1);
            recipe.AddIngredient<Jade.JadeChunk.JadeChunk>(1);
            recipe.AddTile(TileID.Bottles);
            recipe.Register();
        }
    }
    public class SpineItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spine");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 26;

            Item.maxStack = 999; //change after labor of love
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;
        }
    }

    public class SpineBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spine");
            Description.SetDefault("Taking any damage fires spines at nearby enemies");
            Main.buffNoSave[Type] = true;
        }
    }
    public class SpinePlayer : ModPlayer
    {
        public override void PostHurt(bool pvp, bool quiet, double damageTaken, int hitDirection, bool crit, int cooldownCounter)
        {
            if (!Player.HasBuff<SpineBuff>()) return;

            if (Main.myPlayer != Player.whoAmI) return;

            const int rangePixels = 400;
            const float vel = 25f;
            const float damageMult = 0.66f;

            const int maxTargets = 3;
            int targets = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (npc.CanBeChasedBy() && Vector2.DistanceSquared(Player.Center, npc.Center) < rangePixels * rangePixels)
                {
                    int p = Projectile.NewProjectile(Player.GetSource_Buff(Player.FindBuffIndex(BuffType<SpineBuff>())), Player.Center, Vector2.Normalize(npc.Center - Player.Center) * vel, ProjectileType<SpineProj>(), (int)(damageTaken * damageMult), 2f, Main.myPlayer, 0, 0); //damage scales with damage taken. could scale based on player damage stats?
                    (Main.projectile[p].ModProjectile as SpineProj).npcToTarget = npc;
                    targets++;
                    if (targets == maxTargets) break;
                }
            }
        }
    }
    public class SpineProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spine");
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.aiStyle = 1;
        }
        public NPC? npcToTarget = null;
        public override bool? CanHitNPC(NPC target)
        {
            if (npcToTarget == null) return false;
            return (target == npcToTarget);
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.Center, 0, 0, DustID.BrownMoss);
            }
        }
    }
}
