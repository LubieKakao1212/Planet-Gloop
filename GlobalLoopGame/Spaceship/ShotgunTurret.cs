﻿using GlobalLoopGame.Asteroid;
using Microsoft.Xna.Framework;
using MonoEngine.Rendering;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Spaceship
{
    public class ShotgunTurret : TurretStation
    {
        public ShotgunTurret(World world, AsteroidManager asteroids, RenderPipeline renderer, float cooldown) : base(world, asteroids, renderer, cooldown)
        {
            bulletCount = 16;
            spread = 30f * MathF.PI / 180f;
            damage = 15;
            UpdateText();
        }

        protected override BulletObject CreateBullet(Vector2 dir, Vector2 pos, float speed)
        {
            return base.CreateBullet(dir, pos, speed).SetLifetime(0.5f).SetDamage(damage).SetColor(new Color(255, 200, 20) * 0.75f);
        }

        protected override float GetBulletSpeed()
        {
            return Random.Shared.NextSingle() * 16f + 64f;
        }
    }
}
