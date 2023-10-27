using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Globals
{
    public static class CollisionCats
    {
        public static readonly Category Planet = Category.Cat5;
        public static readonly Category Spaceship = Category.Cat2;
        public static readonly Category Asteroids = Category.Cat1;
        public static readonly Category Turrets = Category.Cat3;
        public static readonly Category Shield = Category.Cat6;
        public static readonly Category Bullets = Category.Cat4;
        public static readonly Category RepairCharge = Category.Cat7;
        public static readonly Category SupplyBox = Category.Cat8;

        public static readonly Category CollisionsPlanet = Asteroids | Spaceship | Turrets | Bullets | SupplyBox;
        public static readonly Category CollisionsSpaceship = Turrets | Asteroids | Planet | RepairCharge | SupplyBox;
        public static readonly Category CollisionsAsteroids = Spaceship | Planet | Shield | Bullets;
        public static readonly Category CollisionsTurrets = Turrets | Spaceship | Planet;
        public static readonly Category CollisionsShield = Asteroids | Bullets | RepairCharge;
        public static readonly Category CollisionsShieldDestroyed = RepairCharge;
        public static readonly Category CollisionsBullets = Asteroids | Planet | SupplyBox;
        public static readonly Category CollisionsRepairCharge = Shield | Spaceship;// | RepairCharge | SupplyBox;
        public static readonly Category CollisionsSupplyBox = Spaceship | RepairCharge | SupplyBox | Bullets | Planet;

        public static readonly Category TurretTargets = Asteroids | SupplyBox;

    }
}
