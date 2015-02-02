using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    enum CheckpointFeature{
        Start,
        End,
        Default
    }

    enum CheckpointState
    {
        Reached,
        Unchecked,
        Next
    }

    class Checkpoint
    {
        public int Index;

        public string Message;

        public int TimeMin = 0;
        public int TimeSec = 0;
        public int TimeMSec = 0;

        public float BestTime;

        public CheckpointFeature Feature;

        public Vector3 Position;

        public int Dimension = 80;
        public int NumberOfPoints = 20;

        public VertexBuffer VBuffer;
        public IndexBuffer IBuffer;

        public Quaternion Orientation;

        public CheckpointState State;

        public Vector3 Min = Vector3.Zero;

        public Vector3 Max = Vector3.Zero;

        public Checkpoint(int index, int dimension, int numOfPoints, Quaternion orientation)
        {
            Index = index;
            Feature = CheckpointFeature.Default;
            BestTime = 0;
            Message = "";
            State = CheckpointState.Unchecked;
            Orientation = orientation;
            Position = Vector3.Zero;
            Dimension = dimension;
            NumberOfPoints = numOfPoints;
            //GenerateMinMax();
        }


        public void GenerateMinMax()
        {
            Vector3 goUp = Vector3.Up * Dimension / 2;
            Vector3 toLeft = Vector3.Transform(goUp, Matrix.CreateFromYawPitchRoll(0,0,MathHelper.ToRadians(-90)));
            Vector3 toRight = Vector3.Transform(goUp, Matrix.CreateFromYawPitchRoll(0, 0, MathHelper.ToRadians(90)));

            Vector3 tranlationLeft = Vector3.Transform(toLeft, Matrix.CreateFromQuaternion(Orientation));
            Vector3 tranlationRight = Vector3.Transform(toRight, Matrix.CreateFromQuaternion(Orientation));
            Vector3 stepForward = Vector3.Normalize(Vector3.Cross(tranlationLeft, goUp));
            Max = Position + tranlationLeft + goUp + stepForward*2;
            Min = Position + tranlationRight;

            if (Min.X > Max.X)
            {
                float x = Max.X;
                Max.X = Min.X;
                Min.X = x;
            }
            if (Min.Y > Max.Y)
            {
                float y = Max.Y;
                Max.Y = Min.Y;
                Min.Y = y;
            }
            if (Min.Z > Max.Z)
            {
                float z = Max.Z;
                Max.Z = Min.Z;
                Min.Z = z;
            }
        }

        private VertexBuffer GetBufferForThisPosition(Vector3 Position)
        {
            
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];
            vertices[0].Position = Position;
            vertices[0].TextureCoordinate = new Vector2(0, 0);

            vertices[1].Position = Position;
            vertices[1].TextureCoordinate = new Vector2(0, 1);

            vertices[2].Position = Position;
            vertices[2].TextureCoordinate = new Vector2(1, 1);

            vertices[3].Position = Position;
            vertices[3].TextureCoordinate = new Vector2(1, 0);

            VertexBuffer vBuffer  = new VertexBuffer(Util.GetInstance().Device, VertexPositionTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            vBuffer.SetData(vertices);

            return vBuffer;
        }

        public void SetUpBuffers()
        {
            int[] indeces = new int[6];
            indeces[0] = 0;
            indeces[1] = 1;
            indeces[2] = 2;
            indeces[3] = 2;
            indeces[4] = 3;
            indeces[5] = 0;

            IBuffer = new IndexBuffer(Util.GetInstance().Device, typeof(int), indeces.Length, BufferUsage.WriteOnly);
            IBuffer.SetData(indeces);
            VBuffer = GetBufferForThisPosition(Position);
            
        }

        public List<Vector3> GetSpritePoints(float rotationAngle)
        {
            List<Vector3> rotatedPoints = new List<Vector3>();

            float angleToNext = (2 * MathHelper.Pi) / NumberOfPoints;

            for (int i = 0; i < NumberOfPoints; i++)
            {
                Vector3 distanceToMiddle = Vector3.Up * Dimension / 2;
                if(i%2 == 0)
                    distanceToMiddle = Vector3.Transform(distanceToMiddle, Matrix.CreateFromAxisAngle(Vector3.Forward, i * angleToNext + rotationAngle));
                else
                    distanceToMiddle = Vector3.Transform(distanceToMiddle, Matrix.CreateFromAxisAngle(Vector3.Forward, i * angleToNext - rotationAngle));
                
                Vector3 tranlation = Vector3.Transform(distanceToMiddle, Matrix.CreateFromQuaternion(Orientation));
                rotatedPoints.Add(Position + tranlation);
            }
            //rotatedPoints.Add(Min);
            //rotatedPoints.Add(Max);
            return rotatedPoints;
        }
    }
}
