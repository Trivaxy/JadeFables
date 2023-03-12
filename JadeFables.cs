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
using JadeFables.Biomes.JadeLake;
using JadeFables.Core;
using System.Reflection;

namespace JadeFables
{
	public class JadeFables : Mod
	{
        public List<IOrderedLoadable> loadCache;

        public static JadeFables Instance;
		public override void Load()
		{
			Instance = this;
			Main.QueueMainThreadAction(() =>
			{
				LoadDetours();
			});

            loadCache = new List<IOrderedLoadable>();

            foreach (Type type in Code.GetTypes())
            {
                if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IOrderedLoadable)))
                {
                    object instance = Activator.CreateInstance(type);
                    loadCache.Add(instance as IOrderedLoadable);
                }

                loadCache.Sort((n, t) => n.Priority.CompareTo(t.Priority));
            }

            for (int k = 0; k < loadCache.Count; k++)
            {
                loadCache[k].Load();
                SetLoadingText("Loading " + loadCache[k].GetType().Name);
            }


            if (Main.netMode != NetmodeID.Server)
			{
				EquipLoader.AddEquipTexture(Instance, "JadeFables/Items/Jade/JadeArmor/JadeRobe_Legs", EquipType.Legs, null, "JadeRobe_Legs");

                BackgroundTextureLoader.AddBackgroundTexture(JadeFables.Instance, "JadeFables/Biomes/JadeLake/JadeBiomeUnderground0");
                BackgroundTextureLoader.AddBackgroundTexture(JadeFables.Instance, "JadeFables/Biomes/JadeLake/JadeBiomeUnderground1");
                BackgroundTextureLoader.AddBackgroundTexture(JadeFables.Instance, "JadeFables/Biomes/JadeLake/JadeBiomeUnderground2");
                BackgroundTextureLoader.AddBackgroundTexture(JadeFables.Instance, "JadeFables/Biomes/JadeLake/JadeBiomeUnderground3");
                BackgroundTextureLoader.AddBackgroundTexture(JadeFables.Instance, "JadeFables/Assets/Invisible");
            }
        }

		public override void Unload()
		{
			UnloadDetours();
		}

		public void LoadDetours()
		{
			On.Terraria.Main.DrawWater += WaterAlphaMod;
		}

		public void UnloadDetours()
		{
			On.Terraria.Main.DrawWater -= WaterAlphaMod;
		}

		private void WaterAlphaMod(On.Terraria.Main.orig_DrawWater orig, Main self, bool bg, int Style, float Alpha)
		{
		    orig(self, bg, Style, Main.LocalPlayer.InModBiome<JadeLakeBiome>() ? Alpha * 0.3f : Alpha);
		}

        public static void SetLoadingText(string text)
        {
            FieldInfo Interface_loadMods = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface")!.GetField("loadMods", BindingFlags.NonPublic | BindingFlags.Static)!;
            MethodInfo UIProgress_set_SubProgressText = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIProgress")!.GetProperty("SubProgressText", BindingFlags.Public | BindingFlags.Instance)!.GetSetMethod()!;

            UIProgress_set_SubProgressText.Invoke(Interface_loadMods.GetValue(null), new object[] { text });
        }
    }
}
