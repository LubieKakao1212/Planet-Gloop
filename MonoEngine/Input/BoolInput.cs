using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Input
{
    public class BoolInput : ValueInputBase<bool>
    {
        protected override bool Value { get => state; }

        public override string FriendlyName => name;

        private bool state;

        private string name;
        
        internal BoolInput(string name)
        {
            this.name = name;
        }

        /// <param name="newState">new State of this input</param>
        /// <returns>was the input changed</returns>
        internal bool UpdateState(bool newState)
        {
            var changed = state != newState;
            state = newState;
            InvokeEvents(newState, changed);
            return changed;
        }
    }
}
