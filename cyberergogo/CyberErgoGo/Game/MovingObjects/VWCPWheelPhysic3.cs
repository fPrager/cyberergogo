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
    class VWCPWheelPhysic3:IPhysicalRepresentation
    {
        //this version of the wheel is simple vehicle
        #region IPhysicalRepresentation Member
        
        Quaternion MovingOrientation = Quaternion.Identity;

        float SizeFactor = 1 * OverallSetting.SizeFactor;
        float Mass = 1;

        Vehicle Bike;
        Wheel Frontwheel;
        CompoundShapeEntry BodyEntry;
        Quaternion OriginalRotation = Quaternion.Identity;
        Matrix WheelTransform;
        float SteeringAngle = 0;
        float MaxSteering = 100;
        float TurningSpeed = MathHelper.Pi * 0.01f;
        float BikeSteeringCompression = 0.2f;
        float BikeSpeedCompression = 2;


        public void SetUpVehicle(Vector3 position, float mass)
        {
            //Matrix wheelGraphicRotation = Matrix.CreateScale(5) * Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);
            //float dynamicBrakingFrictionCoefficent = 5f;
            //float staticBrakingFrictionCoefficent = 2;
            //float rollingFrictionCoefficent = 30;
            //Frontwheel = new Wheel(
            //                     new RaycastWheelShape(10f, wheelGraphicRotation),
            //                     new WheelSuspension(2000, 100f, Vector3.Down, 8, new Vector3(0, 5, 0)),
            //                     new WheelDrivingMotor(2.5f, 30000, 10000),
            //                     new WheelBrake(dynamicBrakingFrictionCoefficent, staticBrakingFrictionCoefficent, rollingFrictionCoefficent),
            //                     new WheelSlidingFriction(4, 5));
            //List<CompoundShapeEntry>  bodyParts = new List<CompoundShapeEntry>()
            //{new CompoundShapeEntry(new BoxShape(1, 1,1), new Vector3(0,0,0),mass)
            //};
            

            //var body = new CompoundBody(bodyParts, 2);
            //body.CollisionInformation.LocalPosition = new Vector3(0, 0, 0);
            //body.Position = position; //At first, just keep it out of the way.
            //Bike = new Vehicle(body);
            //Bike.AddWheel(Frontwheel);
            
            BodyEntry = new CompoundShapeEntry(new CylinderShape(10 * SizeFactor, 5 * SizeFactor), new RigidTransform(new Vector3(0, 0, -10 * SizeFactor), Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(90), MathHelper.ToRadians(90), 0)), 15 * SizeFactor);
            CompoundShapeEntry bodyEntry2 = new CompoundShapeEntry(new CylinderShape(10 * SizeFactor, 3 * SizeFactor), new RigidTransform(new Vector3(0, 0, 10 * SizeFactor), Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(90), MathHelper.ToRadians(90), 0)), 15 * SizeFactor);
            
            var bodies = new List<CompoundShapeEntry>()
            {
                BodyEntry,
                bodyEntry2
            };
            var body = new CompoundBody(bodies, 30 * SizeFactor);
            body.CollisionInformation.LocalPosition = new Vector3(0, 5 * SizeFactor, 0);
            body.Position = position; //At first, just keep it out of the way.
            Bike = new Vehicle(body);


            //The wheel model used is not aligned initially with how a wheel would normally look, so rotate them.
            Matrix wheelGraphicRotation = Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);
            //WheelTransform = wheelGraphicRotation * Matrix.CreateScale(6  * 4, 6 , 6 );
            Frontwheel = new Wheel(
                                 new RaycastWheelShape(6 * SizeFactor, wheelGraphicRotation * Matrix.CreateScale(6 * SizeFactor / 0.375f * 10 * SizeFactor, 6 * SizeFactor / 0.375f * 1 * SizeFactor, 6 * SizeFactor / 0.375f * 1 * SizeFactor)),
                                 new WheelSuspension(2000, 100f, Vector3.Down, 6 * SizeFactor, new Vector3(0, 0, -10 * SizeFactor)),
                                 new WheelDrivingMotor(100 * SizeFactor, 5000 * SizeFactor, -1000 * SizeFactor),
                                 new WheelBrake(10f * SizeFactor, 10f * SizeFactor, 1),
                                 new WheelSlidingFriction(10, 100));
            Wheel backWheel = new Wheel(
                                  new RaycastWheelShape(6 * SizeFactor, wheelGraphicRotation * Matrix.CreateScale(6 * SizeFactor / 0.375f * 10 * SizeFactor, 6 * SizeFactor / 0.375f * 1 * SizeFactor, 6 * SizeFactor / 0.375f * 1 * SizeFactor)),
                                  new WheelSuspension(2000, 100f, Vector3.Down, 6 * SizeFactor, new Vector3(0, 0, 10 * SizeFactor)),
                                  new WheelDrivingMotor(100 * SizeFactor, 5000 * SizeFactor, -1000 * SizeFactor),
                                  new WheelBrake(10f * SizeFactor, 10f * SizeFactor, 1),
                                  new WheelSlidingFriction(10, 100));


            Bike.AddWheel(Frontwheel);
            Bike.AddWheel(backWheel);
            Bike.Body.Orientation *= Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(180), 0, 0);
            //Bike.AddWheel(backwheel1);
            //Bike.AddWheel(backwheel2);
           
        }

        public VWCPWheelPhysic3(float size, float mass)
        {
            SizeFactor = size / 5;
            Mass = mass;
            SetUpVehicle(Vector3.Zero,mass);
        }

        public void Translate(Microsoft.Xna.Framework.Vector3 translation)
        {
        }

        public void TranslateAbsolute(Microsoft.Xna.Framework.Vector3 translation)
        {
           Bike.Body.Position = translation;
        }

        public void Rotate(Microsoft.Xna.Framework.Quaternion rotation)
        {
           // Bike.Body.Orientation = OriginalRotation * rotation;
        }

        public void SetUpVector(Vector3 newUp)
        {

        }

        public void RotateAbsolute(Microsoft.Xna.Framework.Quaternion rotation)
        {
            OriginalRotation = rotation * Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(180), 0, 0);
            Bike.Body.Orientation = OriginalRotation;
        }

        public Quaternion GetMovingOrientation()
        {
            Matrix yRotation = new Matrix();
            yRotation = Matrix.CreateFromQuaternion(Bike.Body.Orientation);
            yRotation.M21 = 0;
            yRotation.M12 = 0;
            yRotation.M22 = 1;
            yRotation.M32 = 0;
            yRotation.M23 = 0;
            return Quaternion.CreateFromRotationMatrix(yRotation) * Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(180), 0, 0);
        }

        public Vector3 GetUpVector()
        {
            Vector3 up = Vector3.Up;
            if(Bike.Wheels[1].Shape.WorldTransform.Up != Vector3.Zero)
                up = Vector3.Normalize(Vector3.Cross(Bike.Wheels[1].Shape.WorldTransform.Up, Bike.Wheels[1].WorldForwardDirection));
            if (up.Y < 0) up *= -1;
            return up;
        }


        public void Push(Microsoft.Xna.Framework.Vector3 veolation)
        {
        }

        public void SpeedUp(float speed)
        {
            speed *= BikeSpeedCompression * SizeFactor;
            Bike.Wheels[0].DrivingMotor.TargetSpeed = speed;
            Bike.Wheels[1].DrivingMotor.TargetSpeed = speed;
            //Console.WriteLine(Bike.Wheels[0].DrivingMotor.TargetSpeed);
        }

        private float LeastAngle = 0;
        private Vector2 PrevPosition = Vector2.Zero;
        private float BikeLength = 2;

        public void Steer(float angle)
        {
            angle *= BikeSteeringCompression;
            Bike.Wheels[0].Shape.SteeringAngle = MathHelper.ToRadians(angle);
            Bike.Body.Orientation *= Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(-angle / 100), 0, 0);
        }

        public void WeightDown(float mass)
        {
        }

        public void SetMass(float mass)
        {
        }

        float SpeedFactor = 0;

       
        public void MoveForward()
        {
            Bike.Wheels[1].DrivingMotor.TargetSpeed = -50 * SizeFactor;
        }

        public void MoveBack()
        {
            Bike.Wheels[1].DrivingMotor.TargetSpeed = 50 * SizeFactor;
        }

        public void MoveRight()
        {
            if(SteeringAngle >= MathHelper.ToRadians(-MaxSteering))
                SteeringAngle -= TurningSpeed;
            Bike.Wheels[0].Shape.SteeringAngle = SteeringAngle;
            //Bike.Body.Orientation = Bike.Body.Orientation * Quaternion.CreateFromAxisAngle(Bike.Body.WorldTransform.Up, MathHelper.Pi * 0.01f);
        }

        public void MoveLeft()
        {
            if (SteeringAngle <= MathHelper.ToRadians(MaxSteering))
                SteeringAngle += TurningSpeed;
            Bike.Wheels[0].Shape.SteeringAngle = SteeringAngle;
            //Bike.Body.Orientation = Bike.Body.Orientation * Quaternion.CreateFromAxisAngle(Bike.Body.WorldTransform.Up, - MathHelper.Pi * 0.01f);
        }

        public void RollForward(float degree)
        {
        }

        public void SetAbsoluteSize(Microsoft.Xna.Framework.BoundingSphere bounding)
        {
            Bike.Body.Position = bounding.Center;
            SizeFactor = bounding.Radius/5;
            
            AddToCollisionChecker();
        }

        public float GetRadius()
        {
            return SizeFactor * 5;
        }

        public Microsoft.Xna.Framework.Vector3 GetPosition()
        {
            return -Vector3.Transform(Bike.Wheels[1].Suspension.LocalAttachmentPoint, Matrix.CreateFromQuaternion(Bike.Body.Orientation)) + Bike.Body.Position;
        }

        public Microsoft.Xna.Framework.Quaternion GetRotation()
        {
            //Bike.Body.Orientation = Quaternion.CreateFromYawPitchRoll(SteeringAngle, 0, 0);
            return Bike.Body.Orientation;
        }

        public Microsoft.Xna.Framework.Matrix GetWorldTransform()
        {
            //return Wheel.WorldTransform;
            //return WheelTransform * Matrix.CreateTranslation(Frontwheel.Shape.;
            //return Frontwheel.Shape.WorldTransform;
            return Matrix.CreateFromYawPitchRoll(0, -Bike.Wheels[0].Shape.SpinAngle, 0) * Matrix.CreateFromYawPitchRoll(-Bike.Wheels[0].Shape.SteeringAngle, 0, 0) * Matrix.CreateTranslation(Bike.Wheels[0].Suspension.LocalAttachmentPoint) * Matrix.CreateTranslation(new Vector3(0, 5 * SizeFactor, 0)) * Bike.Body.WorldTransform;
        }

        public Microsoft.Xna.Framework.Matrix GetWorldTransform2()
        {
            //return Wheel.WorldTransform;
            //return WheelTransform * Matrix.CreateTranslation(Frontwheel.Shape.;
            //return Frontwheel.Shape.WorldTransform;
            return Matrix.CreateFromYawPitchRoll(0, -Bike.Wheels[1].Shape.SpinAngle, 0) * Matrix.CreateFromYawPitchRoll(-Bike.Wheels[1].Shape.SteeringAngle, 0, 0) * Matrix.CreateTranslation(Bike.Wheels[1].Suspension.LocalAttachmentPoint) * Matrix.CreateTranslation(new Vector3(0, 5 * SizeFactor, 0)) * Bike.Body.WorldTransform;
        }

        public Microsoft.Xna.Framework.Matrix GetWorldTransformFromBackWheel()
        {
            return Matrix.CreateFromYawPitchRoll(0, -Bike.Wheels[1].Shape.SpinAngle, 0) * Matrix.CreateFromYawPitchRoll(-Bike.Wheels[1].Shape.SteeringAngle, 0, 0) * Matrix.CreateTranslation(Bike.Wheels[1].Suspension.LocalAttachmentPoint) * Matrix.CreateTranslation(new Vector3(0, 5 * SizeFactor, 0)) * Bike.Body.WorldTransform;
        }

        public BEPUphysics.ISpaceObject GetBEPUEntity()
        {
            return Bike;
        }

        public void AddToCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.AddObject(Bike, true);
        }

        public void RemoveFromCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.RemoveObject(Bike);
        }

        #endregion
    }
}
