using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using JadeFables.Dusts;

namespace JadeFables.Items.Lotus.LotusFiber
{
    public class LotusFiber : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lotus Fiber");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;

            Item.maxStack = 999;
            Item.value = 0;
            Item.rare = ItemRarityID.White;
        }
    }
}
