using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    enum AnimationName
    {
        Stand,
        Change,
        StartChange,
        EndChange,
        HardCollision,
        SoftCollision,
        StandUp_Behind,
        SitDown_Behind,
        Stand_Behind,
        Walk_Behind,
        Run_Behind,
        Sitting_Behind,
        Turn_Behind_To_Front,
        Turn_Front_To_Behind,
        Wave,
        ReadyToChange,
        Firing,
        Destroyed,
        Exploding
    }

    class SpriteAnimation 
    {
        public AnimationName Name = AnimationName.Stand;
        int SpriteStartX = 0;
        int SpriteStartY = 0;
        int SpriteEndX = 0;
        int SpriteEndY = 0;
        public float FPS = 1;
        int NumberOfFrames = 1;
        Vector2 TextureSpriteDim = Vector2.Zero;
        private AnimationName animationName;
        private int p;
        private int p_2;
        private int p_3;
        private int p_4;
        private int p_5;
        

        public SpriteAnimation(AnimationName name, int startX, int startY, int endX, int endY, float fps)
        {
            Name = name;
            SpriteStartX = startX;
            SpriteStartY = startY;
            SpriteEndX = endX;
            SpriteEndY = endY;
            FPS = fps;
        }

        public void SetSpriteFrameDim(Vector2 spriteDim)
        {
            TextureSpriteDim = spriteDim;
        }

        public void CalculateNumberOfFrames()
        {
            int rows = SpriteEndY - SpriteStartY + 1;
            NumberOfFrames = rows * (int)TextureSpriteDim.X;
            NumberOfFrames -= SpriteStartX;
            NumberOfFrames -= (int)TextureSpriteDim.X - SpriteEndX;
            NumberOfFrames += 1;
        }

        public bool AnimationFinished(int currentFrameNumber)
        {
            if (NumberOfFrames-1 < currentFrameNumber)
                return true;
            else
                return false;
        }

        public Vector2 GetCurrFrameCoord(int currentFrameNumber)
        {
            Vector2 currFrameCoord = new Vector2(SpriteStartX, SpriteStartY);
            currFrameCoord.X = (currentFrameNumber+SpriteStartX) % TextureSpriteDim.X;
            currFrameCoord.Y += (int)((currentFrameNumber + SpriteStartX) / TextureSpriteDim.X);
            return currFrameCoord;
        }
    }

    class AnimationTexture 
    {
        Vector2 SpriteFrameDimension;
        Vector2 CurrentSpritePosition = Vector2.Zero;

        List<SpriteAnimation> AllAnimations;
        protected AnimationName DefaultAnimation;

        SpriteAnimation CurrentAnimation {
            get {
                AnimationName CurrentAnimationName = AnimationStack.First();
                foreach (SpriteAnimation animation in AllAnimations)
                    if (animation.Name == CurrentAnimationName)
                        return animation;
                //Console.WriteLine("Animation " + CurrentAnimationName + " wasn't found.");
                return AllAnimations.First();
            }
        }
        List<AnimationName> AnimationStack;
        float OldTime = 0;
        int CurrentFrameInAnimation = 0;

        public AnimationTexture(int countX, int countY, List<SpriteAnimation> allAnimations)
        {
            SpriteFrameDimension = new Vector2(countX,countY);
            AllAnimations = allAnimations;
            AnimationStack = new List<AnimationName>();

            DefaultAnimation = AllAnimations.First().Name;
            AnimationStack.Add(DefaultAnimation);
            foreach (SpriteAnimation animation in AllAnimations)
            {
                animation.SetSpriteFrameDim(SpriteFrameDimension);
                animation.CalculateNumberOfFrames();
            }
        }

        public void UpdateAnimation(float elapsedTimeInMSec)
        {
            if (AnimationStack.Count == 0) AnimationStack.Add(DefaultAnimation);

            OldTime += elapsedTimeInMSec;
            CurrentFrameInAnimation += NextFrames();
            if (CurrentAnimation.AnimationFinished(CurrentFrameInAnimation))
            {
                //animation finished
                //start the next animation
                CurrentFrameInAnimation = 0;
                //delete the current animation from stack
                AnimationStack.Remove(AnimationStack.First());
                //check if there is still an animation to show
                //if there is no animation add the default animation to show
                if (AnimationStack.Count == 0) AnimationStack.Add(DefaultAnimation);
            }
           
            //show the current animation
            CurrentSpritePosition = CurrentAnimation.GetCurrFrameCoord(CurrentFrameInAnimation);
        }

        private int NextFrames()
        {
            int numberOfFrames = 0;
            numberOfFrames = (int)(OldTime / (1000/CurrentAnimation.FPS));
            OldTime = OldTime % (1000 / CurrentAnimation.FPS);
            //Console.WriteLine(numberOfFrames);
            return numberOfFrames;
        }

        public Vector2 GetTexcoordOfSprite()
        {
            Vector2 frameCoord = CurrentAnimation.GetCurrFrameCoord(CurrentFrameInAnimation);
            return new Vector2(frameCoord.X/SpriteFrameDimension.X, frameCoord.Y/SpriteFrameDimension.Y);
        }

        public Vector2 GetSpriteWidthAndHeight(int textureWidth, int textureHeight)
        {
            return new Vector2((float)textureWidth / SpriteFrameDimension.X, (float)textureHeight / SpriteFrameDimension.Y);
        }

        public void SetDefaultAnimation(AnimationName defaultAni)
        {
            DefaultAnimation = defaultAni;
        }

        public void SetNextAnimation(AnimationName nextAni)
        {
            AnimationStack.Add(nextAni);
        }

        public void SetImidiateAnimation(AnimationName imidiateAni)
        {
            CurrentFrameInAnimation = 0;
            OldTime = 0;
            AnimationStack.Clear();
            AnimationStack.Add(imidiateAni);
        }

        public Vector2 GetSpriteFrameDimension()
        {
            return SpriteFrameDimension;
        }
    }

    class AnimatedBillboard:Billboard
    {

        protected AnimationTexture Animation;

        public AnimatedBillboard(float height, Vector3 pos, String textureFolder, String textureName, AnimationTexture animation)
        {
            TextureFolder = textureFolder;
            TextureName = textureName;

            WorldPosition = pos;
            Height = height;

            Animation = animation;
            IsAnimated = true;

            if (Util.GetInstance().GetRandomNumber(2) == 1)
                Flip = true;

            SetUpBuffer();
        }

        public override void LoadContent()
        {
            base.LoadContent();
            float ratio = (float)(Texture.Width/Animation.GetSpriteFrameDimension().X) / ((float)Texture.Height/Animation.GetSpriteFrameDimension().Y);
            Width = (int)(Height * ratio);
        }

        public void Update(float elapsedTimeInMSec)
        {
            UpdateAnimation(elapsedTimeInMSec);
        }

        protected virtual void UpdateAnimation(float elapsedTimeInMSec)
        {
            Animation.UpdateAnimation(elapsedTimeInMSec);
        }

        public Vector2 GetCurrentSpriteLocation()
        {
            return Animation.GetTexcoordOfSprite();
        }

        public Vector2 GetRelativeSpriteDimension()
        {
            //Vector2 spriteDim = Animation.GetSpriteWidthAndHeight(Texture.Width,Texture.Height);
            return new Vector2((float)1 / (float)Animation.GetSpriteFrameDimension().X, (float)1 / (float)Animation.GetSpriteFrameDimension().Y);
        }
    }
}
