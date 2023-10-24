using GlobalLoopGame.Asteroid;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using MonoEngine.Scenes;
using MonoEngine.Util;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GlobalLoopGame.Planet
{
    public class PlanetObject : PhysicsBodyObject
    {
        public int health {  get; private set; }
        private int maxHealth = 5;
        public bool isDead { get; private set; }

        public PlanetObject(World world) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Static);
            PhysicsBody.Tag = this;

            var fixture = PhysicsBody.CreateCircle(12f, 0f);
            var drawable = new DrawableObject(Color.White, -1f);
            drawable.Parent = this;
            drawable.Transform.LocalPosition = Vector2.Zero;
            drawable.Transform.LocalRotation = 0f;
            drawable.Transform.LocalScale = Vector2.One * 24f;
            drawable.Sprite = GameSprites.Planet;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3, bullets - 4
            fixture.CollisionCategories = Category.Cat5;
            fixture.CollidesWith = Category.None;
            fixture.CollidesWith |= Category.Cat1;
            fixture.CollidesWith |= Category.Cat2;
        }

        public void ModifyHealth(int healthModification)
        {
            health = MathHelper.Clamp(health + healthModification, 0, maxHealth);

            if (!isDead && health <= 0)
            {
                Die();
            }
        }

        public override void Update(GameTime time)
        {
            Transform.LocalRotation += (float)time.ElapsedGameTime.TotalSeconds / 2f;
        }

        void Die()
        {
            isDead = true;
        }
    }
}
