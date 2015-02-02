using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BikeControls;

namespace CyberErgoGo
{
    /// <summary>
    /// Dies ist der Haupttyp für Ihr Spiel
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ScreenManager ScreenManager;
        Camera GameCamera;
        bool IsStereo = false;
        RenderTarget2D LeftSide;
        RenderTarget2D RightSide;
        Effect GlobalGameEffect;
        Color BackGroundColor = Color.LightBlue;

        public MainGame()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);

            OptionCondition oc = (OptionCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.OptionCondition);
            IsStereo = oc.IsStereo;
            graphics.IsFullScreen = oc.IsFullScreen;
            if (IsStereo && !oc.IsFullScreen)
                graphics.PreferredBackBufferWidth = oc.BackBufferWidth * 2;
            else
                graphics.PreferredBackBufferWidth = oc.BackBufferWidth;
            graphics.PreferredBackBufferHeight = oc.BackBufferHeight;

            
        }

        private void SetGraphicParameters(OptionCondition oc)
        {
            graphics.PreferredBackBufferWidth = oc.BackBufferWidth;
            graphics.PreferredBackBufferHeight = oc.BackBufferHeight;
            graphics.IsFullScreen = oc.IsFullScreen;
            IsStereo = oc.IsStereo;
            GraphicsDevice Device = Util.GetInstance().Device;
            PresentationParameters pp = Device.PresentationParameters;
            int TargetWidth = 0;
            if (oc.IsFullScreen) TargetWidth = oc.BackBufferWidth / 2;
            else TargetWidth = oc.BackBufferWidth;
            LeftSide = new RenderTarget2D(Device, TargetWidth, oc.BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount,
                RenderTargetUsage.DiscardContents);

            RightSide = new RenderTarget2D(Device, TargetWidth, oc.BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount,
                RenderTargetUsage.DiscardContents);
        }

        /// <summary>
        /// Ermöglicht dem Spiel, alle Initialisierungen durchzuführen, die es benötigt, bevor die Ausführung gestartet wird.
        /// Hier können erforderliche Dienste abgefragt und alle nicht mit Grafiken
        /// verbundenen Inhalte geladen werden.  Bei Aufruf von base.Initialize werden alle Komponenten aufgezählt
        /// sowie initialisiert.
        /// </summary>
        protected override void Initialize()
        {
            ConditionHandler.GetInstance();
            Util.GetInstance().SetGraphicDevice(graphics.GraphicsDevice);
            Util.GetInstance().SetContentManager(Content);

            GamePlayCondition gc = (GamePlayCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.GamePlayCondition);
            gc.GameColor = new Vector3(0.5f, 0, 0);

            //OptionCondition oc = (OptionCondition)ConditionHandler.GetInstance().RegisterMe(ConditionID.OptionCondition,this);
            OptionCondition oc = (OptionCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.OptionCondition);
            SetGraphicParameters(oc);

            

            CollisionChecker collisions = new CollisionChecker();
            Util.GetInstance().CollisionsChecker = collisions;

            SoundManager soundManager = new SoundManager();
            Util.GetInstance().SoundManager = soundManager;

            ScreenManager = new ScreenManager();
            //LevelScreen testLevel = new LevelScreen("LevelScreen", new LevelLogic(levelContainer.GetFirstLevel()));
            //testLevel.LoadContent();
            //ScreenManager.RegisterScreen(testLevel);
            //ScreenManager.Activate(testLevel);

            //Behaviour cameraBehaviour = new PassivBehaviour(ParameterIdentifier.Position, ParameterIdentifier.MovingOrientation, ConditionID.MovingObjectCondition);
            Behaviour cameraBehaviour;
            float aspectRatio = 0;
            if(IsStereo)
                aspectRatio = (graphics.PreferredBackBufferWidth/2) / (float) graphics.PreferredBackBufferHeight;
            else
                aspectRatio = graphics.PreferredBackBufferWidth  / (float)graphics.PreferredBackBufferHeight;
            TerrainCondition tc = (TerrainCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.TerrainCondition);
            int terrainWidth = tc.AreaWidth;
            int terrainHeight = tc.AreaHeight;
            int maxHeight = tc.MaxHeight;
            Vector3 CameraPosition = new Vector3(40, 30, 40);
            Vector3 CameraLookAt = new Vector3(1,0,1);
            Vector3 CameraTarget = CameraPosition + CameraLookAt;
            cameraBehaviour = new HorizontalRotation(CameraLookAt);
            GameCamera = new Camera(cameraBehaviour, MathHelper.PiOver4, aspectRatio, 3f, 10000f, Vector3.Up, CameraPosition, CameraTarget);
            //((PassivBehaviour)cameraBehaviour).RegisterParent(GameCamera);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent wird einmal pro Spiel aufgerufen und ist der Platz, wo
        /// Ihr gesamter Content geladen wird.
        /// </summary>
        protected override void LoadContent()
        {

            Util.GetInstance().SoundManager.LoadSounds();
            // Erstellen Sie einen neuen SpriteBatch, der zum Zeichnen von Texturen verwendet werden kann.
            GlobalGameEffect = null;
            Util.GetInstance().LoadFile(ref GlobalGameEffect, "Global", "Effect");

            SpriteFont gameFont = null;
            Util.GetInstance().LoadFile(ref gameFont, "Global", "Text");

            OptionCondition oc = (OptionCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.OptionCondition);
            oc.GameFont = gameFont;
            oc.ConditionHasChanged();

            ScreenManager.LoadFirstScreen();


            spriteBatch = new SpriteBatch(GraphicsDevice);

            Util.GetInstance().CollisionsChecker.LoadMobileMeshes();
        }

        /// <summary>
        /// UnloadContent wird einmal pro Spiel aufgerufen und ist der Ort, wo
        /// Ihr gesamter Content entladen wird.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Entladen Sie jeglichen Nicht-ContentManager-Inhalt hier
        }

        /// <summary>
        /// Ermöglicht dem Spiel die Ausführung der Logik, wie zum Beispiel Aktualisierung der Welt,
        /// Überprüfung auf Kollisionen, Erfassung von Eingaben und Abspielen von Ton.
        /// </summary>
        /// <param name="gameTime">Bietet einen Schnappschuss der Timing-Werte.</param>
        protected override void Update(GameTime gameTime)
        {
            // Ermöglicht ein Beenden des Spiels
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            ViewCondition vc = (ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition);
            if (Keyboard.GetState().IsKeyDown(Keys.E) && Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                vc.EyeOffset += 0.05f;
                vc.ConditionHasChanged();
                Console.WriteLine("eyeoffest: " + vc.EyeOffset);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.E) && Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                vc.EyeOffset -= 0.05f;
                vc.ConditionHasChanged();
                Console.WriteLine("eyeoffest: " + vc.EyeOffset);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F) && Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                vc.FocusPlane += 0.5f;
                vc.ConditionHasChanged();
                Console.WriteLine("focusPlane: " + vc.FocusPlane);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F) && Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                vc.FocusPlane -= 0.5f;
                vc.ConditionHasChanged();
                Console.WriteLine("focusPlane: " + vc.FocusPlane);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A) && Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                vc.Aperture += 0.01f;
                vc.ConditionHasChanged();
                Console.WriteLine("aperture: " + vc.Aperture);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A) && Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                vc.Aperture -= 0.01f;
                vc.ConditionHasChanged();
                Console.WriteLine("aperture: " + vc.Aperture);
            }

            if (vc.MovingBehaviour.GetType() != typeof(NotMoving))
                GameCamera.PhysicalUpdate();
            ScreenManager.UpdateScreens(gameTime);
            GameCamera.Update(gameTime.ElapsedGameTime.Milliseconds, 1f);
            //Util.GetInstance().CollisionsChecker.UpdateCollisions();
           
            BikeState state = Bike.GetState();
           // Console.WriteLine("speed " + state.CurrentSpeed.InKilometerPerHour);
            Util.GetInstance().SoundManager.Update(gameTime.ElapsedGameTime.Milliseconds);
            base.Update(gameTime);
        }

        /// <summary>
        /// Dies wird aufgerufen, wenn das Spiel selbst zeichnen soll.
        /// </summary>
        /// <param name="gameTime">Bietet einen Schnappschuss der Timing-Werte.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            if (!IsStereo)
            {
                GraphicsDevice.Clear(BackGroundColor);
                GameCamera.ChangeEye(EyeRepresentation.Center);
                ScreenManager.DrawScreens(GameCamera);
            }
            else
            {
                //draw left side
                
                GraphicsDevice Device = Util.GetInstance().Device;
                Device.SetRenderTarget(LeftSide);
                GameCamera.ChangeEye(EyeRepresentation.LeftEye);
                GraphicsDevice.Clear(BackGroundColor);
                ScreenManager.DrawScreens(GameCamera);

                Device.SetRenderTarget(RightSide);
                GameCamera.ChangeEye(EyeRepresentation.RightEye);
                GraphicsDevice.Clear(BackGroundColor);
                ScreenManager.DrawScreens(GameCamera);

                Device.SetRenderTarget(null);
                GlobalGameEffect.CurrentTechnique = GlobalGameEffect.Techniques["SimpleQuadShader"];

                Util.GetInstance().DrawFullscreenQuad(LeftSide, 0, LeftSide.Width, LeftSide.Height, GlobalGameEffect);
                Util.GetInstance().DrawFullscreenQuad(RightSide, RightSide.Width, RightSide.Width, RightSide.Height, GlobalGameEffect);
            }

            base.Draw(gameTime);
        }

        #region IConditionObserver Member

        void ConditionChanged(Condition condition, List<ParameterIdentifier> changedParameters)
        {
            if (condition.GetID() == ConditionID.OptionCondition &&
                (changedParameters.Contains(ParameterIdentifier.BackBufferHeight) || changedParameters.Contains(ParameterIdentifier.BackBufferWidth) || changedParameters.Contains(ParameterIdentifier.IsFullScreen) || (changedParameters.Contains(ParameterIdentifier.IsStereo))))
                SetGraphicParameters((OptionCondition)condition);
        }

        #endregion
    }
}
