using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.GameContent.TilePaintSystemV2;

namespace JadeFables.Helpers
{
	public static class DrawHelper
	{
        public static readonly BasicEffect basicEffect = Main.dedServ ? null : new BasicEffect(Main.graphics.GraphicsDevice);

        public static Vector2 PointAccur(this Vector2 input) => input.ToPoint().ToVector2();

        public static float ConvertX(float input) => input / (Main.screenWidth * 0.5f) - 1;

        public static float ConvertY(float input) => -1 * (input / (Main.screenHeight * 0.5f) - 1);

        public static Vector2 ConvertVec2(Vector2 input) => new Vector2(1, -1) * (input / (new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f) - Vector2.One);

        public static void DrawHitbox(this SpriteBatch spriteBatch, NPC NPC, Color color) => spriteBatch.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value, NPC.getRect().WorldToScreenCoords(), color);

        public static Rectangle WorldToScreenCoords(this Rectangle rect) => new Rectangle(rect.X - (int)Main.screenPosition.X, rect.Y - (int)Main.screenPosition.Y, rect.Width, rect.Height);

        public static void DrawTriangle(Texture2D tex, Vector2[] target, Vector2[] source)
        {
            if (basicEffect is null) return;

            basicEffect.TextureEnabled = true;
            basicEffect.Texture = tex;
            basicEffect.Alpha = 1;
            basicEffect.View = new Matrix
                (
                    Main.GameViewMatrix.Zoom.X, 0, 0, 0,
                    0, Main.GameViewMatrix.Zoom.X, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                );

            var gd = Main.graphics.GraphicsDevice;
            var points = new VertexPositionTexture[3];
            var buffer = new VertexBuffer(gd, typeof(VertexPositionTexture), 3, BufferUsage.WriteOnly);

            for (int k = 0; k < 3; k++)
                points[k] = new VertexPositionTexture(new Vector3(ConvertX(target[k].X), ConvertY(target[k].Y), 0), source[k] / tex.Size());

            buffer.SetData(points);

            gd.SetVertexBuffer(buffer);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            }

            gd.SetVertexBuffer(null);
        }

        public static void DrawLine(SpriteBatch spritebatch, Vector2 startPoint, Vector2 endPoint, Texture2D texture, Color color, Rectangle sourceRect = default)
        {
            Vector2 edge = endPoint - startPoint;
            // calculate angle to rotate line
            float angle =
                (float)Math.Atan2(edge.Y, edge.X);

            Vector2 offsetStart = startPoint + new Vector2(0, -(sourceRect.Width / 2)).RotatedBy(angle);//multiply before adding to startpoint to make the points closer

            spritebatch.Draw(texture,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)offsetStart.X,
                    (int)offsetStart.Y,
                    (int)edge.Length(), //sb will stretch the texture to fill this rectangle
                    sourceRect.Width), //width of line, change this to make thicker line (may have to offset?)
                sourceRect,
                color, //colour of line
                angle, //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                default);
        }

        public static void DrawElectricity(Vector2 point1, Vector2 point2, int dusttype, float scale = 1, int armLength = 30, Color color = default, float frequency = 0.05f)
        {
            int nodeCount = (int)Vector2.Distance(point1, point2) / armLength;
            Vector2[] nodes = new Vector2[nodeCount + 1];

            nodes[nodeCount] = point2; //adds the end as the last point

            for (int k = 1; k < nodes.Length; k++)
            {
                //Sets all intermediate nodes to their appropriate randomized dot product positions
                nodes[k] = Vector2.Lerp(point1, point2, k / (float)nodeCount) +
                    (k == nodes.Length - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.58f) * Main.rand.NextFloat(-armLength / 2, armLength / 2));

                //Spawns the dust between each node
                Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];
                for (float i = 0; i < 1; i += frequency)
                {
                    Dust.NewDustPerfect(Vector2.Lerp(prevPos, nodes[k], i), dusttype, Vector2.Zero, 0, color, scale);
                }
            }
        }

        /// <summary>
        /// Draws a flat colored block that respects slopes, for making multitile structures
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="spriteBatch"></param>
        public static void TileDebugDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            int height = tile.TileFrameY == 36 ? 18 : 16;
            if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                spriteBatch.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value, ((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height), Color.Magenta * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            else if (tile.IsHalfBlock)
                spriteBatch.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value, ((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition + new Vector2(0, 10), new Rectangle(tile.TileFrameX, tile.TileFrameY + 10, 16, 6), Color.Red * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            else
            {
                byte b3 = (byte)tile.Slope;
                int num34;
                for (int num226 = 0; num226 < 8; num226 = num34 + 1)
                {
                    int num227 = num226 << 1;
                    Rectangle value5 = new Rectangle(tile.TileFrameX, tile.TileFrameY + num226 * 2, num227, 2);
                    int num228 = 0;
                    switch (b3)
                    {
                        case 2:
                            value5.X = 16 - num227;
                            num228 = 16 - num227;
                            break;
                        case 3:
                            value5.Width = 16 - num227;
                            break;
                        case 4:
                            value5.Width = 14 - num227;
                            value5.X = num227 + 2;
                            num228 = num227 + 2;
                            break;
                    }
                    spriteBatch.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value, ((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition + new Vector2((float)num228, (num226 * 2)), value5, Color.Blue * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    num34 = num226;
                }
            }
        }
    }
    public static class PaintHelper
    {
        /*public static void DrawWithPaint(byte paintType, string texturePath, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            color = color.MultiplyRGBA(WorldGen.paintColor(paintType).MaxAlpha());

            Texture2D texture = GetVariantTexture(texturePath, GetEffectForPaint(paintType));
            Main.spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }
        public static void DrawWithPaint(byte paintType, string texturePath, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            color = color.MultiplyRGBA(WorldGen.paintColor(paintType).MaxAlpha());

            Texture2D texture = GetVariantTexture(texturePath, GetEffectForPaint(paintType));
            Main.spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }*/

        public static void DrawWithCoating(bool fullbright, bool invisible, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            if (invisible && !Main.ShouldShowInvisibleWalls()) return;
            if (fullbright) color = Color.White;
            Main.spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }
        public static void DrawWithCoating(bool fullbright, bool invisible, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            if (invisible && !Main.ShouldShowInvisibleWalls()) return;
            if (fullbright) color = Color.White;
            Main.spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }

        /*public static Color MaxAlpha(this Color color)
        {
            return new Color(color.R, color.G, color.B, 255);
        }

        public static VariantEffect? GetEffectForPaint(byte paintType)
        {
            if (paintType == PaintID.None || paintType == PaintID.IlluminantPaint) return None;
            else if (paintType == PaintID.NegativePaint) return MakeNegative;
            else return MakeGrayscale;
        }

        public static Dictionary<string, Texture2D> textureVariants = new Dictionary<string, Texture2D>();

        public static Texture2D GetVariantTexture(string texturePath, VariantEffect effect)
        {
            string key = texturePath + effect.Method.Name;
            if (!textureVariants.ContainsKey(key)) textureVariants[key] = InitializeVariantTexture(texturePath, effect);
            return textureVariants[key];
        }

        private static Texture2D InitializeVariantTexture(string texturePath, VariantEffect effect)
        {
            Texture2D baseTexture = Request<Texture2D>(texturePath, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (effect == None) return baseTexture;

            Texture2D variantTexture = new Texture2D(Main.graphics.GraphicsDevice, baseTexture.Width, baseTexture.Height);
            Main.QueueMainThreadAction(() =>
            {
                Color[] pixels = new Color[baseTexture.Width * baseTexture.Height];
                baseTexture.GetData(pixels);

                effect(pixels);

                variantTexture.SetData(pixels);
            });
            return variantTexture;
        }

        public delegate Color[] VariantEffect(Color[] pixels);
        public static Color[] MakeGrayscale(Color[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                byte grayscaleValue = (byte)(pixels[i].R * 0.299f + pixels[i].G * 0.587f + pixels[i].B * 0.114f);
                pixels[i] = new Color(grayscaleValue, grayscaleValue, grayscaleValue, pixels[i].A);
            }
            return pixels;
        }
        public static Color[] MakeNegative(Color[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].A == 0) continue;
                pixels[i] = new Color((byte)(255 - pixels[i].R), (byte)(255 - pixels[i].G), (byte)(255 - pixels[i].B), pixels[i].A);
            }
            return pixels;
        }
        public static Color[] None(Color[] pixels)
        {
            return pixels;
        }*/
    }
    public class PaintAnythingSystem : ModSystem
    {

        //public static Dictionary<UniversalVariationKey, WhateverPaintRenderTargetHolder> _paintRenders = new();

        private static IList<ARenderTargetHolder> paintSystemRequests;

        public override void Load()
        {
            var tilePaintSystemRequests = typeof(TilePaintSystemV2).GetField("_requests", BindingFlags.NonPublic | BindingFlags.Instance);
            paintSystemRequests = tilePaintSystemRequests.GetValue(Main.instance.TilePaintSystem) as IList<ARenderTargetHolder>; // grab the requests through reflection
        }

        public static event Action ClearRenderTargets;
        public override void Unload() => ClearRenderTargets?.Invoke();

        private static void RequestPaintTexture(Dictionary<UniversalVariationKey, WhateverPaintRenderTargetHolder> textureDict, ref UniversalVariationKey lookupKey, string texturePath, int copySettingsFrom = -1, TreePaintingSettings customSettings = null)
        {
            if (!textureDict.TryGetValue(lookupKey, out WhateverPaintRenderTargetHolder target))
            {
                target = new WhateverPaintRenderTargetHolder(lookupKey, texturePath, copySettingsFrom, customSettings);
                textureDict.Add(lookupKey, target);
            }

            //We don't need to process the requests ourselves, just let the paint system do it, gg ez
            if (!target.IsReady)
                paintSystemRequests.Add(target);
        }

        public static Texture2D TryGetTexturePaintAndRequestIfNotReady(Dictionary<UniversalVariationKey, WhateverPaintRenderTargetHolder> textureDict, int type, int paintColor, string texturePath, int copySettingsFrom = -1, TreePaintingSettings customSettings = null)
        {
            UniversalVariationKey variationKey = new UniversalVariationKey(type, paintColor);
            if (textureDict.TryGetValue(variationKey, out WhateverPaintRenderTargetHolder value) && value.IsReady)
                return value.Target;

            RequestPaintTexture(textureDict, ref variationKey, texturePath, copySettingsFrom, customSettings);
            return null;
        }


    }

    public static class PaintAnythingExtensions
    {
        public static Texture2D TryGetTexturePaintAndRequestIfNotReady(this Dictionary<UniversalVariationKey, WhateverPaintRenderTargetHolder> textureDict, int type, int paintColor, string texturePath, int copySettingsFrom = -1, TreePaintingSettings customSettings = null)
        {
            return PaintAnythingSystem.TryGetTexturePaintAndRequestIfNotReady(textureDict, type, paintColor, texturePath, copySettingsFrom, customSettings);
        }
        public static TreePaintingSettings moreColorful = new TreePaintingSettings
        {
            UseSpecialGroups = true,
            SpecialGroupMinimalHueValue = 0.5f,
            SpecialGroupMaximumHueValue = 0.9f,
            SpecialGroupMinimumSaturationValue = 0.5f,
            SpecialGroupMaximumSaturationValue = 0.9f,
            InvertSpecialGroupResult = true,
        };
    }

    public class WhateverPaintRenderTargetHolder : ARenderTargetHolder
    {
        public UniversalVariationKey Key;
        public int tileTypeToCopySettingsFrom;
        public TreePaintingSettings paintSettings;
        public string texturePath;

        public WhateverPaintRenderTargetHolder(UniversalVariationKey key, string texture, int copySettingsFrom = -1, TreePaintingSettings customSettings = null)
        {
            Key = key;
            texturePath = texture;

            //Lets us use custom parameters for the paint settings 
            if (customSettings != null)
            {
                Main.NewText(customSettings.SpecialGroupMaximumHueValue);
                paintSettings = customSettings;
            }
            //Else copy vanilla settings (by default -1, so not anything fancy)
            else
                paintSettings = TreePaintSystemData.GetTileSettings(copySettingsFrom, 0);
        }

        public override void Prepare()
        {
            Asset<Texture2D> asset = ModContent.Request<Texture2D>(texturePath);
            asset.Wait?.Invoke();
            PrepareTextureIfNecessary(asset.Value);
        }

        public override void PrepareShader() => PrepareShader(Key.PaintColor, paintSettings);
    }

    public struct UniversalVariationKey
    {
        public int ThingType;
        public int PaintColor;

        public UniversalVariationKey(int type, int color)
        {
            ThingType = type;
            PaintColor = color;
        }

        public bool Equals(UniversalVariationKey other)
        {
            if (ThingType == other.ThingType)
                return PaintColor == other.PaintColor;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is UniversalVariationKey)
                return Equals((UniversalVariationKey)obj);

            return false;
        }

        public override int GetHashCode() => (7302013 ^ ThingType.GetHashCode()) * (7302013 ^ PaintColor.GetHashCode());

        public static bool operator ==(UniversalVariationKey left, UniversalVariationKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UniversalVariationKey left, UniversalVariationKey right)
        {
            return !left.Equals(right);
        }
    }
}
