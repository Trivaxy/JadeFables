//TODO:
//Actual sprite
//Sparkles (maybe)
//Tooltip (maybe)
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using JadeFables.Dusts;

namespace JadeFables.Items.Jade.JadeChunk
{
    public class JadeChunk : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Chunk");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 999;
        }
    }
}
