using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Math
{
    public class Transform
    {
        public event Action Changed;

        public Vector2 Right => localToWorld.TransformDirection(Vector2.UnitX);
        public Vector2 Up => localToWorld.TransformDirection(Vector2.UnitY);

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
                OnChanged();
            }
        }

        public float LocalRotation
        {
            get => rotation;
            set
            {
                rotation = value;
                OnChanged();
            }
        }

        public Vector2 LocalScale
        {
            get => scale;
            set
            {
                scale = value;
                OnChanged();
            }
        }

        public TransformMatrix LocalToWorld
        {
            get
            {
                CalculateLocalToWrorld();
                return localToWorld;
            }
        }

        public TransformMatrix WorldToLocal
        {
            get
            {
                CalculateWorldToLocal();
                return worldToLocal;
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

        private TransformMatrix localToWorld;
        private TransformMatrix worldToLocal;

        private bool isDirtyLtW;
        private bool isDirtyWtL;

        private Vector2 translation = default;
        private float rotation = 0;
        private Vector2 scale = new Vector2(1f, 1f);

        public Transform()
        {
            isDirtyLtW = true;
            isDirtyWtL = true;
        }

        private void OnChanged()
        {
            isDirtyLtW = true;
            isDirtyWtL = true;
            Changed?.Invoke();
        }

        private void CalculateLocalToWrorld()
        {
            if (!isDirtyLtW)
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

            localToWorld = parentLtW * TransformMatrix.TranslationRotationScale(translation, rotation, scale);

            isDirtyLtW = false;
        }

        private void CalculateWorldToLocal()
        {
            if (!isDirtyWtL)
            {
                return;
            }
            CalculateLocalToWrorld();
            worldToLocal = localToWorld.Inverse();

            isDirtyWtL = false;
        }
    }
}
