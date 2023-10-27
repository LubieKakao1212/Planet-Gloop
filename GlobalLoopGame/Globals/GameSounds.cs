using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Globals
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

        public static SoundEffect[] shotSounds = new SoundEffect[3];
        public static SoundEffect shotgunReloadSound;

        public static SoundEffect shieldHurt;
        public static SoundEffect shieldHeal;
        public static SoundEffect shieldDestroy;

        public static SoundEffect chargePickup;
        public static SoundEffect chargeAlert;

        public static SoundEffect musicIntensityOne;
        public static SoundEffect musicIntensityTwo;
        public static SoundEffect musicIntensityThree;
        public static SoundEffect musicIntensityFour;
        public static SoundEffect musicIntensityFive;
        public static SoundEffect musicIntensitySix;
        public static SoundEffect musicIntensitySeven;

        public static SoundEffectInstance boostEmitter;
        public static SoundEffectInstance boostChargingEmitter;
        public static SoundEffectInstance magnetEmitter;
        public static SoundEffectInstance thrusterEmitter;
        public static SoundEffectInstance sideThrusterEmitter;

        public static SoundEffectInstance firstMusicInstance;
        public static SoundEffectInstance secondMusicInstance;
        public static SoundEffectInstance thirdMusicInstance;
        public static SoundEffectInstance fourthMusicInstance;
        public static SoundEffectInstance fifthMusicInstance;
        public static SoundEffectInstance sixthMusicInstance;
        public static SoundEffectInstance seventhMusicInstance;

        public static void PlaySound(SoundEffect soundEffect, int variance)
        {
            SoundEffectInstance instance = soundEffect.CreateInstance();

            instance.Pitch = Random.Shared.Next(-variance, variance) * 1f / 10f;

            instance.Play();
        }
    }
}
