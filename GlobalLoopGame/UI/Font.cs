using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.UI
{
    public class Font
    {
        private Dictionary<int, SpriteFont> sizes;

        public Font()
        {
            sizes = new Dictionary<int, SpriteFont>();
        }

        public Font AddSize(int size, SpriteFont font)
        {
            sizes.Add(size, font);
            return this;
        }

        public SpriteFont GetSize(int size)
        {
            return sizes[size];
        }
    }
}
