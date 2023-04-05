using JadeFables.Items.Potions.Heartbeat;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Recipe;
using JadeFables.Biomes.JadeLake;
using Terraria.Localization;
using Terraria.DataStructures;
using Terraria.Audio;
using JadeFables.Items.Jade.JadeChunk;

namespace JadeFables.Items.Potions.JasmineTea
{
    public class JasmineTea : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Jasmine Tea");
            // Tooltip.SetDefault("'Do you know why they call me The Dragon of The West?'");
            ItemID.Sets.DrinkParticleColors[Item.type] = new Color[1] { Color.Green };
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

            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Blue;

            Item.consumable = true;
        }

        //Yes we need this for UseStyle() to run
        public override bool? UseItem(Player player)
        {
            return true;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.itemTime == 2) //We want there to be a delay for the buff and sfx
            {
                SoundStyle stylea = new SoundStyle("Terraria/Sounds/Custom/dd2_betsy_flame_breath") with { Volume = 0.6f, Pitch = 0.64f };
                SoundEngine.PlaySound(stylea, player.Center);

                SoundStyle styleb = new SoundStyle("Terraria/Sounds/Custom/dd2_betsy_fireball_shot_2") with { Volume = 0.6f, Pitch = 0.42f };
                SoundEngine.PlaySound(styleb, player.Center);

                SoundEngine.PlaySound(SoundID.Item3, player.Center);

                player.AddBuff(ModContent.BuffType<JasmineTeaBuff>(), 300);
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.BottledWater, 1);
            recipe.AddIngredient<Tiles.JasmineFlower.JasmineFlower>(1);
            recipe.AddTile(TileID.TeaKettle);
            recipe.Register();
        }
    }

    public class JasmineTeaBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Jasmine Tea");
            // Description.SetDefault("You need a mint!");
            Main.buffNoSave[Type] = true;
        }

        private int timer = 0;
        public override void Update(Player player, ref int buffIndex)
        {
            
            if (timer % 8 == 0)
            {
                SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_betsy_fireball_shot_1") with { Pitch = -.53f, PitchVariance = 0.5f, MaxInstances = -1, Volume = 0.2f };
                SoundEngine.PlaySound(style, player.Center);
            }

            for (int i = 0; i < 2; i++)
                Projectile.NewProjectileDirect(player.GetSource_Buff(buffIndex), player.Center + new Vector2(player.direction * 4, -7), new Vector2(player.direction, 0).RotatedByRandom(0.3f) * Main.rand.NextFloat(3, 8), ModContent.ProjectileType<JasmineTeaFire>(), 20, 0, player.whoAmI);

            timer++;
        }
    }
    internal class JasmineTeaFire : ModProjectile
    {
        Color color = Color.White;

        public override void Load()
        {
            Terraria.On_Main.DrawPlayers_AfterProjectiles += Main_DrawPlayers_AfterProjectiles;
        }

        private void Main_DrawPlayers_AfterProjectiles(Terraria.On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
        {
            orig(self);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile is JasmineTeaFire modProj)
                {
                    modProj.DrawOverPlayer();
                }
            }
            Main.spriteBatch.End();
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Jasmine Fire");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = Main.rand.NextFloat(0.1f, 0.5f);
            Projectile.rotation = Main.rand.NextFloat(6.28f);
        }

        public override void AI()
        {
            Projectile.velocity *= 0.95f;

            Projectile.scale += 0.01f;

            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.6f);

            if (Projectile.timeLeft > 30)
            {
                color = Color.Lerp(Color.Yellow, Color.OrangeRed, 1 - ((Projectile.timeLeft - 30) / 30f));
            }
            else
            {
                color = Color.Lerp(Color.OrangeRed, Color.Gray, 1 - (Projectile.timeLeft / 30f));
            }
        }

        public void DrawOverPlayer()
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            float opacity = Projectile.timeLeft / 60f;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color * opacity, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 180);
        }
    }
}
