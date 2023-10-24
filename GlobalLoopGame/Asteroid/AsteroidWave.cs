using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidWave
    {
        public List<AsteroidPlacement> asteroidPlacements = new List<AsteroidPlacement>();
        public int difficultyStage;
        public List<float> warningPlacements = new List<float>();

        public AsteroidWave(List<AsteroidPlacement> asteroids, int diffStage, List<float> warnings)
        {
            asteroidPlacements = asteroids;
            difficultyStage = diffStage;
            warningPlacements = warnings;
        }
    }
}
