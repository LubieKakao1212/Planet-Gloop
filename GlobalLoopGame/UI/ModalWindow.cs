using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.UI
{
    internal class ModalWindow : HierarchyObject
    {
        public float order => 1f;

        private DrawableObject modalWindow;
        private String windowAction;
        private Color mainColor;
        private Color accentColor;

        public ModalWindow()
        {

        }
    }
}
