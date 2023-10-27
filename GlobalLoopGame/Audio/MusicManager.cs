using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using MonoEngine.Scenes;
using MonoEngine.Util;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Audio
{
    public class MusicManager : IGameComponent, IUpdateable, IResettable
    {
        public bool Enabled => true;

        public int UpdateOrder { get; }

        public event EventHandler<EventArgs> EnabledChanged;

        public event EventHandler<EventArgs> UpdateOrderChanged;

        public float intensity { get; private set; } = 0f;
        public static float targetIntensity = 0f;

        public MusicManager()
        {

        }

        public void Initialize()
        {
            
        }

        public void Update(GameTime gameTime)
        {
            intensity = MathHelper.Clamp(intensity + MathF.Sign(targetIntensity - intensity) * (float)gameTime.ElapsedGameTime.TotalSeconds / 3f, 0f, 2f);
            
            UpdateMusic();
        }

        public static void ModifyIntensity(float intensityModification)
        {
            SetIntensity(targetIntensity + intensityModification);
        }

        public static void SetIntensity(float newIntensity)
        {
            targetIntensity = MathHelper.Clamp(newIntensity, 0f, 2f);
        }

        public void UpdateMusic()
        {
            
            GameSounds.firstMusicInstance.Volume = MathF.Sqrt(MathHelper.Clamp(1f - intensity, 0f, 1f));
            GameSounds.secondMusicInstance.Volume = MathF.Sqrt(1f - MathF.Abs(1f - intensity));
            GameSounds.thirdMusicInstance.Volume  = MathF.Sqrt(1f - MathF.Abs(1f - intensity));
            GameSounds.fourthMusicInstance.Volume = MathF.Sqrt(1f - MathF.Abs(1f - intensity));
            GameSounds.fifthMusicInstance.Volume = MathF.Sqrt(1f - MathF.Abs(1f - intensity));
            GameSounds.sixthMusicInstance.Volume = MathF.Sqrt(1f - MathF.Abs(1f - intensity));
            GameSounds.seventhMusicInstance.Volume = MathF.Sqrt(1f - MathF.Abs(1f - intensity));

            //GameSounds.thirdMusicInstance.Volume = MathF.Sqrt(MathHelper.Clamp(intensity - 1f, 0f, 1f));
            //Console.WriteLine("music intensity mwahaha..:" + Math.Round(intensity));
            Console.WriteLine("1: " + ((MathF.Sqrt(1f - MathF.Abs(1f - intensity)) )));
            Console.WriteLine("2: " + ((MathF.Sqrt(1f - MathF.Abs(2f - intensity)) )));
            Console.WriteLine("3: " + ((MathF.Sqrt(1f - MathF.Abs(3f - intensity)) )));
            Console.WriteLine("4: " + ((MathF.Sqrt(1f - MathF.Abs(4f - intensity)) )));

            //Console.WriteLine("second:" + Math.Round(GameSounds.secondMusicInstance.Volume));
            //Console.WriteLine("third:" + Math.Round(GameSounds.thirdMusicInstance.Volume));
        }

        public void OnGameEnd()
        {
            SetIntensity(0f);
        }

        public void Reset()
        {

        }
    }
}
