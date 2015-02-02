using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    enum CurrentGameSituation
    {
        Start,
        Selection,
        Play,
        Pause,
        Option
    }

    /// <summary>
    /// This class should handle the right sequence of the screens.
    /// </summary>
    class ScreenLogic
    {
        ScreenManager Manager;
        LevelSelectionScreen Selection;
        LevelScreen Play;
        CurrentGameSituation Situation;

        public ScreenLogic(ScreenManager manager)
        {
            Manager = manager;
            Selection = new LevelSelectionScreen("LevelSelectionScreen");

            Situation = CurrentGameSituation.Selection;
        }

        public void Load()
        {
            Manager.RegisterScreen(Selection);
            Selection.LoadContent();
            Manager.Activate(Selection);
        }

        public void Update()
        {
            switch (Situation)
            { 
                case CurrentGameSituation.Selection:
                    SelectLevel();
                    break;
                case CurrentGameSituation.Play:
                    EndGame();
                    break;
                default:
                    break;
            }
        }

        private void SelectLevel()
        {
            if (Selection.AllSelected)
            {
                Selection.AllSelected = false;
                Play = new LevelScreen("LevelScreen", new LevelLogic(Selection.GetSelectedLevel(), Selection.GetSelectedMovingObject(), Selection.GetTrees()));
                Play.LoadContent();
                Situation = CurrentGameSituation.Play;
                Manager.RegisterScreen(Play);
                Manager.Activate(Play);
            }
        }

        private void EndGame()
        {
            if (Play.Selected && Play.CurrentSelection == CyberErgoGo.Selection.End)
            {
                Manager.Activate(Selection);
                Selection.LoadMovingObjects();
                Selection.LoadLevelData();
                Selection.SetUpCamera();
                Manager.UnregisterScreen(Play);
                Situation = CurrentGameSituation.Selection;
               
            }
        }

        public void Draw(GameTime gameTime, bool stereo)
        { 
        
        }

    }
}
