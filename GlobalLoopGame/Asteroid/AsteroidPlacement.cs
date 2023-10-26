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

        public AsteroidPlacement(Vector2 startingSize, float startingTheta, float endingTheta, float startingSpeed, int startingHealth)
        {
            size = startingSize;
            float randTheta = MathHelper.ToRadians(MathHelper.Lerp(startingTheta, endingTheta, Random.Shared.NextSingle()));
            Vector2 thetaVector = new Vector2(MathF.Cos(randTheta), MathF.Sin(randTheta));
            location = (thetaVector * Random.Shared.Next(65, 85));
            velocity = -thetaVector;
            speed = startingSpeed;
            maxHealth = startingHealth;
        }
    }
}
