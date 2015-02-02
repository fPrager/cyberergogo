using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CyberErgoGoLevelData;

namespace CyberErgoGo
{
    /// <summary>
    /// The LevelContainer load and stores all playable level.
    /// </summary>
    class LevelContainer
    {
        //the list of all levels
        List<Level> Levels;

        //baseTexture for each terrain
        Texture2D IslandBase;

        //effect to combine the terrains
        Effect CombineEffect;

        /// <summary>
        /// Returns the width of the texture.
        /// <returns>the width of the texture</returns>
        /// </summary>
        public LevelContainer() 
        {
            Levels = new List<Level>();
        }

        public void LoadLevels()
        {
            SetUpDummyLevels();
        }

        private void SetUpDummyLevels()
        {
            LevelData[] levelList = null;
            Util.GetInstance().LoadFile(ref levelList, "Level", "LevelList");
            TerrainCondition newCondition = new TerrainCondition();

            Util.GetInstance().LoadFile(ref IslandBase, "Terrain", "IslandBase");
            Util.GetInstance().LoadFile(ref CombineEffect, "Terrain", "Effect");

            int bigWidth = 600;
            int bigHeight = 600;

            foreach(LevelData ld in levelList)
            {
                Texture2D terrainMap = null;
                if (ld.MapName != "NoMap")
                {
                    Util.GetInstance().LoadFile(ref terrainMap, "Level", ld.MapName);
                }
                else
                {
                    int smallWidth = 250;
                    int smallHeight = 250;
                    Color[] colors = new Color[smallWidth * smallHeight];
                    float[,] heightMap = Util.GetInstance().GetFancyTerrainHeightData(smallWidth, smallHeight, 100, 5, 30, 50);
                    Smooth(ref heightMap);
                    for (int x = 0; x < smallWidth; x++)
                        for (int y = 0; y < smallHeight; y++)
                        {
                            colors[smallWidth * x + y] = new Color(new Vector3(heightMap[x, y]));
                        }
                    terrainMap = new Texture2D(Util.GetInstance().Device, smallWidth, smallHeight, false, SurfaceFormat.Color);
                    terrainMap.SetData(colors);
                }

                terrainMap = RenderTerrainOnBase(terrainMap, bigWidth, bigHeight);

                Street street = new Street(ld.GetStreetPoints(), ld.GetIndecesOfWallPoints(), ld.GetIndecesOfStonePoints(), ld.GetIndecesOfBigSpherePoints(), ld.GetIndecesOfNoCheckpoints(), ld.StreetWidth, ld.MaxStreetHeight, (int)(bigWidth * newCondition.Zooming), (int)(bigHeight * newCondition.Zooming), (float)ld.ColorToHeightRate * 1f / (float)newCondition.Zooming, newCondition.Zooming, terrainMap);
                    
                    Terrain terrain = new StaticTerrain(street, ld.ColorToHeightRate, new HeightMap(terrainMap));
                    terrain.SetUpFullTerrain();

                    String[] players = null;
                    int[] times = null;
                    List<PlayerEntry> highcores = new List<PlayerEntry>();
                    ld.GetPlayerListAndTimes(ref players, ref times);
                    for (int i = 0; i < players.Length; i++)
                    {
                        PlayerEntry entry = new PlayerEntry();
                        entry.Name = players[i];
                        entry.Time = times[i];
                        highcores.Add(entry);
                    }
                    ld.AddPlayerEntry("Egon", 234);

                    Level level = new Level(terrain, ld.LevelName, DegreeOfDifficulty.Easy, highcores);

                    Levels.Add(level);
                    
            }
            Util.GetInstance().LevelList = Levels;
        }

        private Texture2D RenderTerrainOnBase(Texture2D terrain, int width, int height)
        {
            Texture2D combinedTerrains = null;

            GraphicsDevice Device = Util.GetInstance().Device;
            PresentationParameters pp = Device.PresentationParameters;

            RenderTarget2D rt = new RenderTarget2D(Device, width * 2, height * 2, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            Device.BlendState = BlendState.Opaque;
            Device.SetRenderTarget(rt);

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[6];

            vertices[0].Position = new Vector3(-1f, -1f, 0f);
            vertices[0].TextureCoordinate = new Vector2(0, 0);

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

            CombineEffect.CurrentTechnique = CombineEffect.Techniques["TerrainMappingWithSmoothingAndBase"];
            CombineEffect.Parameters["xTerrainTexture"].SetValue(terrain);
            CombineEffect.Parameters["xBaseTexture"].SetValue(IslandBase);

            foreach (EffectPass pass in CombineEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
            }

            Device.SetRenderTarget(null);
            combinedTerrains = rt;

            return combinedTerrains;
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

        public Level GetFirstLevel()
        {
            if (Levels.Count > 0)
                return Levels.First();
            else
                return null;
        }

        public Level GetLevelAt(int index)
        {
            return Levels.ElementAt(index);
        }

        public List<Level> GetListOfLevels()
        {
            return Levels;
        }

    }
}
