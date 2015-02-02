using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Vehicle;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;

namespace CyberErgoGo
{
    class RollingWheel:IPhysicalRepresentation
    {
        Wheel Object;
        Vehicle VehicleOfTheWheel;

        public RollingWheel(float radius, Vector3 position, int mass)
        {
            Matrix wheelGraphicRotation = Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);
            Object = new Wheel( new RaycastWheelShape(radius, wheelGraphicRotation),
                                 new WheelSuspension(2000, 100f, Vector3.Down, .8f, new Vector3(-1.1f, 0, 1.8f)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(1.5f, 2, .02f),
                                 new WheelSlidingFriction(4, 5));
            var bodies = new List<CompoundShapeEntry>()
            {
                new CompoundShapeEntry(new BoxShape(1f, 1f, 1f),new Vector3(0, 0, 0),1),
            };
            var body = new CompoundBody(bodies, 1);
            body.CollisionInformation.LocalPosition = new Vector3(0, .5f, 0);
            body.Position = position + new Vector3(0,-10,0);
            body.Mass = mass;
            VehicleOfTheWheel = new Vehicle(body);
            VehicleOfTheWheel.AddWheel(Object); 
        }

        #region IPhysicalRepresentation Member

        public void Translate(Vector3 translation)
        {
            //translation.Y = 0;
            Object.DrivingMotor.TargetSpeed = translation.Length() * 400;
        }

        public void Rotate(Quaternion rotation)
        {
            throw new NotImplementedException();
        }

        public void Push(Vector3 veolation)
        {
            throw new NotImplementedException();
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

        public void SetAbsoluteSize(BoundingSphere bounding)
        {
        }


        public Vector3 GetPosition()
        {
            return Object.Shape.WorldTransform.Translation;
        }

        public Quaternion GetRotation()
        {
            return Quaternion.CreateFromRotationMatrix(Object.Shape.WorldTransform);
        }

        public Matrix GetWorldTransform()
        {
            return Object.Shape.WorldTransform;
        }

        public ISpaceObject GetBEPUEntity()
        {
            return VehicleOfTheWheel;
        }

        public void TranslateAbsolute(Vector3 translation)
        {
            VehicleOfTheWheel.Body.Position = translation;
        }

        public void RotateAbsolute(Quaternion rotation)
        {
            Object.Shape.SteeringAngle = rotation.X;
        }

        public void AddToCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.AddObject(VehicleOfTheWheel);
        }

        public void RemoveFromCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.RemoveObject(VehicleOfTheWheel);
        }

        public void RollForward(float degree)
        {
        }

        public void MoveForward()
        {
            throw new NotImplementedException();
        }

        public void MoveBack()
        {
            throw new NotImplementedException();
        }

        public void MoveRight()
        {
            throw new NotImplementedException();
        }

        public float GetRadius()
        {
            return 0;
        }

        public void MoveLeft()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
