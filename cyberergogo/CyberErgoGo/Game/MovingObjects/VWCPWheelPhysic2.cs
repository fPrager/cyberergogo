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
    class VWCPWheelPhysic2:IPhysicalRepresentation
    {
        //this version of the wheel is a rolling zylinder
        #region IPhysicalRepresentation Member
        
        //MobileMesh Wheel;
        Cylinder Wheel;
        Quaternion MovingOrientation = Quaternion.Identity;
        float Size = 1;
        float Mass = 1;
        float ModelToOne = 1;
        int ScaleWidth = 2;

        float MovingSizeFactor = 1;
        float MovingMassFactor = 1;
        float SteeringSpeed = 0.01f;

        Vector3[] ModelVertices;
        int[] ModelIndices;
        
        public VWCPWheelPhysic2(float size, float mass)
        {
            Model wheel = null;
            Util.GetInstance().LoadFile(ref wheel, "Models", "cylinder");

            ModelToOne = Util.GetInstance().CalculateModelScaleToOneFactor(wheel);

            List<VertexHelper.TriangleVertexIndices> indices = new List<VertexHelper.TriangleVertexIndices>();
            List<Vector3> vertices = new List<Vector3>();
            VertexHelper.ExtractTrianglesFrom(wheel, vertices, indices, Matrix.Identity);
            ModelVertices = new Vector3[vertices.Count];
            int i = 0;
            foreach (Vector3 v in vertices)
            {
                ModelVertices[i] = v;
                i++;
            }
            ModelIndices = new int[indices.Count * 3];
            i = 0;
            foreach (VertexHelper.TriangleVertexIndices tvi in indices)
            {
                ModelIndices[i] = tvi.A;
                ModelIndices[i + 1] = tvi.B;
                ModelIndices[i + 2] = tvi.C;
                i += 3;
            }
            Size = size;
            Mass = mass;

            //Wheel = new MobileMesh(ModelVertices, ModelIndices, new AffineTransform(new Vector3(Size * ModelToOne, ScaleWidth * Size * ModelToOne, Size * ModelToOne), Quaternion.CreateFromYawPitchRoll(0, 0, MathHelper.ToRadians(90)), Vector3.Zero), MobileMeshSolidity.DoubleSided, Mass);
            Wheel = new Cylinder(Vector3.Zero, Size, Size, Mass);
            Wheel.Material = new BEPUphysics.Materials.Material(50, 50, -5);
            Wheel.WorldTransform *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(90), MathHelper.ToRadians(90), 0);
            MovingSizeFactor = Size;
            MovingMassFactor = 10;
        }

        public void Translate(Microsoft.Xna.Framework.Vector3 translation)
        {
           //Wheel.Orientation = Quaternion.Lerp(Wheel.Orientation, Quaternion.CreateFromYawPitchRoll(Vector3.Dot(currentForward, translation), 0, 0), 0.01f);
            
        }

        public void SetUpVector(Vector3 newUp)
        {

        }

        public void TranslateAbsolute(Microsoft.Xna.Framework.Vector3 translation)
        {
            Wheel.Position = translation;
        }

        public void Rotate(Microsoft.Xna.Framework.Quaternion rotation)
        {
        //    Vector3 currentForward = Wheel.OrientationMatrix.Up;
        //    Wheel.Orientation *= Quaternion.CreateFromYawPitchRoll(0,0,Vector3.Dot(currentForward, Matrix.CreateFromQuaternion(rotation * Quaternion.CreateFromYawPitchRoll(0, 0, MathHelper.ToRadians(90))).Forward));
        }

        public void RotateAbsolute(Microsoft.Xna.Framework.Quaternion rotation)
        {
            if (MovingOrientation == Quaternion.Identity)
            {
                MovingOrientation = rotation;
                Wheel.Orientation *= rotation;
            }
        }

        public Quaternion GetMovingOrientation()
        {
            return MovingOrientation;
        }


        public void Push(Microsoft.Xna.Framework.Vector3 veolation)
        {
        }
        public Vector3 GetUpVector()
        {
            return Vector3.Up;
        }

        public void SpeedUp(float speed)
        {
            //if (speed > 0)
            //{

            //}
            //SpeedFactor = speed / 10;
            //MoveIt();
            Vector3 mementum = speed / 10 * Matrix.CreateFromQuaternion(Wheel.Orientation).Up * (Wheel.Mass * MovingMassFactor) * (Size * MovingSizeFactor);
            Wheel.AngularMomentum = mementum;
        }

        private float CurrentAngle = 0;
        private Vector2 PrevPosition = Vector2.Zero;
        private float BikeLength = 2;

        public void Steer(float angle)
        {
            float diff = MathHelper.ToRadians(CurrentAngle - angle);
            //Vector3 forward = Vector3.Normalize(Vector3.Cross(Vector3.Up, Wheel.OrientationMatrix.Right));
            //if (PrevPosition == Vector2.Zero)
            //    PrevPosition = new Vector2(Wheel.Position.X, Wheel.Position.Z);

            //Vector2 currPosition = new Vector2(Wheel.Position.X, Wheel.Position.Z);
            //float distance = 0;
            //if (currPosition != PrevPosition)
            //    distance = (currPosition - PrevPosition).Length();

            ////Console.WriteLine("distance: " + distance);
            //Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(forward, Wheel.OrientationMatrix.Left)), MathHelper.ToRadians(diff));
            //if (distance != 0)
            //    rotation *= Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(forward, Wheel.OrientationMatrix.Left)), MathHelper.ToRadians(angle / 10 * -distance / 10));

            //Wheel.Orientation = rotation * Wheel.Orientation;
            //MovingOrientation *= rotation;

            CurrentAngle = angle;
            //PrevPosition = currPosition;
            Quaternion rightRotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(Vector3.Normalize(Vector3.Cross(Vector3.Up, Wheel.OrientationMatrix.Up)), Wheel.OrientationMatrix.Down)), diff);
            Wheel.Orientation = rightRotation * Wheel.Orientation;
            MovingOrientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, diff);
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
            Vector3 mementum = SpeedFactor * Matrix.CreateFromQuaternion(Wheel.Orientation).Up * (Wheel.Mass * MovingMassFactor) * (Size * MovingSizeFactor);
            Wheel.AngularMomentum = mementum;
        }

        public void MoveForward()
        {
            SpeedFactor += 0.01f;
            MoveIt();
        }

        public void MoveBack()
        {
            SpeedFactor -= 0.01f;
            MoveIt();
        }

        public void MoveRight()
        {
            Quaternion rightRotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(Vector3.Normalize(Vector3.Cross(Vector3.Up,Wheel.OrientationMatrix.Up)),Wheel.OrientationMatrix.Down)), -SteeringSpeed);
            Wheel.Orientation = rightRotation * Wheel.Orientation;
            MovingOrientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, -SteeringSpeed);
        }

        public void MoveLeft()
        {
            Quaternion leftRotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(Vector3.Normalize(Vector3.Cross(Vector3.Up, Wheel.OrientationMatrix.Up)), Wheel.OrientationMatrix.Down)), SteeringSpeed);
            Wheel.Orientation = leftRotation * Wheel.Orientation;
            MovingOrientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, SteeringSpeed);
        }

        public void RollForward(float degree)
        {
        }

        public void SetAbsoluteSize(Microsoft.Xna.Framework.BoundingSphere bounding)
        {
            RemoveFromCollisionChecker();
            Size = bounding.Radius;
            MovingOrientation = Quaternion.Identity;
           
            Wheel.Radius = Size*2;
            Wheel.Position = bounding.Center;
            Wheel.Material = new BEPUphysics.Materials.Material(50, 50, -5);
            AddToCollisionChecker();
        }

        public float GetRadius()
        {
           return Size;
        }

        public Microsoft.Xna.Framework.Vector3 GetPosition()
        {
            return Wheel.Position;
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
            //Vector3 falseVector = Vector3.Normalize(Wheel.OrientationMatrix.Left);
            //Vector3 rightVector = Vector3.Normalize(new Vector3(Wheel.OrientationMatrix.Left.X, 0, Wheel.OrientationMatrix.Left.Z));
            ////float angleX = Vector3.Dot(new Vector3(rightVector.X, 0, 0), new Vector3(falseVector.X, 0, 0));
            ////float angleZ = Vector3.Dot(new Vector3(0, 0, rightVector.Z), new Vector3(0, 0, falseVector.Z));
            //double dot = (double)(Vector3.Dot(falseVector, rightVector));
            //float angle = (float)Math.Acos(dot);
            ////angle = MathHelper.ToDegrees(angle);
            //Quaternion goUp = Quaternion.CreateFromAxisAngle(Vector3.Cross(Vector3.Up, Wheel.OrientationMatrix.Up),(float)Math.Acos(Vector3.Dot(Vector3.Up, Wheel.OrientationMatrix.Up)));
            //Console.WriteLine(angle);
            
            //Quaternion toCenter = Quaternion.CreateFromAxisAngle(Vector3.Transform(Wheel.OrientationMatrix.Forward, goUp), angle);
            //if (angle > 0 && angle<0.1f)
            //    Wheel.Orientation *= goUp;

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

            //if (angleBetween < 0)
            //{
            //    DestVectorsRight = Wheel.OrientationMatrix.Backward;
            //    DestVectorsRight.Normalize();
            //    angleBetween = AngleBetween(DestVector, DestVectorsRight, FromVector);
            //}

            //if (angleBetween != oldAngle)
            //{
            //    Console.WriteLine("angle: " + angleBetween);
            //    oldAngle = angleBetween;
            //}

            Vector3 RotationAxis = Vector3.Cross(Wheel.OrientationMatrix.Right, Vector3.Up);
            //rotationWithoutRolling *= Quaternion.CreateFromAxisAngle(RotationAxis, (float)angleBetween);
            return Wheel.Orientation;
        }

        public Microsoft.Xna.Framework.Quaternion GetRotation()
        {
            //StayUpward();
            return Wheel.Orientation;
        }

        public Microsoft.Xna.Framework.Matrix GetWorldTransform()
        {
            return Wheel.WorldTransform*Matrix.CreateScale(0.6f);
        }

        public BEPUphysics.ISpaceObject GetBEPUEntity()
        {
            return Wheel;
        }

        public void AddToCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.AddObject(Wheel, true);
        }

        public void RemoveFromCollisionChecker()
        {
            Util.GetInstance().CollisionsChecker.RemoveObject(Wheel);
        }

        #endregion
    }
}
