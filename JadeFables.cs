global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using static Terraria.ModLoader.ModContent;
global using Terraria;
global using Terraria.ModLoader;
global using Terraria.ID;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace JadeFables
{
    public class TemporaryFix : PreJITFilter
    {
        public override bool ShouldJIT(MemberInfo member)
        {
            return false;
        }
    }
    public class JadeFables : Mod
    {
        public static JadeFables Instance;

        public JadeFables()
        {
            PreJITFilter = new TemporaryFix();
        }
        public override void Load()
        {
            Instance = this;
            Main.QueueMainThreadAction(() =>
            {
                LoadDetours();
            });

            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.AddEquipTexture(Instance, "JadeFables/Items/Jade/JadeArmor/JadeRobe_Legs", EquipType.Legs, null, "JadeRobe_Legs");

                BackgroundTextureLoader.AddBackgroundTexture(JadeFables.Instance, "JadeFables/Biomes/JadeLake/JadeBiomeUnderground0");
                BackgroundTextureLoader.AddBackgroundTexture(JadeFables.Instance, "JadeFables/Biomes/JadeLake/JadeBiomeUnderground1");
                BackgroundTextureLoader.AddBackgroundTexture(JadeFables.Instance, "JadeFables/Biomes/JadeLake/JadeBiomeUnderground2");
                BackgroundTextureLoader.AddBackgroundTexture(JadeFables.Instance, "JadeFables/Biomes/JadeLake/JadeBiomeUnderground3");
            }
        }

        public override void Unload()
        {
            UnloadDetours();
        }

        public void LoadDetours()
        {
            //Terraria.On_Main.DrawWater += WaterAlphaMod;
        }

        public void UnloadDetours()
        {
            //Terraria.On_Main.DrawWater -= WaterAlphaMod;
        }

        /*private void WaterAlphaMod(Terraria.On_Main.orig_DrawWater orig, Main self, bool bg, int Style, float Alpha)
		{
		    orig(self, bg, Style, Main.LocalPlayer.InModBiome<JadeLakeBiome>() ? Alpha * 0.3f : Alpha);
		}*/
    }
}
