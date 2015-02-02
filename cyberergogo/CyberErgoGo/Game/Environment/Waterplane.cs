using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    /// <summary>
    /// This class handles the water plane/sea of the game.
    /// </summary>
    class Waterplane
    {
        float WaterLevel = 0;

        //at the begining it's just a blue plane, maybe it could be increase by adding textures, ripples or displacements
        float MinXDim = 0;
        float MaxXDim = 0;
        float MinYDim = 0;
        float MaxYDim = 0;

        float TideOffset;
        const float TideSpeed = 0.5f;

        const float TideHeight = 0.3f;
        bool IsComing = true;

        VertexPositionColorTexture[] Vertices;

        Color WaterColor = Color.DarkBlue;

        int SurfaceDivision = 100;

        VertexBuffer VBuffer;
        IndexBuffer IBuffer;

        /// <summary>
        /// create a planar water surface parallel to the X-Y-Plain
        /// </summary>
        /// <param name="minX">smallest X coord</param>
        /// <param name="maxX">biggest X coord</param>
        /// <param name="minY">smallest Y coord</param>
        /// <param name="maxY">biggest Y coord</param>
        /// <param name="level">the z-level of the plane</param>
        /// <param name="waterColor">color of the surface</param>
        public Waterplane(float minX, float maxX, float minY, float maxY, float level, Color waterColor)
        {
            MinXDim = minX;
            MaxXDim = maxX;

            MinYDim = minY;
            MaxYDim = maxY;

            WaterLevel = level;

            WaterColor = waterColor;

            SetUpBuffers();
        }

        /// <summary>
        /// create a planar water surface parallel to the X-Y-Plain in CornFlowerBlue
        /// </summary>
        /// <param name="minX">smallest X coord</param>
        /// <param name="maxX">biggest X coord</param>
        /// <param name="minY">smallest Y coord</param>
        /// <param name="maxY">biggest Y coord</param>
        /// <param name="level">the z-level of the plane</param>
        public Waterplane(float minX, float maxX, float minY, float maxY, float level)
            : this(minX, maxX, minY, maxY, level, Color.CornflowerBlue) { }

        /// <summary>
        /// create a planar water surface on the X-Y-Plain in CornFlowerBlue
        /// </summary>
        /// <param name="minX">smallest X coord</param>
        /// <param name="maxX">biggest X coord</param>
        /// <param name="minY">smallest Y coord</param>
        /// <param name="maxY">biggest Y coord</param>
        /// <param name="level">the z-level of the plane</param>
        public Waterplane(float minX, float maxX, float minY, float maxY)
            : this(minX, maxX, minY, maxY, 0, Color.CornflowerBlue) { }

        public void Update(float elapsedGameTimeInMilliseconds)
        {
            if (TideOffset >= 1)
                IsComing = false;
            if(TideOffset <= 0)
                IsComing = true;

            if(IsComing)
                TideOffset += TideSpeed * elapsedGameTimeInMilliseconds / 1000;
            
            if (!IsComing)
                TideOffset -= TideSpeed * elapsedGameTimeInMilliseconds / 1000;
        }

        /// <summary>
        /// Set up buffers with colored vertices
        /// </summary>
        private void SetUpBuffers()
        {
            Vector2 lb = new Vector2(0, 0);
            Vector2 rb = new Vector2(1, 0);
            Vector2 lt = new Vector2(0, 1);
            Vector2 rt = new Vector2(1, 1);

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];

            vertices[0] = new VertexPositionColorTexture(new Vector3(MinXDim, WaterLevel, MinYDim), WaterColor, new Vector2(0,1));
            vertices[1] = new VertexPositionColorTexture(new Vector3(MinXDim, WaterLevel, MaxYDim), WaterColor, new Vector2(0, 0));
            vertices[2] = new VertexPositionColorTexture(new Vector3(MaxXDim, WaterLevel, MaxYDim), WaterColor, new Vector2(1, 0));
            vertices[3] = new VertexPositionColorTexture(new Vector3(MaxXDim, WaterLevel, MinYDim), WaterColor, new Vector2(1, 1));

            Vertices = vertices;

            VBuffer = new VertexBuffer(Util.GetInstance().Device, VertexPositionColorTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            VBuffer.SetData(vertices);

            int[] indeces = new int[6];
            indeces[0] = 0;
            indeces[1] = 1;
            indeces[2] = 2;
            indeces[3] = 2;
            indeces[4] = 3;
            indeces[5] = 0;

            IBuffer = new IndexBuffer(Util.GetInstance().Device, typeof(int), indeces.Length, BufferUsage.WriteOnly);
            IBuffer.SetData(indeces);
        }


        public VertexBuffer GetVertexBuffer()
        {
            return VBuffer;
        }

        public IndexBuffer GetIndexBuffer()
        {
            return IBuffer;
        }

        public VertexPositionColorTexture[] GetVertices()
        {
            return Vertices;
        }

        public float GetTideOffset()
        {
            return TideOffset;
        }

        public float GetMaxTideHeight()
        {
            return TideHeight;
        }
    }
}
