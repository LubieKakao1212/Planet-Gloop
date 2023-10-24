using Microsoft.Xna.Framework;
using MonoEngine.Rendering.Sprites;

namespace GlobalLoopGame
{
    public class GameSprites
    {
        public static Sprite NullSprite; 
        public static Sprite Planet;
        public static Sprite SpaceshipBody;
        public static Sprite[] SpaceshipThrusterFrames;
        public static Sprite SpaceshipMagnet;

        public static Vector2 PlanetSize; 
        public static Vector2 SpaceshipBodySize;
        //We assume all frames have the same size
        public static Vector2 SpaceshipThrusterFrameSize; 
        public static Vector2 SpaceshipMagnetSize;

        public static Vector2 GetRelativeSize(Sprite referenceSprite, Vector2 referenceSize, Sprite targetSprite)
        {
            var ratioX = targetSprite.TextureRect.Width / referenceSprite.TextureRect.Width;
            var ratioY = targetSprite.TextureRect.Height / referenceSprite.TextureRect.Height;

            return new Vector2(referenceSize.X * ratioX, referenceSize.Y * ratioY);
        }

        public static void Init()
        {
            PlanetSize = new Vector2(24f, 24f);
            SpaceshipBodySize = GetRelativeSize(Planet, PlanetSize, SpaceshipBody);
            //We assume all frames have the same size
            SpaceshipThrusterFrameSize = GetRelativeSize(Planet, PlanetSize, SpaceshipThrusterFrames[0]);
            SpaceshipMagnetSize = GetRelativeSize(Planet, PlanetSize, SpaceshipMagnet);
        }
    }
}
