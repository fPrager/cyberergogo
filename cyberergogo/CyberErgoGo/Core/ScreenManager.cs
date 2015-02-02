using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    /// <summary>
    /// This class manages the game screens.
    /// </summary>
    class ScreenManager
    {
        public List<GameScreen> GameScreens;
        public GameScreen FocusedScreen;
        private ScreenLogic ScreenLogic;

        //DefaultScreen: Is the screen which will be shown, if no other screen want the focus. In principle it's a kind of error screen, because it should never be shown.
        public GameScreen DefaultScreen;

        public ScreenManager() 
        {
            ScreenLogic = new ScreenLogic(this);
            GameScreens = new List<GameScreen>();
        }

        /// <summary>
        /// This method will remove a screen to the possible shown screens.
        /// </summary>
        public void UnregisterScreen(GameScreen screen) 
        {
            GameScreens.Remove(screen);
        }

        /// <summary>
        /// This method will add a screen to the possible shown screens.
        /// </summary>
        public void RegisterScreen(GameScreen screen)
        {
            GameScreens.Add(screen);
        }

        /// <summary>
        /// This method focuses the game on this screen. It's important that the screen is allready registred in the screen list and that the screen is loaded and ready to be shown.
        /// It's important that the screen is allready registred in the screen list and that the screen is loaded and ready to be shown.
        /// </summary>
        public void Activate(GameScreen screen)
        {
            if (GameScreens.Contains(screen))
            {
                if (screen.InState == ScreenState.IsLoaded || screen.InState == ScreenState.IsInitialized)
                {
                    if (FocusedScreen != null) FocusedScreen.LostFocus();
                    FocusedScreen = screen;
                    screen.GetFocus();
                }
                else
                    Console.WriteLine(screen + " isn't ready to be shown. It should be in the state 'IsLoaded'");
            }
            else
                Console.WriteLine(screen + " is missing in the list of the ScreenManager");
        }

        public void LoadFirstScreen()
        {
            ScreenLogic.Load();
        }

        /// <summary>
        /// Update the screens which has the focus.
        /// <param name="gameTime">the current time of the game</param>
        /// </summary>
        public void UpdateScreens(GameTime gameTime) 
        {
            ScreenLogic.Update();
            if (FocusedScreen != null)
            {
                FocusedScreen.Update(gameTime);
            }
            else
                Console.WriteLine("There exists no focused screen to update!");
        }

        /// <summary>
        /// Present the focused screen.
        /// <param name="camera">the specific view on the screens</param>
        /// </summary>
        public void DrawScreens(Camera camera) 
        {
                FocusedScreen.Draw(camera);
        }

    }
}
