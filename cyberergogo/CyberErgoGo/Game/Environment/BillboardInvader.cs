using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CyberErgoGo
{
    class BillboardInvader:AnimatedBillboard
    {
        KeyboardState Old_KeyState = Keyboard.GetState();

        public BillboardInvader(float height, Vector3 pos)
            : base(height, pos, "Invader", "Invader", SetUpAnimationTexture())
        { }

        private static AnimationTexture SetUpAnimationTexture()
        {
            return new AnimationTexture(10, 18, SetUpAnimationList());
        }

        private static List<SpriteAnimation> SetUpAnimationList()
        {
            List<SpriteAnimation> animations = new List<SpriteAnimation>();

            animations.Add(new SpriteAnimation(AnimationName.Stand, 3, 7, 4, 10, 15));
            animations.Add(new SpriteAnimation(AnimationName.Stand_Behind, 0, 3, 4, 5, 15));
            animations.Add(new SpriteAnimation(AnimationName.Sitting_Behind, 5, 11, 0, 12, 10));
            animations.Add(new SpriteAnimation(AnimationName.Change, 0, 0, 7, 0, 20));
            animations.Add(new SpriteAnimation(AnimationName.ReadyToChange, 0, 0, 1, 0, 1));
            animations.Add(new SpriteAnimation(AnimationName.StartChange, 8, 12, 2, 13, 5));
            animations.Add(new SpriteAnimation(AnimationName.Walk_Behind, 5, 5, 5, 6, 15));
            animations.Add(new SpriteAnimation(AnimationName.Run_Behind, 6, 10, 4, 11, 20));
            animations.Add(new SpriteAnimation(AnimationName.Wave, 0, 15, 3, 16, 10));
            animations.Add(new SpriteAnimation(AnimationName.HardCollision, 8, 0, 8, 1, 5));
            animations.Add(new SpriteAnimation(AnimationName.SoftCollision, 9, 1, 2, 2, 5));
            animations.Add(new SpriteAnimation(AnimationName.StandUp_Behind, 3, 2, 9, 3, 10));
            animations.Add(new SpriteAnimation(AnimationName.SitDown_Behind, 6, 6, 2, 7, 20));
            animations.Add(new SpriteAnimation(AnimationName.Turn_Behind_To_Front, 1, 12, 8, 12, 15));
            animations.Add(new SpriteAnimation(AnimationName.Turn_Front_To_Behind, 3, 13, 1, 14, 15));

            return animations;
        }

        protected override void UpdateAnimation(float elapsedTimeInMSec)
        {
            //KeyboardState keyState = Keyboard.GetState();
            //if (keyState.GetPressedKeys().Contains(Keys.A) && !Old_KeyState.GetPressedKeys().Contains(Keys.A))
            //{
            //    Animation.SetImidiateAnimation(AnimationName.Turn_Front_To_Behind);
            //    Animation.SetDefaultAnimation(AnimationName.Stand_Behind);
            //}

            //if (keyState.GetPressedKeys().Contains(Keys.D) && !Old_KeyState.GetPressedKeys().Contains(Keys.D))
            //{
            //    Animation.SetImidiateAnimation(AnimationName.Turn_Behind_To_Front);
            //    Animation.SetDefaultAnimation(AnimationName.Stand);
            //}

            //if (keyState.GetPressedKeys().Contains(Keys.S) && !Old_KeyState.GetPressedKeys().Contains(Keys.S))
            //{
            //    Animation.SetImidiateAnimation(AnimationName.SitDown_Behind);
            //    Animation.SetDefaultAnimation(AnimationName.Sitting_Behind);
            //}


            //if (keyState.GetPressedKeys().Contains(Keys.W) && !Old_KeyState.GetPressedKeys().Contains(Keys.W))
            //{
            //    Animation.SetImidiateAnimation(AnimationName.StandUp_Behind);
            //    Animation.SetDefaultAnimation(AnimationName.Stand_Behind);
            //}


            //Old_KeyState = keyState;

            base.UpdateAnimation(elapsedTimeInMSec);
        }


        public void Turn()
        {
            Animation.SetImidiateAnimation(AnimationName.Turn_Front_To_Behind);
            Animation.SetDefaultAnimation(AnimationName.Stand_Behind);
            Console.WriteLine("keep standing");
        }

        public void TurnBack()
        {
            Animation.SetImidiateAnimation(AnimationName.Turn_Behind_To_Front);
            Animation.SetDefaultAnimation(AnimationName.Stand);
            Console.WriteLine("keep looking");
        }

        public void TurnBackAndWave()
        {
            Animation.SetImidiateAnimation(AnimationName.Turn_Behind_To_Front);
            Animation.SetDefaultAnimation(AnimationName.Wave);
            Console.WriteLine("keep looking");
        }

        public void StartChangeFromBehind()
        {
            Animation.SetImidiateAnimation(AnimationName.Turn_Behind_To_Front);
            Animation.SetNextAnimation(AnimationName.StartChange);
            Animation.SetDefaultAnimation(AnimationName.ReadyToChange);
            Console.WriteLine("ready to change");
        }

        public void StartChange()
        {
            Animation.SetImidiateAnimation(AnimationName.StartChange);
            Animation.SetDefaultAnimation(AnimationName.ReadyToChange);
            Console.WriteLine("ready to change");
        }

        public void StopChangeAndTurn()
        {
            Animation.SetImidiateAnimation(AnimationName.Turn_Front_To_Behind);
            Animation.SetDefaultAnimation(AnimationName.Stand_Behind);
            Console.WriteLine("keep stand");
        }

        public void StopChange()
        {
            Animation.SetImidiateAnimation(AnimationName.Stand);
        }

        public void DoAChange()
        {
            Console.WriteLine("wechsel");
            Animation.SetImidiateAnimation(AnimationName.Change);
        }

        public void Walk()
        {
            Animation.SetImidiateAnimation(AnimationName.Walk_Behind);
            Animation.SetDefaultAnimation(AnimationName.Walk_Behind);
            Console.WriteLine("keep walking");
        }

        public void Run()
        {
            Animation.SetImidiateAnimation(AnimationName.Run_Behind);
            Animation.SetDefaultAnimation(AnimationName.Run_Behind);
            Console.WriteLine("keep running");
        }

        public void StopWalking()
        {
            Animation.SetDefaultAnimation(AnimationName.Stand_Behind);
        }

        public void SitDown()
        {
            Animation.SetImidiateAnimation(AnimationName.SitDown_Behind);
            Animation.SetDefaultAnimation(AnimationName.Sitting_Behind);
            Console.WriteLine("keep sitting");
        }

        public void StandUp()
        {
            Animation.SetImidiateAnimation(AnimationName.StandUp_Behind);
            Animation.SetDefaultAnimation(AnimationName.Stand_Behind);
            Console.WriteLine("keep standing");
        }
    }
}
