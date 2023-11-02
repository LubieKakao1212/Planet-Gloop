using Microsoft.Xna.Framework;
using Custom2d_Engine.Scenes.Events;
using Custom2d_Engine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Custom2d_Engine.Util.Ticking;

namespace GlobalLoopGame.UI
{
    internal class TextObjectTM : TextObject, IUpdatable
    {
        Color color_1;
        Color color_2;
        
        AutoTimeMachine aTM;
        public TextObjectTM(Color p_color_1, Color p_color_2, float p_interval) 
        {
            color_1 = p_color_1;
            color_2 = p_color_2;
            aTM = new AutoTimeMachine(ColorChanger, p_interval);
        }

        public float Order { get; set; }

        public void Update(GameTime time)
        {
            aTM.Forward(time.ElapsedGameTime.TotalSeconds);
        }

        private void ColorChanger()
        {
            if(this.Color == color_1)
            {
                this.Color = color_2;
            }
            else if(this.Color == color_2)
            {
                this.Color = color_1;
            }
            else
            {
                this.Color = color_1;
            }
        }
    }
}
