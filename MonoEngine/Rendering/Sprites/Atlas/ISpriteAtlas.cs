using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Rendering.Sprites.Atlas
{
    public interface ISpriteAtlas : IDisposable
    {
        Sprite[] AddTextureRects(Texture2D texture, params Rectangle[] regions);

        public void Compact(int maxSize);
    }
}
