using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Math;
using MonoEngine.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MonoEngine.Rendering.Sprites.Atlas
{
    public class SpriteAtlas<T> : ISpriteAtlas where T : struct
    {
        public Texture3D AtlasTextures => atlasTextures;

        private const int maxSizeInternal = 8192;

        private List<AtlasRegion> regions = new List<AtlasRegion>();

        private Texture3D atlasTextures;

        private int size;

        private int textureCount;

        private GraphicsDevice graphics;

        private SurfaceFormat textureFormat;

        private bool compacted;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumTextureSize">Currently not used, atlas is always full size</param>
        public SpriteAtlas(GraphicsDevice graphics, int minimumTextureSize = maxSizeInternal)
        {
            size = minimumTextureSize;
            this.graphics = graphics;
            if (typeof(T) == typeof(Vector4))
            { 
                textureFormat = SurfaceFormat.Vector4;
            }
            else if (typeof(T) == typeof(Color))
            {
                textureFormat = SurfaceFormat.Color;
            }
            else
            {
                throw new ApplicationException($"Data type {typeof(T)} is not supported.");
            }
        }

        public Sprite[] AddTextureRects(Texture2D texture, params Rectangle[] rects)
        {
            var output = new Sprite[MathHelper.Max(rects.Length, 1)];

            regions.Capacity += output.Length;

            if (rects.Length == 0)
            {
                output[0] = new Sprite();
                regions.Add(new AtlasRegion()
                {
                    sourceTexture = texture,
                    sourceRect = texture.Bounds,
                    destinationSprite = output[0]
                });
            }
            else
                for (int i = 0; i < output.Length; i++)
                {
                    output[i] = new Sprite();
                    regions.Add(new AtlasRegion() 
                    {
                        sourceTexture = texture,
                        sourceRect = rects[i],
                        destinationSprite = output[i]
                    });

                }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxSize">Currently not used, atlas is always full size</param>
        public void Compact(int maxSize = maxSizeInternal)
        {
            if (compacted)
            {
                throw new ApplicationException($"Atlas already compacted");
            }

            textureCount = 1;

            regions.Sort((a, b) => b.sourceRect.Height - a.sourceRect.Height);

            SortedDictionary<int, List<Rectangle>> spaces = new SortedDictionary<int, List<Rectangle>>();

            spaces.AddNested(size, new Rectangle(0, 0, size, size));

            for (int i=0; i<regions.Count; i++)
            {
                var region = regions[i];
                var sourceRect = region.sourceRect;
                if (region.IsValid)
                {
                    if (spaces.Count == 0 || spaces.Last().Value[0].Height < sourceRect.Height)
                    {
                        //TODO Resize And insert
                        //Add txture
                        IncrementTextureCount(ref sourceRect, spaces);
                        continue;
                    }
                    
                    bool flag = true;
                    IEnumerator<List<Rectangle>> spacesEnumerator = spaces.Values.GetEnumerator();

                    while (flag && spacesEnumerator.MoveNext())
                    {
                        List<Rectangle> space = spacesEnumerator.Current;
                        if (space[0].Height < sourceRect.Height)
                        {
                            continue;
                        }

                        var spaceHeight = space[0].Height;
                        var perfectH = spaceHeight == sourceRect.Height;

                        for (int k = 0; k < space.Count; k++)
                        {
                            if (space[k].Width >= sourceRect.Width)
                            {
                                Rectangle rect = sourceRect;
                                rect.X = space[k].X;
                                rect.Y = space[k].Y;
                                sourceRect = rect;
                                if (perfectH && space[k].Width == sourceRect.Width) // remove
                                {
                                    spaces.RemoveNested(spaceHeight, space[k]);
                                }
                                else if (perfectH) //shrink horizontally
                                {
                                    //Don't have to add and remove, height doesn't change
                                    space[k] = new Rectangle(space[k].X + sourceRect.Width, space[k].Y, space[k].Width - sourceRect.Width, space[k].Height);
                                }
                                else if (space[k].Width == sourceRect.Width) //shrink vertically
                                {
                                    Rectangle newSpace = new Rectangle(space[k].X, space[k].Y + sourceRect.Height, space[k].Width, spaceHeight - sourceRect.Height);
                                    spaces.RemoveNested(spaceHeight, space[k]);
                                    spaces.AddNested(newSpace.Height, newSpace);
                                }
                                else
                                {
                                    //Top
                                    var topHeight = spaceHeight - sourceRect.Height;
                                    spaces.AddNested(topHeight, new Rectangle(space[k].X, space[k].Y + sourceRect.Height, sourceRect.Width, topHeight));
                                    //Right
                                    space[k] = new Rectangle(space[k].X + sourceRect.Height, space[k].Y, space[k].Width - sourceRect.Width, spaceHeight);
                                }

                                flag = false;
                                break;
                            }
                        }
                    }

                    if (flag)
                    {
                        IncrementTextureCount(ref sourceRect, spaces);
                    }
                }
                
                region.destinationPosition = sourceRect.Location;
                regions[i] = region;
            }

            compacted = true;

            CreateAtlasTextures();
        }

        private void CreateAtlasTextures()
        {
            atlasTextures = new Texture3D(graphics, size, size, textureCount, false, textureFormat);

            var texturePixelCount = size * size;

            var atlasPixels = new T[texturePixelCount * textureCount];

            foreach (var region in regions)
            {
                #region Fill Atlas
                var rawData = GetTextureData(region.sourceTexture, region.sourceRect);
                var data = new T[rawData.Length];
                if (data is Vector4[] vArr)
                {
                    rawData.CopyTo(vArr, 0);
                }
                else if(data is Color[] cArr)
                {
                    for(int i=0; i<data.Length; i++)
                    {
                        cArr[i] = new Color(rawData[i]);
                    }
                }
                #endregion

                #region SetSprite Data
                var pos = region.destinationPosition;

                atlasPixels.SetRectUnchecked(size, data, new Rectangle(
                    pos.X, pos.Y,
                    region.sourceRect.Width, region.sourceRect.Height));
                var x = pos.X % size;
                var idx = pos.X / size;

                region.destinationSprite.TextureRect = new BoundingRect(
                    new Vector2((float)x / size, (float)pos.Y / size), 
                    new Vector2(
                        (float)region.sourceRect.Width / size, 
                        (float)region.sourceRect.Height/ size)
                    );
                region.destinationSprite.TextureIndex = idx;

                #endregion
            }

            var fullRect = new Rectangle(0, 0, size, size);

            atlasTextures.SetData(atlasPixels);
        }

        //Can be done better, I think
        private Vector4[] GetTextureData(Texture2D source, Rectangle sourceRect)
        {
            var dataSize = sourceRect.Width * sourceRect.Height;

            if (source.Format == SurfaceFormat.Color)
            {
                var pixels = new Color[dataSize];
                source.GetData(0, sourceRect, pixels, 0, dataSize);

                return pixels.Select((p) => p.ToVector4()).ToArray();
            }
            else if (source.Format == SurfaceFormat.Vector4)
            {
                var pixels = new Vector4[dataSize];
                source.GetData(0, sourceRect, pixels, 0, dataSize);
                return pixels;
            }
            else
                throw new ApplicationException($"Unsuported texture format {source.Format}");
            
        }

        private void IncrementTextureCount(ref Rectangle sourceRect, SortedDictionary<int, List<Rectangle>> spaces)
        {
            var textureCount = this.textureCount++;
            sourceRect.X = textureCount * size;
            sourceRect.Y = 0;
            Rectangle right = new Rectangle(textureCount * size + sourceRect.Width, 0, size - sourceRect.Width, size),
                top = new Rectangle(textureCount * size, sourceRect.Height, sourceRect.Width, size - sourceRect.Height);

            spaces.AddNested(right.Height, right);
            spaces.AddNested(top.Height, top);
        }

        public void Dispose()
        {
            atlasTextures?.Dispose();
        }
    }
}
