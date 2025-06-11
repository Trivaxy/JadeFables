using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Localization;
using Terraria.ObjectData;

namespace JadeFables.Tiles.Banners
{
    /// <summary>
    /// Default ModTile that acts as the basis for any ModTile in the mod that acts
    /// like an NPC banner from vanilla.
    /// </summary>
    [Autoload(false)]
    public class DefaultNPCBanner : ModTile
    {
        private static readonly Delegate AddSpecialPoint;

        public override string Texture => tileTexturePath;

        public override string Name => tileName;

        /// <summary>
        /// The npc type that this banner applies to.
        /// </summary>
        public readonly int npcType;

        /// <summary>
        /// The associated item type that this banner gets
        /// placed by.
        /// </summary>
        public readonly int itemType;

        /// <summary>
        /// The texture path for the tile.
        /// </summary>
        public readonly string tileTexturePath;

        /// <summary>
        /// The internal name of this banner tile.
        /// </summary>
        public readonly string tileName;

        static DefaultNPCBanner()
        {
            //Committing cardinal sins in order to call a one line private method
            //AddSpecialPoint = typeof(TileDrawing).GetMethod("AddSpecialPoint", BindingFlags.Instance | BindingFlags.NonPublic)!.CreateDelegate<Action<int, int, int>>(Main.instance.TilesRenderer);
        }

        public DefaultNPCBanner(int npcType, int itemType, string tileTexturePath, string tileName)
        {
            this.npcType = npcType;
            this.itemType = itemType;
            this.tileTexturePath = tileTexturePath;
            this.tileName = tileName;
        }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileID.Sets.DrawFlipMode[Type] = 1;

            TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Banners, 0));
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(13, 88, 130), Language.GetText("MapObject.Banner"));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            //This is what we need the itemType for
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 48, itemType);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer)
            {
                return;
            }

            Main.SceneMetrics.NPCBannerBuff[npcType] = true;
            Main.SceneMetrics.hasBanner = true;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                AddSpecialPoint.DynamicInvoke(i, j, 5 /* MultiTileVine */);
            }

            return false;
        }

        /// <summary>
        /// Adds the associated <seealso cref="DefaultNPCBanner"/> and <seealso cref="DefaultNPCBannerItem"/> instances
        /// for the passed in npc type. Should ONLY be called ONCE for each NPC type, and ONLY during loading in
        /// <seealso cref="ModNPC.IsLoadingEnabled"/>.
        /// </summary>
        /// <param name="mod">
        /// The mod. Has to be passed in, since <seealso cref="JadeFables.Instance"/> isn't initialized at this point yet.
        /// </param>
        /// <param name="npcType">
        /// The npc type this banner will belong to. Since you are calling this before the NPC is actually being loaded, you can
        /// use <seealso cref="NPCLoader.NPCCount"/>.
        /// </param>
        /// <param name="npcName">
        /// The name of the NPC that PREPENDS the texture path. For example, if the enemy is the Pufferfish, should be
        /// "Pufferfish", and the tile and item texture paths
        /// should be "PufferfishBannerTile" and "PufferfishBannerItem" respectively.
        /// </param>
        /// <param name="bannerType"> The item type of the banner item (that places the banner tile) created by this method. </param>
        public static void AddBannerAndItemForNPC(Mod mod, int npcType, string npcName, out int bannerType)
        {
            int bannerTileType = TileLoader.TileCount;
            int bannerItemType = ItemLoader.ItemCount;

            DefaultNPCBannerItem bannerItem = new(bannerTileType, $"{nameof(JadeFables)}/Assets/NPCBanners/{npcName}BannerItem", $"{npcName}BannerItem");
            mod.AddContent(new DefaultNPCBanner(npcType, bannerItemType, $"{nameof(JadeFables)}/Assets/NPCBanners/{npcName}BannerTile", $"{npcName}BannerTile"));
            mod.AddContent(bannerItem);

            bannerType = bannerItem.Type;
        }
    }

    /// <summary>
    /// Default ModItem that places its corresponding <seealso cref="DefaultNPCBanner"/> ModTile.
    /// </summary>
    [Autoload(false)]
    public class DefaultNPCBannerItem : ModItem
    {
        public override string Texture => itemTexturePath;

        public override string Name => itemName;

        protected override bool CloneNewInstances => true;

        /// <summary>
        /// The tile ID/type that this item places.
        /// </summary>
        [CloneByReference]
        public readonly int tileType;

        /// <summary>
        /// The texture path for this item.
        /// </summary>
        [CloneByReference]
        public readonly string itemTexturePath;

        /// <summary>
        /// The internal name of this banner item.
        /// </summary>
        [CloneByReference]
        public readonly string itemName;

        public DefaultNPCBannerItem(int tileType, string itemTexturePath, string itemName)
        {
            this.tileType = tileType;
            this.itemTexturePath = itemTexturePath;
            this.itemName = itemName;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.SlimeBanner);
            Item.createTile = tileType;
            Item.placeStyle = 0;
        }
    }
}