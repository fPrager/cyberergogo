using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace CyberErgoGo
{
    /// <summary>
    /// This class represents the street or the way of the game.
    /// It is the simple spline curve threw the terrain. It doesn't adjust to the terrain (the other way around)!
    /// </summary>
    class Street:IConditionObserver
    {
        //the points which descripes (completely) the shape of the street
        List<Vector3> SplinePoints;
        List<Vector3> SplinePointsInWorldSpace;

        //the spline points which are (direct) point of the street could be important checkpoints to pass
        //TODO: CheckPoints errechnen/raussuchen!
        List<Vector3> CheckPoints;
        List<int> IndecesOfWallPoints;
        List<int> IndecesOfStonePoints; 
        List<int> IndecesOfBigSpherePoints;
        List<int> IndecesOfNoCheckPoints;
        

        //these points a the left and right boundary of the street
        //they are the vertical projection of the inner points on the side
        List<Vector3> SidePoints;

        //the points a the calculated points of the line-segments and formes the middle line of the street
        List<Vector3> InnerPoints;


        public List<Vector3> WallPoints
        {
            get {
                List<Vector3> points = new List<Vector3>();
                foreach (int index in IndecesOfWallPoints)
                {
                    points.Add(SplinePointsInWorldSpace.ElementAt(index));
                }
                return points;
            }
        }

        public List<Vector3> StonePoints
        {
            get
            {
                List<Vector3> points = new List<Vector3>();
                foreach (int index in IndecesOfStonePoints)
                {
                    points.Add(InnerPoints.ElementAt(index));
                }
                return points;
            }
        }

        public List<Vector3> SpherePoints
        {
            get
            {
                List<Vector3> points = new List<Vector3>();
                foreach (int index in IndecesOfBigSpherePoints)
                {
                    points.Add(SplinePointsInWorldSpace.ElementAt(index));
                }
                return points;
            }
        }

        public List<Vector3> StreetCheckPoints
        {
            get { return CheckPoints; }
        }

        public List<Vector3> StreetMiddlePoints
        {
            get { return InnerPoints; }
        }

        public Vector3 StartPoint
        {
            get { 
                if(InnerPoints.Count > 5)
                    return InnerPoints.ElementAt(2);
                else 
                    return Vector3.Zero;
            
            }
        }

        public Vector3 EndPoint
        {
            get { return CheckPoints.Last(); }
        }

        public int Width
        {
            get { return StreetWidth; }
        }

        List<Vector3> BorderPoints;

        //the vertices of the street bound
        VertexPositionColor[] Vertices;
        //and its indices
        int[] Indices;

        VertexBuffer VertexBufferToRender;
        IndexBuffer IndexBufferToRender;

        VertexBuffer VertexBufferTerrainPoints;
        IndexBuffer IndexBufferTerrainPoints;

        //the width of the street
        //or the double distance of the innerpoints to the side points
        int StreetWidth;

        int BorderWidth;

        //the controlpoints are just ratios of the maximum x-,y- and z-boundaries
        int MaxStreetHeight;    //max y
        int AreaWidth;          //max x
        int AreaHeight;         //max z
        float TerrainZoom;

        //the calculated lenght of the street
        //TODO:Berechnen!
        int StreetLength;

        //the special effect of the street
        //it's important for the "bluring" of the street texture
        Effect StreetEffect;

        //this texture stores an image of the street from above
        //that makes it possible to handle the street as a special part of a heightfield
        Texture2D StreetTexture;

        //this factor can reduce the resolution of the street-texture, the maxima resultion is AreaWidth x AreaHeight
        float TextureSizeFactor = 0.5f;

        //a factor of bluring the street, the higher it is, the more the street is smoother in the terrain (i hope)
        float BluringDegree = 20f;

        float ColorHeightRate;

        Texture2D TerrainHeightMap;

        public Street(List<Vector3> splinePoints, List<int> wallPoints, List<int> stonePoints, List<int> bigSpherePoints, List<int> noCheckPoints, int width, int maxStreetHeight, int areaWidth, int areaHeight, float colorHeightRate, float zoom) 
        {
            SplinePoints = splinePoints;
            
            CheckPoints = new List<Vector3>();
            InnerPoints = new List<Vector3>();
            SidePoints = new List<Vector3>();
            BorderPoints = new List<Vector3>();

            IndecesOfWallPoints = wallPoints;
            IndecesOfStonePoints = stonePoints;
            IndecesOfBigSpherePoints = bigSpherePoints;
            IndecesOfNoCheckPoints = noCheckPoints;

            StreetWidth = (int)(width* zoom);

            BorderWidth = StreetWidth*2;

            ColorHeightRate = colorHeightRate;

            MaxStreetHeight =(int)(maxStreetHeight / colorHeightRate);
            AreaWidth = areaWidth;
            AreaHeight = areaHeight;
            TerrainZoom = zoom;

            Util.GetInstance().LoadFile(ref StreetEffect, "Street", "Effect");
            //SetUpStreet();
        }

        public Street(List<Vector3> splinePoints, List<int> wallPoints, List<int> stonePoints, List<int> bigSpherePoints, List<int> noCheckpoints, int width, int maxStreetHeight, int areaWidth, int areaHeight, float colorHeightRate, float zoom, Texture2D terrainHeightMap)
            : this(splinePoints, wallPoints, stonePoints, bigSpherePoints, noCheckpoints, width, maxStreetHeight, areaWidth, areaHeight, colorHeightRate, zoom)
        {
            TerrainHeightMap = terrainHeightMap;
            SetUpStreet();
        }

        /// <summary>
        /// To change the splinepoints of the street. This forces a new calculation of the street points and changes it's shape 
        /// (which should forces the terrain to update!).
        /// <param name="splinePoints">the new spline points</param>
        /// </summary>
        public void SetSplinePoints(List<Vector3> splinePoints) 
        {
            SplinePoints = splinePoints;
            SetUpStreet();
        }

        /// <summary>
        /// Calculates all important points an vertices of the street and generate the street texture.
        /// </summary>
        private void SetUpStreet() 
        {
            if (AreaWidth != 0 && AreaHeight != 0) 
            {
                SetUpStreetPoints();
                SetUpStreetVertices();
                RenderStreetToTexture();
            }
            else
            {
                //without the dimensions of the terrain, it is useless to calculate the street (maybe at the outside?!)
                Console.WriteLine("The street can't be created because the terrain-values are missing");
            }
        
        }

        /// <summary>
        /// Fill the lists of important points.
        /// </summary>
        private void SetUpStreetPoints() 
        {
            CheckPoints.Clear();
            InnerPoints.Clear();
            SidePoints.Clear();
            BorderPoints.Clear();

                SplinePointsInWorldSpace = new List<Vector3>();
                Color[] terrainColors = new Color[1];
                if (TerrainHeightMap != null)
                {
                    terrainColors = new Color[TerrainHeightMap.Width * TerrainHeightMap.Height];
                    TerrainHeightMap.GetData(terrainColors);
                }
            
                float border = 50f/300f;
                foreach(Vector3 sp in SplinePoints)
                {

                    //Vector3 sp = new Vector3(border + (1 - border * 2) * splinePoint.X, splinePoint.Y + 0.05f, border + (1 - border * 2) * splinePoint.Z);
                    //the splinepoints stores just a value between 0 and 1, so it's just a ratio of the three maxima "dimensions-values"
                    
                    Vector3 spInWorldSpace = Vector3.Zero;
                    if (sp.Y < 0)
                    {
                        if (TerrainHeightMap != null)
                        {
                            int index = (int)(TerrainHeightMap.Width * TerrainHeightMap.Height * Math.Abs(sp.Z) + TerrainHeightMap.Width * Math.Abs(sp.X));
                            float color = terrainColors[index].R;
                            spInWorldSpace = new Vector3(sp.X * (float)AreaWidth, color / ColorHeightRate, sp.Z * (float)AreaHeight);
                        }
                        else
                        {
                            spInWorldSpace = new Vector3(sp.X * (float)AreaWidth, 0, sp.Z * (float)AreaHeight);
                        }

                    }
                    else
                    spInWorldSpace = new Vector3(sp.X * (float)AreaWidth, sp.Y * (float)MaxStreetHeight, sp.Z * (float)AreaHeight);
                    
                    SplinePointsInWorldSpace.Add(spInWorldSpace);
                } 

                BezierPath path = new BezierPath();
                
                //at this point it is important that the SplinePoints are enough points to set up a possible number of spline segments
                //TODO: Fehlerbehandlung, falls zuwenig
                //möglicherwiese letzten punkt kopieren
                path.SetControlPoints(SplinePointsInWorldSpace);
                InnerPoints = path.GetDrawingPoints0();

            //if(InnerPoints.Count > 2)
            //{
            //    Vector3 streetStartPoint = 2 * InnerPoints.ElementAt(0) - InnerPoints.ElementAt(1);
            //    streetStartPoint = 2 * Vector3.Normalize(streetStartPoint);
            //    InnerPoints.Insert(0, streetStartPoint);
            //}

            List<int> notInnerpointAsCheckpoint = new List<int>();
                foreach (int index in IndecesOfNoCheckPoints)
                {
                    Vector3 startPoint = SplinePointsInWorldSpace.ElementAt(index*3);
                    Vector3 endPoint = SplinePointsInWorldSpace.ElementAt((index + 1)*3);
                    bool allIndeces = false;
                    bool startIndeces = false;
                    int i = 0;
                    while (!allIndeces && i<InnerPoints.Count)
                    {
                        if (InnerPoints.ElementAt(i) == startPoint)
                            startIndeces = true;
                        if (InnerPoints.ElementAt(i) == endPoint)
                            allIndeces = true;

                        if (startIndeces)
                        {
                            notInnerpointAsCheckpoint.Add(i);
                        }
                        i++;
                    }
                    
                }

                List<int> newInnerStonePoints = new List<int>();
                foreach (int index in IndecesOfStonePoints)
                {
                    Vector3 startPoint = SplinePointsInWorldSpace.ElementAt(index * 3);
                    Vector3 endPoint = SplinePointsInWorldSpace.ElementAt((index + 1) * 3);
                    bool allIndeces = false;
                    bool startIndeces = false;
                    int i = 0;
                    while (!allIndeces && i < InnerPoints.Count)
                    {
                        if (InnerPoints.ElementAt(i) == startPoint)
                            startIndeces = true;
                        if (InnerPoints.ElementAt(i) == endPoint)
                            allIndeces = true;

                        if (startIndeces)
                        {
                            newInnerStonePoints.Add(i);
                        }
                        i++;
                    }

                }
                IndecesOfStonePoints = newInnerStonePoints;

                Vector3 sideVector = new Vector3();

                foreach (Vector3 ip in InnerPoints)
                {
                    int index = InnerPoints.IndexOf(ip);
                    if (index > 0 && index < InnerPoints.Count - 1)
                    {
                        //the vectors to the previous and next point help to "watch aside"
                        Vector3 toPrevVector = Vector3.Subtract(InnerPoints.ElementAt(index - 1), ip);
                        Vector3 toNextVector = InnerPoints.ElementAt(index + 1) - ip;

                        //small differences to 1 or 0 aim in false and unpossible calvulations, so it's important to round!
                        //the calculation is simplified by the zero y-value, so we force a x-z-parallel street
                        toPrevVector = new Vector3((float)Math.Round(toPrevVector.X, 2), 0, (float)Math.Round(toPrevVector.Z, 2));
                        toNextVector = new Vector3((float)Math.Round(toNextVector.X, 2), 0, (float)Math.Round(toNextVector.Z, 2));

                        Vector3 upVector = new Vector3();
                        Vector3 normPrevVector = Vector3.Normalize(toPrevVector);
                        Vector3 normNextVector = Vector3.Normalize(toNextVector);

                        //rounding, rounding, rounding to garanty the right result
                        normPrevVector = new Vector3((float)Math.Round(normPrevVector.X), (float)Math.Round(normPrevVector.Y), (float)Math.Round(normPrevVector.Z));
                        normNextVector = new Vector3((float)Math.Round(normNextVector.X), (float)Math.Round(normNextVector.Y), (float)Math.Round(normNextVector.Z));

                        //we know were is up
                        upVector = new Vector3(0, 1, 0);

                        Vector3 prevSideVector = sideVector;

                        if (Vector3.Cross(upVector, toPrevVector).Y < 0)
                            upVector = Vector3.Negate(upVector);

                        sideVector = Vector3.Normalize(Vector3.Cross(upVector, toNextVector));
                        float streetfactor = 1;
                        
                        if (notInnerpointAsCheckpoint.Contains(index))
                            streetfactor = 0;

                        Vector3 leftPoint = ((StreetWidth / 2 * streetfactor) * sideVector) + ip;
                        Vector3 leftBorderPoint = (BorderWidth * sideVector * streetfactor) + leftPoint;

                        Vector3 rightPoint = -((StreetWidth / 2 * streetfactor) * sideVector) + ip;
                        Vector3 rightBorderPoint = -(BorderWidth * sideVector * streetfactor) + rightPoint;

                        if (notInnerpointAsCheckpoint.Contains(index))
                        {
                            leftPoint.Y = -1;
                            leftBorderPoint.Y = -1;
                            rightPoint = leftPoint;
                            rightBorderPoint = leftBorderPoint;
                        }

                        if (index == 1)
                        {
                            BorderPoints.Add((BorderWidth*normPrevVector) + leftPoint);
                            BorderPoints.Add((BorderWidth*normPrevVector) + rightPoint);
                        }

                        //look to the left and go a half street width 
                        SidePoints.Add(leftPoint);
                        BorderPoints.Add(leftBorderPoint);

                        //and look to the right and go a half street width
                        SidePoints.Add(rightPoint);
                        BorderPoints.Add(rightBorderPoint);

                        if (index == InnerPoints.Count - 2)
                        {
                            //look to the left and go a half street width 
                            SidePoints.Add((BorderWidth * normNextVector)+leftPoint);
                            BorderPoints.Add((BorderWidth * normNextVector)+leftBorderPoint);

                            //and look to the right and go a half street width
                            SidePoints.Add((BorderWidth * normNextVector)+rightPoint);
                            BorderPoints.Add((BorderWidth * normNextVector)+rightBorderPoint);

                            BorderPoints.Add((BorderWidth * normNextVector) * 2 + leftPoint);
                            BorderPoints.Add((BorderWidth * normNextVector) * 2 + rightPoint);
                        }
                    }
                }

                foreach (Vector3 sp in InnerPoints)
                {
                    int indexOfSplinePoint = InnerPoints.IndexOf(sp);
                    if ((indexOfSplinePoint % 10 == 0 && indexOfSplinePoint > 20) && !notInnerpointAsCheckpoint.Contains(indexOfSplinePoint))
                        CheckPoints.Add(sp);
                }
        }

        /// <summary>
        /// That up the vertices of the street.
        /// </summary>
        private void SetUpStreetVertices() 
        {
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            
            foreach (Vector3 sp in SidePoints)
            {

                if (sp.Y < minHeight)
                    minHeight = sp.Y;
                if (sp.Y > maxHeight)
                    maxHeight = sp.Y;
            }

            float colorHeightRate = (float)ColorHeightRate;

            int numOfVertices = SidePoints.Count + BorderPoints.Count;

            Vertices = new VertexPositionColor[numOfVertices];

            for (int i = 0; i < numOfVertices; i++)
            { 
                Vector3 position;
                float alpha = 0;
                if (i < 2)
                    position = BorderPoints.ElementAt(i);
                else
                    if (i == numOfVertices - 1)
                        position = BorderPoints.ElementAt(BorderPoints.Count-1);
                else
                    if(i == numOfVertices - 2)
                        position = BorderPoints.ElementAt(BorderPoints.Count-2);
                    else
                    {
                        int mod = (i - 2) % 4;
                        if (mod == 0 || mod == 3)
                        {
                            position = BorderPoints.ElementAt(((int)(i - 2) / 4)*2 + mod/3 + 2);
                        }
                        else
                        {
                            position = SidePoints.ElementAt(((int)(i - 2) / 4)*2 + mod - 1);
                            alpha = 1;
                        }
                    }

                Vertices[i].Position = position;
                Vertices[i].Position.Y = 0;
                Vertices[i].Position.Z *= -1;
                float colorValue = (((float)position.Y) * (float)colorHeightRate) / 255f;
                Vertices[i].Color = new Color(colorValue * alpha, alpha, alpha, 1);
            }

            VertexBufferToRender = new VertexBuffer(Util.GetInstance().Device, VertexPositionColor.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
            VertexBufferToRender.SetData(Vertices);

            //Indices = new int[((Vertices.Length - 8) / 4 * 6 * 3) + (2 * 4 * 3)];
            Indices = new int[(((SidePoints.Count/2)-1)*3*6) + 2*(3*4)];

            int counter = 0;

            for (int i = 0; i < (SidePoints.Count/2) - 1; i++)
            {
                if (i == 0)
                {
                    Indices[counter++] = 0;
                    Indices[counter++] = 2;
                    Indices[counter++] = 3;

                    Indices[counter++] = 0;
                    Indices[counter++] = 3;
                    Indices[counter++] = 4;

                    Indices[counter++] = 0;
                    Indices[counter++] = 4;
                    Indices[counter++] = 1;

                    Indices[counter++] = 1;
                    Indices[counter++] = 4;
                    Indices[counter++] = 5;
                }
                
                for (int j = 0; j < 3; j++)
                {
                    int lowerLeft = (i*4 + 2) + j;
                    int lowerRight = (i * 4 + 2) + j + 1;
                    int topLeft = (i * 4 + 2) + j + 4;
                    int topRight = (i * 4 + 2) + j + 4 + 1;

                    Indices[counter++] = topLeft;
                    Indices[counter++] = lowerRight;
                    Indices[counter++] = lowerLeft;

                    Indices[counter++] = topLeft;
                    Indices[counter++] = topRight;
                    Indices[counter++] = lowerRight;
                }

                if (i == (SidePoints.Count / 2) - 2)
                {
                    int startIndex = (i * 4 + 2) + 4;
                    Indices[counter++] = startIndex;
                    Indices[counter++] = startIndex + 4;
                    Indices[counter++] = startIndex + 1;

                    Indices[counter++] = startIndex + 1;
                    Indices[counter++] = startIndex + 4;
                    Indices[counter++] = startIndex + 5;

                    Indices[counter++] = startIndex + 1;
                    Indices[counter++] = startIndex + 5;
                    Indices[counter++] = startIndex + 2;

                    Indices[counter++] = startIndex + 2;
                    Indices[counter++] = startIndex + 5;
                    Indices[counter++] = startIndex + 3;
                }
            }
            


            IndexBufferToRender = new IndexBuffer(Util.GetInstance().Device, typeof(int), Indices.Length, BufferUsage.WriteOnly);
            IndexBufferToRender.SetData(Indices);
        }

        public void LoadTerrainStreetPoints()
        {
            VertexStructures.VertexPositionNormal[] streetOnTerrainVertices = new VertexStructures.VertexPositionNormal[SidePoints.Count];
            List<Vector3> normals = new List<Vector3>();
            foreach (Vector3 sp in SidePoints)
            {
                int index = SidePoints.IndexOf(sp);
                streetOnTerrainVertices[index].Position = sp+new Vector3(0,1,0);
                if (index > 1 && index < SidePoints.Count - 2)
                {
                    streetOnTerrainVertices[index].Normal = Vector3.Cross((sp - SidePoints.ElementAt(index - 2)), (sp - SidePoints.ElementAt(index + 2)));
                    if (streetOnTerrainVertices[index].Normal.Y < 0) streetOnTerrainVertices[index].Normal.Y *= -1;
                }
                else
                    streetOnTerrainVertices[index].Normal = Vector3.Up;
                streetOnTerrainVertices[index].Normal.Normalize();
                normals.Add(streetOnTerrainVertices[index].Normal);
            }

            VertexBufferTerrainPoints = new VertexBuffer(Util.GetInstance().Device, VertexStructures.VertexPositionNormal.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
            VertexBufferTerrainPoints.SetData(streetOnTerrainVertices);

            int[] streetOnTerrainIndices = new int[(SidePoints.Count-2)*6];
            int indexOfSegmentPoint = 0;
            int counter = 0;
            foreach (Vector3 sp in SidePoints)
            {
                if (indexOfSegmentPoint % 2 == 0 && indexOfSegmentPoint < streetOnTerrainVertices.Count() - 2)
                {
                    int lowerLeft = indexOfSegmentPoint;
                    int lowerRight = indexOfSegmentPoint + 1;
                    int topLeft = indexOfSegmentPoint + 2;
                    int topRight = indexOfSegmentPoint + 3;

                    streetOnTerrainIndices[counter++] = topLeft;
                    streetOnTerrainIndices[counter++] = lowerRight;
                    streetOnTerrainIndices[counter++] = lowerLeft;


                    streetOnTerrainIndices[counter++] = topLeft;
                    streetOnTerrainIndices[counter++] = topRight;
                    streetOnTerrainIndices[counter++] = lowerRight;
                }
                indexOfSegmentPoint++;
            }

            IndexBufferTerrainPoints = new IndexBuffer(Util.GetInstance().Device, typeof(int), streetOnTerrainIndices.Length, BufferUsage.WriteOnly);
            IndexBufferTerrainPoints.SetData(streetOnTerrainIndices);
        }

        /// <summary>
        /// To get a nice height-texture we render the street in a picture from above.
        /// </summary>
        private void RenderStreetToTexture()
        {
            GraphicsDevice Device = Util.GetInstance().Device;

            PresentationParameters pp = Device.PresentationParameters;


            int TextureWidth = 4096;
            int TextureHeight = 4096;

            RenderTarget2D rt_1 = new RenderTarget2D(Device, TextureWidth, TextureHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            RenderTarget2D rt_2 = new RenderTarget2D(Device, TextureWidth, TextureHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            RenderTarget2D rt_3 = new RenderTarget2D(Device, TextureWidth, TextureHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            Device.SetRenderTarget(rt_1);

            BlendState blendState = new BlendState();
            //blendState.AlphaSourceBlend = Blend.One;
            //blendState.AlphaDestinationBlend = Blend.One;
            blendState.ColorDestinationBlend = Blend.One;
            blendState.ColorSourceBlend = Blend.One;
            blendState.ColorBlendFunction = BlendFunction.Max;
            Device.BlendState = blendState;

            //draw the street from above
            DrawStreet();
            
            //StreetEffect.CurrentTechnique = StreetEffect.Techniques["Fading"];

            ////fade the border of the street horizontal 
            ////and store the image in  rendertarget 2
            //SetStreetEffectParameters(1.0f / (float)rt_1.Width, 0, true);

            //Util.GetInstance().DrawFullscreenQuad(rt_1, rt_2, StreetEffect);

            //FileStream stream0 = new FileStream("StreetPoly.png", FileMode.Create);
            //rt_1.SaveAsPng(stream0, 500, 500);

            ////fade the border of the street vertical 
            ////and store the image in  rendertarget 3
            //SetStreetEffectParameters(0, 1.0f / (float)rt_1.Height, true);

            //Util.GetInstance().DrawFullscreenQuad(rt_1, rt_3, StreetEffect);

            //Device.SetRenderTarget(rt_1);

            ////combine the texturecolors/heightvalues of rendertarget 2 and 3
            ////just take the highest values
            //StreetEffect.CurrentTechnique = StreetEffect.Techniques["Combine"];
            //Device.Textures[0] = rt_2;
            //Device.Textures[1] = rt_3;

            //Util.GetInstance().DrawFullscreenQuad(rt_2, rt_1, StreetEffect);


            ////blur the faded street to get smoother corners (which appear by fading horizontal and vertical but not sloping)

            //StreetEffect.CurrentTechnique = StreetEffect.Techniques["Bluring"];

            //SetStreetEffectParameters(1.0f / (float)rt_1.Width, 0, true);

            //Util.GetInstance().DrawFullscreenQuad(rt_1, rt_2, StreetEffect);

            //FileStream stream1 = new FileStream("StreetPrev.png", FileMode.Create);
            //rt_1.SaveAsPng(stream1, 500, 500);

            //SetStreetEffectParameters(0, 1.0f / (float)rt_1.Height, true);

            //Util.GetInstance().DrawFullscreenQuad(rt_1, rt_3, StreetEffect);

            //Device.SetRenderTarget(rt_1);

            ////combine the texturecolors/heightvalues of rendertarget 2 and 3
            ////just take the highest values
            //StreetEffect.CurrentTechnique = StreetEffect.Techniques["Combine"];
            //Device.Textures[0] = rt_2;
            //Device.Textures[1] = rt_3;

            //FileStream stream2 = new FileStream("StreetPrev2.png", FileMode.Create);
            //rt_2.SaveAsPng(stream2, 500, 500);

            //FileStream stream3 = new FileStream("StreetPrev3.png", FileMode.Create);
            //rt_3.SaveAsPng(stream3, 500, 500);

            //Util.GetInstance().DrawFullscreenQuad(rt_2, rt_1, StreetEffect);


            Device.SetRenderTarget(null);

            StreetTexture = rt_1;
        }

        /// <summary>
        /// Set the parameters of the street effect, to fade or blur horizontal or vertical.
        /// <param name="dx">fading/bluring in horizontal direction, set zero to fade/blur vertical</param>
        /// <param name="dy">fading/bluring in vertical direction, set zero to fade/blur horizontal</param>
        /// </summary>
        private void SetStreetEffectParameters(float dx, float dy, bool toFade) 
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;
            Viewport viewport = Util.GetInstance().Device.Viewport; ;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
            StreetEffect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);

            weightsParameter = StreetEffect.Parameters["SampleWeights"];
            offsetsParameter = StreetEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = Util.GetInstance().ComputeGaussian(0, BluringDegree);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = Util.GetInstance().ComputeGaussian(i + 1, BluringDegree);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }
            if (toFade)
            {
                float diff = sampleWeights[0] - sampleWeights[sampleWeights.Length - 1];
                //Normalize the list of sample weightings, so they will always sum to one.
                for (int i = (sampleWeights.Length - 1); i >= 0; i--)
                {
                    sampleWeights[i] = 1 - ((sampleWeights[0] - sampleWeights[i]) / diff);
                }
            }
            else
            {
                // Normalize the list of sample weightings, so they will always sum to one.
                for (int i = 0; i < sampleWeights.Length; i++)
                {
                    sampleWeights[i] /= totalWeights;
                }
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        /// <summary>
        ///Draw the street from above.
        /// </summary>
        private void DrawStreet() 
        {
            GraphicsDevice Device = Util.GetInstance().Device;
            Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(0, 0, 1), 1.0f, 0);

            int TextureWidth = (int)(AreaWidth * TextureSizeFactor);
            int TextureHeight = (int)(AreaHeight * TextureSizeFactor);

            

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            Device.RasterizerState = rs;

            Matrix worldMatrix = Matrix.Identity;
            StreetEffect.CurrentTechnique = StreetEffect.Techniques["StreetFromAbove"];

            Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(AreaWidth / 2, 1000, -AreaHeight / 2), new Vector3(AreaWidth / 2, 0, -AreaHeight / 2), new Vector3(0, 0, -1));
            Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Device.Viewport.AspectRatio, 5.0f, 1000.0f);          
      
            StreetEffect.Parameters["xView"].SetValue(viewMatrix);
            StreetEffect.Parameters["xProjection"].SetValue(projectionMatrix);
            StreetEffect.Parameters["xWorld"].SetValue(worldMatrix);

            StreetEffect.Parameters["xAreaWidth"].SetValue(AreaWidth);
            StreetEffect.Parameters["xAreaHeight"].SetValue(AreaHeight);


            foreach (EffectPass pass in StreetEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Device.Indices = IndexBufferToRender;
                Device.SetVertexBuffer(VertexBufferToRender);
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Vertices.Length, 0, Indices.Length / 3);
            }
        }

        ///// <summary>
        ///// Listen to changes in the terrain, this forces a new calculation of the street
        ///// <param name="condition">the (external) changed condition</param>
        ///// </summary>
        //public void ConditionChanged(Condition condition)
        //{
        //    if (condition.GetID() == ConditionID.TerrainCondition) 
        //    {
        //        TerrainCondition tc = (TerrainCondition)condition;
        //        if (AreaWidth != tc.TerrainWidth * tc.Zooming || AreaHeight != tc.TerrainHeight * tc.Zooming)
        //        {
        //            AreaWidth = (int)(tc.TerrainWidth * tc.Zooming);
        //            AreaHeight = (int)(tc.TerrainHeight * tc.Zooming);
        //            SetUpStreet();
        //        }
        //    }
        //}

        /// <summary>
        /// Listen to changes in the terrain, this forces a new calculation of the street
        /// <param name="condition">the (external) changed condition</param>
        /// </summary>
        public void ConditionChanged(Condition condition, List<ParameterIdentifier> changedParameters)
        {
            if (condition.GetID() == ConditionID.TerrainCondition)
            {
                bool forcesNewStreet = false;
                TerrainCondition tc = (TerrainCondition)condition;
                foreach (ParameterIdentifier p in changedParameters)
                {
                    
                    switch (p)
                    { 
                        case ParameterIdentifier.Width:
                            AreaWidth = tc.AreaWidth;
                            forcesNewStreet = true;
                            break;

                        case ParameterIdentifier.Height:
                            AreaHeight = tc.AreaHeight;
                            forcesNewStreet = true;
                            break;
                        default:
                            break;
                    }
                }
                if (forcesNewStreet) 
                    SetUpStreet();
            }
        }

        /// <summary>
        /// Returns the generated heightfield of the street.
        /// <returns>(height-)texture of the street</returns>
        /// </summary>
        public Texture2D GetStreetTexture()
        {
            return StreetTexture;
        }

        public VertexBuffer GetVertexBuffer()
        {
            return VertexBufferTerrainPoints;
        }

        public IndexBuffer GetIndexBuffer()
        {
            return IndexBufferTerrainPoints;
        }

        public Quaternion ToTheNextOrientation(Vector3 pointPosition)
        { 
          Quaternion orientation = Quaternion.Identity;
          foreach (Vector3 position in StreetMiddlePoints)
          {
              if (position == pointPosition)
              {
                  Vector3 nextPoint = Vector3.Zero;
                  if (StreetMiddlePoints.Last() != position)
                      nextPoint = StreetMiddlePoints.ElementAt(StreetMiddlePoints.IndexOf(position) + 1);
                  else
                      nextPoint = (position-StreetMiddlePoints.ElementAt(StreetMiddlePoints.Count - 2)) + position;
                  {
                      if (position != nextPoint)
                          orientation = Util.GetInstance().ToNextPointRotation(position, nextPoint);
                  }
              }
          }
          return orientation;
        }

        public bool IsThisPositionInRegion(Vector3 position)
        {
            float minDistance = 2*StreetWidth;
            position.Y = 0;
            foreach (Vector3 streetPoint in InnerPoints)
            {
                float length = (position - new Vector3(streetPoint.X, 0, streetPoint.Z)).Length();
                if (length < minDistance)
                    minDistance = length;
            }
            if (minDistance > StreetWidth)
                return false;
            else
                return true;
        }

    }
}
