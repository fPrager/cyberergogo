using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BikeControls;
using Microsoft.Xna.Framework.Input;

namespace CyberErgoGo
{
    /// <summary>
    /// It represents the playing of a level.
    /// </summary>
    class LevelScreen:GameScreen, IConditionObserver
    {
        //behind the represention stands a logic
        LevelLogic Logic;

        //the specific effect
        //which plays an important part of the visual impression of a game
        Effect LevelEffect;

        //the important datas of the terrain
        TerrainDrawingData TerrainData;
        GraphicsDevice Device;

        MenuCanvas AbortBackground;
        Vector2 PauseTitlePosition;
        Vector2 EndFontPosition;
        Vector2 RestartFontPosition;
        SpriteFont Font;
        SpriteBatch ScreenText;
        bool Pause = false;
        bool WarningSignal = false;

        bool LevelFinished = false;
        int LevelFinishedTimeZPosition = (int)(-60 * OverallSetting.SizeFactor);
        int LevelFinishedTimeYPosition = 0;
        int LevelFinishedTimeXPosition = 0;

        public bool Selected = false;
        float CheckPointRotation = 0;
        float CheckPointRotationSpeed = 0.001f;

        int PauseScreenZOffset = (int)(-50* OverallSetting.SizeFactor);
        int PauseScreenZOffsetSelected = (int)(-45 * OverallSetting.SizeFactor);

        float StartTimeInMilli = 0;
        int StartingTimeInSec = 2;
        bool ReadyToStart = false;

        float PauseTrickerTimeInMilli = 0;
        float MinPauseTrickerInMilli = 1500;
        bool PauseTrickered = false;

        const float TimePosX = 0;
        const float TimePosY = 0;
        const float TimeDepth = (int)(-80* OverallSetting.SizeFactor);

        const float TimePlateSize = 1;
        const float TimePosXFinish = 0;
        const float TimePosYFinish = 0;
        const float TimeDepthFinish = (int)(-40* OverallSetting.SizeFactor);

        const float AbortX = 0;
        const float AbortY = 0;
        const float AbortWidth = 0;
        const float AbortHeight = 0;
        const float AbortDepth = 0;
        const float AbortDepthSelected = 0.9f;

        private bool ChangePhysic = false;

        Waterplane Water;
        Skydome Sky;

        GameTime CurrentGameTime;

        //for testing
        Model Xwing;

        Matrix WorldMatrix;

        /// <summary>
        /// This simple structure stores the few important values of the terrain.
        /// It's just loaded at the beginning and at changes of the terrain.
        /// </summary>
        struct TerrainDrawingData 
        {
            public TerrainDrawingData(VertexBuffer vb, IndexBuffer ib, VertexBuffer svb, IndexBuffer sib,int numV, int numI)
            {
                VB = vb;
                IB = ib;
                StreetVB = svb;
                StreetIB = sib;
                NumV = numV;
                NumI = numI;
            }

            //every form the terrain should offer a vertex buffer to garanty a quick drawing
            public VertexBuffer VB;
            //and should be indexed
            public IndexBuffer IB;
            public VertexBuffer StreetVB;
            public IndexBuffer StreetIB;
            //number of veritces
            public int NumV;
            //number of indices
            public int NumI;
        }

        MenuCanvas TimePlate;
        MenuCanvas WarningCanvas;
        MenuCanvas FogInTheMiddle;

        MenuCanvas RestartCanvas;
        MenuCanvas AbortCanvas;

        TimeDisplay Time;

        Dictionary<GameObjectShape, Model> ShapeToModel;

        public LevelScreen(String name, LevelLogic logic) : base(name) 
        {
            Logic = logic;
            ConditionHandler.GetInstance().RegisterMe(ConditionID.TerrainCondition, this);
            ConditionHandler.GetInstance().RegisterMe(ConditionID.ViewCondition, this);
            Device = Util.GetInstance().Device;
            ScreenText = new SpriteBatch(Device);

            ShapeToModel = new Dictionary<GameObjectShape, Model>();
            ShapeToModel.Add(GameObjectShape.Cube, null);
            ShapeToModel.Add(GameObjectShape.Sphere, null);
            ShapeToModel.Add(GameObjectShape.Pin, null);
        }

        private void LoadPauseScreen(float x, float y, float z, float size, float opacity)
        { 
            Texture2D background1 = null;            
            Util.GetInstance().LoadFile(ref background1, "Screen", "Restart");
            Texture2D[] backgroundList1 = new Texture2D[1] { background1 };
            Vector2 pos = Util.GetInstance().GetXYScreenPosition(-0.6f, -0.5f, -PauseScreenZOffset, 25);

            RestartCanvas = new MenuCanvas(pos, PauseScreenZOffset, size, size * background1.Height / background1.Width, backgroundList1, opacity);

            Texture2D background2 = null;
            Util.GetInstance().LoadFile(ref background2, "Screen", "Back");
            Texture2D[] backgroundList2 = new Texture2D[1] { background2 };
            AbortCanvas = new MenuCanvas(new Vector2(-pos.X, pos.Y), PauseScreenZOffset, size, size * background2.Height / background2.Width, backgroundList2, opacity);
        }

        private void LoadGameObjetModels(Effect effect)
        {
            int i = 0;
            while(i<ShapeToModel.Keys.Count)
            {
                GameObjectShape shape = ShapeToModel.Keys.ElementAt(i);
                Model modelOfshape = null;
                try
                {
                    Util.GetInstance().LoadFile(ref modelOfshape, "Models", shape.ToString());
                }
                catch
                {
                    Console.WriteLine("There exists no modelfile in folder 'Models' with the name " + shape.ToString() + "!");
                }
                if (modelOfshape != null)
                {
                    Util.GetInstance().SetEffect(ref modelOfshape, effect);
                    ShapeToModel[shape] = modelOfshape;
                }
                i++;
            }
        }

        public override void UpdateScreenPoints(ViewCondition oc)
        {
            LoadTimeCanvas(oc);
        }

        private void LoadTimeCanvas(ViewCondition vc)
        {
            TimePlate = new MenuCanvas(Vector3.Zero, TimePlateSize * 0.5f, TimePlateSize, new Texture2D[1], 1);

            int CurrentMin = 0;
            int CurrentSec = 0;
            int CurrentMSec = 0;

            if (Time != null)
            {
                CurrentMin = Time.CurrentMin;
                CurrentSec = Time.CurrentSec;
                CurrentMSec = Time.CurrentMSec;
            }
            int timeDepth = (int)(vc.FocusPlane);
            Vector2 timeXYPos = Util.GetInstance().GetXYScreenPosition(0.8f, 0.9f, timeDepth, vc.Aperture);
            Vector2 timeFinishXYPos = Util.GetInstance().GetXYScreenPosition(-0.2f, 0.4f, timeDepth, vc.Aperture);

            Time = new TimeDisplay((int)(TimePlateSize), (int)TimePlateSize, timeXYPos,- timeDepth, 0, timeFinishXYPos, -(int)(timeDepth*0.9f));
            Time.SetTime(CurrentMSec, CurrentSec, CurrentMin);
        }

        private void LoadWarningCanvas()
        {
            Texture2D background = null;
            Util.GetInstance().LoadFile(ref background, "Screen", "Warning");
            Texture2D[] backgroundList = new Texture2D[1] { background };
            WarningCanvas = new MenuCanvas(new Vector2(0, 0), -10, 35, 15, backgroundList, 1);
        }

        private void LoadFogInTheMiddle()
        {
            Texture2D background = null;
            Util.GetInstance().LoadFile(ref background, "Screen", "Fog");
            Texture2D[] backgroundList = new Texture2D[1] { background };
            FogInTheMiddle = new MenuCanvas(new Vector2(0, -1.5f) * OverallSetting.SizeFactor, -50 * OverallSetting.SizeFactor, 30 * OverallSetting.SizeFactor, 30 * OverallSetting.SizeFactor, backgroundList, 1);
            FogInTheMiddle.Opacity = 0;
        }

        /// <summary>
        /// To load a level.
        /// </summary>
        public override void LoadContent()
        {
            Util.GetInstance().LoadFile(ref LevelEffect, "Level", "Effect");
            OptionCondition oc = (OptionCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.OptionCondition);
            
           // UpdateScreenPoints(oc);
            Font = oc.GameFont;

            LoadTimeCanvas((ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition));
            LoadWarningCanvas();
            LoadFogInTheMiddle();
            LoadPauseScreen(0, 5, PauseScreenZOffset, 5, 0);

            Util.GetInstance().CollisionsChecker.Initialize();
            Logic.Load(LevelEffect);

            LoadSky();
            LoadWater();
            LoadGameFigure();

            UpdateTerrainData();
            SetEffectParameters();   
            LoadGameObjetModels(LevelEffect);
            base.LoadContent();
            Util.GetInstance().SoundManager.PlayGameMusic();
        }

        private void LoadGameFigure()
        {
            Logic.GetFigure().LoadContent(LevelEffect);
        }

        private void LoadSky()
        {
            Sky = new Skydome();
            Sky.LoadContent(LevelEffect);
            LevelEffect.Parameters["xSkyboxTexture"].SetValue(Sky.GetCloudTexture());
        }

        private void LoadWater()
        {
            Water = new Waterplane(-2000, 2000, -2000, 2000, 0.5f);
            LevelEffect.Parameters["xTideHeight"].SetValue(Water.GetMaxTideHeight());
        }

        private Model LoadModel(string assetName)
        {

            Model newModel = null;
            Util.GetInstance().LoadFile(ref newModel, "TestData", assetName); 
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = LevelEffect.Clone();
            return newModel;
        }

        /// <summary>
        /// The constant parameters should be set at the beginning or at few changes to reduce the useless access rate.
        /// </summary>
        private void SetEffectParameters() 
    {
        Vector3 lightDirection = new Vector3(0, -1, 0.5f);
            lightDirection.Normalize();

            LevelEffect.Parameters["xLightDirection"].SetValue(lightDirection);

            LevelEffect.Parameters["xAmbient"].SetValue(0.3f);

            LevelEffect.Parameters["xCPSpriteSize"].SetValue(10);
            Texture2D cpSpriteTexture = null;
            Util.GetInstance().LoadFile(ref cpSpriteTexture, "Screen", "Point");
            LevelEffect.Parameters["xCPSpriteTexture"].SetValue(cpSpriteTexture);

            LevelEffect.Parameters["xHeightMapWithStreet"].SetValue(Logic.GetPlayingTerrain().GetHeightMapWithStreet().GetMap());

            
            Texture2D simplePoint = null;
            Util.GetInstance().LoadFile(ref simplePoint, "Screen", "Point");
            LevelEffect.Parameters["xSimplePoint"].SetValue(simplePoint);

            //Texture2D Zero = null;
            //Util.GetInstance().LoadFile(ref Zero, "Screen", "Zero");
            LevelEffect.Parameters["xZero"].SetValue(Util.GetInstance().GetFontTexture("0", 100, 100));

            //Texture2D one = null;
            //Util.GetInstance().LoadFile(ref one, "Screen", "One");
            LevelEffect.Parameters["xOne"].SetValue(Util.GetInstance().GetFontTexture("1", 100, 100));

            //Texture2D two = null;
            //Util.GetInstance().LoadFile(ref two, "Screen", "Two");
            LevelEffect.Parameters["xTwo"].SetValue(Util.GetInstance().GetFontTexture("2", 100, 100));

            //Texture2D Three = null;
            //Util.GetInstance().LoadFile(ref Three, "Screen", "Three");
            LevelEffect.Parameters["xThree"].SetValue(Util.GetInstance().GetFontTexture("3", 100, 100));

            //Texture2D Four = null;
            //Util.GetInstance().LoadFile(ref Four, "Screen", "Four");
            LevelEffect.Parameters["xFour"].SetValue(Util.GetInstance().GetFontTexture("4", 100, 100));

            //Texture2D Five = null;
            //Util.GetInstance().LoadFile(ref Five, "Screen", "Five");
            LevelEffect.Parameters["xFive"].SetValue(Util.GetInstance().GetFontTexture("5", 100, 100));

            //Texture2D Six = null;
            //Util.GetInstance().LoadFile(ref Six, "Screen", "Six");
            LevelEffect.Parameters["xSix"].SetValue(Util.GetInstance().GetFontTexture("6", 100, 100));

            //Texture2D Seven = null;
            //Util.GetInstance().LoadFile(ref Seven, "Screen", "Seven");
            LevelEffect.Parameters["xSeven"].SetValue(Util.GetInstance().GetFontTexture("7", 100, 100));

            //Texture2D Eight = null;
            //Util.GetInstance().LoadFile(ref Eight, "Screen", "Eight");
            LevelEffect.Parameters["xEight"].SetValue(Util.GetInstance().GetFontTexture("8", 100, 100));

            //Texture2D Nine = null;
            //Util.GetInstance().LoadFile(ref Nine, "Screen", "Nine");
            LevelEffect.Parameters["xNine"].SetValue(Util.GetInstance().GetFontTexture("9", 100, 100));

            TerrainCondition terrainCondition = (TerrainCondition)(ConditionHandler.GetInstance().GetCondition(ConditionID.TerrainCondition));
            LevelEffect.Parameters["xLevel0Texture"].SetValue(terrainCondition.Level0Texture);
            LevelEffect.Parameters["xLevel0to1"].SetValue(terrainCondition.Level0to1);
            LevelEffect.Parameters["xLevel1Texture"].SetValue(terrainCondition.Level1Texture);
            LevelEffect.Parameters["xLevel1to2"].SetValue(terrainCondition.Level1to2);
            LevelEffect.Parameters["xLevel2Texture"].SetValue(terrainCondition.Level2Texture);
            LevelEffect.Parameters["xLevel2to3"].SetValue(terrainCondition.Level2to3);
            LevelEffect.Parameters["xLevel3Texture"].SetValue(terrainCondition.Level3Texture);
            LevelEffect.Parameters["xStreetTexture"].SetValue(terrainCondition.StreetTexture);

            //MovingObject-Specific-Paramateres via
            //LevelEffect.Paramaters...
            Logic.GetMovingObject().UpdateMeshEffect(LevelEffect);
            //foreach (GameObjectShape shape in ShapeToModel.Keys)
            //{
            //    Model modelShape = ShapeToModel[shape];

            //    Util.GetInstance().SetEffect(ref modelShape, LevelEffect);
            //}
        }

        /// <summary>
        /// That the paramers depending to the terrain shading
        /// </summary>
        private void SetTerrainShadingParameters() 
        { 
            
        }

        #region Update

        /// <summary>
        /// Update the terraindata, if the terrain is changing.
        /// </summary>
        private void UpdateTerrainData()
        {
            Terrain terrain = Logic.GetPlayingTerrain();
            TerrainData = new TerrainDrawingData(terrain.GetVertexBuffer(), terrain.GetIndexBuffer(), terrain.GetStreetVertexBuffer(),terrain.GetStreetIndexBuffer(), terrain.GetNumberOfVertices(), terrain.GetNumberOfIndices());
            SetTerrainShadingParameters();
        }

        public void SelectPhysicWithBike()
        {
            BikeSelection currentSelection = BikeNavigation.GetLeftCenterOrRight();

            if (currentSelection == BikeSelection.Right && CurrentSelection != Selection.LeftObject)
            {
                CurrentSelection = Selection.LeftObject;
                Logic.ChangePhysicTo(Selection.LeftObject);
                Console.WriteLine("to leftobject");
            }
            else
                if (currentSelection == BikeSelection.Center && CurrentSelection != Selection.CenterObject)
                {
                    CurrentSelection = Selection.CenterObject;
                    Logic.ChangePhysicTo(Selection.CenterObject);
                    Console.WriteLine("to centerobject");
                }
                else
                    if (currentSelection == BikeSelection.Left && CurrentSelection != Selection.RightObject)
                    {
                        CurrentSelection = Selection.RightObject;
                        Logic.ChangePhysicTo(Selection.RightObject);
                        Console.WriteLine("to rightobject");
                    }
        }

        public void SelectPhysicWithKeyboard()
        {
            if (Keyboard.GetState().GetPressedKeys().Contains(Keys.NumPad1) && CurrentSelection != Selection.LeftObject)
            {
                CurrentSelection = Selection.LeftObject;
                Logic.ChangePhysicTo(Selection.LeftObject);
                Console.WriteLine("to leftobject");
            }
            else
                if (Keyboard.GetState().GetPressedKeys().Contains(Keys.NumPad2) && CurrentSelection != Selection.CenterObject)
                {
                    CurrentSelection = Selection.CenterObject;
                    Logic.ChangePhysicTo(Selection.CenterObject);
                    Console.WriteLine("to centerobject");
                }
                else
                    if (Keyboard.GetState().GetPressedKeys().Contains(Keys.NumPad3) && CurrentSelection != Selection.RightObject)
                    {
                        CurrentSelection = Selection.RightObject;
                        Logic.ChangePhysicTo(Selection.RightObject);
                        Console.WriteLine("to rightobject");
                    }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (StartTimeInMilli / 1000 > StartingTimeInSec)
            {
                StartTimeInMilli += gameTime.ElapsedGameTime.Milliseconds;
                ReadyToStart = true;
            }
            else
                StartTimeInMilli += gameTime.ElapsedGameTime.Milliseconds;

            if (LevelFinished && !Time.OnFinishedPosition)
            {
                float finishAnimationStep = gameTime.ElapsedGameTime.Milliseconds / 10f;
                Time.JumpToFinishedPosition(finishAnimationStep);
            }

            if (!Pause && !LevelFinished)
            {
                if ((Bike.PluggedIn && !Bike.GetState().IsSitting && ReadyToStart && !ChangePhysic && Logic.WaitingIsOver) || Keyboard.GetState().IsKeyDown(Keys.P))
                {
                    PauseTrickerTimeInMilli += gameTime.ElapsedGameTime.Milliseconds;
                    if (PauseTrickerTimeInMilli > MinPauseTrickerInMilli)
                    {
                        Pause = true;
                        Logic.GetFigure().TurnToFront();
                    }
                }
                else
                {
                    if (Keyboard.GetState().GetPressedKeys().Contains(Keys.NumPad5) || Bike.GetState().Fire.IsFiring && ReadyToStart)
                    {
                        if (!ChangePhysic && !LevelFinished)
                        {
                            ChangePhysic = true;
                            Logic.StartPhysicChanging();
                        }
                    }
                    else
                    {
                        if (ChangePhysic)
                        {
                            ChangePhysic = false;
                            Logic.StopPhysicChanging();
                        }
                    }

                            if (ChangePhysic)
                            {
                                if (Bike.PluggedIn)
                                    SelectPhysicWithBike();
                                else
                                    SelectPhysicWithKeyboard();
                                Logic.Update(gameTime, Time.CurrentMSec, Time.CurrentSec, Time.CurrentMin, false);
                            }
                            else
                                Logic.Update(gameTime, Time.CurrentMSec, Time.CurrentSec, Time.CurrentMin, true);
                }
            }
            else
            {
                

                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    CurrentSelection = Selection.Restart;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    CurrentSelection = Selection.End;

                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                    Selected = true;

                if(CurrentSelection == Selection.End && Selected)
                    Logic.ChangePhysicTo(Selection.CenterObject);
                if(ReadyToStart && Bike.PluggedIn)
                switch (BikeNavigation.GetCurrentSelection())
                {
                    case BikeSelection.Right:
                        CurrentSelection = Selection.End;
                        break;
                    case BikeSelection.Left:
                        CurrentSelection = Selection.Restart;
                        break;
                    case BikeSelection.Click:
                        Selected = true;
                        break;
                    default:
                        break;
                }

                if (Pause || LevelFinished)
                {
                    float animationStep = gameTime.ElapsedGameTime.Milliseconds / 10f;

                    switch (CurrentSelection)
                    { 
                        case Selection.Restart:
                         if (RestartCanvas.PositionZ < PauseScreenZOffsetSelected)
                                RestartCanvas.ChangeZOffset(RestartCanvas.PositionZ + animationStep);
                         if (AbortCanvas.PositionZ > PauseScreenZOffset)
                             AbortCanvas.ChangeZOffset(AbortCanvas.PositionZ - animationStep);
                            break;
                       case Selection.End:
                            if (AbortCanvas.PositionZ < PauseScreenZOffsetSelected)
                                AbortCanvas.ChangeZOffset(AbortCanvas.PositionZ + animationStep);
                            if (RestartCanvas.PositionZ > PauseScreenZOffset)
                             RestartCanvas.ChangeZOffset(RestartCanvas.PositionZ - animationStep);
                         break;
                        default:
                            break;
                    }
}

                if (Bike.GetState().IsSitting || Keyboard.GetState().IsKeyDown(Keys.E))
                {
                    if (!LevelFinished)
                    {
                        Logic.GetFigure().TurnToBack();
                        Pause = false;
                        PauseTrickerTimeInMilli = 0;
                        Selected = false;
                    }
                }

            
            if (Selected && CurrentSelection == Selection.Restart)
            {
                Selected = false;
                ReadyToStart = false;
                Pause = false;
                PauseTrickerTimeInMilli = 0;
                LoadTimeCanvas((ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition));
                StartTimeInMilli = 0;
                Logic.Restart();
                LevelFinished = false;
            }

            if (Selected && CurrentSelection == Selection.End)
            {
                ReadyToStart = false;
                Pause = false;
                PauseTrickerTimeInMilli = 0;
                LoadTimeCanvas((ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition));
                StartTimeInMilli = 0;
                LevelFinished = false;

                Logic.Unload();
            }


        }
            float step = gameTime.ElapsedGameTime.Milliseconds / 1000f;

            WarningSignal = !Logic.OnTheStreet;
            if (WarningSignal && WarningCanvas.Opacity<0.5f)
            {
                WarningCanvas.Opacity += step*2;
            }
            if (!WarningSignal && WarningCanvas.Opacity >0)
            {
                WarningCanvas.Opacity -= step*2;
            }
            if ((Pause || LevelFinished) && FogInTheMiddle.Opacity < 1f)
            {
                FogInTheMiddle.Opacity += step;
            }
            if (!(Pause || LevelFinished) && FogInTheMiddle.Opacity > 0f)
            {
                FogInTheMiddle.Opacity -= step;
            }
            if (RestartCanvas.Opacity > 0 && !Pause && !LevelFinished)
            {
                RestartCanvas.Opacity -= step;
            }

            if (AbortCanvas.Opacity > 0 && !Pause && !LevelFinished)
            {
                AbortCanvas.Opacity -= step;
            }

            if (!Pause && !LevelFinished && ReadyToStart)
            {
                Time.AddElapsedTime(gameTime.ElapsedGameTime.Milliseconds);
            }

            if (!Pause && ReadyToStart)
            {
                foreach (BillboardCanon canon in Logic.Canons)
                    canon.Update(gameTime.ElapsedGameTime.Milliseconds);
            }

            if (Logic.LevelFinished)
            {
                LevelFinished = true;
            }

            Sky.Update(gameTime.ElapsedGameTime.Milliseconds);
            Water.Update(gameTime.ElapsedGameTime.Milliseconds);
            Logic.GetFigure().Update(gameTime.ElapsedGameTime.Milliseconds);

            if (AbortCanvas.Opacity < 1 && (Pause || LevelFinished))
            {
                AbortCanvas.Opacity += step;
            }

            if (RestartCanvas.Opacity < 1 && (Pause || LevelFinished))
            {
                RestartCanvas.Opacity += step;
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw the street (on the ground).
        /// </summary>
        private void DrawStreet()
        {
            LevelEffect.CurrentTechnique = LevelEffect.Techniques["StreetShading"];

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;

            Device.BlendState = BlendState.Opaque;

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.Indices = TerrainData.StreetIB;
                Device.SetVertexBuffer(TerrainData.StreetVB);
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TerrainData.StreetVB.VertexCount, 0, TerrainData.StreetIB.IndexCount / 3);
            }
        }

        /// <summary>
        /// Draw the ground.
        /// </summary>
        private void DrawTerrain(Matrix view, Matrix projection)
        {
            LevelEffect.CurrentTechnique = LevelEffect.Techniques["StaticTerrainShading"];

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;

            Device.BlendState = BlendState.Opaque;

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.Indices = TerrainData.IB;
                Device.SetVertexBuffer(TerrainData.VB);
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TerrainData.NumV, 0, TerrainData.NumI / 3);
            }
        }

         /// <summary>
        /// Draw the MenuCanvas
        /// </summary>
        private void DrawCanvas(MenuCanvas canvas)
        {
            LevelEffect.CurrentTechnique = LevelEffect.Techniques["MenuCanvasShading"];

            Device.BlendState = new BlendState()
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.One,
                BlendFactor = new Color(1.0F, 1.0F, 1.0F, 1.0F),
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorSourceBlend = Blend.One,
                ColorWriteChannels = ColorWriteChannels.All,
                ColorWriteChannels1 = ColorWriteChannels.All,
                ColorWriteChannels2 = ColorWriteChannels.All,
                ColorWriteChannels3 = ColorWriteChannels.All,
                // MultiSampleMask = -1
            };
            LevelEffect.Parameters["xCanvasPosition"].SetValue(canvas.Position);
            LevelEffect.Parameters["xCanvasOpacity"].SetValue(canvas.Opacity);
            LevelEffect.Parameters["xCanvasTexture"].SetValue(canvas.CurrentTexture);
            LevelEffect.Parameters["xCanvasIsMask"].SetValue(false);
            LevelEffect.Parameters["xCanvasColor"].SetValue(new Vector3(1,0,0));

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.Indices = canvas.GetIBuffer();
                Device.SetVertexBuffer(canvas.GetVBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            Device.BlendState = BlendState.Opaque;
        }


        /// <summary>
        /// Draw the MenuCanvas
        /// </summary>
        private void DrawTime(Matrix view, Matrix projection, Vector3 lookAt)
        {
            LevelEffect.CurrentTechnique = LevelEffect.Techniques["TimeShading"];

            Device.BlendState = new BlendState()
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.One,
                BlendFactor = new Color(1.0F, 1.0F, 1.0F, 1.0F),
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorSourceBlend = Blend.One,
                ColorWriteChannels = ColorWriteChannels.All,
                ColorWriteChannels1 = ColorWriteChannels.All,
                ColorWriteChannels2 = ColorWriteChannels.All,
                ColorWriteChannels3 = ColorWriteChannels.All,
                // MultiSampleMask = -1
            };

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;


            LevelEffect.Parameters["xInWorldSpace"].SetValue(false);

            LevelEffect.Parameters["xTimePosition"].SetValue(new Vector3(Time.Min10Pos, Time.ZDepth));
            LevelEffect.Parameters["xNumber"].SetValue(Time.Min10);
            LevelEffect.Parameters["xTimePlate"].SetValue(0);

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.Indices = TimePlate.GetIBuffer();
                Device.SetVertexBuffer(TimePlate.GetVBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            LevelEffect.Parameters["xTimePosition"].SetValue(new Vector3(Time.Min1Pos, Time.ZDepth));
            LevelEffect.Parameters["xNumber"].SetValue(Time.Min1);
            LevelEffect.Parameters["xTimePlate"].SetValue(1);

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.Indices = TimePlate.GetIBuffer();
                Device.SetVertexBuffer(TimePlate.GetVBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            LevelEffect.Parameters["xTimePosition"].SetValue(new Vector3(Time.Sec10Pos, Time.ZDepth));
            LevelEffect.Parameters["xNumber"].SetValue(Time.Sec10);
            LevelEffect.Parameters["xTimePlate"].SetValue(2);

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.Indices = TimePlate.GetIBuffer();
                Device.SetVertexBuffer(TimePlate.GetVBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            LevelEffect.Parameters["xTimePosition"].SetValue(new Vector3(Time.Sec1Pos, Time.ZDepth));
            LevelEffect.Parameters["xNumber"].SetValue(Time.Sec1);
            LevelEffect.Parameters["xTimePlate"].SetValue(3);

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.Indices = TimePlate.GetIBuffer();
                Device.SetVertexBuffer(TimePlate.GetVBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            LevelEffect.Parameters["xTimePosition"].SetValue(new Vector3(Time.MSec10Pos, Time.ZDepth));
            LevelEffect.Parameters["xNumber"].SetValue(Time.MSec10);
            LevelEffect.Parameters["xTimePlate"].SetValue(4);

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.Indices = TimePlate.GetIBuffer();
                Device.SetVertexBuffer(TimePlate.GetVBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }
            //LevelEffect.Parameters["xTimePosition"].SetValue(new Vector3(Time.MSec1Pos, Time.ZDepth));
            //LevelEffect.Parameters["xNumber"].SetValue(Time.MSec1);
            //LevelEffect.Parameters["xTimePlate"].SetValue(5);

            //foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();
            //    Device.Indices = TimePlate.GetIBuffer();
            //    Device.SetVertexBuffer(TimePlate.GetVBuffer());
            //    Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            //}

            Device.BlendState = BlendState.Opaque;
        }

        private void DrawBillboard(Billboard billboard)
        {
            LevelEffect.CurrentTechnique = LevelEffect.Techniques["Billboarding"];
            LevelEffect.Parameters["xBillboardWidth"].SetValue(billboard.GetWidth());
            LevelEffect.Parameters["xBillboardHeight"].SetValue(billboard.GetHeight());
            LevelEffect.Parameters["xBillboardTexture"].SetValue(billboard.GetTexture());
            LevelEffect.Parameters["xAllowedRotDir"].SetValue(Vector3.Up);
            LevelEffect.Parameters["xToFlip"].SetValue(billboard.GetFlip());
            LevelEffect.Parameters["xTranslation"].SetValue(billboard.GetTranslation());
            LevelEffect.Parameters["xToDraw"].SetValue(billboard.ToDraw);

            if (billboard.GetIsAnimated())
            {
                LevelEffect.Parameters["xIsAnimated"].SetValue(true);
                Vector2 framePos = ((AnimatedBillboard)billboard).GetCurrentSpriteLocation();
                Vector2 frameDim = ((AnimatedBillboard)billboard).GetRelativeSpriteDimension();
                LevelEffect.Parameters["xFramePos"].SetValue(framePos);
                LevelEffect.Parameters["xFrameDim"].SetValue(frameDim);
            }
            else
            {
                LevelEffect.Parameters["xIsAnimated"].SetValue(false);
            }

            Device.BlendState = BlendState.Opaque;
            Device.DepthStencilState = DepthStencilState.Default;
            Device.RasterizerState = RasterizerState.CullNone;
            Device.SamplerStates[0] = SamplerState.LinearClamp;

            LevelEffect.Parameters["xAlphaTestDirection"].SetValue(1f);

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Device.Indices = billboard.GetIndexBuffer();
                Device.SetVertexBuffer(billboard.GetVertexBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, billboard.GetVertexBuffer().VertexCount, 0, billboard.GetIndexBuffer().IndexCount / 3);
            }

            LevelEffect.Parameters["xAlphaTestDirection"].SetValue(-1f);

            Device.BlendState = BlendState.NonPremultiplied;
            Device.DepthStencilState = DepthStencilState.DepthRead;

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Device.Indices = billboard.GetIndexBuffer();
                Device.SetVertexBuffer(billboard.GetVertexBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, billboard.GetVertexBuffer().VertexCount, 0, billboard.GetIndexBuffer().IndexCount / 3);
            }

        }

        /// <summary>
        /// Draw the Checkpoints.
        /// </summary>
        private void DrawCheckpoints(Matrix view, Matrix projection)
        {
            LevelEffect.CurrentTechnique = LevelEffect.Techniques["CheckpointShading"];

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;

            Device.BlendState = new BlendState()
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.One,
                BlendFactor = new Color(1.0F, 1.0F, 1.0F, 1.0F),
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorSourceBlend = Blend.One,
                ColorWriteChannels = ColorWriteChannels.All,
                ColorWriteChannels1 = ColorWriteChannels.All,
                ColorWriteChannels2 = ColorWriteChannels.All,
                ColorWriteChannels3 = ColorWriteChannels.All,
                // MultiSampleMask = -1
            };


            foreach (Checkpoint cp in Logic.Checkpoints)
            {
                
                LevelEffect.Parameters["xCPState"].SetValue(2);
                if (cp.State == CheckpointState.Reached)
                    LevelEffect.Parameters["xCPState"].SetValue(0);
                if (cp.State == CheckpointState.Next)
                    LevelEffect.Parameters["xCPState"].SetValue(1);
                float pointRotation = CheckPointRotation/2f;
                if (cp.State == CheckpointState.Next)
                    pointRotation *= 2f;
                if (cp.State == CheckpointState.Reached)
                    pointRotation = 0;
                foreach (Vector3 pos in cp.GetSpritePoints(pointRotation))
                {
                    LevelEffect.Parameters["xCPSpritePosition"].SetValue(pos);
                    foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Device.Indices = cp.IBuffer;
                        Device.SetVertexBuffer(cp.VBuffer);
                        Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
                    }
                }
            }

            Device.BlendState = BlendState.Opaque;
        }

        /// <summary>
        /// Draw the objects of the terrain, like trees, bushes and so on.
        /// </summary>
        private void DrawTerrainObjects()
        {

        }

        private void DrawSky(Matrix world, Matrix view, Matrix projection)
        {
            Sky.Draw(world, view, projection);
        }

        /// <summary>
        /// Draw objects which are important to the gameplay.
        /// </summary>
        private void DrawGameObjects(Matrix view, Matrix projection)
        {
            foreach(GameObjectShape shape in ShapeToModel.Keys)
            {
                Model model = ShapeToModel[shape];
                List<Matrix> worldmatrices = Logic.GetGameObjectWorldTransform(shape);
                foreach (Matrix worldMatrix in worldmatrices)
                {
                    Matrix[] modelTransforms = new Matrix[model.Bones.Count];
                    model.CopyAbsoluteBoneTransformsTo(modelTransforms);

                    foreach (ModelMesh mesh in model.Meshes)
                    {
                        foreach (Effect meshEffect in mesh.Effects)
                        {

                            meshEffect.CurrentTechnique = meshEffect.Techniques["MovingObjectShading"];
                            meshEffect.Parameters["xObjectColor"].SetValue(new Vector3(0.8f,0,0));
                            meshEffect.Parameters["xWorldMatrix"].SetValue(worldMatrix);
                            meshEffect.Parameters["xProjectionMatrix"].SetValue(projection);
                            meshEffect.Parameters["xViewMatrix"].SetValue(view);
                        }
                        mesh.Draw();
                    }
                }



            }
        }

        /// <summary>
        /// Draw the flying/moving/controlled object. 
        /// </summary>
        private void DrawMovingObject(Matrix projection, Matrix view) 
        {
            LevelEffect.CurrentTechnique = LevelEffect.Techniques["MovingObjectShading"];
            Logic.GetMovingObject().Draw(LevelEffect, projection, view);
        }

        private void DrawWater()
        {
            //draw water
            LevelEffect.Parameters["xWorldMatrix"].SetValue(Matrix.Identity);

            LevelEffect.CurrentTechnique = LevelEffect.Techniques["WaterShading"];

            LevelEffect.Parameters["xTideOffset"].SetValue(Water.GetTideOffset());

            //to get the mirage of the sky

            LevelEffect.Parameters["xCloudMoving"].SetValue(Sky.GetCloudSetOff());

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Device.Indices = Water.GetIndexBuffer();
                Device.SetVertexBuffer(Water.GetVertexBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Water.GetVertexBuffer().VertexCount, 0, Water.GetIndexBuffer().IndexCount / 3);
            }
        }

        private void DrawPauseScreen(Matrix prjection, Matrix view)
        {
            ScreenText.Begin();
            ScreenText.DrawString(Font, "Pause", PauseTitlePosition, Color.White);
            if(CurrentSelection == Selection.End)
                ScreenText.DrawString(Font, "Ende", EndFontPosition, Color.Gray);
            else
                ScreenText.DrawString(Font, "Ende", EndFontPosition, Color.White);

            if (CurrentSelection == Selection.Restart)
                ScreenText.DrawString(Font, "Neustart", RestartFontPosition, Color.Gray);
            else
                ScreenText.DrawString(Font, "Neustart", RestartFontPosition, Color.White);
            ScreenText.End();

            //Device.BlendState = BlendState.Opaque;
            //RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            //Device.RasterizerState = rs;

            //MenuCanvasEffect.CurrentTechnique = MenuCanvasEffect.Techniques["CanvasShading"];

            //MenuCanvasEffect.Parameters["xWorldMatrix"].SetValue(Matrix.Invert(view));
            //MenuCanvasEffect.Parameters["xProjectionMatrix"].SetValue(prjection);
            //MenuCanvasEffect.Parameters["xViewMatrix"].SetValue(view);
            //MenuCanvasEffect.Parameters["xOpacity"].SetValue(AbortBackground.Opacity);
            //MenuCanvasEffect.Parameters["xXPosition"].SetValue(AbortBackground.PositionX);
            //MenuCanvasEffect.Parameters["xYPosition"].SetValue(AbortBackground.PositionY);
            //MenuCanvasEffect.Parameters["xZPosition"].SetValue(-5);
            //MenuCanvasEffect.Parameters["xCanvasTexture"].SetValue(AbortBackground.CurrentTexture);

            //Device.BlendState = new BlendState()
            //{
            //    AlphaBlendFunction = BlendFunction.Add,
            //    AlphaDestinationBlend = Blend.InverseSourceAlpha,
            //    AlphaSourceBlend = Blend.One,
            //    BlendFactor = new Color(1.0F, 1.0F, 1.0F, 1.0F),
            //    ColorBlendFunction = BlendFunction.Add,
            //    ColorDestinationBlend = Blend.InverseSourceAlpha,
            //    ColorSourceBlend = Blend.One,
            //    ColorWriteChannels = ColorWriteChannels.All,
            //    ColorWriteChannels1 = ColorWriteChannels.All,
            //    ColorWriteChannels2 = ColorWriteChannels.All,
            //    ColorWriteChannels3 = ColorWriteChannels.All,
            //    // MultiSampleMask = -1
            //};
            //RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            //Device.RasterizerState = rs;

            //foreach (EffectPass pass in MenuCanvasEffect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();
            //    Device.SetVertexBuffer(AbortBackground.GetBuffer());
            //    Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            //}
        }

        private void DrawModel(Matrix View, Matrix Projection)
        {
            //xwingTransforms = new Matrix[Xwing.Bones.Count];
            //Xwing.CopyAbsoluteBoneTransformsTo(xwingTransforms);
            //foreach (ModelMesh mesh in Xwing.Meshes)
            //{
            //    foreach (Effect currentEffect in mesh.Effects)
            //    {
            //        currentEffect.CurrentTechnique = currentEffect.Techniques["ColoredModelShading"];
            //        currentEffect.Parameters["xWorldMatrix"].SetValue(xwingTransforms[mesh.ParentBone.Index] * worldMatrix);
            //        currentEffect.Parameters["xViewMatrix"].SetValue(View);
            //        currentEffect.Parameters["xProjectionMatrix"].SetValue(Projection);
            //    }
            //    mesh.Draw();
            //}
        }

        /// <summary>
        /// This methods handles the drawing of the level.
        /// Important in this method is the sequential construction of the level screen.
        /// - we make or show the terrain
        /// - we criss-cross a nice street
        /// - we put some terrain objects on it
        /// - we show some important gameojects
        /// - we set the player in the world (draw the moving object)
        /// <param name="camera">the specific view on the level</param>
        /// </summary>
        public override void Draw(Camera camera)
        {
            Device.BlendState = BlendState.Opaque;
            WorldMatrix = Matrix.Identity;

            Matrix skydomeWMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(600) * Matrix.CreateTranslation(camera.Position.X, 80, camera.Position.Z);
            DrawSky(skydomeWMatrix, camera.ViewMatrix, camera.ProjectionMatrix);


            LevelEffect.Parameters["xWorldMatrix"].SetValue(WorldMatrix);
            LevelEffect.Parameters["xProjectionMatrix"].SetValue(camera.ProjectionMatrix);
            LevelEffect.Parameters["xViewMatrix"].SetValue(camera.ViewMatrix);
            LevelEffect.Parameters["xCameraPosition"].SetValue(camera.Position);
            LevelEffect.Parameters["xCameraUp"].SetValue(camera.Up);
            LevelEffect.Parameters["xInvertedViewMatrix"].SetValue(Matrix.Invert(camera.ViewMatrix));


            //Device.DepthStencilState = DepthStencilState.Default;
            DrawWater();


            DrawTerrain(camera.ViewMatrix, camera.ProjectionMatrix);
            DrawMovingObject(camera.ProjectionMatrix, camera.ViewMatrix);

            DrawGameObjects(camera.ViewMatrix, camera.ProjectionMatrix);

            foreach (BillboardTree tree in Logic.GetTrees())
                DrawBillboard(tree);

            foreach (BillboardCanon canon in Logic.Canons)
            {
                DrawBillboard(canon);
                if (canon.Firing)
                    DrawBillboard(canon.Bullet);
            }

            DrawCanvas(FogInTheMiddle);
            Logic.GetFigure().Draw(Device, Matrix.Identity, camera.ViewMatrix, camera.ProjectionMatrix);

            //DrawTerrainObjects();
            //DrawStreet();
            DrawCanvas(WarningCanvas);
            DrawCanvas(RestartCanvas);
            DrawCanvas(AbortCanvas);
            DrawTime(camera.ViewMatrix, camera.ProjectionMatrix, camera.LookAt);
            //DrawCheckpoints(camera.ViewMatrix, camera.ProjectionMatrix);



            
            

            //Logic.GetCollisionChecker().DrawStuff(camera.ProjectionMatrix, camera.ViewMatrix);
            //Matrix scale = Matrix.CreateScale(0.0005f, 0.0005f, 0.0005f);

            //WorldMatrix = scale * Matrix.CreateTranslation(new Vector3(0, 0, 0));
            //DrawModel(camera.ViewMatrix, camera.ProjectionMatrix);

            //WorldMatrix = scale * Matrix.CreateTranslation(new Vector3(1, 0, 0));
            //DrawModel(camera.ViewMatrix, camera.ProjectionMatrix);

            //WorldMatrix = scale * Matrix.CreateTranslation(new Vector3(-1, 0, 0));
            //DrawModel(camera.ViewMatrix, camera.ProjectionMatrix);

            //WorldMatrix = scale * Matrix.CreateTranslation(new Vector3(0, 0, 1));
            //DrawModel(camera.ViewMatrix, camera.ProjectionMatrix);

            //WorldMatrix = scale * Matrix.CreateTranslation(new Vector3(0, 0, -1));
            //DrawModel(camera.ViewMatrix, camera.ProjectionMatrix);
            //base.Draw(camera);

        }

        #endregion

        /// <summary>
        /// Listen to changes.
        /// <example>- changes of the terrain result in changes of the stored terrain-buffers</example>
        /// </summary>
        public void ConditionChanged(Condition condition, List<ParameterIdentifier> changedParameters)
        {
            if (condition.GetID() == ConditionID.TerrainCondition)
                UpdateTerrainData();
            if (condition.GetID() == ConditionID.ViewCondition && (changedParameters.Contains(ParameterIdentifier.Aperture) || changedParameters.Contains(ParameterIdentifier.FocusPlane)))
                UpdateScreenPoints((ViewCondition)condition);
        }
    }
}
