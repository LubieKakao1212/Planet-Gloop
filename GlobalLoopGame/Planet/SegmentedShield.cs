using GlobalLoopGame.Globals;
using GlobalLoopGame.Asteroid;
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

        public int MaxSegmentHealth { get; private set; }

        public int TotalSegmentHealth => MaxSegmentHealth * segments.Length;

        public int TotalHeaelthLeft => segmentHealth.Sum();

        private static Color[] healthColors = new Color[]
        {
            Color.Cyan * 0.25f,
            Color.Red,
            Color.Orange,
            Color.Cyan
        };

        private HashSet<Body> hitBy = new HashSet<Body>();

        public SegmentedShield(World world, RenderPipeline renderer, int segments, float radius, float arcAngle, int healthPerSegment) : base(null)
        {
            var body = world.CreateBody(bodyType: BodyType.Kinematic);
            body.Tag = this;
            PhysicsBody = body;

            float angle = arcAngle / segments;
            var deltaAngle = angle + (angle / (segmentResolution - 2)) * 2;
    
            display = new ShieldDisplay(renderer, segments, radius, 1f, 0f, 100f);
            display.Transform.LocalScale = new Vector2(radius * 3f);
            display.Parent = this;

            MaxSegmentHealth = healthPerSegment;
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
                fixture.Tag = i;
                var i1 = i;
                fixture.OnCollision += (thisFixture, otherFixture, contact) =>
                {
                    if (otherFixture.CollisionCategories.HasFlag(CollisionCats.Asteroids) && !hitBy.Contains(otherFixture.Body))
                    {
                        hitBy.Add(otherFixture.Body);
                        ModifySegment(i1, -1);
                    }
                    return true;
                };
            }
        }

        public int GetSegmentHealth(int idx)
        {
            return segmentHealth[idx];
        }

        public void ModifySegment(int idx, int amount)
        {
            var health = segmentHealth[idx];

            health = MathHelper.Clamp(health + amount, 0, MaxSegmentHealth);

            segmentHealth[idx] = health;

            UpdateSegment(idx);
        }

        private void UpdateSegment(int idx)
        {
            var health = segmentHealth[idx];

            if (health == 0)
            {
                segments[idx].CollidesWith = CollisionCats.CollisionsShieldDestroyed;
            }
            else
            {
                segments[idx].CollidesWith = CollisionCats.CollisionsShield;
            }

            display.SetSegmentColor(idx, healthColors[MathHelper.Min(health, healthColors.Length)]);
        }

        public override void Update(GameTime time)
        {
            hitBy.Clear();
        }

        public void OnGameEnd()
        {

        }

        public void Reset()
        {
            for (int i = 0; i < segmentHealth.Length; i++)
            {
                segmentHealth[i] = MaxSegmentHealth;
                UpdateSegment(i);
            }
        }
    }
}
