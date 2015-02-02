using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    abstract class Behaviour
    {
        Camera DependingCamera;
        protected Vector3 NewPosition;
        protected Vector3 NewLookAt;
        protected Vector3 NewUp;
        protected Vector3 OriginalLookAt;
        protected Vector3 OriginalPosition;
        protected Vector3 OriginalUpVector;

        public Behaviour()
        {
            OriginalLookAt = Vector3.Zero;
            OriginalPosition = Vector3.Zero;
            OriginalUpVector = Vector3.Zero;
        }

        public void Update(float elapsedGameTime)
        {
            if(DependingCamera == null)
            {
                Console.WriteLine("no camera to update");
            }
            else
            {
                bool newView = false;
                Vector3 oldPosition = DependingCamera.GetCenterPosition();
                Vector3 oldLookAt = DependingCamera.GetCenterLookAt();
                Vector3 oldUp = DependingCamera.Up;

                if (OriginalLookAt == Vector3.Zero) OriginalLookAt = oldLookAt;
                if (OriginalPosition == Vector3.Zero) OriginalPosition = oldPosition;
                if (OriginalUpVector == Vector3.Zero) OriginalUpVector = oldUp;

                CalculateNewValues(oldPosition, oldLookAt,oldUp, elapsedGameTime);

                if (oldPosition != NewPosition)
                {
                    DependingCamera.Position = NewPosition;
                    newView = true;
                }

                if (oldLookAt != NewLookAt)
                {
                    DependingCamera.LookAt = NewLookAt;
                    newView = true;

                }

                if (oldUp != NewUp)
                {
                    DependingCamera.Up = NewUp;
                    newView = true;
                }

                if(newView)
                    DependingCamera.UpdateView();
            }
        }

        public abstract void CalculateNewValues(Vector3 oldPosition, Vector3 oldLookAt, Vector3 oldUp, float time);

        public void RegisterCamera(Camera camera)
        {
            DependingCamera = camera;
        }

    }
}
