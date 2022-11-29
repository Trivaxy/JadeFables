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
using Terraria.GameContent;
using JadeFables.Biomes.JadeLake;
using Terraria.Graphics.Light;
using Terraria.Graphics;
using Terraria.GameContent.Liquid;

namespace JadeFables
{
	public class JadeFables : Mod
	{
		public static JadeFables Instance;
		public override void Load()
		{
			Instance = this;
			Main.QueueMainThreadAction(() =>
			{
				LoadDetours();
			});
		}
		
		public override void Unload()
		{
			UnloadDetours();
		}
		
		public void LoadDetours()
		{
			On.Terraria.Main.DrawBlack += Main_DrawBlack;
			On.Terraria.GameContent.Drawing.TileDrawing.DrawPartialLiquid += TileDrawing_DrawPartialLiquid;
			On.Terraria.Main.DrawWater += WaterAlphaMod;
		}
		
		public void UnloadDetours()
		{
			On.Terraria.Main.DrawBlack -= Main_DrawBlack;
			On.Terraria.GameContent.Drawing.TileDrawing.DrawPartialLiquid -= TileDrawing_DrawPartialLiquid;
			On.Terraria.Main.DrawWater -= WaterAlphaMod;
		}
		
		private void Main_DrawBlack(On.Terraria.Main.orig_DrawBlack orig, Main self, bool force)
		{
		    Vector2 value = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
		    int num = (Main.tileColor.R + Main.tileColor.G + Main.tileColor.B) / 3;
		    float num2 = (float)((double)num * 0.4) / 255f;
		    if (Lighting.Mode == LightMode.Retro)
		    {
			num2 = (float)(Main.tileColor.R - 55) / 255f;
			if (num2 < 0f)
			{
			    num2 = 0f;
			}
		    }
		    else if (Lighting.Mode == LightMode.Trippy)
		    {
			num2 = (float)(num - 55) / 255f;
			if (num2 < 0f)
			{
			    num2 = 0f;
			}
		    }
		    Point screenOverdrawOffset = Main.GetScreenOverdrawOffset();
		    Point point = new Point(-Main.offScreenRange / 16 + screenOverdrawOffset.X, -Main.offScreenRange / 16 + screenOverdrawOffset.Y);
		    int num3 = (int)((Main.screenPosition.X - value.X) / 16f - 1f) + point.X;
		    int num4 = (int)((Main.screenPosition.X + (float)Main.screenWidth + value.X) / 16f) + 2 - point.X;
		    int num5 = (int)((Main.screenPosition.Y - value.Y) / 16f - 1f) + point.Y;
		    int num6 = (int)((Main.screenPosition.Y + (float)Main.screenHeight + value.Y) / 16f) + 5 - point.Y;
		    if (num3 < 0)
		    {
			num3 = point.X;
		    }
		    if (num4 > Main.maxTilesX)
		    {
			num4 = Main.maxTilesX - point.X;
		    }
		    if (num5 < 0)
		    {
			num5 = point.Y;
		    }
		    if (num6 > Main.maxTilesY)
		    {
			num6 = Main.maxTilesY - point.Y;
		    }
		    if (!force)
		    {
			if (num5 < Main.maxTilesY / 2)
			{
			    num6 = Math.Min(num6, (int)Main.worldSurface + 1);
			    num5 = Math.Min(num5, (int)Main.worldSurface + 1);
			}
			else
			{
			    num6 = Math.Max(num6, Main.UnderworldLayer);
			    num5 = Math.Max(num5, Main.UnderworldLayer);
			}
		    }
		    for (int i = num5; i < num6; i++)
		    {
			bool flag = i >= Main.UnderworldLayer;
			if (flag)
			{
			    num2 = 0.2f;
			}
			for (int j = num3; j < num4; j++)
			{
			    int num7 = j;
			    for (; j < num4; j++)
			    {
				if (!WorldGen.InWorld(j, i))
				{
				    return;
				}
				if (Main.tile[j, i] == null)
				{
				    Main.tile[j, i].ClearEverything();
				}
				Tile tile = Main.tile[j, i];
				float num8 = Lighting.Brightness(j, i);
				num8 = (float)Math.Floor(num8 * 255f) / 255f;
				byte b = tile.LiquidAmount;

				if (num8 > 0f || ((flag || b >= 250) && !WorldGen.SolidTile(tile) && (b < 200 || num8 != 0f)) || (WallID.Sets.Transparent[tile.WallType] && (!Main.tile[j, i].HasTile || !Main.tileBlockLight[(int)tile.BlockType])) || (!Main.drawToScreen && LiquidRenderer.Instance.HasFullWater(j, i) && tile.WallType == 0 && !tile.IsHalfBlock && !((double)i <= Main.worldSurface)))
				{
				    if ((Framing.GetTileSafely(j, i).Slope != SlopeType.Solid && (Framing.GetTileSafely(j, i - 1).LiquidAmount > 0 || Framing.GetTileSafely(j, i + 1).LiquidAmount > 0 || Framing.GetTileSafely(j + 1, i).LiquidAmount > 0 || Framing.GetTileSafely(j - 1, i).LiquidAmount > 0)) || (Framing.GetTileSafely(j, i).IsHalfBlock && Framing.GetTileSafely(j, i - 1).LiquidAmount > 0))
				    {
					if(num8 > 0)
					    break;
				    }
				    else
				    {
					break;
				    }
				}
			    }
			    if (j - num7 > 0)
			    {
				Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Vector2(num7 << 4, i << 4) - Main.screenPosition + value, new Rectangle(0, 0, j - num7 << 4, 16), Microsoft.Xna.Framework.Color.Black);
			    }
			}
		    }
		}

		private unsafe void TileDrawing_DrawPartialLiquid(On.Terraria.GameContent.Drawing.TileDrawing.orig_DrawPartialLiquid orig, Terraria.GameContent.Drawing.TileDrawing self, Tile tileCache, Vector2 position, Rectangle liquidSize, int liquidType, Color aColor)
		{
		    Vector2 rectangle = new Vector2(Main.offScreenRange, Main.offScreenRange);
		    if (Main.drawToScreen)
		    {
			rectangle = Vector2.Zero;
		    }

		    Vector2 archivePos = position;

		    position += Main.screenPosition;
		    position -= rectangle;

		    Vector2 drawOffset = (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange)) - Main.screenPosition;

		    int i = (int)((position.X) / 16f);
		    int j = (int)((position.Y) / 16f);

		    int slope = (int)tileCache.Slope;

		    float[] DEFAULT_OPACITY = new float[3]
		    {
			    0.6f,
			    0.95f,
			    0.95f
		    };

		    float num = DEFAULT_OPACITY[Main.tile[i, j].LiquidType];
		    int num2 = Main.tile[i, j].LiquidType;
		    switch (num2)
		    {
			case 0:
				num2 = Main.waterStyle;
				num *= 0.75f;
			    break;
			case 2:
			    num2 = 11;
			    break;
		    }


		    num = Math.Min(1f, num);

		    if (Framing.GetTileSafely(i, j).WallType != 0) num = 0.5f;

		    Lighting.GetCornerColors(i, j, out VertexColors vertices);

		    vertices.BottomLeftColor *= num;
		    vertices.BottomRightColor *= num;
		    vertices.TopLeftColor *= num;
		    vertices.TopRightColor *= num;

		    if (!TileID.Sets.BlocksWaterDrawingBehindSelf[(int)tileCache.BlockType] || slope == 0)
		    {
			Main.tileBatch.Begin();

			Main.DrawTileInWater(drawOffset, i, j);

			if (Main.tile[i, j].IsHalfBlock && Main.LocalPlayer.InModBiome<JadeLakeBiome>())
			    Main.tileBatch.Draw(TextureAssets.Liquid[num2].Value, archivePos + new Vector2(0, 8), liquidSize, vertices, default(Vector2), 1f, SpriteEffects.None);
			else if(Main.tile[i, j].IsHalfBlock)
			    Main.tileBatch.Draw(TextureAssets.Liquid[num2].Value, archivePos, liquidSize, vertices, default(Vector2), 1f, SpriteEffects.None);
			else
			    Main.tileBatch.Draw(TextureAssets.Liquid[num2].Value, archivePos, liquidSize, vertices, default(Vector2), 1f, SpriteEffects.None);

			Main.tileBatch.End();

			return;
		    }

		    liquidSize.X += 18 * (slope - 1);

		    if (tileCache.Slope == (SlopeType)1 || tileCache.Slope == (SlopeType)2 || tileCache.Slope == (SlopeType)3 || tileCache.Slope == (SlopeType)4)
		    {
			Main.NewText("NO WAY!!!");
			Main.spriteBatch.Draw(TextureAssets.LiquidSlope[num2].Value, archivePos, liquidSize, aColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		    }
		}

		private void WaterAlphaMod(On.Terraria.Main.orig_DrawWater orig, Main self, bool bg, int Style, float Alpha)
		{
		    orig(self, bg, Style, Main.LocalPlayer.InModBiome<JadeLakeBiome>() ? Alpha / 3f : Alpha);
		}
	}
}
