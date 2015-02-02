using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    /// <summary>
    /// Every a level should be characterized by its difficulty.
    /// </summary>
    enum DegreeOfDifficulty 
    { 
        Easy,
        Medium,
        Hard,
        UnKnown
    }

    struct PlayerEntry 
    {
        public String Name;
        public int Time;
    }

    /// <summary>
    /// This is a playable level of the game.
    /// </summary>
    class Level
    {
        //the terrain, which is the "ground of the world"
        Terrain PlayingTerrain;

        //players can mass themselves in a level-specific highscorelist
        List<int> HighScores;

        DegreeOfDifficulty Difficulty;

        public List<PlayerEntry> PlayerEntries;

        String Title;

        public Vector3 Start 
        {
            get { return PlayingTerrain.StreetStartPoint; }
        }

        public List<Vector3> CheckPoints
        {
            get { return PlayingTerrain.StreetCheckPoints; }
        }

        public List<Vector3> WallPoints
        {
            get { return PlayingTerrain.WallPoints; }
        }

        public List<Vector3> BigSpherePoints
        {
            get { return PlayingTerrain.SpherePoints; }
        }

        public List<Vector3> StonePoints
        {
            get { return PlayingTerrain.StonePoints; }
        }

        public Vector3 End
        {
            get { return PlayingTerrain.SteetEndPoint; }
        }

        public VertexBuffer StreetVertexBuffer
        {
            get { return PlayingTerrain.GetStreetVertexBuffer(); }
        }

        public IndexBuffer StreetIndexBuffer
        {
            get { return PlayingTerrain.GetStreetIndexBuffer(); }
        }

        public Level(Terrain terrain, String title, DegreeOfDifficulty difficulty, List<PlayerEntry> highscores) 
        {
            PlayingTerrain = terrain;
            HighScores = new List<int>();
            Difficulty = difficulty;
            Title = title;
            PlayerEntries = highscores;
        }

        public String GetTitle()
        {
            return Title;
        }

        public Quaternion GetStreetOrientation(Vector3 positionOnStreet)
        {
            return PlayingTerrain.GetStreetOrientation(positionOnStreet);
        }

        public DegreeOfDifficulty GetDegreeOfDifficulty()
        {
            return Difficulty;
        }

        /// <summary>
        /// Get access to the terrain.
        /// This is usefull to handle the right drawing of the terrain, for instance.
        /// <returns>the terrain</returns>
        /// </summary>
        internal Terrain GetPlayingTerrain()
        {
            return PlayingTerrain;
        }


        public void LoadTerrainData()
        {
            PlayingTerrain.SetMyCondition();
        }

        public bool IsPositionOverStreet(Vector3 position)
        {
            return PlayingTerrain.IsThisPositionOverStreet(position);
        }

        private void CalculateDegreeOfDiffculty()
        {
            //TODO: Schwierigkeitsgrad berechnen
            //mögliche Faktoren: Straßenlänge, SpieleObjekte (Hindernisse, Boni), Straßenverlauf
        }
    }
}
