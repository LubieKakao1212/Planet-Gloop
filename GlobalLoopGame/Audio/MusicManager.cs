using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using Custom2d_Engine.Scenes;
using Custom2d_Engine.Util;
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
            intensity = intensity + MathF.Sign(targetIntensity - intensity) * (float)gameTime.ElapsedGameTime.TotalSeconds / 3f;

            UpdateMusic();
        }

        public static void ModifyIntensity(float intensityModification)
        {
            SetIntensity(targetIntensity + intensityModification);
        }

        public static void SetIntensity(float newIntensity)
        {
            targetIntensity = MathHelper.Clamp(newIntensity, 0f, 11f);
        }
        public void UpdateMusicVolume()
        {
            //GameSounds.firstMusicInstance.Volume = MathF.Sqrt(MathHelper.Clamp(1f - intensity, 0f, 1f));
            //GameSounds.MusicInstance7.Volume = MathF.Sqrt(1f - MathF.Abs(1f - intensity));
            //GameSounds.thirdMusicInstance.Volume = MathF.Sqrt(MathHelper.Clamp(intensity - 1f, 0f, 1f));
        }
        public void UpdateMusic()
        {
            //GameSounds.firstMusicInstance.Volume   = MathF.Sqrt(MathHelper.Clamp(1f - intensity, 0f, 1f));
            GameSounds.MusicInstance1.Volume  = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(0f - intensity), 0, 1) ); 
            GameSounds.MusicInstance2.Volume  = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(1f - intensity), 0, 1) );
            GameSounds.MusicInstance3.Volume  = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(2f - intensity), 0, 1) );
            GameSounds.MusicInstance4.Volume  = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(3f - intensity), 0, 1) );
            GameSounds.MusicInstance5.Volume  = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(4f - intensity), 0, 1) );
            GameSounds.MusicInstance6.Volume  = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(5f - intensity), 0, 1) );
            GameSounds.MusicInstance7.Volume  = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(6f - intensity), 0, 1) );
            GameSounds.MusicInstance8.Volume  = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(7f - intensity), 0, 1) );
            GameSounds.MusicInstance9.Volume  = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(8f - intensity), 0, 1) );
            GameSounds.MusicInstance10.Volume = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(9f - intensity), 0, 1) );
            GameSounds.MusicInstance11.Volume = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(10f - intensity), 0, 1) );
            GameSounds.MusicInstance12.Volume = MathF.Sqrt( MathHelper.Clamp(1f - MathF.Abs(11f - intensity), 0, 1) );
            //Console.WriteLine(targetIntensity);
            //Console.WriteLine(GameSounds.MusicInstance1.Volume);
            //Console.WriteLine(GameSounds.MusicInstance5.Volume);

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
