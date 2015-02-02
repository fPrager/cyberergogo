using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo{

    /// <summary>
    /// This class handles the access to a highfield.
    /// </summary>
    class HeightMap
    {
        //the underlying texture which is "covert" with heightvalues
        Texture2D Map;
        Texture2D MapAsVector4;

        public HeightMap(Texture2D map):this(map, false)
        {
        }

        public HeightMap(Texture2D map, bool convertToVector4) 
        {
            Map = map;

            if(convertToVector4)
                MapAsVector4 = Util.GetInstance().MakeTextureInVector4(Map);
        }

        /// <summary>
        /// Returns the width of the texture.
        /// <returns>the width of the texture</returns>
        /// </summary>
        public int GetWidth()
        {
            return Map.Width;
        }

        /// <summary>
        /// Returns the height of the texture.
        /// <returns>the height of the texture</returns>
        /// </summary>
        public int GetHeight()
        {
            return Map.Height;
        }

        /// <summary>
        /// Returns the texture.
        /// <returns>the texture</returns>
        /// </summary>
        public Texture2D GetMap()
        {
            return Map;
        }

        public Texture2D GetMapAsVector4()
        {
            if (MapAsVector4 == null)
               MapAsVector4 = Util.GetInstance().MakeTextureInVector4(Map);
            return MapAsVector4;
        }
    }
}
