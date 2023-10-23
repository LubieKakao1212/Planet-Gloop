using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidWave
    {
        public List<AsteroidPlacement> asteroidPlacements = new List<AsteroidPlacement>();
        public int difficultyStage;

        public AsteroidWave(List<AsteroidPlacement> asteroids, int diffStage)
        {
            asteroidPlacements = asteroids;
            difficultyStage = diffStage;
        }
    }
}
