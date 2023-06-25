using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Rendering.Sprites.Atlas
{
    public struct AtlasRegion : IDisposable
    {
        public bool IsValid => sourceTexture != null && sourceRect.Width > 0 && sourceRect.Height > 0;

        public Texture2D sourceTexture;
        public Rectangle sourceRect;
        public Point destinationPosition;
        public Sprite destinationSprite;

        public void Dispose()
        {
            sourceTexture?.Dispose();
            sourceTexture = null;
        }
    }
}
