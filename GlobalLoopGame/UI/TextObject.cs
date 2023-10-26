using Microsoft.Xna.Framework;
using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.UI
{
    public class TextObject : HierarchyObject
    {
        public string Text { get; set; }

        public Color Color { get; set; }

        public int FontSize { get; set; }

    }
}
