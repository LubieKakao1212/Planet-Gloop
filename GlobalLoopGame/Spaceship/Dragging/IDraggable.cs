using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Spaceship.Dragging
{
    public interface IDraggable
    {
        bool IsDestroyed { get; }

        void OnBecomeDragged(IDragger dragger);

        void OnBecomeDropped(IDragger dragger);
    }
}
