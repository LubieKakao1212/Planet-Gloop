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

        public static SoundEffect musicIntensity1;
        public static SoundEffect musicIntensity2;
        public static SoundEffect musicIntensity3;
        public static SoundEffect musicIntensity4;
        public static SoundEffect musicIntensity5;
        public static SoundEffect musicIntensity6;
        public static SoundEffect musicIntensity7;
        public static SoundEffect musicIntensity8;
        public static SoundEffect musicIntensity9;
        public static SoundEffect musicIntensity10;
        public static SoundEffect musicIntensity11;
        public static SoundEffect musicIntensity12;

        public static SoundEffectInstance boostEmitter;
        public static SoundEffectInstance boostChargingEmitter;
        public static SoundEffectInstance magnetEmitter;
        public static SoundEffectInstance thrusterEmitter;
        public static SoundEffectInstance sideThrusterEmitter;

        public static SoundEffectInstance MusicInstance1;
        public static SoundEffectInstance MusicInstance2;
        public static SoundEffectInstance MusicInstance3;
        public static SoundEffectInstance MusicInstance4;
        public static SoundEffectInstance MusicInstance5;
        public static SoundEffectInstance MusicInstance6;
        public static SoundEffectInstance MusicInstance7;
        public static SoundEffectInstance MusicInstance8;
        public static SoundEffectInstance MusicInstance9;
        public static SoundEffectInstance MusicInstance10;
        public static SoundEffectInstance MusicInstance11;
        public static SoundEffectInstance MusicInstance12;

        public static void PlaySound(SoundEffect soundEffect, int variance)
        {
            SoundEffectInstance instance = soundEffect.CreateInstance();

            instance.Pitch = Random.Shared.Next(-variance, variance) * 1f / 10f;

            instance.Play();
        }
    }
}
