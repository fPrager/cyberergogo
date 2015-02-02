using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

namespace CyberErgoGo
{
    /// <summary>
    /// This class is the abstraction of a terrain. 
    /// It's not important to know how the terrain data looks like or how it will be drawn (if it is static or dynamic).
    /// This pattern offers the ability to implement a bridge pattern.
    /// </summary>
    abstract class Terrain
    {
        //the terrain specific height field (stores the texture) without the street
        protected HeightMap HeightMap;

        //the street in the terrain
        protected Street Street;

        public Vector3 StreetStartPoint
        {
            get { return new Vector3(Street.StartPoint.X, Street.StartPoint.Y, Street.StartPoint.Z); }
        }

        public Vector3 SteetEndPoint
        {
            get { return Street.EndPoint; }
        }

        public List<Vector3> StreetCheckPoints
        {
            get { return Street.StreetCheckPoints; }
        }

        public List<Vector3> WallPoints
        {
            get { return Street.WallPoints; }
        }

        public List<Vector3> SpherePoints
        {
            get { return Street.SpherePoints; }
        }

        public List<Vector3> StonePoints
        {
            get { return Street.StonePoints; }
        }

        public VertexBuffer GetStreetVertexBuffer()
        {
            return Street.GetVertexBuffer();
        }

        public IndexBuffer GetStreetIndexBuffer()
        {
            return Street.GetIndexBuffer();
        }

        //the terrain height field with the street "on" it
        protected HeightMap HeightMapWithStreet;

        //the effect to "brand"/render the street in the terrain
        protected Effect TerrainEffect;

        protected float[,] HeightValues;

        //the condition of the terrain
        //it's changed just by the terrain instance
        protected TerrainCondition MyCondition;

        //the buffers of the terrain
        protected VertexBuffer VertexBuffer;
        protected IndexBuffer IndexBuffer;

        //the texture of the island
        protected Texture2D IslandBase;

        public Terrain(Street street, int colorToHeightRate, HeightMap heightMap) 
        {
            Street = street;
            HeightMap = heightMap;

            Util.GetInstance().LoadFile(ref TerrainEffect, "Terrain", "Effect");
            Util.GetInstance().LoadFile(ref IslandBase, "Terrain", "IslandBaseP");
            MyCondition = new TerrainCondition();

            //the dimensions of the terrain are strikted by the texture-dimensions
            //attention: it doesn't means the size in worldspace! it will be zoomed (Zooming)
            
            MyCondition.ColorHeightRate = colorToHeightRate;
        }

        public void SetUpFullTerrain()
        {
            GenerateHeightMapWithStreet();
            SetUpBuffers();
        }

        public HeightMap GetHeightMapWithStreet()
        {
            return HeightMapWithStreet;
        }

        public float GetColorToHeightRate()
        {
            return MyCondition.ColorHeightRate;
        }

        /// <summary>
        /// This method draw the street on the terrain. It brings the terrain into line with the street.
        /// <param name="Device">the graphic device</param>
        /// </summary>
        private void RenderStreetOnTerrain(GraphicsDevice Device) 
        {
            SpriteBatch spriteBatch = new SpriteBatch(Device);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, null, new DepthStencilState(), Device.RasterizerState, TerrainEffect);
            TerrainEffect.CurrentTechnique = TerrainEffect.Techniques["StreetOnTerrainMapping"];
            TerrainEffect.Parameters["xStreetTexture"].SetValue(Street.GetStreetTexture());
            TerrainEffect.Parameters["xTerrainTexture"].SetValue(HeightMap.GetMap());            
            Device.SamplerStates[0] = SamplerState.AnisotropicClamp;
            Device.SamplerStates[1] = SamplerState.AnisotropicClamp;
            spriteBatch.Draw(HeightMap.GetMap(), new Rectangle(0, 0, MyCondition.TerrainWidth * 2, MyCondition.TerrainHeight * 2), Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// This method draw the street on the terrain AFTER smoothing the terrain to an island...wow. It brings the terrain into line with the street.
        /// <param name="Device">the graphic device</param>
        /// </summary>
        private void RenderStreetOnTerrainWithSmoothing(GraphicsDevice Device)
        {
            SpriteBatch spriteBatch = new SpriteBatch(Device);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, null, new DepthStencilState(), Device.RasterizerState, TerrainEffect);
            TerrainEffect.CurrentTechnique = TerrainEffect.Techniques["StreetOnTerrainMappingWithSmoothing"];
            TerrainEffect.Parameters["xStreetTexture"].SetValue(Street.GetStreetTexture());
            TerrainEffect.Parameters["xTerrainTexture"].SetValue(HeightMap.GetMap());
            spriteBatch.Draw(HeightMap.GetMap(), new Rectangle(0, 0, MyCondition.TerrainWidth * 2, MyCondition.TerrainHeight * 2), Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// This method draw the street on the smoothed terrain with an underlying static terrain in the shape of an island. It brings the terrain into line with the street.
        /// <param name="Device">the graphic device</param>
        /// </summary>
        private void RenderStreetOnTerrainWithSmoothingAsIsland(GraphicsDevice Device)
        {

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[6];

            vertices[0].Position = new Vector3(-1f, -1f, 0f);
            vertices[0].TextureCoordinate = new Vector2(0,0);

            vertices[1].Position = new Vector3(-1, 1f, 0f);
            vertices[1].TextureCoordinate = new Vector2(0, 1);

            vertices[2].Position = new Vector3(1f, -1f, 0f);
            vertices[2].TextureCoordinate = new Vector2(1, 0);

            vertices[3].Position = new Vector3(-1f, 1f, 0f);
            vertices[3].TextureCoordinate = new Vector2(0, 1);

            vertices[4].Position = new Vector3(1f, 1f, 0f);
            vertices[4].TextureCoordinate = new Vector2(1, 1);

            vertices[5].Position = new Vector3(1f, -1f, 0f);
            vertices[5].TextureCoordinate = new Vector2(1, 0);

            TerrainEffect.CurrentTechnique = TerrainEffect.Techniques["StreetOnTerrainMappingWithSmoothingAndBase"];
            TerrainEffect.Parameters["xStreetTexture"].SetValue(Street.GetStreetTexture());
            TerrainEffect.Parameters["xTerrainTexture"].SetValue(HeightMap.GetMap());
            TerrainEffect.Parameters["xBaseTexture"].SetValue(IslandBase);
            
            foreach (EffectPass pass in TerrainEffect.CurrentTechnique.Passes)
             {
                 pass.Apply();
                 Device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
             }
        }

        /// <summary>
        /// This method sets the rendertargets to draw the street texture on the terrain's heightfield and starts the rendering process.
        /// </summary>
        private void GenerateHeightMapWithStreet() 
        {
            //MyCondition.TerrainWidth = 4096;
            //MyCondition.TerrainHeight = 4096;

            GraphicsDevice Device = Util.GetInstance().Device;
            PresentationParameters pp = Device.PresentationParameters;

            RenderTarget2D rt = new RenderTarget2D(Device, MyCondition.TerrainWidth*2, MyCondition.TerrainHeight*2, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            Device.BlendState = BlendState.Opaque;
            Device.SetRenderTarget(rt);

            RenderStreetOnTerrain(Device);

            Device.SetRenderTarget(null);
            HeightMapWithStreet = new HeightMap(rt);

            //FileStream stream1 = new FileStream("TerrainWithStreet.png", FileMode.Create);
            //rt.SaveAsPng(stream1, MyCondition.TerrainWidth * 2, MyCondition.TerrainHeight * 2);

            //FileStream stream2 = new FileStream("Street.png", FileMode.Create);
            //Street.GetStreetTexture().SaveAsPng(stream2, MyCondition.TerrainWidth * 2, MyCondition.TerrainHeight * 2);

            //FileStream stream3 = new FileStream("Terrain.png", FileMode.Create);
            //HeightMap.GetMap().SaveAsPng(stream3, MyCondition.TerrainWidth * 2, MyCondition.TerrainHeight * 2);
        }

        /// <summary>
        /// This method should be overridden by the subclasses, which are different in ther vertex-structures -> buffers
        /// </summary>
        protected virtual void SetUpBuffers(){}

        /// <summary>
        /// Returns the street with its points and texture.
        /// <returns>the street of the terrain</returns>
        /// </summary>
        public Street GetStreet()
        {
            return Street;
        }

        public void SetMyCondition()
        {
            Street.LoadTerrainStreetPoints();
            ConditionHandler.GetInstance().SetCondition(MyCondition);
        }

        /// <summary>
        /// Returns the index buffer for drawing.
        /// <returns>index buffer</returns>
        /// </summary>
        public IndexBuffer GetIndexBuffer()
        {
            return IndexBuffer;
        }

        /// <summary>
        /// Returns the vertex buffer for drawing.
        /// <returns>vertex buffer</returns>
        /// </summary>
        public VertexBuffer GetVertexBuffer()
        {
            return VertexBuffer;
        }

        /// <summary>
        /// This getter returns the number of vertices (for drawing). 
        /// It is abstract because the abstract terrain can't know his vertices and how much it has.
        /// <returns>the number of terrain vertices</returns>
        /// </summary>
        public abstract int GetNumberOfVertices();

        /// <summary>
        /// This getter returns the number of indices (for drawing). 
        /// It is abstract because the abstract terrain can't know his vertices and how much indices it needs to index them.
        /// <returns>the number of indices</returns>
        /// </summary>
        public abstract int GetNumberOfIndices();


        public abstract Vector3[] GetVertices();

        public abstract int[] GetIndices();

        public Quaternion GetStreetOrientation(Vector3 positionOnStreet)
        {
            return Street.ToTheNextOrientation(positionOnStreet);
        }

        public bool IsThisPositionOverStreet(Vector3 position)
        {
            return Street.IsThisPositionInRegion(position);
        }

        public float GetHeightAtThisPoint(Vector2 xzCoordinates)
        {
            float height = 0;
            Vector2 relativeXZ = new Vector2(xzCoordinates.X/MyCondition.AreaWidth, xzCoordinates.Y/MyCondition.AreaHeight);
            height = MyCondition.HeightValues[(int)(relativeXZ.X * MyCondition.HeightValues.GetLength(0)), (int)(relativeXZ.Y * MyCondition.HeightValues.GetLength(1))];
            return height;
        }
    }
}
