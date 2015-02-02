using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using System.IO;
using CyberErgoGoLevelData;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Xml;

namespace CyberErgoGo
{
    /// <summary>
    /// This class just stores useful functions and global instances of important classes.
    /// It is a singleton to garanty the right handling of global variables over the whole game.
    /// <example>
    /// - draw full screen rectangles (sounds silly, but is important!)
    /// - handles the loading of files
    /// - access to the graphic device, content manager
    /// </example>
    /// </summary>
    class Util
    {
        //it is part of the singleton to capsule the access of the util-instance  
        private static Util Instance;

        //the global instance of the graphic device
        public GraphicsDevice Device {get; set; }

        //the global instance of the content manager
        //TODO: Muss das "public" sein, wenn keiner den ContentManager direkt nutzt?
        public ContentManager Content{get; set;}

        //the spritebatch to draw full screen quads
        private SpriteBatch SpriteBatch;

        private PerlinGenerator Perlin;

        public CollisionChecker CollisionsChecker { get; set; }

        public SoundManager SoundManager { get; set; }

        private Random RandnumberGenerator;

        public List<Level> LevelList = new List<Level>();

        /// <summary>
        /// the constructor is private that it's not possible to make an "external" instance
        /// </summary>
        private Util() 
        {
            RandnumberGenerator = new Random();
        }

        /// <summary>
        /// This mehtod handles the access to the only one instance of Util
        /// <returns>the global Util-instance</returns>
        /// </summary>
        public static Util GetInstance() 
        {
            if (Instance == null)
            {
                Instance = new Util();
            }
            return Instance;
        }

        /// <summary>
        /// At the beginning the global instance of the graphic device will be setted by the MainGame
        /// <param name="device">the graphic device of the game</param>
        /// </summary>
        public void SetGraphicDevice(GraphicsDevice device)
        {
            Device = device;
            SpriteBatch = new SpriteBatch(Device);
        }

        /// <summary>
        /// At the beginning the global instance of the content manager will be setted by the MainGame
        /// <param name="content">the content manager of the xna-game</param>
        /// </summary>
        public void SetContentManager(ContentManager content)
        {
            Content = content;
        }

        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// <param name="effect">the udes effect</param>
        /// <param name="renderTarget">the target of the quad/rectangle represantation</param>
        /// <param name="texture">the texture of the quad/rectangle</param>
        /// </summary>
        public void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget,
                                Effect effect)
        {
            Device.SetRenderTarget(renderTarget);
            DrawFullscreenQuad(texture, 0,
                               renderTarget.Width, renderTarget.Height,
                               effect);
        }

        public float CalculateModelScaleToOneFactor(Model model)
        {
            BoundingSphere bounding = new BoundingSphere(Vector3.Zero, 0);
            foreach (ModelMesh mesh in model.Meshes)
                bounding = BoundingSphere.CreateMerged(bounding, mesh.BoundingSphere);
            return (float)1 / bounding.Radius;
        }

        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// <param name="effect">the used effect</param>
        /// <param name="height">the height of the quad/rectangle</param>
        /// <param name="texture">the texture of the quad/rectangle</param>
        /// <param name="width">the width of the quad/rectangle</param>
        /// <param name="x">the left offset/border 
        /// (It is important to be able to draw a left screen and a "right moved" right screen)</param>
        /// </summary>
        public void DrawFullscreenQuad(Texture2D texture, int x, int width, int height,
                                Effect effect)
        {
            SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, null, new DepthStencilState(), Device.RasterizerState, effect);
            SpriteBatch.Draw(texture, new Rectangle(x, 0, width, height), Color.White);
            SpriteBatch.End();
        }

        public Texture2D MakeTextureInVector4(Texture2D originalTexture)
        {
            if (Device == null)
            {
                Console.WriteLine("GraphicDevice is missing to convert color texture in vector4 texture");
                return null;
            }

            Color[] colors = new Color[originalTexture.Width * originalTexture.Height];

            originalTexture.GetData(colors);

            Vector4[] newTextureValues = new Vector4[originalTexture.Width * originalTexture.Height];
            for (int i = 0; i < colors.Length; i++)
            {
                newTextureValues[i] = new Vector4((float)colors[i].R / 255f, (float)colors[i].G / 255f, (float)colors[i].B / 255f, (float)colors[i].A / 255f);
            }

            Texture2D newTexture = new Texture2D(Device, originalTexture.Width, originalTexture.Height, false, SurfaceFormat.Vector4);
            newTexture.SetData(newTextureValues);

            return newTexture;
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// <param name="n">thats the distance to the middle
        /// TODO: Überprüfen</param>
        /// <param name="theta">thats the shaping of the gaussian curve</param>
        /// </summary>
        public float ComputeGaussian(float n, float theta)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        /// <summary>
        /// This methods offers an easy way to load an effect file
        /// <param name="directory"> the directory to the file</param>
        /// <param name="effect">the effect instance which will hold the effect</param>
        /// <param name="fileName">the name of the effect/file</param>
        /// </summary>
        public void LoadFile(ref Effect effect, String directory, String fileName)
        {
            effect = Content.Load<Effect>(directory+"/"+fileName);
        }

        /// <summary>
        /// This methods offers an easy way to load a sound file
        /// <param name="directory"> the directory to the file</param>
        /// <param name="effect">the effect instance which will hold the effect</param>
        /// <param name="fileName">the name of the effect/file</param>
        /// </summary>
        public void LoadFile(ref Song sound, String directory, String fileName)
        {
            sound = Content.Load<Song>(directory + "/" + fileName);
        }

        /// <summary>
        /// This methods offers an easy way to load a sound file
        /// <param name="directory"> the directory to the file</param>
        /// <param name="effect">the effect instance which will hold the effect</param>
        /// <param name="fileName">the name of the effect/file</param>
        /// </summary>
        public void LoadFile(ref SoundEffect soundeffect, String directory, String fileName)
        {
            soundeffect = Content.Load<SoundEffect>(directory + "/" + fileName);
        }

        /// <summary>
        /// This methods offers an easy way to load a texture file
        /// <param name="directory"> the directory to the file</param>
        /// <param name="texture">the texture instance which will hold the image</param>
        /// <param name="fileName">the name of the texture/file</param>
        /// </summary>
        public void LoadFile(ref Texture2D texture, String directory, String fileName)
        {
            
            //String fileloc = directory + "/" + fileName;
            //if (directory == "Canon")
            //{
            //    foreach (var c in fileloc)
            //    {
            //        Console.WriteLine(c + "  " );
            //    }
            //}
            texture = Content.Load<Texture2D>(directory + "/" + fileName);
        }

        /// <summary>
        /// This methods offers an easy way to load a model
        /// <param name="directory"> the directory to the file</param>
        /// <param name="model">the model instance which will hold the pointnet</param>
        /// <param name="fileName">the name of the model/file</param>
        /// </summary>
        public void LoadFile(ref Model model, String directory, String fileName)
        {
            model = Content.Load<Model>(directory + "/" + fileName);
        }

        /// <summary>
        /// This methods offers an easy way to load a font
        /// <param name="directory"> the directory to the file</param>
        /// <param name="font">the font object</param>
        /// <param name="fileName">the name of the model/file</param>
        /// </summary>
        public void LoadFile(ref SpriteFont font, String directory, String fileName)
        {
            font = Content.Load<SpriteFont>(directory + "/" + fileName);
        }

        /// <summary>
        /// This methods offers an easy way to load a list of level datas.
        /// XNA offers an interesting way to serialize XML-files. 
        /// This file just uses the data structure of LevelData (in a seperate library).
        /// The XMLSerialization is a part of the ConentManager.
        /// <param name="directory"> the directory to the file</param>
        /// <param name="levelData">the list of level datas, which stores the inforamtions of some levels</param>
        /// <param name="fileName">the name of the xml/file</param>
        /// </summary>
        public void LoadFile(ref LevelData[] levelData, String directory, String fileName)
        {
            levelData = Content.Load<LevelData[]>(directory + "/" + fileName);
            
        }

        public void SaveLevelData(LevelData[] levelData, String directory, String fileName)
        {
            FileStream fs = new FileStream(Path.Combine(Content.RootDirectory, directory, fileName + ".xml"), FileMode.Create);
            XmlSerializer xs = new XmlSerializer(levelData.GetType());
            xs.Serialize(fs, levelData);
            fs.Close();
        }

        public void SaveLevelData()
        {
            FileStream fs = new FileStream(Path.Combine(Content.RootDirectory, "Level", "LevelList.xml"), FileMode.Create);
            XmlSerializer xs = new XmlSerializer(LevelList.GetType());
            xs.Serialize(fs, LevelList);
            fs.Close();
        }

        public void SetEffect(ref Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
        }

        public int GetRandomNumber(int max)
        {
            return RandnumberGenerator.Next(max);
        }

        public int GetRandomNumber(int min, int max)
        {
            return RandnumberGenerator.Next(min,max);
        }

        public Quaternion ToNextPointRotation(Vector3 pos1, Vector3 pos2)
        {
            Vector3 toNext = pos1 - pos2;
            toNext = Vector3.Normalize(toNext);
            toNext = new Vector3((float)Math.Round(toNext.X, 2), (float)Math.Round(toNext.Y, 2), (float)Math.Round(toNext.Z, 2));
            Vector3 right = Vector3.Cross(toNext, Vector3.Up);
            right = Vector3.Normalize(right);
            right = new Vector3((float)Math.Round(right.X, 2), (float)Math.Round(right.Y, 2), (float)Math.Round(right.Z, 2));
            

            Vector3 up = Vector3.Cross(right, toNext);
            up = Vector3.Normalize(up);
            up = new Vector3((float)Math.Round(up.X, 2), (float)Math.Round(up.Y, 2), (float)Math.Round(up.Z, 2));
            

            Matrix rotation = Matrix.Identity;
            rotation.Forward = toNext;
            rotation.Right = right;
            rotation.Up = up;

            return Quaternion.CreateFromRotationMatrix(rotation);
        }

        public float[,] GetFancyTerrainHeightData(int width, int height, float noiseScale, float smoothness, float d, float f)
        {
            float[,] heightField = new float[width, height];

            //Console.WriteLine("jetzt wird neues generiert");
            Perlin = new PerlinGenerator(Util.GetInstance().GetRandomNumber(214));
            //Grundmuster
            float lowestLevel = 0;
            float highestLevel = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float heightValue = Perlin.Noise(f * x / (float)width, f * y / (float)width, 0) * noiseScale;
                    //if (heightValue > maxHeight) maxHeight = heightValue;
                    //if (heightValue < minHeight) minHeight = heightValue;
                    

                    heightField[x , y] = heightValue;

                        if (heightValue < lowestLevel)
                        {
                            lowestLevel = heightValue;
                        }
                        if (heightValue > highestLevel)
                        {
                            highestLevel = heightValue;
                        }
                }
            }

            // Console.WriteLine("Lowest Level = " + lowestLevel + " (" + howMuch + ").");

            //  Aufmischung

            int u, v;
            float[,] temp = new float[width,height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; ++j)
                {
                    u = i + (int)(Perlin.Noise(f * i / (float)width, f * j / (float)height, 0) * d);
                    v = j + (int)(Perlin.Noise(f * i / (float)width, f * j / (float)height, 1) * d);
                    if (u < 0) u = 0; if (u >= width) u = width - 1;
                    if (v < 0) v = 0; if (v >= height) v = height - 1;
                    temp[i,j] = heightField[u , v ];
                }
            }
            heightField = temp;



            //Errodieren

            for (int k = 0; k < 10; k++)
                for (int i = 1; i < width - 1; i++)
                {
                    for (int j = 1; j < height - 1; j++)
                    {
                        float d_max = 0.0f;
                        int[] match = { 0, 0 };

                        for (u = -1; u <= 1; u++)
                        {
                            for (v = -1; v <= 1; v++)
                            {
                                if (Math.Abs(u) + Math.Abs(v) > 0)
                                {
                                    float d_i = heightField[i , j] - heightField[i + u , j + v];
                                    if (d_i > d_max)
                                    {
                                        d_max = d_i;
                                        match[0] = u; match[1] = v;
                                    }
                                }
                            }
                        }

                        if (0 < d_max && d_max <= (smoothness / (float)width))
                        {
                            float d_h = 0.5f * d_max;
                            heightField[i , j] -= d_h;
                            heightField[i + match[0] , (j + match[1])] += d_h;
                        }
                    }
                }
            float range = Math.Abs(lowestLevel) + highestLevel;
            if (range < 255)
                range = 255;

            //Glätten
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //float total = 0.0f;
                    //for (u = -1; u <= 1; u++)
                    //{
                    //    for (v = -1; v <= 1; v++)
                    //    {
                    //        total += heightField[i + u * width + j + v];
                    //    }
                    //}

                    heightField[i ,  j] += Math.Abs(lowestLevel);
                    heightField[i ,  j] = heightField[i ,j] / (range);
                }
            }

            //Glätten
            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; ++j)
                {
                    float total = 0.0f;
                    for (u = -1; u <= 1; u++)
                    {
                        for (v = -1; v <= 1; v++)
                        {
                            total += heightField[i + u, j + v];
                        }
                    }

                    heightField[i , j] = total / 9.0f;
                }
            }

            return heightField;
        }

        public Texture2D GetFontTexture(String text, int textureWidth, int textureHeight)
        {
            Texture2D fontTexture = new Texture2D(Device, textureWidth, textureHeight);
            SpriteFont font = ((OptionCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.OptionCondition)).GameFont;

            PresentationParameters pp = Device.PresentationParameters;

            RenderTarget2D rt = new RenderTarget2D(Device, (int)font.MeasureString(text).X, (int)font.MeasureString(text).Y, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            Device.BlendState = BlendState.Opaque;
            Device.SetRenderTarget(rt);
            Device.Clear(Color.Transparent);
            SpriteBatch.Begin();
            SpriteBatch.DrawString(font, text, new Vector2(0, 0), Color.White);
            SpriteBatch.End();

            Device.SetRenderTarget(null);

            return rt;
        }

        public Vector2 GetXYScreenPosition(float xRatio, float yRatio, int depth, float aperture)
        {
            float height = (float)Math.Sin(MathHelper.ToRadians(aperture / 2)) * depth;
            float xPos = height * xRatio;
            float yPos = height * yRatio;
            return new Vector2(xPos, yPos);
        }

        public void GetVeticesAndIndeces(ref Vector3[] veritces, ref int[] indeces, Model model)
        {
            List<VertexHelper.TriangleVertexIndices> iList = new List<VertexHelper.TriangleVertexIndices>();
            List<Vector3> vList = new List<Vector3>();
            VertexHelper.ExtractTrianglesFrom(model, vList, iList, Matrix.Identity);
            veritces = new Vector3[vList.Count];
            int i = 0;
            foreach (Vector3 v in vList)
            {
                veritces[i] = v;
                i++;
            }
            indeces = new int[iList.Count * 3];
            i = 0;
            foreach (VertexHelper.TriangleVertexIndices tvi in iList)
            {
                indeces[i] = tvi.A;
                indeces[i + 1] = tvi.B;
                indeces[i + 2] = tvi.C;
                i += 3;
            }
        }

    }
}
