using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities.Prefabs;

namespace CyberErgoGo
{
    class CameraPhysic:IPhysicalRepresentation
    {
        #region IPhysicalRepresentation Member

        Vector3 Position;
        Quaternion Rotation;
        Sphere Object;

        public CameraPhysic(Vector3 position, Quaternion rotation)
        {
            Object = new Sphere(position, 5, 1);
            Object.Position = position;
            Object.Orientation = rotation;
            Rotation = rotation;
        }

        public void Translate(Microsoft.Xna.Framework.Vector3 translation)
        {
            Object.LinearVelocity = 10*translation;
        }

        public void TranslateAbsolute(Microsoft.Xna.Framework.Vector3 translation)
        {
            Object.Position = translation;
        }

        public void Rotate(Microsoft.Xna.Framework.Quaternion rotation)
        {
            Object.Orientation *= rotation;
            Rotation *= rotation;
        }

        public void RotateAbsolute(Microsoft.Xna.Framework.Quaternion rotation)
        {
            Object.Orientation = rotation;
            Rotation = rotation;
        }

        public void Push(Microsoft.Xna.Framework.Vector3 veolation)
        {
            //Position += veolation;
        }

        public void SpeedUp(float speed)
        {
            throw new NotImplementedException();
        }

        public void Steer(float angle)
        {
            throw new NotImplementedException();
        }

        public void WeightDown(float mass)
        {
            throw new NotImplementedException();
        }

        public void SetMass(float mass)
        {
            throw new NotImplementedException();
        }

        public void SetAbsoluteSize(Microsoft.Xna.Framework.BoundingSphere bounding)
        {
            throw new NotImplementedException();
        }

        public Microsoft.Xna.Framework.Vector3 GetPosition()
        {
            return Object.Position;
        }

        public Microsoft.Xna.Framework.Quaternion GetRotation()
        {
            return Rotation;
        }

        public Microsoft.Xna.Framework.Matrix GetWorldTransform()
        {
            return (Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position));
        }

        public BEPUphysics.ISpaceObject GetBEPUEntity()
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

        public void RollForward(float degree)
        {
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
