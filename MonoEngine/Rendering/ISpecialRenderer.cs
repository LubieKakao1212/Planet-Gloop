using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Rendering
{
    public interface ISpecialRenderer
    {
        void Render(GraphicsDevice device, Camera camera);
    }
}
