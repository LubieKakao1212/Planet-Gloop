using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame
{
    public class GameSounds
    {
        public static SoundEffect asteroidDeathSound;
        public static SoundEffect dropTurretSound;
        public static SoundEffect magnetSound;
        public static SoundEffect pickupTurretSound;
        public static SoundEffect planetHurtSound;
        public static SoundEffect playerHurtSound;
        public static SoundEffect thrusterSound;
        public static SoundEffect sideThrustSound;
        public static SoundEffect warningSound;

        public static SoundEffect musicIntensityOne;
        public static SoundEffect musicIntensityTwo;
        public static SoundEffect musicIntensityThree;

        public static SoundEffectInstance magnetEmitter;
        public static SoundEffectInstance thrusterEmitter;
        public static SoundEffectInstance sideThrusterEmitter;

        public static SoundEffectInstance firstMusicInstance;
        public static SoundEffectInstance secondMusicInstance;
        public static SoundEffectInstance thirdMusicInstance;
    }
}
