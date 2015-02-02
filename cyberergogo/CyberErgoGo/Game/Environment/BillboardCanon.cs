using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    enum CanonFeature
    {
        FirstCanon,
        BossCanon,
        Default
    }

    enum CanonState
    {
        Destroyed,
        Undestroyed,
        Next
    }

    class BillboardCanon:AnimatedBillboard
    {
        

        public int Index;

        public string Message;

        public int TimeMin = 0;
        public int TimeSec = 0;
        public int TimeMSec = 0;

        public float BestTime;


        public CanonState State = CanonState.Undestroyed;

        public CanonFeature Feature;

        public Vector3 Min = Vector3.Zero;

        public Vector3 Max = Vector3.Zero;

        public Quaternion Orientation;

        public bool Firing = false;

        private int FirePS = 10;

        private float GameTime = 0;

        public Billboard Bullet;

        const int RandTranslation = 10;

        const int BulletFlyDistance = 100;
        const int BulletFlyTimeInSec = 1;

        public BillboardCanon(int height, Vector3 pos, Quaternion orientation)
            : base(height, RandomLeftRightTranslation(pos), "Canon", "canon", SetUpAnimationTexture())
        {
            Orientation = orientation;
            FirePS -= Util.GetInstance().GetRandomNumber(5);
            GameTime += Util.GetInstance().GetRandomNumber(5) * 1000;
            Bullet = new Billboard(1, WorldPosition, "Canon", "bullet"); 
        }

        private static Vector3 RandomLeftRightTranslation(Vector3 orgPos)
        {
            return orgPos;
           // return orgPos - new Vector3(Util.GetInstance().GetRandomNumber(RandTranslation) - RandTranslation / 2, 0, Util.GetInstance().GetRandomNumber(RandTranslation) - RandTranslation / 2);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            Bullet.LoadContent();
            Width *= 2;
        }

        private static AnimationTexture SetUpAnimationTexture()
        {
            return new AnimationTexture(6, 6, SetUpAnimationList());
        }

        private static List<SpriteAnimation> SetUpAnimationList()
        {
            List<SpriteAnimation> animations = new List<SpriteAnimation>();

            animations.Add(new SpriteAnimation(AnimationName.Stand, 0, 0, 0, 1, 3));
            animations.Add(new SpriteAnimation(AnimationName.Firing, 1, 1, 0, 2, 10));
            animations.Add(new SpriteAnimation(AnimationName.Destroyed, 0, 5, 2, 5, 3));
            animations.Add(new SpriteAnimation(AnimationName.Exploding, 0, 4, 4, 4, 10));
            return animations;
        }

        protected override void UpdateAnimation(float elapsedTimeInMSec)
        {
            GameTime += elapsedTimeInMSec;
            
            if (!Firing && State != CanonState.Destroyed)
            {
                //there is no Bullet flying
                if (GameTime >= FirePS * 1000)
                {
                    Firing = true;
                    GameTime = GameTime % (FirePS * 1000);
                    Bullet.ChangeWorldPosition(WorldPosition);
                    Animation.SetImidiateAnimation(AnimationName.Firing);
                    MovingObjectCondition condition = (MovingObjectCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.MovingObjectCondition);
                    float minDistanceToSound = 100;
                    float currentDistance = (condition.Position - WorldPosition).Length() ;
                    if (currentDistance < minDistanceToSound)
                    {
                        float volumePercent = currentDistance / minDistanceToSound;
                        Util.GetInstance().SoundManager.PlayCanonFire(1-(volumePercent * volumePercent));
                    }
                }
            }
            else
            {
                if (GameTime >= BulletFlyTimeInSec * 1000)
                {
                    //Bullet flew long enough
                    Firing = false;
                    GameTime = GameTime % (BulletFlyTimeInSec * 1000);
                }
                else
                {
                    float flyTimeRatio = GameTime / (float)(BulletFlyTimeInSec * 1000);
                    Bullet.ChangeWorldPosition(WorldPosition + new Vector3(0, (float)BulletFlyDistance * flyTimeRatio, 0));
                }
            }

            base.UpdateAnimation(elapsedTimeInMSec);
        }

        public void Explode()
        {
            Animation.SetDefaultAnimation(AnimationName.Destroyed);
            Animation.SetImidiateAnimation(AnimationName.Exploding);
        }

        public void Restore()
        {
            Animation.SetImidiateAnimation(AnimationName.Stand);
            Animation.SetDefaultAnimation(AnimationName.Stand);
        }

        public void GenerateMinMax()
        {
            Vector3 goUp = Vector3.Up * Height*2;
            Vector3 toLeft = Vector3.Transform(goUp, Matrix.CreateFromYawPitchRoll(0, 0, MathHelper.ToRadians(-90)));
            Vector3 toRight = Vector3.Transform(goUp, Matrix.CreateFromYawPitchRoll(0, 0, MathHelper.ToRadians(90)));

            Vector3 tranlationLeft = Vector3.Transform(toLeft, Matrix.CreateFromQuaternion(Orientation));
            Vector3 tranlationRight = Vector3.Transform(toRight, Matrix.CreateFromQuaternion(Orientation));
            Vector3 stepForward = Vector3.Normalize(Vector3.Cross(tranlationLeft, goUp));
            Max = WorldPosition + Translation + tranlationLeft + goUp + stepForward * 2;
            Min = WorldPosition + Translation + tranlationRight - goUp;

            if (Min.X > Max.X)
            {
                float x = Max.X;
                Max.X = Min.X;
                Min.X = x;
            }
            if (Min.Y > Max.Y)
            {
                float y = Max.Y;
                Max.Y = Min.Y;
                Min.Y = y;
            }
            if (Min.Z > Max.Z)
            {
                float z = Max.Z;
                Max.Z = Min.Z;
                Min.Z = z;
            }
        }

        
    }
}
