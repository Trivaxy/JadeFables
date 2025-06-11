using JadeFables.NPCs.Koi;

namespace JadeFables.Tiles.Banners;

public class Banners
{
    public class GiantSnailBannerTile : ModBannerTile;
    public class BullfrogBannerTile : ModBannerTile;
    public class JadeMantisBannerTile : ModBannerTile;
    public class MediumKoiBannerTile : ModBannerTile;
    public class PufferfishBannerTile : ModBannerTile;
    
    public class GiantSnailBannerItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.createTile = TileType<GiantSnailBannerTile>();
            Item.width = 12;
            Item.height = 12;
        }
    }
    
    public class BullfrogBannerItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.createTile = TileType<BullfrogBannerTile>();
            Item.width = 12;
            Item.height = 12;
        }
    }
    
    public class JadeMantisBannerItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.createTile = TileType<JadeMantisBannerTile>();
            Item.width = 12;
            Item.height = 12;
        }
    }
    
    public class MediumKoiBannerItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.createTile = TileType<MediumKoiBannerTile>();
            Item.width = 12;
            Item.height = 12;
        }
    }
    
    public class PufferfishBannerItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.createTile = TileType<PufferfishBannerTile>();
            Item.width = 12;
            Item.height = 12;
        }
    }
}