using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using BEPUphysics;

namespace CyberErgoGo
{
    class FlyingObject:IPhysicalRepresentation
    {
        Sphere Object;
        Vector3 OldTranslation;
        Quaternion OriginalOrientation = Quaternion.Identity;

        public FlyingObject(float radius, Vector3 position, int mass)
        {
            if(mass==0)
                Object = new Sphere(position, radius);
            else
                Object = new Sphere(position, radius, mass);
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
            //if (Object.IsDynamic)
            //{
            translation.Y = 0;
            translation *= Object.Mass;
            Object.LinearMomentum += translation;
            //}
            //else
            //Object.LinearMomentum = translation * Object.Mass;
        }

        public void Rotate(Quaternion rotation)
        {
            OriginalOrientation = rotation;
        }

        public void Push(Vector3 veolation)
        {
        }

        public void SpeedUp(float speed)
        {
        }

        public void Steer(float angle)
        {
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
        }


        public Vector3 GetPosition()
        {
            return Object.Position;
        }

        public Quaternion GetRotation()
        {
            return OriginalOrientation;
        }

        public Quaternion GetMovingOrientation()
        {
            return Quaternion.Identity;
        }

        public Matrix GetWorldTransform()
        {
            return Matrix.CreateFromQuaternion(OriginalOrientation) * Matrix.CreateTranslation(Object.Position);
        }

        public ISpaceObject GetBEPUEntity()
        {
            return Object;
        }

        public void TranslateAbsolute(Vector3 translation)
        {
            Object.Position = translation;
            Object.LinearMomentum = Vector3.Zero;
        }

        public void RotateAbsolute(Quaternion rotation)
        {
            OriginalOrientation = rotation;
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

        #endregion
    }
}
