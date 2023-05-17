using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Rendering
{
    public class Sprite
    {
        public Rectangle TextureRect => textureRect;
        public Color Tint => tint;

        private Rectangle textureRect;
        private Color tint;
    }
}
