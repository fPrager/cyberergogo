using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace CyberErgoGo
{
    /// <summary>
    /// In this screen you can choose a level to play.
    /// </summary>
    class LevelSelectionScreen:GameScreen
    {
        Effect LevelSelectionEffect;
        LevelContainer Container;
        GraphicsDevice Device;
        SpriteFont Font;
        SpriteBatch SpriteBatch;
        List<SelectableLevelView> LevelList;

        public bool AllSelected = false;

        int TerrainPreviewWidth = 600;
        int TerrainPreviewHeight = 600;
        VertexBuffer MapPreviewVertices;
        IndexBuffer MapPreviewIndices;

        //to block premature interactions there is a delay of two seconds after loading the screen
        float StartTimeInMilli = 0;
        int StartingTimeInSec = 2;
        bool ReadyToStart = false;


        //these parameters descripes the morphing of the two levels
        //the CurrentPreviewMap is the new choosen map with its texture and connected ColorToHeight-Rate
        Texture2D CurrentPreviewMap;
        float CurrentCTHRate;

        //the OldPreviewMap is the older map with its texture and connected ColorToHeight-Rate
        Texture2D OldPreviewMap;
        float OldCTHRate;
        //from 0 to 1, the MorphingFactor shows state of morphing from the preview to the current heightmap  
        float MorphingFactor = 1;
        bool MakeMorphing = true;
        bool EarthQuakeBool = true;

        int CurrentSelectedIndex = 0;

        //the static front-rotation of the preview map
        float TerrainPreviewRotationX = 0;
        //the dynamic (updated) rotation
        float TerrainPreviewRotation = 0;

        const float TerrainScaleFactor = 1;
        float TerrainScaling = TerrainScaleFactor;
        const float TerrainPosX = 0f;
        const float TerrainPosY = -0.5f;
        int TerrainDepth = 100;

        int PreviewHeight = 200;
        int PreviewWidth = 400;

        Skydome Sky;

        //important points
        int ScreenWidth;
        int ScreenHeight;
        Rectangle WindowToTerrainPreview;
        Vector2 LevelTitlePosition;
        Vector2 LevelDescriptionPosition;
        Rectangle BackButton;
        Rectangle StartButton;
        Texture2D WindowTexture;

        AnimatedBillboard Invader;
        List<Billboard> Trees;
        float TreeGrowingFactor = 0;
        Vector3 PrevCamPosition;

        #region old canvas 

        //MenuCanvas BorderCanvas;
        //MenuCanvas SphereCanvas;
        //MenuCanvas DiskCanvas;
        //MenuCanvas WheelCanvas;
        //MenuCanvas TimeCanvas;

        //TimeDisplay LevelTime;
        //MenuCanvas HardIcon;

        #endregion

        #region old variables of static positioning
        //const int BorderHeight = 40;
        //const int BorderWidth = 60;
        //const int BorderDepth = -47;
        // Vector3 BorderColor = new Vector3(0.6f, 0.1f, 0.1f);

        //const float MOPosX = -17f;
        //const float MOPosY = 6f;
        //int MoDepth = -50;
        //const float MOScale = 4f;
        //const float SelectionDepthJump = 5f;

        //const float InfoCanvasPosX = 13.4f;
        //const float InfoCanvasPosY = -4.5f;
        //const float InfoCanvasWidth = 12.9f;
        //const float InfoCanvasHeight = InfoCanvasWidth/3;
        //const float InfoCanvasDepth = -40;
        // Vector3 InfoCanvasColor = new Vector3(0.5f, 0.5f, 0.5f);
        #endregion

        int CurrentModelIndex = 1;

        KeyboardState PrevKeyboardState;

        List<MovingObject> PossibleMovingObjects;
        MovingObject CurrentModel;

        Selection CurrentSelection = Selection.Level;
        Waterplane Water;

        TracPreviewShield TracPreview;
        Billboard HighScores;

        struct SelectableLevelView
        {
            public int LevelIndex;

            public String Title;
            public DegreeOfDifficulty Difficulty;
            public MenuCanvas Preview;
            public Vector2 ScreenPosition;
            public Texture2D TerrainMap;
            public float ColorToHeightRate;
            public List<PlayerEntry> Highscores;

            public SelectableLevelView(MenuCanvas preview, String title, Vector2 screenPosition, DegreeOfDifficulty difficulty, int index, Texture2D terrainMap, float colorToHeightRate, List<PlayerEntry> highscores)
            {
                Preview = preview;
                Title = title;
                ScreenPosition = screenPosition;
                Difficulty = difficulty;
                LevelIndex = index;
                TerrainMap = terrainMap;
                ColorToHeightRate = colorToHeightRate;
                Highscores = highscores;
            }
        }

        public LevelSelectionScreen(String name)
            : base(name)
        {
            Device = Util.GetInstance().Device;
            Vector3 gameColor = ((GamePlayCondition)(ConditionHandler.GetInstance().GetCondition(ConditionID.GamePlayCondition))).GameColor;
            
            //BorderColor = gameColor;
            //InfoCanvasColor = gameColor * 0.5f;

            Container = new LevelContainer();
            SetUpListOfMovingObjects();
        }

        public override void UpdateScreenPoints(ViewCondition oc)
        {
            //ScreenWidth = oc.BackBufferWidth;
            //ScreenHeight = oc.BackBufferHeight;

            //int previewWindowX = ScreenWidth / 15;
            //int previewWindowY = ScreenWidth / 15;

            //int previeWindowWidth = (int)(ScreenHeight * 0.8f);
            //int previeWindowHeight = (int)(ScreenHeight * 0.8f);

            //WindowTexture = new Texture2D(Util.GetInstance().Device, ScreenWidth, ScreenHeight);

            //Color[] textureColor = new Color[ScreenWidth * ScreenHeight];

            //for (int x = 0; x < ScreenWidth; x++)
            //    for (int y = 0; y < ScreenHeight; y++)
            //    {
            //        if ((x > previewWindowX && x < (previewWindowX + previeWindowWidth)) && (y > previewWindowY && y < (previewWindowY + previeWindowHeight)))
            //            textureColor[x + y * ScreenWidth] = Color.Transparent;
            //        else
            //            textureColor[x + y * ScreenWidth] = Color.Black;
            //    }

            //WindowTexture.SetData(textureColor);

            //WindowToTerrainPreview = new Rectangle(previewWindowX, previewWindowY, previeWindowWidth, previeWindowHeight);
            //LevelTitlePosition = new Vector2(previewWindowX,previewWindowY / 2);

            //LevelDescriptionPosition = new Vector2(previewWindowX, previewWindowY * 1.5f + previeWindowHeight);
        }

        private void SetUpListOfMovingObjects()
        {
            PossibleMovingObjects = new List<MovingObject>();
            PossibleMovingObjects.Add(new VWCP(0));
            PossibleMovingObjects.Add(new VWCP(1));
            PossibleMovingObjects.Add(new VWCP(2));
            
            CurrentModel = PossibleMovingObjects.ElementAt(CurrentModelIndex);
        }

        public void SetUpCamera()
        {
            Vector3 CameraLookAt = Vector3.Normalize(new Vector3(0.5f, 0, 1));
            ViewCondition vc = (ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition);
            vc.MovingBehaviour = new HorizontalRotation(CameraLookAt);
            //FreeCam: change the movingbaviour to keyboard control
            //vc.MovingBehaviour = new FreeMouseKeyboardControlled(CameraLookAt);
            PrevCamPosition = new Vector3(40, 35, 40);
            vc.CenterPosition = PrevCamPosition;
            vc.ConditionHasChanged();
            Util.GetInstance().SoundManager.PlayStartMusic();
        }

        public override void LoadContent()
        {
            //UpdateScreenPoints((ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.OptionCondition));

            SetUpCamera();

            LevelSelectionEffect = null;
            Util.GetInstance().LoadFile(ref LevelSelectionEffect, "Selection", "Effect");

            Container.LoadLevels();
            LoadLevelData();
            
            //terrain textures
            TerrainCondition terrainCondition = (TerrainCondition)(ConditionHandler.GetInstance().GetCondition(ConditionID.TerrainCondition));
            LevelSelectionEffect.Parameters["xLevel0Texture"].SetValue(terrainCondition.Level0Texture);
            LevelSelectionEffect.Parameters["xLevel0to1"].SetValue(terrainCondition.Level0to1);
            LevelSelectionEffect.Parameters["xLevel1Texture"].SetValue(terrainCondition.Level1Texture);
            LevelSelectionEffect.Parameters["xLevel1to2"].SetValue(terrainCondition.Level1to2);
            LevelSelectionEffect.Parameters["xLevel2Texture"].SetValue(terrainCondition.Level2Texture);
            LevelSelectionEffect.Parameters["xLevel2to3"].SetValue(terrainCondition.Level2to3);
            LevelSelectionEffect.Parameters["xLevel3Texture"].SetValue(terrainCondition.Level3Texture);

            LevelSelectionEffect.Parameters["xStreetTexture"].SetValue(terrainCondition.StreetTexture);

            CurrentPreviewMap = LevelList.First().TerrainMap;
            CurrentCTHRate = LevelList.First().ColorToHeightRate;
            LevelSelectionEffect.Parameters["xTerrainPreviewTextureCurrent"].SetValue(CurrentPreviewMap);
            LevelSelectionEffect.Parameters["xTerrainPreviewCTHRateCurrent"].SetValue(CurrentCTHRate);
            CurrentSelectedIndex = 0;

            Vector3 lightDirection = new Vector3(0, -1, 0.5f);
            lightDirection.Normalize();
            LevelSelectionEffect.Parameters["xLightDirection"].SetValue(lightDirection);
            LevelSelectionEffect.Parameters["xAmbient"].SetValue(0.3f);
            LevelSelectionEffect.Parameters["xPreviewWidth"].SetValue(TerrainPreviewWidth);
            //float ColorToHeightRate = ((TerrainCondition)(ConditionHandler.GetInstance().GetCondition(ConditionID.TerrainCondition))).ColorHeightRate;
            //ColorToHeightRate *= 0.5f;
            //LevelSelectionEffect.Parameters["xColorToHeightRate"].SetValue(ColorToHeightRate);
            LevelSelectionEffect.Parameters["xPreviewHeight"].SetValue(TerrainPreviewHeight);

            Font = ((OptionCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.OptionCondition)).GameFont;

            #region declarations of the old canvas
            //Texture2D background = null;
            //Util.GetInstance().LoadFile(ref background, "Screen", "selectionBorder");
            //Texture2D[] backgroundList = new Texture2D[1] { background };
            //BorderCanvas = new MenuCanvas(new Vector2(0, 0), BorderDepth, BorderWidth, BorderHeight, backgroundList, 1, BorderColor);

            //Texture2D background1 = null;
            //Util.GetInstance().LoadFile(ref background1, "Screen", "wheel");
            //Texture2D[] backgroundList1 = new Texture2D[1] { background1 };
            //WheelCanvas = new MenuCanvas(new Vector2(InfoCanvasPosX, InfoCanvasPosY), InfoCanvasDepth, InfoCanvasWidth, InfoCanvasHeight, backgroundList1, 0, InfoCanvasColor);

            //Texture2D background2 = null;
            //Util.GetInstance().LoadFile(ref background2, "Screen", "disk");
            //Texture2D[] backgroundList2 = new Texture2D[1] { background2 };
            //DiskCanvas = new MenuCanvas(new Vector2(InfoCanvasPosX, InfoCanvasPosY), InfoCanvasDepth, InfoCanvasWidth, InfoCanvasHeight, backgroundList2, 0, InfoCanvasColor);

            //Texture2D background3 = null;
            //Util.GetInstance().LoadFile(ref background3, "Screen", "sphere");
            //Texture2D[] backgroundList3 = new Texture2D[1] { background3 };
            //SphereCanvas = new MenuCanvas(new Vector2(InfoCanvasPosX, InfoCanvasPosY), InfoCanvasDepth, InfoCanvasWidth, InfoCanvasHeight, backgroundList3, 0, InfoCanvasColor);

            #endregion

            SpriteBatch = new SpriteBatch(Util.GetInstance().Device);
            SetUpBuffer();

            LoadMovingObjects();

            LoadTerrainObjects();

            LoadWater();

            LoadInvader();

            LoadTrees();

            LoadSky();

            base.LoadContent();
        }

        public void LoadLevelData()
        {
            List<Level> levels = Util.GetInstance().LevelList;
            LevelList = new List<SelectableLevelView>();
            foreach (Level level in levels)
            {
                String title = level.GetTitle();
                HeightMap terrainMap = level.GetPlayingTerrain().GetHeightMapWithStreet();
                float colorToHeightRate = level.GetPlayingTerrain().GetColorToHeightRate();
                DegreeOfDifficulty difficulty = level.GetDegreeOfDifficulty();
                int nextLevelView = 400;
                int index = 0;
                Texture2D testTexture = null;
                Util.GetInstance().LoadFile(ref testTexture, "Models", "wings");
                MenuCanvas levelImage = new MenuCanvas(new Vector2(10, 0), PreviewWidth, PreviewHeight, 0, new Texture2D[1] { testTexture }, 1);
                SelectableLevelView newView = new SelectableLevelView(levelImage, title, new Vector2(0, nextLevelView * index), difficulty, index, terrainMap.GetMapAsVector4(), colorToHeightRate * 0.5f, level.PlayerEntries);
                LevelList.Add(newView);
            }
            if (HighScores != null)
                UpdateHighscroes();
            else
                LoadHighScores();
        }

        private void LoadHighScores()
        {
            List<PlayerEntry> currentHighscores = LevelList.ElementAt(CurrentSelectedIndex).Highscores;
            String highscoreText = "";
            foreach (PlayerEntry entry in currentHighscores)
                highscoreText += "\n " + entry.Name + " : " + entry.Time;
            Texture2D highscoreTex = Util.GetInstance().GetFontTexture(highscoreText, 300, 300);

            HighScores = new Billboard(30, new Vector3(200, 30f, 200), highscoreTex);
            HighScores.LoadContent();
        }

        private void UpdateHighscroes()
        {
            List<PlayerEntry> currentHighscores = LevelList.ElementAt(CurrentSelectedIndex).Highscores;
            String highscoreText = "";
            foreach (PlayerEntry entry in currentHighscores)
                highscoreText += "\n " + entry.Name + " : " + entry.Time;
            Texture2D highscoreTex = Util.GetInstance().GetFontTexture(highscoreText, 300, 300);
            HighScores.ChangeTexture(highscoreTex);
        }

        private void LoadSky()
        {
            Sky = new Skydome();
            Sky.LoadContent(LevelSelectionEffect);
            LevelSelectionEffect.Parameters["xSkyboxTexture"].SetValue(Sky.GetCloudTexture());
        }

        private void LoadTerrainObjects()
        {
            TracPreview = new TracPreviewShield(new Vector3(22, 3.9f, 40), Quaternion.CreateFromAxisAngle(Vector3.Up,MathHelper.ToRadians(110)), TerrainPreviewWidth, TerrainPreviewHeight, TerrainScaling);
            TracPreview.Load(LevelSelectionEffect);
        }

        private void LoadTrees()
        {
            int numberOfForests = 10;
            
            int treeDistance = 10;
            Trees = new List<Billboard>();
            List<Vector3> positions = new List<Vector3>();
            int treeFreeUntilX = 200;
            int treeFreeUntilZ = 200;
            

            for (int i = 0; i < numberOfForests; i++)
            {
                bool conflict = true;
                int xPos = 0;
                int zPos = 0;
                int numberOfTreesInForest = (Util.GetInstance().GetRandomNumber(3)+1)*2;

                while (conflict)
                {
                    conflict = false;
                    xPos = Util.GetInstance().GetRandomNumber(TerrainPreviewWidth - 200) + 50;
                    zPos = Util.GetInstance().GetRandomNumber(TerrainPreviewHeight - 200) + 50;

                    if (xPos < treeFreeUntilX && zPos < treeFreeUntilZ)
                        conflict = true;

                    foreach (Vector3 usedPos in positions)
                    {
                        bool inXConflict = false;
                        bool inZConflict = false;
                        int forestDim = (int)usedPos.Z * treeDistance;

                        if (xPos > usedPos.X && xPos < (usedPos.X + forestDim))
                            inXConflict = true;
                        if (zPos > usedPos.Y && zPos < (usedPos.Y + forestDim))
                            inZConflict = true;

                        if (inXConflict && inZConflict)
                        {
                            conflict = true;
                        }
                    }
                }

                positions.Add(new Vector3(xPos, zPos, numberOfTreesInForest));

                for (int j = 0; j < numberOfTreesInForest*numberOfTreesInForest; j++)
                    Trees.Add(new BillboardTree(8, new Vector3(xPos + (j / numberOfTreesInForest) * treeDistance, 0, zPos + (j % numberOfTreesInForest) * treeDistance)));
            }

            foreach (BillboardTree tree in Trees)
                tree.LoadContent();
        
        }

        private void LoadInvader()
        {
            Invader = new BillboardInvader(6, new Vector3(75,14.5f, 110));
            Invader.LoadContent();
        }

        public void LoadMovingObjects()
        {
            ReadyToStart = false;
            StartTimeInMilli = 0;
            //foreach (MovingObject movingObject in PossibleMovingObjects)
            //{
            //    movingObject.SetPhysicalRepresentation(new NotPhysical());
            //    movingObject.Load(LevelSelectionEffect);
            //    movingObject.SetPosition(new Vector3(40 + 5 * PossibleMovingObjects.IndexOf(movingObject), 25, 70));
            //    movingObject.PhysicalUpdate();
            //}
            MovingObject bowlingSphere = PossibleMovingObjects.ElementAt(2);
            bowlingSphere.SetPhysicalRepresentation(new NotPhysical());
            bowlingSphere.Load(LevelSelectionEffect);
            bowlingSphere.SetTransformationAbsolute(1, Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(180), 0, MathHelper.ToRadians(80)), new Vector3(45, 21, 120));
            bowlingSphere.PhysicalUpdate();
            MovingObject tire = PossibleMovingObjects.ElementAt(1);
            tire.SetPhysicalRepresentation(new NotPhysical());
            tire.Load(LevelSelectionEffect);
            tire.SetTransformationAbsolute(1, Quaternion.CreateFromYawPitchRoll(0, 0, MathHelper.ToRadians(90)), new Vector3(50, 22f, 100));
            tire.PhysicalUpdate();
            MovingObject disk = PossibleMovingObjects.ElementAt(0);
            disk.SetPhysicalRepresentation(new NotPhysical());
            disk.Load(LevelSelectionEffect);
            disk.SetTransformationAbsolute(1, Quaternion.CreateFromYawPitchRoll(0, MathHelper.ToRadians(-10), MathHelper.ToRadians(-10)), new Vector3(50, 23, 120));
            disk.PhysicalUpdate();
            //ScaleObjects(MOScale);
        }

        private void LoadWater()
        {
            Water = new Waterplane(-2000, 2000, -2000, 2000,0.5f);
            LevelSelectionEffect.Parameters["xTideHeight"].SetValue(Water.GetMaxTideHeight());
        }

        public void ScaleObjects(float scale)
        {
            //BoundingSphere sphere = new BoundingSphere(new Vector3(MOPosX, MOPosY, -MoDepth), scale);
            //foreach (MovingObject movingObject in PossibleMovingObjects)
            //{
            //    movingObject.SetBounding(sphere);
            //}
        }

        public void MoveObjects(Vector3 translate)
        {
            foreach (MovingObject movingObject in PossibleMovingObjects)
            {
                movingObject.Translate(translate);
            }
        }

        private void NextModel()
        {
            if (PossibleMovingObjects.Count == CurrentModelIndex + 1)
                CurrentModelIndex = 0;
            else
                CurrentModelIndex++;

            CurrentModel = PossibleMovingObjects.ElementAt(CurrentModelIndex);
        }

        private void NextLevel()
        {
            if (LevelList.Count == 1) return;

            

            OldPreviewMap = LevelList.ElementAt(CurrentSelectedIndex).TerrainMap;
            OldCTHRate = LevelList.ElementAt(CurrentSelectedIndex).ColorToHeightRate;

            CurrentSelectedIndex = (CurrentSelectedIndex + 1) % LevelList.Count;
            CurrentPreviewMap = LevelList.ElementAt(CurrentSelectedIndex).TerrainMap;
            CurrentCTHRate = LevelList.ElementAt(CurrentSelectedIndex).ColorToHeightRate;

            TracPreview.SetStreetMap(OldPreviewMap, CurrentPreviewMap);

            MakeMorphing = true;
            MorphingFactor = 0;
            Util.GetInstance().SoundManager.PlayEarthQuake();

            LevelSelectionEffect.Parameters["xTerrainPreviewTextureCurrent"].SetValue(CurrentPreviewMap);
            LevelSelectionEffect.Parameters["xTerrainPreviewCTHRateCurrent"].SetValue(CurrentCTHRate);
            LevelSelectionEffect.Parameters["xTerrainPreviewTextureOld"].SetValue(OldPreviewMap);
            LevelSelectionEffect.Parameters["xTerrainPreviewCTHRateOld"].SetValue(OldCTHRate);

            UpdateHighscroes();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //TerrainPreviewRotation += gameTime.ElapsedGameTime.Milliseconds * 0.0005f;

            if (StartTimeInMilli / 1000 > StartingTimeInSec)
            {
                StartTimeInMilli += gameTime.ElapsedGameTime.Milliseconds;
                ReadyToStart = true;
            }
            else
                StartTimeInMilli += gameTime.ElapsedGameTime.Milliseconds;

            if (Keyboard.GetState().IsKeyDown(Keys.I) && Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                CurrentModel.Translate(new Vector3(0, 0, -0.3f));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.I) && Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                CurrentModel.Translate(new Vector3(0, 0, 0.3f));
            }

            if (MakeMorphing && TreeGrowingFactor == 0)
            {
                MorphingFactor += gameTime.ElapsedGameTime.Milliseconds * 0.0005f;
                if (MorphingFactor >= 1)
                {
                    MorphingFactor = 1;
                    MakeMorphing = false;
                }
            }
            if(!MakeMorphing && MorphingFactor == 1)
            {
                
                    TreeGrowingFactor += gameTime.ElapsedGameTime.Milliseconds * 0.005f;
                    if (TreeGrowingFactor >= 1) 
                        TreeGrowingFactor = 1;
            }

            if (MakeMorphing && MorphingFactor == 0)
            {
                TreeGrowingFactor -= gameTime.ElapsedGameTime.Milliseconds * 0.005f;
                    if (TreeGrowingFactor <= 0)
                    {
                        TreeGrowingFactor = 0;
                        MorphingFactor = 0;
                    }
            }
            TracPreview.SetMorphing(MorphingFactor);

            if (ReadyToStart)
            {
                if (PrevKeyboardState == null) PrevKeyboardState = Keyboard.GetState();
                KeyboardState curKeyboardState = Keyboard.GetState();

                if (curKeyboardState.IsKeyDown(Keys.W) && !PrevKeyboardState.IsKeyDown(Keys.W))
                    NextLevel();

                if (curKeyboardState.IsKeyDown(Keys.O) && !PrevKeyboardState.IsKeyDown(Keys.O))
                    TerrainPreviewRotationX += 0.1f;

                if (curKeyboardState.IsKeyDown(Keys.L) && !PrevKeyboardState.IsKeyDown(Keys.L))
                    TerrainPreviewRotationX -= 0.1f;

                if (curKeyboardState.IsKeyDown(Keys.Down) && !PrevKeyboardState.IsKeyDown(Keys.Down))
                    NextModel();

                if (curKeyboardState.IsKeyDown(Keys.Enter) && !PrevKeyboardState.IsKeyDown(Keys.Enter))
                    AllSelected = true;

                PrevKeyboardState = curKeyboardState;
                BikeSelection currentSelection = BikeNavigation.GetCurrentSelection();

                if (currentSelection == BikeSelection.Left && CurrentSelection == Selection.MovingObject)
                {
                    //MoveObjects(new Vector3(-2, 1, SelectionDepthJump));
                    //TerrainDepth += (int)SelectionDepthJump;
                    CurrentSelection = Selection.Level;
                }
                if (currentSelection == BikeSelection.Right && CurrentSelection == Selection.Level)
                {
                    //MoveObjects(new Vector3(2, -1, -SelectionDepthJump));
                    //TerrainDepth -= (int)SelectionDepthJump;
                    CurrentSelection = Selection.MovingObject;
                }

                if (currentSelection == BikeSelection.Up && MorphingFactor>0.9f)
                    NextLevel();

                if (currentSelection == BikeSelection.Click)
                    AllSelected = true;
            }
            //CurrentModel.Rotate(Quaternion.CreateFromYawPitchRoll(gameTime.ElapsedGameTime.Milliseconds * 0.0005f, 0, 0));
           

            float step = gameTime.ElapsedGameTime.Milliseconds / 1000f;

            //switch (CurrentModelIndex) 
            //{ 
            //    case 0:
            //        if (DiskCanvas.Opacity < 1) DiskCanvas.Opacity += step;
            //        if (SphereCanvas.Opacity > 0) SphereCanvas.Opacity -= step;
            //        if (WheelCanvas.Opacity > 0) WheelCanvas.Opacity -= step;
            //        break;
            //        case 1:
            //        if (WheelCanvas.Opacity < 1) WheelCanvas.Opacity += step;
            //        if (SphereCanvas.Opacity > 0) SphereCanvas.Opacity -= step;
            //        if (DiskCanvas.Opacity > 0) DiskCanvas.Opacity -= step;
            //        break;
            //        case 2:
            //        if (SphereCanvas.Opacity < 1) SphereCanvas.Opacity += step;
            //        if (DiskCanvas.Opacity > 0) DiskCanvas.Opacity -= step;
            //        if (WheelCanvas.Opacity > 0) WheelCanvas.Opacity -= step;
            //        break;
            //    default:
            //        break;
            //}

            Sky.Update(gameTime.ElapsedGameTime.Milliseconds);
            Water.Update(gameTime.ElapsedGameTime.Milliseconds);
            Invader.Update(gameTime.ElapsedGameTime.Milliseconds);
        }

        private void EarthQuakeSimulation()
        {
            ViewCondition vc = (ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition);
            Vector3 currentPosition = vc.CenterPosition;
            int num = Util.GetInstance().GetRandomNumber(1, 2);
            //if (((int)(MorphingFactor * 1000)) % 2 == 0)
            //{
            Util.GetInstance().SoundManager.ChangeEarthQuakeVolume(1 - MorphingFactor);
            if (EarthQuakeBool)
            {
                vc.CenterPosition = PrevCamPosition + (Vector3.Up * 0.1f) * (1-MorphingFactor);
            }
            else
            {
                vc.CenterPosition = PrevCamPosition - (Vector3.Up * 0.1f)*(1 - MorphingFactor);
            }
            EarthQuakeBool = !EarthQuakeBool;
        //}
            vc.ConditionHasChanged();
        }

        public MovingObject GetSelectedMovingObject()
        {
            return CurrentModel;
        }

        public Level GetSelectedLevel()
        {
            return Container.GetLevelAt(CurrentSelectedIndex);
        }

        public List<Billboard> GetTrees()
        {
            return Trees;
        }

        private void SetUpBuffer()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[TerrainPreviewWidth * TerrainPreviewHeight];
            
            for (int x = 0; x < TerrainPreviewWidth; x++)
                for(int y= 0; y < TerrainPreviewHeight; y++)
            {
                Vector3 position = new Vector3(x, 0, y);
                Vector2 texCoord = new Vector2((float)x / (float)TerrainPreviewWidth, (float)y / (float)TerrainPreviewHeight);
                vertices[x * TerrainPreviewWidth + y] = new VertexPositionTexture(position, texCoord);
            }

            int[] indices = new int[(TerrainPreviewWidth - 1) * (TerrainPreviewHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < TerrainPreviewHeight - 1; y++)
            {
                for (int x = 0; x < TerrainPreviewWidth - 1; x++)
                {
                    int lowerLeft = x + y * TerrainPreviewWidth;
                    int lowerRight = (x + 1) + y * TerrainPreviewWidth;
                    int topLeft = x + (y + 1) * TerrainPreviewWidth;
                    int topRight = (x + 1) + (y + 1) * TerrainPreviewWidth;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }

            GraphicsDevice Device = Util.GetInstance().Device;
            MapPreviewVertices = new VertexBuffer(Device, VertexPositionTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            MapPreviewVertices.SetData(vertices);
            
            MapPreviewIndices = new IndexBuffer(Device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            MapPreviewIndices.SetData(indices);
        }

        private void SetUpScreen()
        { 
            
        }

        /// <summary>
        /// Draw the MenuCanvas
        /// </summary>
        private void DrawCanvas(MenuCanvas canvas)
        {
            LevelSelectionEffect.CurrentTechnique = LevelSelectionEffect.Techniques["MenuCanvasShading"];

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
            LevelSelectionEffect.Parameters["xCanvasPosition"].SetValue(canvas.Position);
            LevelSelectionEffect.Parameters["xCanvasOpacity"].SetValue(canvas.Opacity);
            LevelSelectionEffect.Parameters["xCanvasTexture"].SetValue(canvas.CurrentTexture);
            LevelSelectionEffect.Parameters["xCanvasIsMask"].SetValue(canvas.AsMask);
            LevelSelectionEffect.Parameters["xCanvasColor"].SetValue(canvas.Color);

            foreach (EffectPass pass in LevelSelectionEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.Indices = canvas.GetIBuffer();
                Device.SetVertexBuffer(canvas.GetVBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            Device.BlendState = BlendState.Opaque;
        }

        private void DrawSky(Matrix world, Matrix view, Matrix projection)
        {
            Sky.Draw(world, view, projection);
        }

        private void DrawWater()
        {
            //draw water
            LevelSelectionEffect.Parameters["xWorldMatrix"].SetValue(Matrix.Identity);

            LevelSelectionEffect.CurrentTechnique = LevelSelectionEffect.Techniques["WaterShading"];

            LevelSelectionEffect.Parameters["xTideOffset"].SetValue(Water.GetTideOffset());

            //to get the mirage of the sky

            LevelSelectionEffect.Parameters["xCloudMoving"].SetValue(Sky.GetCloudSetOff());

            foreach (EffectPass pass in LevelSelectionEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Device.Indices = Water.GetIndexBuffer();
                Device.SetVertexBuffer(Water.GetVertexBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Water.GetVertexBuffer().VertexCount, 0, Water.GetIndexBuffer().IndexCount / 3);
            }
        }


        private void DrawBillboard(Billboard billboard, float growingFactor)
        {

            LevelSelectionEffect.CurrentTechnique = LevelSelectionEffect.Techniques["Billboarding"];
            LevelSelectionEffect.Parameters["xBillboardWidth"].SetValue(billboard.GetWidth());
            LevelSelectionEffect.Parameters["xBillboardHeight"].SetValue(billboard.GetHeight());
            LevelSelectionEffect.Parameters["xBillboardTexture"].SetValue(billboard.GetTexture());
            LevelSelectionEffect.Parameters["xAllowedRotDir"].SetValue(Vector3.Up);
            LevelSelectionEffect.Parameters["xToFlip"].SetValue(billboard.GetFlip());
            LevelSelectionEffect.Parameters["xGrowingFactor"].SetValue(growingFactor);

            if (billboard.GetIsAnimated())
            {
                LevelSelectionEffect.Parameters["xIsAnimated"].SetValue(true);
                Vector2 framePos = ((AnimatedBillboard)billboard).GetCurrentSpriteLocation();
                Vector2 frameDim = ((AnimatedBillboard)billboard).GetRelativeSpriteDimension();
                LevelSelectionEffect.Parameters["xFramePos"].SetValue(framePos);
                LevelSelectionEffect.Parameters["xFrameDim"].SetValue(frameDim);
            }
            else
            {
                LevelSelectionEffect.Parameters["xIsAnimated"].SetValue(false);
            }

            LevelSelectionEffect.Parameters["xSetOnTerrain"].SetValue(true);
            Vector2 relPos = billboard.GetRelativePosition(TerrainPreviewWidth, TerrainPreviewHeight);
            LevelSelectionEffect.Parameters["xPositionAsTextureCoord"].SetValue(relPos);

            Device.BlendState = BlendState.Opaque;
            Device.DepthStencilState = DepthStencilState.Default;
            Device.RasterizerState = RasterizerState.CullNone;
            Device.SamplerStates[0] = SamplerState.LinearClamp;

            LevelSelectionEffect.Parameters["xAlphaTestDirection"].SetValue(1f);

            foreach (EffectPass pass in LevelSelectionEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Device.Indices = billboard.GetIndexBuffer();
                Device.SetVertexBuffer(billboard.GetVertexBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, billboard.GetVertexBuffer().VertexCount, 0, billboard.GetIndexBuffer().IndexCount / 3);
            }

            LevelSelectionEffect.Parameters["xAlphaTestDirection"].SetValue(-1f);

            Device.BlendState = BlendState.NonPremultiplied;
            Device.DepthStencilState = DepthStencilState.DepthRead;

            foreach (EffectPass pass in LevelSelectionEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Device.Indices = billboard.GetIndexBuffer();
                Device.SetVertexBuffer(billboard.GetVertexBuffer());
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, billboard.GetVertexBuffer().VertexCount, 0, billboard.GetIndexBuffer().IndexCount / 3);
            }

        }

        public override void Draw(Camera camera)
        {
            if (MorphingFactor < 1)
            {
                EarthQuakeSimulation();
            }
            GraphicsDevice Device = Util.GetInstance().Device;

            Matrix worldMatrix = Matrix.CreateRotationY(TerrainPreviewRotation) * Matrix.CreateScale(TerrainScaling) * Matrix.CreateRotationX(TerrainPreviewRotationX);
            
            LevelSelectionEffect.CurrentTechnique = LevelSelectionEffect.Techniques["TerrainShading"];
            LevelSelectionEffect.Parameters["xViewMatrix"].SetValue(camera.ViewMatrix);
            LevelSelectionEffect.Parameters["xProjectionMatrix"].SetValue(camera.ProjectionMatrix);
            LevelSelectionEffect.Parameters["xCameraLookAt"].SetValue(camera.CenterPosition-camera.CenterLookAt);
            LevelSelectionEffect.Parameters["xCameraPosition"].SetValue(camera.CenterPosition);
            LevelSelectionEffect.Parameters["xWorldMatrix"].SetValue(worldMatrix);
            LevelSelectionEffect.Parameters["xInvertedViewMatrix"].SetValue(Matrix.Invert(camera.ViewMatrix));
            LevelSelectionEffect.Parameters["xMorphingFactor"].SetValue(MorphingFactor);

                //Device.SamplerStates[0] = SamplerState.PointClamp;

                Device.BlendState = BlendState.Opaque;

                RasterizerState rs = new RasterizerState();
                rs.CullMode = CullMode.None;
                Device.RasterizerState = rs;

                Matrix skydomeWMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(600) * Matrix.CreateTranslation(camera.Position.X, 80, camera.Position.Z);
                DrawSky(skydomeWMatrix, camera.ViewMatrix, camera.ProjectionMatrix);

                foreach (EffectPass pass in LevelSelectionEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    Device.Indices = MapPreviewIndices;
                    Device.SetVertexBuffer(MapPreviewVertices);
                    Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, MapPreviewVertices.VertexCount, 0, MapPreviewIndices.IndexCount / 3);
                }



            MenuCanvasEffect.CurrentTechnique = MenuCanvasEffect.Techniques["SimpleQuadShader"];
                
           // Util.GetInstance().DrawFullscreenQuad(WindowTexture, 0, ScreenWidth, ScreenHeight, MenuCanvasEffect);
            

            DrawWater();
           

            //LevelSelectionEffect.CurrentTechnique = LevelSelectionEffect.Techniques["MovingObjectShading"];

            foreach (MovingObject model in PossibleMovingObjects)
            {
                model.Draw(LevelSelectionEffect, camera.ProjectionMatrix, camera.ViewMatrix);
            }

            //LevelSelectionEffect.CurrentTechnique = LevelSelectionEffect.Techniques["TerrainObjectShading"];

            //TracPreview.Draw(LevelSelectionEffect, camera.ProjectionMatrix, camera.ViewMatrix);

            worldMatrix = Matrix.CreateScale(TerrainScaling);

            LevelSelectionEffect.Parameters["xWorldMatrix"].SetValue(worldMatrix);

            DrawBillboard(Invader, 1f);

            foreach (BillboardTree tree in Trees)
                DrawBillboard(tree, TreeGrowingFactor);
            Device.BlendState = BlendState.Opaque;

            //DrawBillboard(HighScores, 1);
            LevelTitlePosition = new Vector2(200, 800);
            SpriteBatch.Begin();
            SpriteBatch.DrawString(Font, "PaceInvader", LevelTitlePosition, Color.White);
            //SpriteBatch.DrawString(Font, LevelList.ElementAt(CurrentSelectedIndex).Difficulty.ToString(), LevelDescriptionPosition, Color.White);
            SpriteBatch.End();

            //DrawCanvas(BorderCanvas);
            //DrawCanvas(WheelCanvas);
            //DrawCanvas(DiskCanvas);
            //DrawCanvas(SphereCanvas);
            //DrawCanvas(TestCanvas);

        }

        
    }
}
