using Microsoft.Xna.Framework;
using System;

namespace MonoEngine.Math
{
    //TODO Add Origin
    //TODO Add Split ??
    public class Transform
    {
        public event Action Changed;

        public Vector2 Right => LocalToWorld.TransformDirection(Vector2.UnitX);
        public Vector2 Up => LocalToWorld.TransformDirection(Vector2.UnitY);

        public Vector2 GlobalPosition
        {
            get => Parent != null ? Parent.LocalToWorld.TransformPoint(LocalPosition) : LocalPosition;
            set => LocalPosition = Parent != null ? Parent.WorldToLocal.TransformPoint(value) : value;
        }
        
        public Vector2 LocalPosition
        {
            get => translation;
            set
            {
                translation = value;
                OnSelfChanged();
            }
        }

        public float GlobalRotation
        {
            get => LocalToWorldData.Rotation;
            set => LocalRotation = MathUtil.LoopAngle(value- Parent.LocalToWorldData.Rotation);
        }

        public float LocalRotation
        {
            get => rotation;
            set
            {
                rotation = value;
                OnSelfChanged();
            }
        }

        public float GlobalShear => LocalToWorldData.Shear;

        public float LocalShear
        {
            get => shear;
            set
            {
                shear = value;
                OnSelfChanged();
            }
        }

        public Vector2 GlobalScale => LocalToWorldData.Scale;

        public Vector2 LocalScale
        {
            get => scale;
            set
            {
                scale = value;
                OnSelfChanged();
            }
        }

        public TransformData LocalToWorldData
        {
            get
            {
                CalculateLocalToWrorld();
                return localToWorld.Value;
            }
        }

        public TransformMatrix LocalToWorld
        {
            get
            {
                CalculateLocalToWrorld();
                return localToWorld.Value;
            }
        }

        public TransformMatrix WorldToLocal
        {
            get
            {
                CalculateWorldToLocal();
                return worldToLocal.Value;
            }
        }

        public Transform Parent
        {
            get => parent;
            set
            {
                if (parent != null)
                {
                    parent.Changed -= OnChanged;
                }
                parent = value;
                if (parent != null)
                {
                    parent.Changed += OnChanged;
                }
                Changed?.Invoke();
            }
        }

        private Transform parent = null;

        private TransformMatrix? localToParent = null;
        private TransformData? localToWorld = null;
        private TransformMatrix? worldToLocal = null;

        private Vector2 translation = default;
        private float rotation = 0;
        private float shear = 0;
        private Vector2 scale = new Vector2(1f, 1f);

        public Transform()
        {
            
        }

        public void SetRelativePosition(Transform relativeTo, Vector2 position)
        {
            var pos = relativeTo.LocalToWorld.TransformPoint(position);
            GlobalPosition = pos;
        }

        public TransformMatrix GetRelativeMatrix(Transform relativeTo)
        {
            TransformMatrix.FromTo(LocalToWorld, relativeTo.LocalToWorld, out var mOut);
            return mOut;
        }

        private void OnChanged()
        {
            localToWorld = null;
            worldToLocal = null;
            Changed?.Invoke();
        }

        private void OnSelfChanged()
        {
            localToParent = null;
            OnChanged();
        }

        private void CalculateLocalToParent()
        {
            if (localToParent != null)
            {
                return;
            }

            localToParent = TransformMatrix.TranslationRotationShearScale(translation, rotation, shear, scale);
        }

        private void CalculateLocalToWrorld()
        {
            if (localToWorld != null)
            {
                return;
            }
            TransformMatrix parentLtW = default;
            
            if (parent != null)
            {
                parentLtW = parent.LocalToWorld;
            }
            else
            {
                parentLtW.SetIdentity();
            }

            CalculateLocalToParent();

            localToWorld = parentLtW * localToParent;
        }

        private void CalculateWorldToLocal()
        {
            if (worldToLocal != null)
            {
                return;
            }
            worldToLocal = LocalToWorld.Inverse();
        }

        public struct TransformData
        {
            public Vector2 Position 
            { 
                get
                {
                    Decompose();
                    return position;
                } 
            }

            public float Rotation
            {
                get
                {
                    Decompose();
                    return rotation;
                }
            }

            public float Shear
            {
                get
                {
                    Decompose();
                    return shear;
                }
            }

            public Vector2 Scale
            {
                get
                {
                    Decompose();
                    return scale;
                }
            }
            
            private Vector2 position;
            private float rotation;
            private float shear;
            private Vector2 scale;

            private bool decomposed;
            
            private TransformMatrix matrix;

            public static implicit operator TransformData(TransformMatrix mat)
            {
                return new TransformData() { matrix = mat, decomposed = false };
            }

            public static implicit operator TransformMatrix(TransformData mat)
            {
                return mat.matrix;
            }

            private void Decompose()
            {
                if (!decomposed)
                {
                    (position, rotation, shear, scale) = matrix;
                    decomposed = true;
                }
            }
        }
    }
}
