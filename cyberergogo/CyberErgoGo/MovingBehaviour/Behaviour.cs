using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    abstract class Behaviour
    {

        //protected Vector3 NewLookAt;
        //protected Vector3 NewUp;
        //protected Quaternion Rotation;
        //protected Vector3 Translation;
        //protected Vector3 Position;

        //protected Vector3 OriginalLookAt;
        //protected Vector3 OriginalPosition;

        //protected Vector3 OriginalUpVector;
        protected IPhysicalRepresentation PhysicalRepresentation;
        protected Vector3 LookAt;
        protected Quaternion MovingOrientation;

        private Quaternion GlobalRotation = Quaternion.Identity;
        private Vector3 GlobalTranslation = Vector3.Zero;
        private float GlobalScale = 1;

        public Behaviour(Vector3 lookAt)
        {
            LookAt = lookAt;
            PhysicalRepresentation = new NotPhysical();
        }

        public void SetPhysicalRepresentation(ref IPhysicalRepresentation physicalRepresentation)
        {
            if (PhysicalRepresentation != null)
                PhysicalRepresentation.RemoveFromCollisionChecker();
            PhysicalRepresentation = physicalRepresentation;
            physicalRepresentation.AddToCollisionChecker();
        }

        public IPhysicalRepresentation GetPhysicalRepresentation()
        {
            return PhysicalRepresentation;
        }

        //public Behaviour(Vector3 orgPos, Vector3 orgLookAt, Vector3 orgUp)
        //{
        //    OriginalLookAt = orgLookAt;
        //    OriginalPosition = orgPos;
        //    OriginalUpVector = orgUp;
        //}

        public void Update(float elapsedGameTime, float motionFactor)
        {
            CalculateNewValues(elapsedGameTime, motionFactor);
        }

        //public void Update(float elapsedGameTime, Vector3 oldPosition, Vector3 oldLookAt, Vector3 oldUp)
        //{
        //   if (OriginalLookAt == Vector3.Zero) OriginalLookAt = oldLookAt;
        //   if (OriginalPosition == Vector3.Zero) OriginalPosition = oldPosition;
        //   if (OriginalUpVector == Vector3.Zero) OriginalUpVector = oldUp;

        //   CalculateNewValues(oldPosition, oldLookAt,oldUp, elapsedGameTime);
        //}

        //public Vector3 GetNewLookAt()
        //{
        //    return NewLookAt;
        //}

        public Vector3 GetPosition()
        {
            return PhysicalRepresentation.GetPosition();
        }

        public Vector3 GetUp()
        {
            return PhysicalRepresentation.GetUpVector();
        }

        public Quaternion GetRotation()
        {
            return PhysicalRepresentation.GetRotation();
        }

        public Vector3 GetTranslation()
        {
            return PhysicalRepresentation.GetWorldTransform().Translation;
        }

        public Vector3 GetLookAt()
        {
            return PhysicalRepresentation.GetPosition()+LookAt;
        }

        public Quaternion GetMovingOrientation()
        {
            Quaternion mOOfThePhysicalObject = PhysicalRepresentation.GetMovingOrientation();
            if (mOOfThePhysicalObject != null)
                MovingOrientation = mOOfThePhysicalObject;
            return MovingOrientation;
        }

        public Matrix GetWorldMatrix()
        {
            return PhysicalRepresentation.GetWorldTransform();
        }

        public void SetBounding(BoundingSphere bounding)
        {
            PhysicalRepresentation.SetAbsoluteSize(bounding);
        }

        public void ExternalRotation(Quaternion rotation)
        {
            PhysicalRepresentation.RotateAbsolute(rotation);
        }

        public void ExternalTranslation(Vector3 translation)
        {
            PhysicalRepresentation.TranslateAbsolute(translation);
        }

        public void ExternalScaling(float scale)
        {
            PhysicalRepresentation.SetAbsoluteSize(new BoundingSphere(PhysicalRepresentation.GetPosition(), scale));
        }

        public void SetUpVector(Vector3 newUp)
        {
            PhysicalRepresentation.SetUpVector(newUp);
        }

        //public abstract void CalculateNewValues(Vector3 oldPosition, Vector3 oldLookAt, Vector3 oldUp, float time);
        
        public abstract void CalculateNewValues(float time, float motionFactor);

        //public void SetOriginalVectors(Vector3 orgPos, Vector3 orgLookAt, Vector3 orgUp)
        //{
        //    OriginalLookAt = orgLookAt;
        //    OriginalPosition = orgPos;
        //    OriginalUpVector = orgUp;
        //}

    }
}
