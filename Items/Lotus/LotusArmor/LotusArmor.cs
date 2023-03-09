using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace JadeFables.Items.Lotus.LotusArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class LotusChestplate : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lotus Chestplate");
            Tooltip.SetDefault("Increased fishing power");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = 0;
            Item.rare = ItemRarityID.White;
            Item.defense = 2;
        }

        public override void UpdateEquip(Player player)
        {
            player.fishingSkill += 10;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<LotusFiber.LotusFiber>(30)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    [AutoloadEquip(EquipType.Head)]
    public class LotusHelmet : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lotus Helmet");
            Tooltip.SetDefault("Increased fishing power");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = 0;
            Item.rare = ItemRarityID.White;
            Item.defense = 1;
        }

        public override void UpdateEquip(Player player)
        {
            player.fishingSkill += 10;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<LotusFiber.LotusFiber>(20)
                .AddTile(TileID.WorkBenches)
                .Register();
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<LotusChestplate>() && legs.type == ModContent.ItemType<LotusLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Decreases enemy spawnrates by 50%"; 
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class LotusLeggings : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lotus Greaves");
            Tooltip.SetDefault("Increased fishing power");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = 0;
            Item.rare = ItemRarityID.White;
            Item.defense = 1;
        }

        public override void UpdateEquip(Player player)
        {
            player.fishingSkill += 10;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<LotusFiber.LotusFiber>(25)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
