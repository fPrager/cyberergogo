using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    /// <summary>
    /// The GameScreen can be in several states. 
    /// The states is important to switch between screens or activate them.
    /// </summary>
    enum ScreenState {
        //The screen is initialized.
        IsInitialized,
        //The screen completly loaded
        IsLoaded,
        //The screen is at the background but is still updated
        IsSleeping,
        //The screen is at the background an freezed (not updating)
        IsFrozen,
        //The screen is at the foreground
        IsActive,
        //The screen is unloading
        IsExiting,
    }

    /// <summary>
    /// This is a screen of the game. 
    /// It represents a (visual) game state, like "in optionmode", "at the startscreen" or "playing a level".
    /// </summary>
    class GameScreen: IConditionObserver
    {
        //Reference to the ScreenManager, which stores the GameScreen
        public ScreenManager ScreenManager { set; get; }

        public Selection CurrentSelection = Selection.Nothing;
        public bool SelectionPressed = false;

        protected GraphicsDevice Device { set; get; }

        //the state of the screen
        private ScreenState State;

        //an identifier as simple string
        private String Name;

        //indirect access to the state
        public ScreenState InState { get { return State; } }

        //It's a list of methods with their update-frequence
        //TODO: Wirklich wichtig?
        private List<UpdateMethod> UpdateMethods;

        protected BikeSelectionHelper BikeNavigation;

        protected Effect MenuCanvasEffect;

        /// <summary>
        /// This is a screen of the game. 
        /// It represents a (visual) game state, like "in optionmode", "at the startscreen" or "playing a level".
        /// </summary>
        public GameScreen(String name) 
        {
            this.Name = name;
            BikeNavigation = new BikeSelectionHelper();
            Initialize();
        }

        public virtual void UpdateScreenPoints(ViewCondition condition)
        { 
        
        }

        /// <summary>
        /// Initialization of the screen
        /// </summary>
        public virtual void Initialize() 
        {
            UpdateMethods = new List<UpdateMethod>();
            this.State = ScreenState.IsInitialized;
        }

        /// <summary>
        /// Loading all data of the screen
        /// </summary>
        public virtual void LoadContent()
        {
            MenuCanvasEffect = null;
            Util.GetInstance().LoadFile(ref MenuCanvasEffect, "Screen", "Effect");
            this.State = ScreenState.IsLoaded;
        }

        /// <summary>
        /// Add one method to the list of updating methods
        /// <param name="method">the new method</param>
        /// </summary>
        public void SetOnUpdateList(UpdateMethod method) 
        {
            UpdateMethods.Add(method);
        }

        /// <summary>
        /// Sets the references to the connected ScreenMananger and GraphicDevice
        /// <param name="device">the used GraphicDevice</param>
        /// <param name="manager">the ScreenManager, which handles this screen</param>
        /// </summary>
        public void SetScreenManager(ScreenManager manager, GraphicsDevice device)
        {
            ScreenManager = manager;
            Device = device;
        }

        /// <summary>
        /// Unloading the screen
        /// </summary>
        public virtual void Exit() 
        {
            ScreenManager.UnregisterScreen(this);
        }

        /// <summary>
        /// The screen "says" his screenmanager to catch the focus.
        /// </summary>
        public virtual void Open() 
        {
            ScreenManager.Activate(this);
        }

        /// <summary>
        /// It changes the screen to the background mode.
        /// </summary>
        public virtual void Sleep() 
        {
            State = ScreenState.IsSleeping;
        }

        /// <summary>
        /// Refresh the screen (depending on the time).
        /// <param name="gameTime">the current time of the game</param>
        /// </summary>
        public virtual void Update(GameTime gameTime) 
        {
            if (State != ScreenState.IsSleeping)
            {
                BikeNavigation.Update(gameTime.ElapsedGameTime.Milliseconds);
            }
        }

        /// <summary>
        /// The screen become active (just internal).
        /// </summary>
        public void GetFocus() 
        {
            State = ScreenState.IsActive;
        }

        public void LostFocus()
        {
            State = ScreenState.IsInitialized;
        }

        /// <summary>
        /// The external change of the screenstate.
        /// <param name="state">the wanted state</param>
        /// </summary>
        public void ChangeState(ScreenState state) 
        {
            State = state;
        }

        /// <summary>
        /// This method is called to prepare the presentation of the screen.
        /// <param name="gameTime">the current time of the game
        /// TODO: Wird das genutzt?</param>
        /// </summary>
        public virtual void PreDraw(GameTime gameTime)
        {

        }

        /// <summary>
        /// This method handles the presentation or "drawing" of the screen.
        /// <param name="camera">the view on the screen</param>
        /// </summary>
        public virtual void Draw(Camera camera) 
        { 
        
        }

        /// <summary>
        /// This method finishs the representation of the screen.
        /// <param name="gameTime">the current time of the game
        /// TODO: Wird das genutzt?</param>
        /// </summary>
        public virtual void PostDraw(GameTime gameTime)
        { 
        
        }

        /// <summary>
        /// This method gives the identifier of the screen.
        /// <returns>the identifier</returns>
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #region IConditionObserver Member

        public void ConditionChanged(Condition condition, List<ParameterIdentifier> changedParameters)
        {
            if (condition.GetID() == ConditionID.OptionCondition && (changedParameters.Contains(ParameterIdentifier.BackBufferHeight) || changedParameters.Contains(ParameterIdentifier.BackBufferWidth)))
                UpdateScreenPoints((ViewCondition)condition);
        }

        #endregion
    }
}
