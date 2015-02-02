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
    /// This version of a terrain represents a static ground. Every shape is clear at the beginning and normaly wont change.
    /// This fact simplifies the drawing of the terrain data (no heightmapping or normal calculation while rendering).
    /// </summary>
    class StaticTerrain:Terrain
    {
        //the vertices just stores the postion and normal (the texture or color will be mapped via texture weights)
        //VertexStructures.VertexPositionNormal[] Vertices;
        private VertexPositionNormalTexture[] Vertices;
        private int[] Indices;

        //the aray which stores the heightvalues as floats
        float[,] HeightData;

        public StaticTerrain(Street street, int colorToHeightRate, HeightMap heightMap)
            : base(street, colorToHeightRate, heightMap)
        {
        }

        /// <summary>
        /// Override the method to set the buffer. It is important because the vertex structure coudl be diffrent to a nother version of the terrain. 
        /// This influences the uffer structure.
        /// </summary>
        protected override void SetUpBuffers()
        {
            SetUpVertices();
            SetUpIndices();
            CalculateNormals();

            GraphicsDevice device = Util.GetInstance().Device;
            VertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(Vertices);

            IndexBuffer = new IndexBuffer(device, typeof(int), Indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices);
        }

        /// <summary>
        /// Set up the list of vertices with positions (depending on the height data and field dimensions).
        /// </summary>
        private void SetUpVertices() 
        {
            int width = MyCondition.TerrainWidth;
            int height = MyCondition.TerrainHeight;
            float zooming = MyCondition.Zooming;

            int maxHeight = 0;
            int minHeight = 1000;

            LoadHeightData();

            Vector3[] justPositions = new Vector3[width * height];

            Vertices = new VertexPositionNormalTexture[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float p_y = HeightData[x,y];

                    if (p_y > maxHeight) maxHeight = (int)p_y;
                    if (p_y < minHeight) minHeight = (int)p_y;

                    //to scale the terrain the x- and z-dimension is multiplicated by a zoom factor 
                    float p_x = x*zooming;
                    float p_z = y*zooming;
                    int index = x + y * width;
                    Vertices[index].Position = new Vector3(p_x, p_y, p_z);
                    Vertices[index].TextureCoordinate = new Vector2((float)x / (float)width, (float)y / (float)height);
                    justPositions[index] =  new Vector3(p_x, p_y, p_z);
                }
            }

            MyCondition.Vertices = justPositions;
            MyCondition.MaxHeight = maxHeight;
            MyCondition.MinHeight = minHeight;
        }

        /// <summary>
        /// To set up the indices of the terrain
        /// TODO: Kann das in die parent classe?
        /// gibt es unterschiede in unterklassen?
        /// </summary>
        private void SetUpIndices()
        {
            int width = MyCondition.TerrainWidth;
            int height = MyCondition.TerrainHeight;

            Indices = new int[(width - 1) * (height - 1) * 6];
            int counter = 0;
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int lowerLeft = x + y * width;
                    int lowerRight = (x + 1) + y * width;
                    int topLeft = x + (y + 1) * width;
                    int topRight = (x + 1) + (y + 1) * width;

                    Indices[counter++] = topLeft;
                    Indices[counter++] = lowerLeft;
                    Indices[counter++] = lowerRight;

                    Indices[counter++] = topLeft;
                    Indices[counter++] = lowerRight; 
                    Indices[counter++] = topRight;
                }
            }

            MyCondition.Indices = Indices;
        }

        /// <summary>
        /// Calculates the normals of the terrain vertices depending on the direct neighborhood. 
        /// It garanties a nice shading. 
        /// </summary>
        private void CalculateNormals()
        {
            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < Indices.Length / 3; i++)
            {
                int index1 = Indices[i * 3];
                int index2 = Indices[i * 3 + 1];
                int index3 = Indices[i * 3 + 2];

                Vector3 side1 = Vertices[index1].Position - Vertices[index2].Position;
                Vector3 side2 = Vertices[index1].Position - Vertices[index3].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                Vertices[index1].Normal += normal;
                Vertices[index2].Normal += normal;
                Vertices[index3].Normal += normal;
            }

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal *= -1;
                Vertices[i].Normal.Normalize();
            }
        }

        /// <summary>
        /// Load the height values stored in a heigh-field-texture
        /// </summary>
        private void LoadHeightData()
        { 
            int width = MyCondition.TerrainWidth;
            int height = MyCondition.TerrainHeight;
            float colorHeightRate = MyCondition.ZoomedColorHeightRate;

            Color[] heightMapColors = new Color[HeightMapWithStreet.GetMap().Width * HeightMapWithStreet.GetMap().Height];

            HeightMapWithStreet.GetMap().GetData(heightMapColors);

            HeightData = new float[width,height];
            int textureSizeFactor = HeightMapWithStreet.GetMap().Width / width;

            //every color value will be compremized to get a smoother terrain 
            //(instead of y-values between 0-255, we have 0/colorHeightRate - 255/colorHeightRate)
            for (int x = 0; x < width; x ++)
                for (int y = 0; y < height; y++)
                {
                    float colorValue = 0;
                    for (int xi = 0; xi < textureSizeFactor; xi++)
                    for (int yi = 0; yi < textureSizeFactor; yi++)
                    {
                        colorValue += (float)(heightMapColors[(((x * textureSizeFactor + xi) + (y * textureSizeFactor + yi) * HeightMapWithStreet.GetMap().Width))].R) / colorHeightRate;
                    }
                    HeightData[x, y] = (float)colorValue/(float)(textureSizeFactor*textureSizeFactor);
                }

            //Smooth(ref HeightData);

            //for (int x = 0; x < width; x++)
            //    for (int y = 0; y < height; y++)
            //        HeightData[x, y] /= (float)colorHeightRate;
            //Texture2D reversMap = new Texture2D(Util.GetInstance().Device, HeightMapWithStreet.GetMap().Width, HeightMapWithStreet.GetMap().Height);
            
            //for (int x = 0; x < width; x++)
            //    for (int y = 0; y < height; y++)
            //        heightMapColors[x + y * width].R = (byte)(HeightData[x, y] * (float)colorHeightRate);

            //reversMap.SetData(heightMapColors);

            //FileStream stream1 = new FileStream("TerrainWithStreet2.png", FileMode.Create);
            //reversMap.SaveAsPng(stream1, MyCondition.TerrainWidth, MyCondition.TerrainHeight);

            MyCondition.HeightValues = HeightData;
        }

        private void Smooth(ref float[,] field)
        {
            int width = field.GetLength(0);
            int height = field.GetLength(1);

            float weight0 = 16f;
            float weight1 = 8f;
            float weight2 = 1f;
            for (int x = 0; x < width - 4; x++)
            {
                for (int y = 0; y < height - 4; y++)
                {
                    float sum = (float)(
                        (field[x + 2, y + 2] * weight0) +
                        (field[x + 1, y + 1] * weight1) +
                        (field[x + 1, y + 2] * weight1) +
                        (field[x + 1, y + 3] * weight1) +
                        (field[x + 2, y + 1] * weight1) +
                        (field[x + 2, y + 3] * weight1) +
                        (field[x + 3, y + 1] * weight1) +
                        (field[x + 3, y + 2] * weight1) +
                        (field[x + 3, y + 3] * weight1) +
                        (field[x, y] * weight2) +
                        (field[x, y + 1] * weight2) +
                        (field[x, y + 2] * weight2) +
                        (field[x, y + 3] * weight2) +
                        (field[x, y + 4] * weight2) +
                        (field[x + 1, y] * weight2) +
                        (field[x + 1, y + 4] * weight2) +
                        (field[x + 2, y] * weight2) +
                        (field[x + 3, y] * weight2) +
                        (field[x + 4, y] * weight2) +
                        (field[x + 4, y + 1] * weight2) +
                        (field[x + 4, y + 2] * weight2) +
                        (field[x + 4, y + 3] * weight2) +
                        (field[x + 4, y + 4] * weight2));
                    float value = sum / (float)(weight0 + 8 * weight1 + 16 * weight2);
                    //if(HeightData[x + 2, y + 2]*colorHeightRate < 100)
                    field[x + 2, y + 2] = (float)value;
                }
            }
        }



        /// <summary>
        /// Returns the number of vertices.
        /// <returns>number of vertices</returns>
        /// </summary>
        public override int GetNumberOfVertices()
        {
            return Vertices.Length;
        }

        /// <summary>
        /// Returns the number of indices.
        /// <returns>number of indices</returns>
        /// </summary>
        public override int GetNumberOfIndices()
        {
            return Indices.Length;
        }

        public override Vector3[] GetVertices()
        {
            Vector3[] result = new Vector3[Vertices.Count()];
            
            for (int i = 1; i < Vertices.Count(); i++)
                result[i] = Vertices[i].Position;

            return result;
        }

        public override int[] GetIndices()
        {
            return Indices;
        }
    }
}
