using JadeFables.Biomes.JadeLake;
using JadeFables.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
using static Terraria.ModLoader.ModContent;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace JadeFables.NPCs
{
    public class JadeSpawnPool : GlobalNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome(ModContent.GetInstance<JadeLakeBiome>())) 
            {
                if (!spawnInfo.Water)
                {
                    pool[NPCID.GreenDragonfly] = 35f;
                    pool[NPCID.RedDragonfly] = 35f;
                    pool[NPCID.BlueDragonfly] = 35f;
                    pool[NPCID.YellowDragonfly] = 35f;
                }
            }
        }
    }
}