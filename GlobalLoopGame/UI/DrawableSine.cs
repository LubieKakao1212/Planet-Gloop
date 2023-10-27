using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using MonoEngine.Rendering.Sprites;
using MonoEngine.Scenes;
using MonoEngine.Scenes.Events;
using MonoEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.UI
{
    internal class DrawableSine : DrawableObject, IUpdatable
    {
        float phase;
        Color org_color;
        public DrawableSine(Color p_color, float drawOrder, float p_phase) : base(p_color, drawOrder) 
        {
            phase = p_phase;
            org_color = Color;
        }

        public float Order { get; set; }

        public void Update(GameTime time)
        {
            float factor = MathF.Abs(MathF.Sin((float)time.TotalGameTime.TotalSeconds + phase));
            this.Color = org_color * factor;
        }
    }
}
