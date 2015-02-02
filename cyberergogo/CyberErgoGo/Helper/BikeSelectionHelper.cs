using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BikeControls;

namespace CyberErgoGo
{
    public enum BikeSelection
    { 
        Right,
        Left,
        Center,
        Up,
        Down,
        NotMoving,
        Click
    }

    class BikeSelectionHelper
    {
        private BikeSelection SelectionMoving;
        private bool WasReaded = false;
        private int SmallRounds = 0;
        private int LeastRounds = 0;
        private int FullRounds = 0;
        private int RoundsToScroll = 2;

        int oldleftRigthMoving = 0;
        int leftRightThreshold = 1;
        BikeState OldState;
        float ScrollFactor = 0;
        BikeSelection PrevSelection = BikeSelection.Center;

        public BikeSelectionHelper()
        { }

        public void Update(float time)
        {

            BikeState state = Bike.GetState();
            //Console.WriteLine("oldstate: " + OldState.IsRolling + " newstate: " + state.IsRolling);
            //if (state.IsRolling && !OldState.IsRolling)
            //    Console.WriteLine("roling start");
            //if (!state.IsRolling && OldState.IsRolling)
            //    Console.WriteLine("roling end");
            if (state.CurrentSpeed.InKilometerPerHour > 3)
            {
                float step = time * 0.001f;
                step = (state.CurrentSpeed.InMeterPerSecond * 0.5f) * time*0.01f;
                ScrollFactor += step;
                SelectionMoving = PrevSelection;
            }
            else {
               // ScrollFactor = 0;
            }
            
            //Console.WriteLine("scrollfactor: " + ScrollFactor);

            if (ScrollFactor > 110)
            {
                ScrollFactor = 0;
                PrevSelection = SelectionMoving;
                MovingChanged(BikeSelection.Up);
            }
            //if (state.IsRolling)
            //{
            //    if (LeastRounds != state.CurrentSpeed.Rounds)
            //    {
            //        SmallRounds += state.CurrentSpeed.Rounds;
            //        FullRounds = SmallRounds / 10;
            //        if (FullRounds >= RoundsToScroll)
            //        {
            //            MovingChanged(BikeSelection.Up);
            //            SmallRounds = SmallRounds % 20;
            //        }
            //        LeastRounds = state.CurrentSpeed.Rounds;
            //       // Console.WriteLine("smallrounds: " + SmallRounds + " fullrounds: " + FullRounds);
            //    }
            //}
            else
            {
                if (state.CurrentSteering.InDegree > 0 && SelectionMoving != BikeSelection.Right)
                {
                    MovingChanged(BikeSelection.Right);
                    PrevSelection = BikeSelection.Right;
                    oldleftRigthMoving = state.CurrentSteering.InDegree;
                }
                if (state.CurrentSteering.InDegree < 0 && SelectionMoving != BikeSelection.Left)
                {
                    MovingChanged(BikeSelection.Left);
                    PrevSelection = BikeSelection.Left;
                    oldleftRigthMoving = state.CurrentSteering.InDegree;
                }
            }
            if (state.Fire.IsFiring)
            {
                if (SelectionMoving != BikeSelection.Click)
                {
                    MovingChanged(BikeSelection.Click);
                }
            }
            //OldState = Bike.GetState();
        }

        public void MovingChanged(BikeSelection newSelectionMoving)
        {
            WasReaded = false;
            SelectionMoving = newSelectionMoving;
        }

        public BikeSelection GetLeftCenterOrRight()
        {
            BikeState state = Bike.GetState();
            int minValue = state.CurrentSteering.MinSteering;
            int maxValue = state.CurrentSteering.MaxSteering;
            float threshold = 0.1f;
            int steering = state.CurrentSteering.InDegree;
            if (steering < minValue * threshold)
                return BikeSelection.Left;
            else if (steering > maxValue * threshold)
                return BikeSelection.Right;
            return BikeSelection.Center;
            
        }


        public BikeSelection GetCurrentSelection()
        {
            //if (WasReaded)
            //    return BikeSelection.NotMoving;
            //else
            //{
            //    WasReaded = true;
                return SelectionMoving;
            //}
        }
    }
}
