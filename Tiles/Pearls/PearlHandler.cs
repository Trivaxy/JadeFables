using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Map;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace JadeFables.Tiles.Pearls
{
	public class PearlHandler : ModSystem
	{
		public override void Load()
		{
			Terraria.On_Main.DoDraw_DrawNPCsBehindTiles += DrawPearls;
		}

		public override void Unload()
		{
			Terraria.On_Main.DoDraw_DrawNPCsBehindTiles -= DrawPearls;
		}

		public override void PreUpdateDusts()
		{
			foreach (KeyValuePair<int, TileEntity> item in TileEntity.ByID)
			{
				if (item.Value is Pearl pearl && pearl.IsOnScreen())
                    pearl.CreateSparkles();
			}
		}

		public void DrawPearls(Terraria.On_Main.orig_DoDraw_DrawNPCsBehindTiles orig, Main self)
		{
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			foreach (KeyValuePair<int, TileEntity> item in TileEntity.ByID)
			{
                if (item.Value is Pearl pearl && pearl.IsOnScreen())
                    pearl.Draw(Main.spriteBatch);
			}

			Main.spriteBatch.End();

			orig(self);
		}
	}
}