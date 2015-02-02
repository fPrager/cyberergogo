using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class MovingObjectCondition:Condition
    {
        public Vector3 Position
        {
            get { return (Vector3)GetParameterValue(ParameterIdentifier.Position); }
            set { SetParameter(ParameterIdentifier.Position, value); }
        }

        public Quaternion Rotation
        {
            get { return (Quaternion)GetParameterValue(ParameterIdentifier.Rotation); }
            set { SetParameter(ParameterIdentifier.Rotation, value); }
        }

        public Quaternion MovingOrientation
        {
            get { return (Quaternion)GetParameterValue(ParameterIdentifier.MovingOrientation); }
            set { SetParameter(ParameterIdentifier.MovingOrientation, value); }
        }

        public BoundingSphere Bounding
        {
            get { return (BoundingSphere)GetParameterValue(ParameterIdentifier.Bounding); }
            set { SetParameter(ParameterIdentifier.Bounding, value); 
            }
        }

        public Vector3[] Vertices
        {
            get { return (Vector3[])GetParameterValue(ParameterIdentifier.ShapeVertices); }
            set { SetParameter(ParameterIdentifier.ShapeVertices, value); }
        }

        public int[] Indices
        {
            get { return (int[])GetParameterValue(ParameterIdentifier.ShapeIndices); }
            set { SetParameter(ParameterIdentifier.ShapeIndices, value); }
        }

        public int Mass
        {
            get { return (int)GetParameterValue(ParameterIdentifier.Mass); }
            set { SetParameter(ParameterIdentifier.Mass, value); }
        }

        public float ScaleFactor
        {
            get { return (float)GetParameterValue(ParameterIdentifier.Scale); }
            set { SetParameter(ParameterIdentifier.Scale, value); }
        }

        public Vector3 LinearVelocity
        {
            get { return (Vector3)GetParameterValue(ParameterIdentifier.LinearVelocity); }
            set { SetParameter(ParameterIdentifier.LinearVelocity, value); }
        }

        public Vector3 Up
        {
            get { return (Vector3)GetParameterValue(ParameterIdentifier.Up); }
            set { SetParameter(ParameterIdentifier.Up, value); }
        }

        //public IPhysicalRepresentation PhysicalRepresentation
        //{
        //    get { return (IPhysicalRepresentation)GetParameterValue(ParameterIdentifier.PhysicalRepresentation); }
        //    set { SetParameter(ParameterIdentifier.PhysicalRepresentation, value); }
        //}

        public MovingObjectCondition() : base(ConditionID.MovingObjectCondition) 
        {
            Parameters.Add(new Parameter(Vector3.Zero, ParameterIdentifier.Position, ID));
            Parameters.Add(new Parameter(Quaternion.Identity, ParameterIdentifier.Rotation, ID));
            Parameters.Add(new Parameter(Quaternion.Identity, ParameterIdentifier.MovingOrientation, ID));
            Parameters.Add(new Parameter(new BoundingSphere(Vector3.Zero, 0.1f), ParameterIdentifier.Bounding, ID));
            Parameters.Add(new Parameter(null, ParameterIdentifier.ShapeVertices, ID));
            Parameters.Add(new Parameter(null, ParameterIdentifier.ShapeIndices, ID));
            Parameters.Add(new Parameter(1, ParameterIdentifier.Mass, ID));
            Parameters.Add(new Parameter(1f, ParameterIdentifier.Scale, ID));
            Parameters.Add(new Parameter(Vector3.Zero, ParameterIdentifier.LinearVelocity, ID));
            Parameters.Add(new Parameter(Vector3.Up, ParameterIdentifier.Up, ID));
            //Parameters.Add(new Parameter(null, ParameterIdentifier.PhysicalRepresentation, ID));
        }
    }
}
