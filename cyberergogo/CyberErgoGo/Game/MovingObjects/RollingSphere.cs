using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BikeControls;

namespace CyberErgoGo
{
    class RollingSphere:IPhysicalRepresentation
    {
        Sphere Object;
        Vector3 OldTranslation;
        Quaternion ForwardOrentation;
        float RoundsToRoll = 0;

        public RollingSphere(float radius, Vector3 position, int mass)
        {
            if(mass==0)
                Object = new Sphere(position, radius);
            else
                Object = new Sphere(position, radius, mass);
            OldTranslation = Vector3.Zero; 
        }

        #region IPhysicalRepresentation Member

        public void Translate(Vector3 translation)
        {
            if (!Bike.PluggedIn)
            {
                if (Object.IsDynamic)
                {
                    translation.Y = 0;
                    translation *= Object.Mass;
                    Object.LinearMomentum += translation;
                }
                else
                    Object.Position += translation;
            }
        }

        public void Rotate(Quaternion rotation)
        {
            ForwardOrentation *= rotation;
        }

        public void Push(Vector3 veolation)
        {
        }

        public void SpeedUp(float speed)
        {
            //if (Bike.PluggedIn)
            //{
            //    if (speed != 0)
            //    {
            //        Object.Orientation = Quaternion.Lerp(ForwardOrentation * Quaternion.CreateFromYawPitchRoll(0, , 0),Object.Orientation, 0.1f);
            //    }
            //}
        }

        public void RollForward(float degree)
        {
            RoundsToRoll += 1;
            Object.Orientation = ForwardOrentation * Quaternion.CreateFromYawPitchRoll(0, MathHelper.ToRadians(RoundsToRoll), 0);
        }

        public void Steer(float angle)
        {
        }

        public void WeightDown(float mass)
        {
            throw new NotImplementedException();
        }

        public void SetMass(float mass)
        {
            throw new NotImplementedException();
        }

        public void SetAbsoluteSize(BoundingSphere bounding)
        {
            Object.Radius = bounding.Radius;
            Object.Position = bounding.Center;
        }


        public Vector3 GetPosition()
        {
            return Object.Position;
        }

        public Quaternion GetRotation()
        {
            return Object.Orientation;
        }

        public Matrix GetWorldTransform()
        {
            return Object.WorldTransform;
        }

        public ISpaceObject GetBEPUEntity()
        {
            return Object;
        }

        public void TranslateAbsolute(Vector3 translation)
        {
            Object.Position = translation;
        }

        public void RotateAbsolute(Quaternion rotation)
        {
            ForwardOrentation = rotation;
        }

        public PhysicalObject GetPhysicalObject()
        {
            throw new NotImplementedException();
        }

        public void SetPhysicalObject(PhysicalObject physicalObject)
        {
            throw new NotImplementedException();
        }

        public void AddToCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.AddObject(Object);
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
