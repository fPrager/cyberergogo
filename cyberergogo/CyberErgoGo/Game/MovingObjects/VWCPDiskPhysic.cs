using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics;

namespace CyberErgoGo
{
    class VWCPDiskPhysic:IPhysicalRepresentation
    {
        Sphere Object;
        Vector3 OldTranslation;
        Quaternion OriginalOrientation = Quaternion.Identity;
        Quaternion AdditionalRotation = Quaternion.Identity;
        float MovingMassFactor = 0.1f;
        float MovingRadiusFactor = 0.1f;

        public VWCPDiskPhysic(float radius, Vector3 position, float mass)
        {
            if(mass==0)
                Object = new Sphere(position, radius);
            else
                Object = new Sphere(position, radius, mass);
            Object.Material = new BEPUphysics.Materials.Material(50, 50, 1);
            OldTranslation = Vector3.Zero; 
        }
        public Vector3 GetUpVector()
        {
            return Vector3.Up;
        }
        public void SetUpVector(Vector3 newUp)
        {

        }

        #region IPhysicalRepresentation Member

        public void Translate(Vector3 translation)
        {
            if (Object.IsDynamic)
            {
                translation.Y = 0;
                translation *= Object.Mass/10;
                Object.LinearMomentum += translation * OverallSetting.SizeFactor;
            }
            else
                Object.LinearMomentum = translation * Object.Mass * OverallSetting.SizeFactor;
        }

        public void Rotate(Quaternion rotation)
        {
            //AdditionalRotation *= rotation;
        }

        public void Push(Vector3 veolation)
        {
        }

        public void SpeedUp(float speed)
        {
            Object.LinearMomentum += Matrix.CreateFromQuaternion(OriginalOrientation * AdditionalRotation).Backward * (speed * MovingMassFactor) * (Object.Radius * MovingRadiusFactor);
        }

        public void Steer(float angle)
        {
            AdditionalRotation *= Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(-angle/20),0,0);
        }

        public void WeightDown(float mass)
        {
        }

        public void SetMass(float mass)
        {
        }

        public void SetAbsoluteSize(BoundingSphere bounding)
        {
            Object.Radius = bounding.Radius;
            Object.Position = bounding.Center;
            Object.LinearMomentum = Vector3.Zero;
            Object.AngularMomentum = Vector3.Zero;
        }


        public Vector3 GetPosition()
        {
            return Object.Position;
        }

        public Quaternion GetRotation()
        {
            return OriginalOrientation;
        }

        public Matrix GetWorldTransform()
        {
            return Matrix.CreateFromQuaternion(OriginalOrientation*AdditionalRotation) * Matrix.CreateTranslation(Object.Position);
        }

        public ISpaceObject GetBEPUEntity()
        {
            return Object;
        }

        public void TranslateAbsolute(Vector3 translation)
        {
            Object.Position = translation + new Vector3(0, 10, 0);
            Object.LinearMomentum = Vector3.Zero;
            Object.AngularMomentum = Vector3.Zero;
        }

        public void RotateAbsolute(Quaternion rotation)
        {
            OriginalOrientation = rotation;
            AdditionalRotation = Quaternion.Identity;
            Object.Orientation = OriginalOrientation;
        }

        public Quaternion GetMovingOrientation()
        {
            return OriginalOrientation * AdditionalRotation;
        }

        public void AddToCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.AddObject(Object, true);
        }

        public void RemoveFromCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.RemoveObject(Object);
        }

        public void MoveForward()
        {
           
        }

        public void MoveBack()
        {
           
        }

        public void MoveRight()
        {

        }
        public void RollForward(float degree)
        {
        }

        public void MoveLeft()
        {
           
        }

        public float GetRadius()
        {
            return Object.Radius;
        }

        public Vector3 GetLinearMementum()
        {
            return Object.LinearMomentum;
        }

        #endregion
    }
}
