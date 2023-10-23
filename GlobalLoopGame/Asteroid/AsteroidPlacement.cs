using MonoEngine.Rendering.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidPlacement
    {
        public Vector2 size;
        public Vector2 location;
        public Vector2 velocity;
        public float speed;
        public int maxHealth;
        public float degrees;

        public AsteroidPlacement(Vector2 startingSize, float theta, float startingSpeed, int startingHealth)
        {
            size = startingSize;
            Vector2 thetaVector = new Vector2(MathF.Cos(theta), MathF.Sin(theta));
            location = (thetaVector * 65f) + size;
            velocity = -thetaVector;
            // location = startingLocation;
            // velocity = startingVelocity;
            speed = startingSpeed;
            maxHealth = startingHealth;
        }
    }
}
