using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class NotPhysical:IPhysicalRepresentation
    {
        Vector3 Position;
        Quaternion Rotation;
        float Scale;
        float Mass;
        Vector3 UpVector = Vector3.Up;

        public NotPhysical()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = 1;
            Mass = 1;
        }

        public void Translate(Microsoft.Xna.Framework.Vector3 translation)
        {
            Position += translation;
        }

        public void TranslateAbsolute(Microsoft.Xna.Framework.Vector3 translation)
        {
            Position = translation;
        }

        public void RollForward(float degree)
        { 
        }
        public Vector3 GetUpVector()
        {
            return UpVector;
        }

        public void Rotate(Microsoft.Xna.Framework.Quaternion rotation)
        {
            Rotation *= rotation;
        }

        public void RotateAbsolute(Microsoft.Xna.Framework.Quaternion rotation)
        {
            Rotation = rotation;
        }
        public void SetUpVector(Vector3 newUp)
        {
            UpVector = newUp;
        }

        public Quaternion GetMovingOrientation()
        {
            return Quaternion.Identity;
        }

        public void Push(Microsoft.Xna.Framework.Vector3 veolation)
        {
            Position += veolation;
        }

        public void SpeedUp(float speed)
        {
            Position += Vector3.Transform(Vector3.Forward, Matrix.CreateFromQuaternion(Rotation)) * speed;
        }

        public void Steer(float angle)
        {
            Rotation *= Quaternion.CreateFromYawPitchRoll(angle, 0, 0);
        }

        public void WeightDown(float mass)
        {
            Mass += mass;
        }

        public void SetMass(float mass)
        {
            Mass = mass;
        }

        public void MoveForward()
        {
            Position += Vector3.Transform(Vector3.Forward, Matrix.CreateFromQuaternion(Rotation));
        }

        public void MoveBack()
        {
            Position += Vector3.Transform(Vector3.Backward, Matrix.CreateFromQuaternion(Rotation));
        }

        public void MoveRight()
        {
            Position += Vector3.Transform(Vector3.Right, Matrix.CreateFromQuaternion(Rotation));
        }

        public void MoveLeft()
        {
            Position += Vector3.Transform(Vector3.Left, Matrix.CreateFromQuaternion(Rotation));
        }

        public void SetAbsoluteSize(Microsoft.Xna.Framework.BoundingSphere bounding)
        {
            Position = bounding.Center;
            Scale = bounding.Radius * 2;
        }

        public Microsoft.Xna.Framework.Vector3 GetPosition()
        {
            return Position;
        }

        public Microsoft.Xna.Framework.Quaternion GetRotation()
        {
            return Rotation;
        }

        public Microsoft.Xna.Framework.Matrix GetWorldTransform()
        {
            return Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
        }

        public float GetRadius()
        { 
            return Scale/2f;
        }

        public BEPUphysics.ISpaceObject GetBEPUEntity()
        {
            Console.WriteLine("This PhysicalRepresentation has no BEPUEntity");
            return null;
        }

        public void AddToCollisionChecker()
        {
        }

        public void RemoveFromCollisionChecker()
        {
        }
    }
}
