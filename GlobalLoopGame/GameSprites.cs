using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Rendering.Sprites;

namespace GlobalLoopGame
{
    public static class GameSprites
    {
        public static Sprite NullSprite; 
        public static Sprite Circle;
        public static Sprite Planet;
        public static Sprite SpaceshipBody;
        public static Sprite[] SpaceshipThrusterFrames;
        public static Sprite SpaceshipMagnet;
        public static Sprite TurretBase;
        //0 - active; 2 - piced up/inactive 
        public static Sprite[] TurretCannon;
        public static Sprite[] TurretShotgun;
        public static Sprite[] TurretSniper;
        public static Sprite Laser;
        public static Sprite Warning;

        public static SpriteFont Font;

        public static Sprite MenuBackground;

        public static Vector2 PlanetSize; 
        public static Vector2 SpaceshipBodySize;
        //We assume all frames have the same size
        public static Vector2 SpaceshipThrusterFrameSize; 
        public static Vector2 SpaceshipMagnetSize;
        public static Vector2 TurretBaseSize;
        public static Vector2[] TurretCannonSizes;
        public static Vector2[] TurretShotgunSizes;
        public static Vector2[] TurretSniperSizes;
        public static Vector2 LaserSize;

        public static float pixelsPerUnit;

        public static Vector2 GetRelativeSize(Sprite referenceSprite, Vector2 referenceSize, Sprite targetSprite)
        {
            var ratioX = targetSprite.TextureRect.Width / referenceSprite.TextureRect.Width;
            var ratioY = targetSprite.TextureRect.Height / referenceSprite.TextureRect.Height;

            return new Vector2(referenceSize.X * ratioX, referenceSize.Y * ratioY);
        }

        public static Vector2[] GetRelativeSizeArr(Sprite referenceSprite, Vector2 referenceSize, params Sprite[] targetSprites)
        {
            var arrOut = new Vector2[targetSprites.Length];
            for (int i = 0; i < targetSprites.Length; i++)
            {
                arrOut[i] = GetRelativeSize(referenceSprite, referenceSize, targetSprites[i]);
            }
            return arrOut;
        }

        public static void Init()
        {
            PlanetSize = new Vector2(24f, 24f);
            //128 = Planet texture size
            pixelsPerUnit = 128f / 24f;
            SpaceshipBodySize = GetRelativeSize(Planet, PlanetSize, SpaceshipBody);
            //We assume all frames have the same size
            SpaceshipThrusterFrameSize = GetRelativeSize(Planet, PlanetSize, SpaceshipThrusterFrames[0]);
            SpaceshipMagnetSize = GetRelativeSize(Planet, PlanetSize, SpaceshipMagnet);

            TurretBaseSize = GetRelativeSize(Planet, PlanetSize, TurretBase);
            TurretCannonSizes = GetRelativeSizeArr(Planet, PlanetSize, TurretCannon);
            TurretShotgunSizes = GetRelativeSizeArr(Planet, PlanetSize, TurretShotgun);
            TurretSniperSizes = GetRelativeSizeArr(Planet, PlanetSize, TurretSniper);
            LaserSize = GetRelativeSize(Planet, PlanetSize, Laser);
        }
    }
}
