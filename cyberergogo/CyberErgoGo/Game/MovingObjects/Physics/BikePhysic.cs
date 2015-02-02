using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Vehicle;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;

namespace CyberErgoGo
{
    class BikePhysic:IPhysicalRepresentation
    {
        Vehicle Bike;
        int ForwardSpeed = 30;
        float SizeAsRadius = 1;
        int bodylenght = 10;
        int bodyheight = 5;
        int bodywidth = 10;
        List<CompoundShapeEntry> bodyParts = new List<CompoundShapeEntry>();
        List<Wheel> FrontWheels;
        List<Wheel> BackWheels;
        int[] RackPartsToShow;
        int[] WheelsToShow;
        bool HideParts = true;
        public BikePhysic()
        {
            SetUpVehicle(new Vector3(0, 0, 0));
        }

        private void SetUpVehicle(Vector3 position)
        {
            Matrix wheelGraphicRotation =Matrix.CreateScale(10)* Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);
            FrontWheels = new List<Wheel>();
            BackWheels = new List<Wheel>();
            float dynamicBrakingFrictionCoefficent = 5f;
            float staticBrakingFrictionCoefficent = 2;
            float rollingFrictionCoefficent = 0.02f;

            RackPartsToShow = new int[2]{2,3};
            WheelsToShow = new int[2] {2,4};

            FrontWheels.Add(new Wheel(
                                 new RaycastWheelShape(.375f, wheelGraphicRotation),
                                 new WheelSuspension(2000, 100f, Vector3.Down, 8, new Vector3(10, 0, 5f + 10)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(dynamicBrakingFrictionCoefficent, staticBrakingFrictionCoefficent, rollingFrictionCoefficent),
                                 new WheelSlidingFriction(4, 5)));

            FrontWheels.Add(new Wheel(
                                 new RaycastWheelShape(.375f, wheelGraphicRotation),
                                 new WheelSuspension(2000, 100f, Vector3.Down, 8, new Vector3(-10, 0, 5f + 10)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(1.5f, 2, .02f),
                                 new WheelSlidingFriction(4, 5)));

            FrontWheels.Add(new Wheel(
                                new RaycastWheelShape(.375f, wheelGraphicRotation * Matrix.CreateScale(2)),
                                new WheelSuspension(2000, 100f, Vector3.Down, 8, new Vector3(0, 3, 5f + 10)),
                                new WheelDrivingMotor(2.5f, 30000, 10000),
                                new WheelBrake(1.5f, 2, .02f),
                                new WheelSlidingFriction(4, 5)));

            BackWheels.Add(new Wheel(
                                 new RaycastWheelShape(.375f, wheelGraphicRotation),
                                 new WheelSuspension(2000, 100f, Vector3.Down, 8, new Vector3(7, 0, -5f - 10)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(dynamicBrakingFrictionCoefficent, staticBrakingFrictionCoefficent, rollingFrictionCoefficent),
                                 new WheelSlidingFriction(4, 5)));

            BackWheels.Add(new Wheel(
                                 new RaycastWheelShape(.375f, wheelGraphicRotation),
                                 new WheelSuspension(2000, 100f, Vector3.Down, 8, new Vector3(0, 0, -5f - 10)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(dynamicBrakingFrictionCoefficent, staticBrakingFrictionCoefficent, rollingFrictionCoefficent),
                                 new WheelSlidingFriction(4, 5)));

            BackWheels.Add(new Wheel(
                                 new RaycastWheelShape(.375f, wheelGraphicRotation),
                                 new WheelSuspension(2000, 100f, Vector3.Down, 8, new Vector3(-7, 0, -5f - 10)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(dynamicBrakingFrictionCoefficent, staticBrakingFrictionCoefficent, rollingFrictionCoefficent),
                                 new WheelSlidingFriction(4, 5)));


            bodyParts = new List<CompoundShapeEntry>()
            {
                
                //new CompoundShapeEntry(new BoxShape(2, 3, 20), new RigidTransform(new Vector3(0,0,0), Quaternion.CreateFromYawPitchRoll(0,0.5f,0)), 1),
                new CompoundShapeEntry(new BoxShape(3, 3, 20), new Vector3(0,0,0), 10),
                new CompoundShapeEntry(new BoxShape(10, 3, 3), new Vector3(0,0,-15), 8),
                new CompoundShapeEntry(new BoxShape(3, 3, 15),new RigidTransform(new Vector3(0,5,0), Quaternion.CreateFromYawPitchRoll(0,-0.3f,0)), 0.1f),
                new CompoundShapeEntry(new BoxShape(3, 3, 10),new RigidTransform(new Vector3(0,7.2f,2.5f), Quaternion.CreateFromYawPitchRoll(0,0,0)), 0.1f)
                //new CompoundShapeEntry(new BoxShape(2.5f, .75f, 4.5f), new RigidTransform(new Vector3(0,20,10)), 10)
                //new CompoundShapeEntry(new BoxShape(2.5f, .3f, 2f), new Vector3(0, .75f / 2 + .3f / 2, .5f), 1)
            };
            var body = new CompoundBody(bodyParts, 20);
            body.CollisionInformation.LocalPosition = new Vector3(0,5, 0);
            body.Position = position; //At first, just keep it out of the way.
            Bike = new Vehicle(body);

            foreach (Wheel wheel in FrontWheels)
                Bike.AddWheel(wheel);
            foreach (Wheel wheel in BackWheels)
                Bike.AddWheel(wheel);

            #region RaycastWheelShapes

            //The wheel model used is not aligned initially with how a wheel would normally look, so rotate them.
            //Bike.AddWheel(new Wheel(
            //                     new RaycastWheelShape(.375f, wheelGraphicRotation),
            //                     new WheelSuspension(2000, 100f, Vector3.Down, .8f, new Vector3(1.1f, 0, 1.8f)),
            //                     new WheelDrivingMotor(2.5f, 30000, 10000),
            //                     new WheelBrake(1.5f, 2, .02f),
            //                     new WheelSlidingFriction(4, 5)));
            //Bike.AddWheel(new Wheel(
            //                     new RaycastWheelShape(.375f, wheelGraphicRotation),
            //                     new WheelSuspension(2000, 100f, Vector3.Down, .8f, new Vector3(1.1f, 0, -1.8f)),
            //                     new WheelDrivingMotor(2.5f, 30000, 10000),
            //                     new WheelBrake(1.5f, 2, .02f),
            //                     new WheelSlidingFriction(4, 5)));

            #endregion


            foreach (Wheel wheel in Bike.Wheels)
            {
                //This is a cosmetic setting that makes it looks like the car doesn't have antilock brakes.
                wheel.Shape.FreezeWheelsWhileBraking = true;
                
                //By default, wheels use as many iterations as the space.  By lowering it,
                //performance can be improved at the cost of a little accuracy.
                //However, because the suspension and friction are not really rigid,
                //the lowered accuracy is not so much of a problem.
                wheel.Suspension.SolverSettings.MaximumIterations = 1;
                wheel.Brake.SolverSettings.MaximumIterations = 1;
                wheel.SlidingFriction.SolverSettings.MaximumIterations = 1;
                wheel.DrivingMotor.SolverSettings.MaximumIterations = 1;
            }
        }

        public Vector3 GetUpVector()
        {
            return Vector3.Up;
        }

        public void SetUpVector(Vector3 newUp)
        {

        }

        #region IPhysicalRepresentation Member

        public Matrix GetFrontWheelWordlTransform()
        {
            return Bike.Wheels[0].Shape.WorldTransform;
        }

        public Matrix GetBackWheelWordlTransform()
        {
            return Bike.Wheels[1].Shape.WorldTransform;
        }

        public Matrix GetFrontRightWheelWordlTransform()
        {
            return Bike.Wheels[2].Shape.WorldTransform;
        }

        public Matrix GetBackRightWheelWordlTransform()
        {
            return Bike.Wheels[3].Shape.WorldTransform;
        }


        public List<Matrix> GetRackWordlTransform()
        {
            List<Matrix> rackTransforms = new List<Matrix>();
            foreach (CompoundShapeEntry shape in bodyParts)
            {
                if (RackPartsToShow.Contains(bodyParts.IndexOf(shape)) || !HideParts)
                {
                    BoxShape box = (BoxShape)shape.Shape;
                    rackTransforms.Add(Matrix.CreateScale(box.Width, box.Height, box.Length) * shape.LocalTransform.Matrix);
                }
                }
            return rackTransforms;
            //return Matrix.CreateScale(bodywidth, bodyheight, bodylenght) * Bike.Body.WorldTransform;
        }

        public List<Matrix> GetWheelWorldTransform()
        { 
            List<Matrix> wheelTransform = new List<Matrix>();
            for (int i = 0; i < Bike.Wheels.Count; i++)
            {
                if (WheelsToShow.Contains(i) || !HideParts)
                    wheelTransform.Add(Matrix.CreateFromYawPitchRoll(Bike.Wheels[i].Shape.SpinAngle, 0, 0) * Matrix.CreateFromYawPitchRoll(0, Bike.Wheels[i].Shape.SteeringAngle, 0) * Bike.Wheels[i].Shape.LocalGraphicTransform * Matrix.CreateTranslation(Bike.Wheels[i].Suspension.LocalAttachmentPoint));
            }
            return wheelTransform;
        }

        public Quaternion GetMovingOrientation()
        {
            return Quaternion.Identity;
        }

        private int TranslationVectorToSpeed(Vector3 translation)
        {
            if (translation.Length() > 0)
            {
                Vector3 movingDirection = Vector3.Transform(Vector3.UnitX, Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver2) * Matrix.CreateFromQuaternion(Bike.Body.Orientation));
                double lenght = Math.Cos(Vector3.Dot(Vector3.Normalize(movingDirection), Vector3.Normalize(translation)));
                lenght = (Vector3.Normalize(movingDirection) + Vector3.Normalize(translation)).Length();
                lenght -= 1;
                Console.WriteLine("length:" + lenght);
                return (int)(lenght * ForwardSpeed);
            }
            return 0;
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
            Vector3 movingDirection = Vector3.Transform(Vector3.UnitX, Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver2) * Matrix.CreateFromQuaternion(Bike.Body.Orientation));
            Vector3 rotatedVector = Vector3.Transform(Vector3.UnitX, Matrix.CreateFromQuaternion(rotation));
            float lenght = Vector3.Dot(Vector3.Normalize(movingDirection), Vector3.Normalize(rotatedVector));
            foreach (Wheel wheel in FrontWheels)
                wheel.Shape.SteeringAngle = -lenght;
        }

        public void RotateAbsolute(Microsoft.Xna.Framework.Quaternion rotation)
        {

            Bike.Body.Orientation = rotation;
        }

        public void Push(Microsoft.Xna.Framework.Vector3 veolation)
        {

        }

        public void SpeedUp(float speed)
        {
            if (speed > 0)
            {
                Console.WriteLine("speed:" + speed);
            }
            if (speed == 0)
            {
                foreach (Wheel wheel in BackWheels)
                    wheel.Brake.IsBraking = true;
            }
            else
            {
                foreach (Wheel wheel in BackWheels)
                {
                    wheel.Brake.IsBraking = false;
                    wheel.DrivingMotor.TargetSpeed = -speed * 10;
                }
            }
            //Bike.Wheels[3].DrivingMotor.TargetSpeed = -speed*5;
        }

        public void Steer(float angle)
        {
            if (angle < -45) angle = -45;
            if(angle >45) angle = 45;
            foreach (Wheel wheel in FrontWheels)
                wheel.Shape.SteeringAngle = MathHelper.ToRadians(angle);
            //Bike.Wheels[2].Shape.SteeringAngle = MathHelper.ToRadians(angle);
        }

        public void MoveForward()
        {
            foreach (Wheel wheel in BackWheels)
                wheel.DrivingMotor.TargetSpeed  = ForwardSpeed;
        }

        public void MoveBack()
        {
            foreach (Wheel wheel in BackWheels)
                wheel.DrivingMotor.TargetSpeed = -ForwardSpeed;
        }

        public void WeightDown(float mass)
        {
            Bike.Body.Mass += mass;
        }

        public void SetMass(float mass)
        {
            if(mass!=0)
                Bike.Body.Mass = mass;
        }

        public void SetAbsoluteSize(Microsoft.Xna.Framework.BoundingSphere bounding)
        {
            Bike.Body.Position = bounding.Center;
            SizeAsRadius = bounding.Radius;
        }

        public Microsoft.Xna.Framework.Vector3 GetPosition()
        {
            return Bike.Body.Position;
        }

        public Microsoft.Xna.Framework.Quaternion GetRotation()
        {
            return Bike.Body.Orientation;
        }

        public Microsoft.Xna.Framework.Matrix GetWorldTransform()
        {
            return Bike.Body.WorldTransform;
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

        public void RollForward(float degree)
        {

        }

        public void MoveRight()
        {
            throw new NotImplementedException();
        }

        public void MoveLeft()
        {
            throw new NotImplementedException();
        }

        public float GetRadius()
        {
            return 0;
        }

        #endregion
    }
}
