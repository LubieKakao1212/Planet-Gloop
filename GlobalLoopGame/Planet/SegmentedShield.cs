using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using MonoEngine.Rendering;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalLoopGame.Planet
{
    public class SegmentedShield : PhysicsBodyObject, IResettable
    {
        public const int segmentResolution = 15;

        private ShieldDisplay display;

        private int[] segmentHealth;
        private Fixture[] segments;

        private int initialHealth;

        private static Color[] healthColors = new Color[]
        {
            Color.Transparent,
            Color.Red,
            Color.Orange,
            Color.Cyan
        };

        public SegmentedShield(World world, RenderPipeline renderer, int segments, float radius, float arcAngle, int healthPerSegment) : base(null)
        {
            var body = world.CreateBody(bodyType: BodyType.Kinematic);
            PhysicsBody = body;

            float angle = arcAngle / segments;
            var deltaAngle = angle + (angle / (segmentResolution - 2)) * 2;
    
            display = new ShieldDisplay(renderer, segments, radius, 1f, 0f, 100f);
            display.Transform.LocalScale = new Vector2(radius * 3f);
            display.Parent = this;

            initialHealth = healthPerSegment;
            segmentHealth = Enumerable.Repeat(healthPerSegment, segments).ToArray();
            this.segments = new Fixture[segments];
            
            for (int i = 0; i < segments; i++)
            {
                Vertices vertices = PolygonTools.CreateArc(deltaAngle, segmentResolution, radius);
                vertices.Rotate((angle) * (i - segments / 2));
                var fixture = body.CreateChainShape(vertices);
                fixture.CollisionCategories = CollisionCats.Shield;
                fixture.CollidesWith = CollisionCats.CollisionsShield;
                this.segments[i] = fixture;
                var i1 = i;
                fixture.OnCollision += (thisFixture, otherFixture, contact) =>
                {
                    if (otherFixture.CollisionCategories.HasFlag(CollisionCats.Asteroids))
                    {
                        ModifySegment(i1, -1);
                    }
                    return true;
                };
            }
        }

        public void ModifySegment(int idx, int amount)
        {
            var health = segmentHealth[idx];
            health = MathHelper.Max(health + amount, 0);
            segmentHealth[idx] = health;
            UpdateSegment(idx);
        }

        private void UpdateSegment(int idx)
        {
            var health = segmentHealth[idx];
            if (health == 0)
            {
                segments[idx].CollidesWith = Category.None;
            }
            else
            {
                segments[idx].CollidesWith = CollisionCats.CollisionsShield;
            }
            display.SetSegmentColor(idx, healthColors[MathHelper.Min(health, healthColors.Length)]);
        }

        public void OnGameEnd()
        {

        }

        public void Reset()
        {
            for (int i = 0; i < segmentHealth.Length; i++)
            {
                segmentHealth[i] = initialHealth;
                UpdateSegment(i);
            }
        }
    }
}
