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

namespace CyberErgoGoLevelData
{
    /// <summary>
    /// A single point. To be able to serialize points it is important to set up a simple structure like this (Vector3 can't be serialized).
    /// </summary>
    public class Point
    {
        public decimal X;
        public decimal Y;
        public decimal Z;
    }

    public struct PlayerEntry
    {
        public String Name;
        public decimal Time;
    }

    public class ControlPoint
    {
        public Point ToPrev;
        public Point Position;
        public Point ToNext;
        public String FunctionString = "Nothing";
        public SpecialFunction Function = SpecialFunction.Nothing;
    }

    public enum SpecialFunction
    { 
        Wall,
        Tower,
        Block,
        BigSphere,
        LeftBlock,
        RightBlock,
        Kliff,
        NoCheckPoints,
        Stones,
        Nothing,
        Start,
        End
    }

    /// <summary>
    /// Instances of this class holds the information about one level. Every level has its street, heightmap and name (identifier).
    /// This class must be a own library that the XNA-Serialization knows the structure at compiling time (so I understood this). 
    /// It can't be a class in the game project.
    /// </summary>
    public class LevelData
    {
        //the identifier of a level
        public String LevelName;

        //the points of the street are spline points of a bezier curve
        //they are values between 0 and 1 to be independant of the real terrain dimensions 
        //(and just represent ratios)
        private List<Vector3> StreetPoints;

        //these are the streetpoints in a simpler serializable format
        public List<ControlPoint> Points;

        //the width of the street
        public int StreetWidth;

        //the maxima height of the street
        public int MaxStreetHeight;

        //color-value to height-value
        public int ColorToHeightRate;

        //the file name of the used heightmap/terrain
        public String MapName;

        public List<PlayerEntry> PlayerEntries;
        /// <summary>
        /// "Converts" the simple point structure in 
        /// </summary>
        private void StoreStreetPoints()
        {
            ConvertSpecialFunctionToEnum();
            StreetPoints = new List<Vector3>();
            foreach (ControlPoint p in Points)
            {
                if (p.Function != SpecialFunction.Start)
                    StreetPoints.Add(new Vector3((float)p.ToPrev.X, (float)p.ToPrev.Y, (float)p.ToPrev.Z));
                else
                    Console.WriteLine("start");
                StreetPoints.Add(new Vector3((float)p.Position.X, (float)p.Position.Y, (float)p.Position.Z));
                if (p.Function != SpecialFunction.End)
                    StreetPoints.Add(new Vector3((float)p.ToNext.X, (float)p.ToNext.Y, (float)p.ToNext.Z));
                else
                    Console.WriteLine("end");
            }
        }

        private void ConvertSpecialFunctionToEnum()
        {
            foreach (ControlPoint p in Points)
            {
                try {
                    p.Function = (SpecialFunction)Enum.Parse(typeof(SpecialFunction), p.FunctionString, true);
                }
                catch 
                {
                    Console.WriteLine(p.FunctionString + " can't be converted in SpecialFunction");
                }
            }
        }

        /// <summary>
        /// Returns the convertet points in the vector3-format
        /// <returns>the list of splinepoints in vector3-format</returns>
        /// </summary>
        public List<Vector3> GetStreetPoints()
        {
            StoreStreetPoints();
            return StreetPoints;
        }

        public void GetPlayerListAndTimes(ref String[] names, ref int[] times)
        {
            names = new String[PlayerEntries.Count];
            times = new int[PlayerEntries.Count];
            for (int i = 0; i < PlayerEntries.Count; i++)
            {
                names[i] = PlayerEntries.ElementAt(i).Name;
                times[i] = (int)PlayerEntries.ElementAt(i).Time;
            }
        }

        public List<int> GetIndecesOfWallPoints()
        {
            List<int> indeces = new List<int>();
            int currentIndex = 0;
            foreach (ControlPoint cp in Points)
            {
                if (cp.Function != SpecialFunction.Start)
                    currentIndex++;
                if (cp.Function == SpecialFunction.Wall)
                    indeces.Add(currentIndex);
                currentIndex++;
                currentIndex++;
            }
            return indeces;
        }

        public void AddPlayerEntry(String name, int time)
        {
            PlayerEntry newEntry = new PlayerEntry();
            newEntry.Name = name;
            newEntry.Time = time;
            PlayerEntries.Add(newEntry);
        }

        public List<int> GetIndecesOfBigSpherePoints()
        {
            List<int> indeces = new List<int>();
            int currentIndex = 0;
            foreach (ControlPoint cp in Points)
            {
                if (cp.Function != SpecialFunction.Start)
                    currentIndex++;
                if (cp.Function == SpecialFunction.BigSphere)
                    indeces.Add(currentIndex);
                currentIndex++;
                currentIndex++;
            }
            return indeces;
        }

        public List<int> GetIndecesOfStonePoints()
        {
            List<int> indeces = new List<int>();
            int currentIndex = 0;
            bool prevWasNoCheckpoints = false;
            foreach (ControlPoint cp in Points)
            {
                if (cp.Function == SpecialFunction.Stones)
                {
                    prevWasNoCheckpoints = true;
                    indeces.Add(currentIndex);
                }
                else
                    if (prevWasNoCheckpoints)
                    {
                        prevWasNoCheckpoints = false;
                        indeces.Add(currentIndex);
                    }
                if (cp.Function != SpecialFunction.Start)
                    currentIndex++;
            }
            return indeces;
        }

        public List<int> GetIndecesOfNoCheckpoints()
        {
            List<int> indeces = new List<int>();
            int currentIndex = 0;
            bool prevWasNoCheckpoints = false;
            foreach (ControlPoint cp in Points)
            {
                if (cp.Function == SpecialFunction.NoCheckPoints)
                {
                    prevWasNoCheckpoints = true;
                    indeces.Add(currentIndex);
                }
                else
                    if (prevWasNoCheckpoints)
                    {
                        prevWasNoCheckpoints = false;
                        indeces.Add(currentIndex);
                    }
                if(cp.Function != SpecialFunction.Start)
                currentIndex++;
            }
            return indeces;
        }
    }
}
