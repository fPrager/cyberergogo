using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Collidables;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Vehicle;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Constraints.TwoEntity.Motors;
using BEPUphysics.Constraints.SolverGroups;

namespace CyberErgoGo
{
    class VWCPWheelPhysic:IPhysicalRepresentation
    {
        //this version of the wheel is the combination of a motor and two objects
        #region IPhysicalRepresentation Member
        MobileMesh Wheel;
        
        Quaternion MovingOrentation = Quaternion.Identity;
        float Size = 1;
        float Mass = 1;
        float ModelToOne = 1;
        int ScaleWidth = 2;

        float MovingSizeFactor = 1;
        float MovingMassFactor = 1;
        float SteeringSpeed = 0.01f;


        Cylinder MotorWheel;
        Box MotorBase;
        RevoluteJoint MotorJoint;

        public VWCPWheelPhysic(float size, float mass)
        {
            //motor stuff
            MotorBase = new Box(Vector3.Zero, 0.1f, 0.1f, 0.1f,0.1f);
            MotorWheel = new Cylinder(MotorBase.Position, 1, size, 5);
            MotorWheel.Orientation *= Quaternion.CreateFromYawPitchRoll(0, 0, MathHelper.ToRadians(90));
            MotorJoint = new RevoluteJoint(null, MotorWheel, MotorWheel.Position, Vector3.Left);
            MotorJoint.Motor.IsActive = true;
            MotorJoint.Motor.Settings.VelocityMotor.GoalVelocity = 30;
            MotorJoint.Motor.Settings.MaximumForce = 300;
            
            Size = size;
            Mass = mass;
        }

        public void Translate(Microsoft.Xna.Framework.Vector3 translation)
        {
              }

        public void TranslateAbsolute(Microsoft.Xna.Framework.Vector3 translation)
        {
            MotorWheel.Position = translation;
            MotorBase.Position = translation;
        }

        public void Rotate(Microsoft.Xna.Framework.Quaternion rotation)
        {
         }

        public void RotateAbsolute(Microsoft.Xna.Framework.Quaternion rotation)
        {
         }

        public Quaternion GetMovingOrientation()
        {
            return MotorBase.Orientation;
        }


        public void Push(Microsoft.Xna.Framework.Vector3 veolation)
        {
        }

        public void SpeedUp(float speed)
        {
       }

        private float CurrentAngle = 0;
        private Vector2 PrevPosition = Vector2.Zero;
        private float BikeLength = 2;

        public void Steer(float angle)
        {
        }

        public void WeightDown(float mass)
        {
        }

        public void SetMass(float mass)
        {
        }

        float SpeedFactor = 0;

        private void MoveIt()
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

        public void RollForward(float degree)
        {
        }

        public void SetAbsoluteSize(Microsoft.Xna.Framework.BoundingSphere bounding)
        {
             RemoveFromCollisionChecker();
            Size = bounding.Radius;
            MovingOrentation = Quaternion.Identity;
 
            MotorBase = new Box(bounding.Center, 1, 1, 1,100);
            MotorWheel = new Cylinder(MotorBase.Position+new Vector3(0,0.8f,0), 1, 1, 5);
            MotorJoint = new RevoluteJoint(MotorBase, MotorWheel, (MotorBase.Position + MotorWheel.Position) * 0.5f, Vector3.Up);
            MotorJoint.Motor.IsActive = true;
            MotorJoint.Motor.Settings.VelocityMotor.GoalVelocity = 30;
            MotorJoint.Motor.Settings.MaximumForce = 300;
            
            AddToCollisionChecker();
        }

        public float GetRadius()
        {
             return MotorWheel.Radius;
        }

        public Microsoft.Xna.Framework.Vector3 GetPosition()
        {
            return MotorWheel.Position;
        }
        double oldAngle = 0;
        double oldRight = 0;

        private double AngleBetween(Vector3 desVector, Vector3 destVectorRight, Vector3 destVectorLeft, Vector3 fromVector)
        {
            float forwardDot = Vector3.Dot(fromVector, desVector);
            float rightDot = Vector3.Dot(fromVector, destVectorRight);
            
            forwardDot = MathHelper.Clamp(forwardDot, -1.0f, 1.0f);

            double angleBetween = Math.Acos(forwardDot);

            if (rightDot < 0.0f)
            {
                rightDot = Vector3.Dot(fromVector, destVectorLeft);
            }

            if (rightDot < 0.0f)
                angleBetween *= -1f;

            return angleBetween;
        }

        private Quaternion StayUpward()
        {
            Quaternion rotationWithoutRolling = Quaternion.Identity;
            Vector3 FromVector = new Vector3(Wheel.OrientationMatrix.Right.X,0,Wheel.OrientationMatrix.Right.Z);
            FromVector.Normalize();
            Vector3 DestVector = Wheel.OrientationMatrix.Right;
            DestVector.Normalize();

            Vector3 DestVectorsRight = Wheel.OrientationMatrix.Forward;
            DestVectorsRight.Normalize();

            Vector3 DestVectorsLeft = Wheel.OrientationMatrix.Backward;
            DestVectorsRight.Normalize();

            double angleBetween = AngleBetween(DestVector, DestVectorsRight, DestVectorsLeft,FromVector);
            Vector3 RotationAxis = Vector3.Cross(Wheel.OrientationMatrix.Right, Vector3.Up);
 
            return Wheel.Orientation;
        }

        public Microsoft.Xna.Framework.Quaternion GetRotation()
        {
             return MotorWheel.Orientation;
        }

        public Microsoft.Xna.Framework.Matrix GetWorldTransform()
        {
            return MotorWheel.WorldTransform;
        }

        public BEPUphysics.ISpaceObject GetBEPUEntity()
        {
            return Wheel;
        }

        public void AddToCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.AddObject(MotorBase, true);
            Util.GetInstance().CollisionsChecker.AddObject(MotorWheel, true);
            Util.GetInstance().CollisionsChecker.AddObject(MotorJoint, true);
        }

        public void RemoveFromCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.RemoveObject(MotorBase);
            Util.GetInstance().CollisionsChecker.RemoveObject(MotorWheel);
            Util.GetInstance().CollisionsChecker.RemoveObject(MotorJoint);
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
