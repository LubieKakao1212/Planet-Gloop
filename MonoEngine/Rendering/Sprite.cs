using Microsoft.Xna.Framework;
using MonoEngine.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Rendering
{
    public class Sprite
    {
        public BoundingRect TextureRect => textureRect;
        
        private BoundingRect textureRect;
    }
}
