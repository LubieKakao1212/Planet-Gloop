using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoEngine.Input.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Input
{
    //TODO Gamepad support
    public class InputManager
    {
        /// <summary>
        /// Used for binding inputs
        /// </summary>
        public IInput BindingInput { get; private set; }

        public IInput CursorPosition => cursorPosition;

        private Dictionary<Keys, KeyInput> keys;

        private BoolInput[] mouseButtons;
        private PointInput cursorPosition;

        /// <summary>
        /// Used for MouseInput
        /// </summary>
        private GameWindow window;

        public InputManager(GameWindow window)
        {
            keys = new Dictionary<Keys, KeyInput>();

            mouseButtons = new BoolInput[5]
            {
                new BoolInput("Mouse Left"),
                new BoolInput("Mouse Right"),
                new BoolInput("Mouse Middle"),
                new BoolInput("Mouse 4"),
                new BoolInput("Mouse 5")
            };

            foreach (var key in (Keys[])typeof(Keys).GetEnumValues())
            {
                keys.Add(key, new KeyInput(key));
            }
        }

        /// <summary>
        /// Updates the internal state of <see cref="InputManager"/> and invokes input events
        /// Should be called every Update
        /// </summary>
        public void UpdateState()
        {
            BindingInput = UnboundInput.Value;
            
            #region Keyboard
            var state = Keyboard.GetState();
            
            foreach (var key in keys.Values)
            {
                var keyState = state.IsKeyDown(key.Key);
                SetInput(key, keyState);
            }
            #endregion

            #region Mouse
            var mouse = Mouse.GetState();

            SetInput(mouseButtons[0], mouse.LeftButton == ButtonState.Pressed);
            SetInput(mouseButtons[1], mouse.RightButton == ButtonState.Pressed);
            SetInput(mouseButtons[2], mouse.MiddleButton == ButtonState.Pressed);
            SetInput(mouseButtons[3], mouse.XButton1 == ButtonState.Pressed);
            SetInput(mouseButtons[4], mouse.XButton2 == ButtonState.Pressed);

            cursorPosition.UpdateState(new Point(mouse.X, mouse.Y));
            #endregion
        }

        public IInput GetKey(Keys key)
        {
            return keys[key];
        }

        public IInput GetMouse(MouseButton button) => button switch
        {
            MouseButton.Left => mouseButtons[0],
            MouseButton.Right => mouseButtons[1],
            MouseButton.Middle => mouseButtons[2],
            MouseButton.Button4 => mouseButtons[3],
            MouseButton.Button5 => mouseButtons[4],
            _ => throw new ArgumentException($"Invalid mouse button {button}")
        };

        private void SetInput(BoolInput input, bool state)
        {
            var changed = input.UpdateState(state);

            if (changed && state)
            {
                BindingInput = input;
            }
        }
    }

    public enum MouseButton
    {
        Invalid = 0,
        Left = 1,
        Right = 2,
        Middle = 3,
        Button4 = 4,
        Button5 = 5
    }
}
