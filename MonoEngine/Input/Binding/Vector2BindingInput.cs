using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Input.Binding
{
    //TODO
    public class Vector2BindingInput : ValueInputBase<Vector2>
    {
        /// <summary>
        /// Temporary
        /// </summary>
        public static readonly float deadzone = 0.01f;

        public override string FriendlyName { get; }

        protected override Vector2 Value
        {
            get
            {
                var raw = new Vector2(horizontal.GetCurrentValue<float>(), vertical.GetCurrentValue<float>());
                var mag = raw.LengthSquared();
                if (mag > deadzone)
                {
                    return normalize ? Vector2.Normalize(raw) : raw;
                }
                return Vector2.Zero;
            }
        }

        private ValueInputBase<float> horizontal;
        private ValueInputBase<float> vertical;

        private bool normalize;

        public Vector2BindingInput(string name, ValueInputBase<float> horizontal, ValueInputBase<float> vertical, bool normalize = false)
        {
            FriendlyName = name;
            this.horizontal = horizontal;
            this.vertical = vertical;
            this.normalize = normalize;
        }
    }
}
