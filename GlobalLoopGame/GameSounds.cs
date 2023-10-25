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
        public static SoundEffect magnetSound;

        public static SoundEffect dropTurretSound;
        public static SoundEffect pickupTurretSound;

        public static SoundEffect planetHurtSound;
        public static SoundEffect playerHurtSound;

        public static SoundEffect thrusterSound;
        public static SoundEffect sideThrustSound;

        public static SoundEffect warningSound;

        public static SoundEffect smallAsteroidDeath;
        public static SoundEffect bigAsteroidDeath;
        public static SoundEffect asteroidHurtSound;

        public static SoundEffect boostSound;
        public static SoundEffect boostChargingSound;
        public static SoundEffect boostOverloadSound;
        public static SoundEffect boostUnableSound;

        public static SoundEffect regularShotSound;
        public static SoundEffect shotgunShotSound;
        public static SoundEffect shotgunReloadSound;
        public static SoundEffect sniperShotSound;
        public static SoundEffect[] shotSounds = new SoundEffect[3];

        public static SoundEffect musicIntensityOne;
        public static SoundEffect musicIntensityTwo;
        public static SoundEffect musicIntensityThree;

        public static SoundEffectInstance boostEmitter;
        public static SoundEffectInstance boostChargingEmitter;
        public static SoundEffectInstance magnetEmitter;
        public static SoundEffectInstance thrusterEmitter;
        public static SoundEffectInstance sideThrusterEmitter;
        public static SoundEffectInstance firstMusicInstance;
        public static SoundEffectInstance secondMusicInstance;
        public static SoundEffectInstance thirdMusicInstance;
    }
}
