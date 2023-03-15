using System;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using JadeFables.Helpers;
using JadeFables.Dusts;

namespace JadeFables.Tiles.Pearls
{
	//TODO: Manually load generic artifacts once manual loading for tile entities is supported
	public abstract class Pearl : ModTileEntity
	{
		public virtual string TexturePath => "JadeFables/Tiles/Pearls/" + Name;

		/// <summary>
		/// Size of the artifact. In world coordinates, not tile coordinates
		/// </summary>
		public virtual Vector2 Size => new Vector2(16, 16);

		/// <summary>
		/// The dust the artifact creates.
		/// </summary>
		public virtual int SparkleDust => ModContent.DustType<PearlSparkle>();

		/// <summary>
		/// The rate at which sparkles spawn. Increase for lower spawnrate.
		/// </summary>
		public virtual int SparkleRate => 40;


		/// <summary>
		/// The item the artifact drops
		/// </summary>
		public virtual int ItemType { get; set; }

		public Vector2 WorldPosition => Position.ToVector2() * 16;

		public virtual void Draw(SpriteBatch spriteBatch)
		{
			GenericDraw(spriteBatch);
		}

		public override void Update()
		{
			CheckOpen();
		}

		public override bool IsTileValidForEntity(int x, int y)
		{
			return true;
		}

		public bool IsOnScreen()
		{
			return Helper.OnScreen(new Rectangle((int)WorldPosition.X - (int)Main.screenPosition.X, (int)WorldPosition.Y - (int)Main.screenPosition.Y, (int)Size.X, (int)Size.Y));
		}

		public void CreateSparkles()
		{
			Vector2 pos = WorldPosition + Size * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat());

			Color lightColor = Lighting.GetColor((pos / 16).ToPoint());
			if (lightColor == Color.Black)
				return;

			float sparkleMult = MathHelper.Max(lightColor.R, MathHelper.Max(lightColor.G, lightColor.B)) / 255f;

			if (sparkleMult == 0) //incase for whatever reason the Color.Black check wasn't enough
				return;

			int modifiedSparkleRate = (int)(SparkleRate / sparkleMult); //spawns sparkles relative to light level
			if (Main.rand.NextBool(modifiedSparkleRate))
				Dust.NewDustPerfect(WorldPosition + Size * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat()), SparkleDust, Vector2.Zero);
		}

		public void GenericDraw(SpriteBatch spriteBatch) //I have no idea why but the drawing is offset by -192 on each axis by default, so I had to correct it
		{
			Texture2D tex = ModContent.Request<Texture2D>(TexturePath).Value;

			var offScreen = new Vector2(Main.offScreenRange);
			if (Main.drawToScreen)
			{
				offScreen = Vector2.Zero;
			}

			spriteBatch.Draw(tex, WorldPosition - Main.screenPosition, null, Lighting.GetColor(Position.ToPoint()), 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
		}

		public void CheckOpen()
		{
			for (int i = 0; i < Size.X / 16; i++)
			{
				for (int j = 0; j < Size.Y / 16; j++)
				{
					Tile tile = Main.tile[i + Position.X, j + Position.Y];
					if (tile.HasTile)
						return;
				}
			}

			Kill(Position.X, Position.Y);
			Item.NewItem(Entity.GetSource_None(), WorldPosition, ItemType);
		}
	}

    public class BlackPearl : Pearl
    {
        public override int ItemType => ItemID.BlackPearl;
    }

    public class WhitePearl : Pearl
    {
        public override int ItemType => ItemID.WhitePearl;
    }

    public class PinkPearl : Pearl
    {
        public override int ItemType => ItemID.PinkPearl;
    }
}