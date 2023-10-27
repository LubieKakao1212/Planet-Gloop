using Microsoft.Xna.Framework;
using MonoEngine.Scenes;
using MonoEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Globals
{
    public class CameraManager : IGameComponent, IUpdateable, IResettable
    {
        public bool Enabled => true;

        public int UpdateOrder { get; }

        public event EventHandler<EventArgs> EnabledChanged;

        public event EventHandler<EventArgs> UpdateOrderChanged;

        private Vector2 cameraPositionVariance;
        private float shakeIntensity;
        private AutoTimeMachine cameraShaker;
        private bool isShaking = false;
        private int timeShaken = 0;

        public Camera cameraToShake;

        public CameraManager(Camera cam)
        {
            cameraToShake = cam;
        }

        public void Initialize()
        {

        }

        public void Update(GameTime gameTime)
        {
            if (isShaking && timeShaken % 4 == 0)
            {
                SetCameraPositionVariance(new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle()));

                cameraToShake.Transform.GlobalPosition = cameraPositionVariance * shakeIntensity;
            }
            
            if (cameraShaker != null)
            {
                cameraShaker.Forward(gameTime.ElapsedGameTime.TotalSeconds);
            }

            timeShaken++;

            shakeIntensity = shakeIntensity + MathF.Sign(-shakeIntensity) * (float)gameTime.ElapsedGameTime.TotalSeconds * (shakeIntensity + 1);

            // Console.WriteLine(shakeIntensity);
        }

        public void SetCameraPositionVariance(Vector2 newVariance)
        {
            cameraPositionVariance = newVariance;
        }

        public void SetCameraShake(bool newCameraShake, float intensity)
        {
            isShaking = newCameraShake;

            if (newCameraShake)
            {
                shakeIntensity = intensity;

                cameraShaker = new AutoTimeMachine(() => SetCameraShake(false, 0), 1f);
            }
            else
            {
                cameraToShake.Transform.GlobalPosition = Vector2.Zero;

                shakeIntensity = 0;

                cameraShaker = null;
            }
        }

        public void OnGameEnd()
        {
            SetCameraShake(false, 0);

            shakeIntensity = 0;
        }

        public void Reset()
        {

        }
    }
}
