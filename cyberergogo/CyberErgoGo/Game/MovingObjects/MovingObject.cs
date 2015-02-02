using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    enum ControlerStyle 
    {
        KeyboardControled,
        BikeControled,
        MouseAndKeyboardControled
    }

    abstract class MovingObject:PhysicalObject
    {
        protected MovingObjectCondition MyCondition;
        protected Matrix WorldMatrix = Matrix.Identity;
        protected MovingObject(IPhysicalRepresentation representation, Behaviour movingBehaviour) : base(representation, movingBehaviour) 
        {
            MyCondition = new MovingObjectCondition();
            ConditionHandler.GetInstance().SetCondition(MyCondition);
            MyCondition.Position = Vector3.Zero;
            MyCondition.Bounding = new BoundingSphere(Vector3.Zero, 1f);
        }

        public virtual void Update(float time) 
        {
            base.Update(time, 1f);
        }

        public virtual void Update(float time, float motionFactor)
        {
            base.Update(time, motionFactor);
        }

        public abstract void Draw(Effect effect, Matrix projectionMatrix, Matrix viewMatrix);
        public abstract void Load(Effect effect);
        public abstract void Load();
        public abstract void Unload();
        public abstract void UpdateMeshEffect(Effect effect);


        public void SetScaleFactor(float absoluteScale)
        {
            MovingBehaviour.ExternalScaling(absoluteScale);
            MyCondition.ScaleFactor = absoluteScale;
            MyCondition.ConditionHasChanged();
        }
        public void Rotate(Quaternion relativeRotation)
        {
            MovingBehaviour.ExternalRotation(MovingBehaviour.GetRotation() * relativeRotation);
        }
        public void SetRotation(Quaternion absoluteRotation)
        {
            MovingBehaviour.ExternalRotation(absoluteRotation);
        }
        public void Translate(Vector3 relativeTranslate) 
        {
            MovingBehaviour.ExternalTranslation(MovingBehaviour.GetPosition() + relativeTranslate);
        }
        public void SetPosition(Vector3 absolutePosition)
        {
            MovingBehaviour.ExternalTranslation(absolutePosition);
        }

        public virtual void ChangePhysic()
        { 
        
        }

        public virtual void StartPhysicChanging()
        {

        }

        public virtual void StopPhysicChanging()
        {

        }

        public void Scale(float relativeScale)
        {
            MyCondition.ScaleFactor += relativeScale;
            MovingBehaviour.ExternalScaling(MyCondition.ScaleFactor);
            MyCondition.ConditionHasChanged();
        }
        public void SetTransformationRelative(float scale, Quaternion rotation, Vector3 translation)
        {
            Scale(scale);
            Rotate(rotation);
            Translate(translation);
        }
        public void SetTransformationAbsolute(float scale, Quaternion rotation, Vector3 translation)
        {
            MovingBehaviour.ExternalRotation(rotation);
            MovingBehaviour.ExternalScaling(scale);
            MovingBehaviour.ExternalTranslation(translation);
        }
        public void SetBounding(BoundingSphere wantedBounding)
        {
            float scaleFactor = wantedBounding.Radius;
            SetScaleFactor(scaleFactor);
            SetPosition(wantedBounding.Center);
        }
        public Matrix GetWorldMatrix()
        {
            return WorldMatrix;
        }

        public void SetWorldMatrix(Matrix worldMatrix)
        {
            WorldMatrix = worldMatrix;
        }

        public virtual void ChangePhysicTo(int indexOfPhysic)
        {
            Console.WriteLine("not supported feature");
        }
    }
}
