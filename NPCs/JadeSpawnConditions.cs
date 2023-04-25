using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;

namespace JadeFables.NPCs
{
    public static class JadeSpawnConditions
    {

        public static ModBiomeSpawnCondition JadeSprings = new ModBiomeSpawnCondition("Jade Lake", "JadeFables/Assets/JadeIcon", "JadeFables/Biomes/JadeLake/JadeBG", Color.White);

        public static void Unload()
        {
            JadeSprings = null;
        }
    }
}
