using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Input.Binding
{
    public class AxisBindingInput : ValueInputBase<float>
    {
        public override string FriendlyName { get => name; }

        protected override float Value
        { 
            get 
            {
                if (binding == null)
                {
                    //TODO Binding Exception
                    throw new Exception("Unbound");
                }
                return (binding.GetCurrentValue<bool>() ? activeValue : idleValue);
            }
        }

        private string name;

        private ValueInputBase<bool> binding;

        private float idleValue = 0, activeValue = 1;

        public AxisBindingInput(string name)
        {
            this.name = name;
            binding = null;
        }
        
        public AxisBindingInput Bind(ValueInputBase<bool> binding, bool inheritName = false)
        {
            UnbindCallbacks();
            this.binding = binding;
            BindCallbacks();
            if (inheritName && binding != null)
            {
                name = binding.FriendlyName;
            }
            return this;
        }

        public AxisBindingInput SetValues(float idle, float active) 
        {
            idleValue = idle;
            activeValue = active;
            return this;
        }

        private void UnbindCallbacks()
        {
            if(binding != null)
            {
                binding.Started -= PassStarted;
                binding.Performed -= PassPerformed;
                binding.Canceled -= PassCanceled;
            }
        }

        private void BindCallbacks()
        {
            if (binding != null)
            {
                binding.Started += PassStarted;
                binding.Performed += PassPerformed;
                binding.Canceled += PassCanceled;
            }
        }
    }

    //TODO May potentialy call Canceled and Started on the same frame
    public class CompoundAxixBindingInput : ValueInputBase<float>, IBindingInput
    {
        public override string FriendlyName { get => name; }

        protected override float Value
        {
            get
            {
                var v = 0f;
                foreach (var binding in bindings)
                {
                    v += binding.GetCurrentValue<float>();
                }
                return v;
            }
        }

        private HashSet<ValueInputBase<float>> bindings;

        private string name;

        private int activityState;
        private bool updatedThisTick;

        public CompoundAxixBindingInput(string name)
        {
            this.name = name;
            bindings = new HashSet<ValueInputBase<float>>();
        }

        public CompoundAxixBindingInput Bind(ValueInputBase<float> binding)
        {
            if (bindings.Add(binding))
            {
                BindCallbacks(binding);
            }
            return this;
        }
        
        public CompoundAxixBindingInput UnBind(ValueInputBase<float> binding)
        {
            if (bindings.Remove(binding))
            {
                UnbindCallbacks(binding);
            }
            return this;
        }

        public void Update()
        {
            updatedThisTick = false;
        }

        public void IncrementState(IInput _)
        {
            var changed = activityState == 0;
            activityState++;
            if (changed)
            {
                PassStarted(_);
            }
        }

        public void DecrementState(IInput _) 
        {
            activityState--;
            var changed = activityState == 0;
            if (changed)
            {
                PassCanceled(_);
            }
        }

        private void Perform(IInput _)
        {
            if (!updatedThisTick)
            {
                updatedThisTick = true;
                PassPerformed(_);
            }
        }

        private void BindCallbacks(ValueInputBase<float> binding)
        {
            binding.Started += IncrementState;
            binding.Performed += Perform;
            binding.Canceled += DecrementState;
        }

        private void UnbindCallbacks(ValueInputBase<float> binding)
        {
            binding.Started -= IncrementState;
            binding.Performed -= Perform;
            binding.Canceled -= DecrementState;
        }

    }
}
