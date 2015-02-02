using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities.Prefabs;

namespace CyberErgoGo
{
    class VWCPSpherePhysic:IPhysicalRepresentation
    {
        #region IPhysicalRepresentation Member

        Sphere Object;
        Quaternion MovingOrientation;
        float MovingMassFactor = 30;
        float MovingRadiusFactor = 1;
        float RotationSpeedFactor = 1;

        public VWCPSpherePhysic(float radius, Vector3 position, float mass)
        {
            if (mass == 0)
                Object = new Sphere(position, radius);
            else
                Object = new Sphere(position, radius, mass);
            Object.Material = new BEPUphysics.Materials.Material(0.6f,0.3f, 1);
            MovingRadiusFactor = radius;
        }

        public void Translate(Microsoft.Xna.Framework.Vector3 translation)
        {
        }

        public void TranslateAbsolute(Microsoft.Xna.Framework.Vector3 translation)
        {
            Object.Position = translation + new Vector3(0, 10, 0);
            Object.LinearMomentum = Vector3.Zero;
            Object.AngularMomentum = Vector3.Zero;
        }

        public void Rotate(Microsoft.Xna.Framework.Quaternion rotation)
        {
            //MovingOrientation = rotation;
        }

        public void RotateAbsolute(Microsoft.Xna.Framework.Quaternion rotation)
        {
            MovingOrientation = rotation;
            Object.LinearMomentum = Vector3.Zero;
            Object.AngularMomentum = Vector3.Zero;
        }

        public Quaternion GetMovingOrientation()
        {
            return MovingOrientation;
        }


        public void Push(Microsoft.Xna.Framework.Vector3 veolation)
        {
        }

        public void SpeedUp(float speed)
        {
            Object.AngularMomentum += Matrix.CreateFromQuaternion(MovingOrientation).Right * speed * RotationSpeedFactor * OverallSetting.SizeFactor;
        }

        public void Steer(float angle)
        {
            MovingOrientation *= Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(-angle / 20), 0, 0);
        }

        public void WeightDown(float mass)
        {
            Object.Mass += mass;
        }

        public void SetMass(float mass)
        {
            Object.Mass = mass;
        }

        public void MoveForward()
        {
            Vector3 mementumVector = Vector3.Cross(Matrix.CreateFromQuaternion(MovingOrientation).Forward, Vector3.Up) * (Object.Mass * MovingMassFactor) * (Object.Radius * MovingRadiusFactor);
            Object.AngularMomentum = mementumVector * OverallSetting.SizeFactor;
        }

        public void MoveBack()
        {
            Vector3 mementumVector = Vector3.Cross(Matrix.CreateFromQuaternion(MovingOrientation).Forward, Vector3.Up) * (Object.Mass * MovingMassFactor) * (Object.Radius * MovingRadiusFactor);
            Object.AngularMomentum = -mementumVector;
        }

        public void MoveRight()
        {
            Vector3 mementumVector = -Matrix.CreateFromQuaternion(MovingOrientation).Forward * (Object.Mass * MovingMassFactor) * (Object.Radius * MovingRadiusFactor);
            Object.AngularMomentum = mementumVector;
        }

        public void MoveLeft()
        {
            Vector3 mementumVector = Matrix.CreateFromQuaternion(MovingOrientation).Forward * (Object.Mass * MovingMassFactor) * (Object.Radius * MovingRadiusFactor);
            Object.AngularMomentum = mementumVector;
        }

        public void RollForward(float degree)
        {
            //angular mementum!
        }

        public void SetAbsoluteSize(Microsoft.Xna.Framework.BoundingSphere bounding)
        {
            Object.Position = bounding.Center;
            Object.Radius = bounding.Radius;
            Object.LinearMomentum = Vector3.Zero;
            Object.AngularMomentum = Vector3.Zero;
        }

        public float GetRadius()
        {
            return Object.Radius;
        }

        public Microsoft.Xna.Framework.Vector3 GetPosition()
        {
            return Object.Position;
        }

        public Microsoft.Xna.Framework.Quaternion GetRotation()
        {
            return Object.Orientation;
        }

        public Microsoft.Xna.Framework.Matrix GetWorldTransform()
        {
            return Object.WorldTransform;
        }

        public BEPUphysics.ISpaceObject GetBEPUEntity()
        {
            return null;
        }

        public void AddToCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.AddObject(Object, true);
        }

        public void RemoveFromCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.RemoveObject(Object);
        }

        #endregion
        public Vector3 GetUpVector()
        {
            return Vector3.Up;
        }
        public void SetUpVector(Vector3 newUp)
        {

        }
    }
}
