using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class TimeDisplay
    {
            public int TimePlateWidth;
            public int TimePlateHeight;
            public bool OnFinishedPosition;

            private Vector2 FinishedPositionXY;
            private int FinsihedDepth;

            public Vector2 Min10Pos;
            public int Min10
            {
                get { return (int)CurrentMin / 10; }
            }

            public Vector2 Min1Pos;
            public int Min1
            {
                get { return CurrentMin % 10; }
            }

            public Vector2 Sec10Pos;
            public int Sec10
            {
                get { return (int)CurrentSec / 10; }
            }

            public Vector2 Sec1Pos;
            public int Sec1
            {
                get { return CurrentSec % 10; }
            }

            public Vector2 MSec10Pos;
            public int MSec10
            {
                get { return (int)CurrentMSec / 100; }
            }

            public Vector2 MSec1Pos;
            public int MSec1
            {
                get { return (CurrentMSec % 100)/10; }
            }

            public int CurrentMin = 0;
            public int CurrentSec = 0;
            public int CurrentMSec = 0;

            public float ZDepth;

            public TimeDisplay(int width, int height, Vector2 upperLeft, float depth, float plateDistance, Vector2 finishXY, int finishDepth)
            {
                TimePlateWidth = width;
                TimePlateHeight = height;
                Min10Pos = upperLeft;
                Min1Pos = upperLeft + new Vector2(width + plateDistance, 0);
                Sec10Pos = upperLeft + new Vector2(2*(width + plateDistance), 0);
                Sec1Pos = upperLeft + new Vector2(3*(width + plateDistance), 0);
                MSec10Pos = upperLeft + new Vector2(4*(width + plateDistance), 0);
                MSec1Pos = upperLeft + new Vector2(5*(width + plateDistance), 0);
                ZDepth = depth;
                FinishedPositionXY = finishXY;
                FinsihedDepth = finishDepth;
            }

            public void SetTime(int msec, int sec, int min)
            {
                CurrentMSec = msec;
                CurrentSec = sec;
                CurrentMin = min;
            }

            public void JumpToFinishedPosition(float step)
            {
                float finXPos = FinishedPositionXY.X - TimePlateWidth * 0.5f * 3;
                float finYPos = FinishedPositionXY.Y - TimePlateHeight * 0.5f;
                bool xPositioned = false;
                bool yPositioned = false;
                bool zPositioned = false;

                if (Min10Pos.X > finXPos)
                {
                    Min10Pos.X -= step;
                    Min1Pos.X -= step;
                    MSec10Pos.X -= step;
                    MSec1Pos.X -= step;
                    Sec10Pos.X -= step;
                    Sec1Pos.X -= step;
                }
                else
                    xPositioned = true;

                if (Min10Pos.Y > finYPos)
                {
                    Min10Pos.Y -= step;
                    Min1Pos.Y -= step;
                    MSec10Pos.Y -= step;
                    MSec1Pos.Y -= step;
                    Sec10Pos.Y -= step;
                    Sec1Pos.Y -= step;
                }
                else
                    yPositioned = true;

                if (ZDepth < FinsihedDepth)
                    ZDepth += step;
                else
                    zPositioned = true;
                if (xPositioned && yPositioned && zPositioned)
                    OnFinishedPosition = true;
            }

            public void TimeReset()
            {
                CurrentMSec = 0;
                CurrentSec = 0;
                CurrentMin = 0;
            }
            public void AddElapsedTime(int msec)
            {
                CurrentMSec += msec;
                CurrentSec += CurrentMSec / 1000;
                CurrentMSec = CurrentMSec % 1000;
                CurrentMin += CurrentSec / 60;
                CurrentSec = CurrentSec % 60;
            }

        
    }
}
