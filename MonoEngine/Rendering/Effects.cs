using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Rendering
{
    public static class Effects
    {
        public static Effect TilemapDefault { get; private set; }
        public static Effect Default { get; private set; }

        public const string GridRS = "GridRS";
        public const string GridT = "GridT";

        public const string SpriteAtlas = "SpriteAtlas";

        private const string TilemapDefaultEffectPath = "Tilemap";
        private const string DefaultEffectPath = "Default";

        public static void Init(ContentManager content)
        {
            TilemapDefault = content.Load<Effect>(TilemapDefaultEffectPath);
            Default = content.Load<Effect>(DefaultEffectPath);
        }
    }
}
