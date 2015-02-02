using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    enum EyeRepresentation
    {
        LeftEye,
        RightEye,
        Center
    }

    class Camera : PhysicalObject, IConditionObserver
    {
        public Vector3 Position
        {
            set
            {
                CenterPosition = value;
                MyCondition.CenterPosition = CenterPosition;
            }
            get
            {
                switch (Eye)
                {
                    case EyeRepresentation.LeftEye:
                        return LeftEyeParameters.Position;
                    case EyeRepresentation.RightEye:
                        return RightEyeParameters.Position;
                    default:
                        return CenterPosition;
                }
            }
        }

        public Vector3 GetCenterPosition()
        {
            return CenterPosition;
        }

        public Vector3 GetCenterLookAt()
        {
            return LookAt;
        }

        public void SetPosition(Vector3 newCenterPosition)
        {
            CenterPosition = newCenterPosition;
            UpdateView();
        }

        public Vector3 LookAt
        {
            set
            {
                CenterLookAt = value;
                MyCondition.LookAt = CenterLookAt;
            }
            get
            {
                switch (Eye)
                {
                    case EyeRepresentation.LeftEye:
                        return LeftEyeParameters.LookAt;
                    case EyeRepresentation.RightEye:
                        return RightEyeParameters.LookAt;
                    default:
                        return CenterLookAt;
                }
            }
        }

        public void SetLookAt(Vector3 newCenterLookAt)
        {
            CenterLookAt = newCenterLookAt;
            UpdateView();
        }

        public Matrix ViewMatrix
        {
            get
            {
                switch (Eye)
                {
                    case EyeRepresentation.LeftEye:
                        return LeftEyeParameters.ViewMatrix;
                    case EyeRepresentation.RightEye:
                        return RightEyeParameters.ViewMatrix;
                    default:
                        return CenterViewMatrix;
                }
            }
        }

        public Matrix ProjectionMatrix
        {
            get
            {
                switch (Eye)
                {
                    case EyeRepresentation.LeftEye:
                        return LeftEyeParameters.ProjectionMatrix;
                    case EyeRepresentation.RightEye:
                        return RightEyeParameters.ProjectionMatrix;
                    default:
                        return CenterProjectionMatrix;
                }
            }
        }

        public Matrix HalfProjectionMatrix
        {
            get
            {
                switch (Eye)
                {
                    case EyeRepresentation.LeftEye:
                        return LeftEyeParameters.HalfProjectionMatrix;
                    case EyeRepresentation.RightEye:
                        return RightEyeParameters.HalfProjectionMatrix;
                    default:
                        return CenterProjectionMatrix;
                }
            }
        }

        public Vector3 Up;
        protected float Aperture;
        protected float Fov;
        protected float AspectRatio;
        protected float NearPlane;
        protected float FarPlane;

        public Matrix CenterViewMatrix;
        public Matrix CenterProjectionMatrix;
        public Matrix CenterHalfProjectionMatrix;
        public Vector3 CenterLookAt;
        public Vector3 CenterPosition;


        //private Behaviour MovingBehaviour;

        private EyeRepresentation Eye;
        private Vector3 EyeOffsetVector;
        private ViewCondition MyCondition;

        struct EyeSpecificParameters
        {
          public Matrix ViewMatrix;
          public Matrix ProjectionMatrix;
          public Matrix HalfProjectionMatrix;
          public Vector3 LookAt;
          public Vector3 Position;
        }

        EyeSpecificParameters LeftEyeParameters;
        EyeSpecificParameters RightEyeParameters;

        public Camera(Behaviour behaviour, float fov, float aspectRatio, float nearPlane, float farPlane, Vector3 up, Vector3 position, Vector3 lookAt) : this(behaviour, EyeRepresentation.Center, fov, aspectRatio,nearPlane,farPlane,up,position,lookAt) 
        { }

        //Änderung der Physik
        public Camera(Behaviour behaviour, EyeRepresentation eye, float fov, float aspectRatio, float nearPlane, float farPlane, Vector3 up, Vector3 position, Vector3 lookAt)
            : base(new FlyingSphereWithMass(position, 1,1), behaviour)
        {
            MyCondition = new ViewCondition();
           
            behaviour.ExternalTranslation(position);
            Eye = eye;
            Fov = fov;
            AspectRatio = aspectRatio;
            NearPlane = nearPlane;
            FarPlane = farPlane;
            Up = up;
            CenterPosition = position;
            CenterLookAt = lookAt;
            CalculateViewAndProjection();
            MyCondition.MovingBehaviour = behaviour;

            //MovingBehaviour.SetOriginalVectors(Position, LookAt, Up);
            
            ConditionHandler.GetInstance().SetCondition(MyCondition);
            ConditionHandler.GetInstance().RegisterMe(ConditionID.ViewCondition, this);
        }

        public void ChangeEye(EyeRepresentation eye)
        {
            if (eye == Eye) return;
            Eye = eye;
        }

        //public void ChangeBehaviour(Behaviour newBehaviour)
        //{
        //    MovingBehaviour = newBehaviour;
        //    //MovingBehaviour.SetOriginalVectors(Position, LookAt, Up);
        //}

        public override void PhysicalUpdate()
        {
            //MovingBehaviour.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds/1000.0f, Position, LookAt, Up);
            
            //MovingBehaviour.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f);
            bool viewToUpdate = false;

            Vector3 newLookAt = MovingBehaviour.GetLookAt();
            if (newLookAt != LookAt)
            {
                CenterLookAt = newLookAt;
                viewToUpdate = true;
            }

            Vector3 newPosition = MovingBehaviour.GetPosition();
            if (newPosition != Position)
            {
                Position = newPosition;
                viewToUpdate = true;
            }

            Vector3 newUp = MovingBehaviour.GetUp();
            if (newUp != Up)
            {
                Up = newUp;
                viewToUpdate = true;
            }

            MyCondition.LookAt = CenterLookAt;
            MyCondition.ConditionHasChanged();

            if (viewToUpdate)
                UpdateView();
        }

        private void CalculateProjection()
        { 
            Vector3 position = CenterPosition;
            Vector3 lookAt = CenterLookAt;
        }

        private void CalculateViewAndProjection()
        {
            UpdateView();
            CalculateProjectionMatrix();
        }

        public void UpdateView()
        {
            CalculatePosition();
            CalculateViewMatrix();
        }

        private void CalculateProjectionMatrix()
        {
            CenterProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians((float)MyCondition.Aperture), AspectRatio, NearPlane, FarPlane);

            float radians = (MathHelper.ToRadians((float)MyCondition.Aperture)*AspectRatio) / 2f;
            //h = halbbreite
            float h = MyCondition.FocusPlane * (float)Math.Tan(radians);
            float ndfl = NearPlane / MyCondition.FocusPlane;

            float leftLeft = (-h + 0.5f * MyCondition.EyeOffset)*ndfl;
            float rightLeft = (h + 0.5f * MyCondition.EyeOffset) * ndfl;
            float leftRight = (-h - 0.5f * MyCondition.EyeOffset) * ndfl;
            float rightRight = (h - 0.5f * MyCondition.EyeOffset) * ndfl;
            float top = (NearPlane) * (float)Math.Tan(MathHelper.ToRadians((float)MyCondition.Aperture) / 2f);
            float bottom = -top;
            float fDiv = 0.7f;

            LeftEyeParameters.ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(leftLeft, rightLeft, bottom, top, NearPlane, FarPlane);
            LeftEyeParameters.HalfProjectionMatrix = LeftEyeParameters.ProjectionMatrix;
            RightEyeParameters.ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(leftRight, rightRight, bottom, top, NearPlane, FarPlane);
            RightEyeParameters.HalfProjectionMatrix = RightEyeParameters.ProjectionMatrix;

            //CenterProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1.666666f, 0.2f, 500.0f);
        }

        private void CalculatePosition()
        {
            Vector3 direction = CenterLookAt - CenterPosition;

            EyeOffsetVector = Vector3.Cross(direction, Up);
            EyeOffsetVector.Normalize();

            EyeOffsetVector.X *= MyCondition.EyeOffset / 2.0f;
            EyeOffsetVector.Y *= MyCondition.EyeOffset / 2.0f;
            EyeOffsetVector.Z *= MyCondition.EyeOffset / 2.0f;

            LeftEyeParameters.Position = new Vector3(CenterPosition.X - EyeOffsetVector.X, CenterPosition.Y - EyeOffsetVector.Y, CenterPosition.Z - EyeOffsetVector.Z);
            LeftEyeParameters.LookAt = new Vector3(CenterLookAt.X - EyeOffsetVector.X, CenterLookAt.Y - EyeOffsetVector.Y, CenterLookAt.Z - EyeOffsetVector.Z);
            RightEyeParameters.Position = new Vector3(CenterPosition.X + EyeOffsetVector.X, CenterPosition.Y + EyeOffsetVector.Y, CenterPosition.Z + EyeOffsetVector.Z);
            RightEyeParameters.LookAt = new Vector3(CenterLookAt.X + EyeOffsetVector.X, CenterLookAt.Y + EyeOffsetVector.Y, CenterLookAt.Z + EyeOffsetVector.Z);
        }

        private void CalculateViewMatrix()
        {
            CenterViewMatrix = Matrix.CreateLookAt(Position, LookAt, Up);
            LeftEyeParameters.ViewMatrix = Matrix.CreateLookAt(new Vector3(LeftEyeParameters.Position.X, LeftEyeParameters.Position.Y, LeftEyeParameters.Position.Z)
                                                    , new Vector3(LeftEyeParameters.LookAt.X, LeftEyeParameters.LookAt.Y, LeftEyeParameters.LookAt.Z)
                                                    , Up);
            RightEyeParameters.ViewMatrix = Matrix.CreateLookAt(new Vector3(RightEyeParameters.Position.X, RightEyeParameters.Position.Y, RightEyeParameters.Position.Z)
                                                    , new Vector3(RightEyeParameters.LookAt.X, RightEyeParameters.LookAt.Y, RightEyeParameters.LookAt.Z)
                                                    , Up);

            //CenterViewMatrix = Matrix.CreateLookAt(new Vector3(20, 13, -5), new Vector3(8, 0, -7), new Vector3(0, 1, 0));
        }

        private void ChangeMovingBehaviour(Behaviour movingBehaviour)
        {
            base.SetMovingBehaviour(movingBehaviour);
        }

        public void ConditionChanged(Condition condition, List<ParameterIdentifier> changedParameters)
        { 
            if(condition.GetID() == ConditionID.ViewCondition)
                foreach (ParameterIdentifier p in changedParameters)
                {
                    if (p == ParameterIdentifier.EyeOffset || p== ParameterIdentifier.FocusPlane)
                    {
                        MyCondition = (ViewCondition)condition;
                        CalculateViewAndProjection();
                    }
                    if (p == ParameterIdentifier.Aperture)
                    {
                        MyCondition = (ViewCondition)condition;
                        CalculateViewAndProjection();
                    }
                    else if (p == ParameterIdentifier.MovingBehaviour)
                    {
                        ChangeMovingBehaviour(((ViewCondition)condition).MovingBehaviour);
                    }
                    else if (p == ParameterIdentifier.LookAt)
                    {
                        SetLookAt(((ViewCondition)condition).LookAt);
                    }
                    else if (p == ParameterIdentifier.Position)
                    {
                        MovingBehaviour.ExternalTranslation(((ViewCondition)condition).CenterPosition);
                    }
                    else if (p == ParameterIdentifier.Up)
                    {
                        Up = ((ViewCondition)condition).Up;
                        UpdateView();
                    }
                }
            
        }

        //public void ForceBehaviourUpdate()
        //{
        //    bool viewToUpdate = false;

        //    Vector3 newLookAt = MovingBehaviour.GetNewLookAt();
        //    if (newLookAt != LookAt)
        //    {
        //        LookAt = newLookAt;
        //        viewToUpdate = true;
        //    }

        //    Vector3 newPosition = MovingBehaviour.GetNewPosition();
        //    if (newPosition != Position)
        //    {
        //        Position = newPosition;
        //        viewToUpdate = true;
        //    }

        //    Vector3 newUp = MovingBehaviour.GetNewUp();
        //    if (newUp != Up)
        //    {
        //        Up = newUp;
        //        viewToUpdate = true;
        //    }

        //    MyCondition.LookAt = LookAt - Position;
        //    MyCondition.ConditionHasChanged();

        //    if (viewToUpdate)
        //        UpdateView();
        //}
    }
}
