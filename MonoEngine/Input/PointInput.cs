using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoEngine.Rendering.RenderPipeline;

namespace MonoEngine.Input
{
    public class PointInput : ValueInputBase<Point>
    {
        public override string FriendlyName => name;

        protected override Point Value => state;

        private Point state;
        private bool changedPreviously;

        private string name;

        internal PointInput(string name)
        {
            this.name = name;
        }

        /// <param name="newState">new State of this input</param>
        internal void UpdateState(Point newState)
        {
            var changed = state != newState;
            state = newState;
            InvokeEvents(changed, changedPreviously != changed);

            changedPreviously = changed;
        }
    }
}
