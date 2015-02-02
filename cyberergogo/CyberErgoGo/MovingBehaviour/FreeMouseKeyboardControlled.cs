using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BikeControls;

namespace CyberErgoGo
{
    class FreeMouseKeyboardControlled : Behaviour
    {
        float Yaw = 0;
        float Pitch = 0;
        float Roll = 0;
        float DistanceFactor = 1f;
        Vector2 OffMousePos;
        Vector2 PreMousePos;
        int OldScrollWheelValue;

        const float rotationSpeed = 0.15f;
        const float MoveSpeed = 0.5f;

        float leftrightRot = 0;
        float updownRot = 0;
        MouseState originalMouseState;

        Vector3 OriginalLookAt;
        Quaternion OriginalRotation;

        Vector2 OldMousePos = Vector2.Zero;

        int HalfViewPortWidth;
        int HalfViewPortHeight;

        public FreeMouseKeyboardControlled()
            : this(new Vector3(0, 0, 1))
        { }

        public FreeMouseKeyboardControlled(Vector3 lookAt)
            : base(lookAt)
        {
            OriginalLookAt = lookAt;
            MouseState ms = Mouse.GetState();
            PreMousePos = new Vector2(ms.X, ms.Y);

            HalfViewPortWidth = Util.GetInstance().Device.Viewport.Width / 2;
            HalfViewPortHeight = Util.GetInstance().Device.Viewport.Height / 2;

            Mouse.SetPosition(HalfViewPortWidth, HalfViewPortHeight);
            originalMouseState = Mouse.GetState();
        }

        private Quaternion Rotate(float xRotation, float yRotation, float zRotation)
        {
            Yaw += xRotation;
            Pitch += yRotation;
            Roll += zRotation;

            Quaternion qx = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), Yaw);
            Quaternion qy = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), Pitch);
            Quaternion qz = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), Roll);

            return (OriginalRotation * qx * qy * qz);
        }

        private void CalculateValuesWithKeyboardAndMouse(float time, float motionFactor)
        {
            // Retrieve the mousestate

            MouseState currentMouseState = Mouse.GetState();
            if (OldMousePos == Vector2.Zero) OldMousePos = new Vector2(currentMouseState.X, currentMouseState.Y);
            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - OldMousePos.X;
                float yDifference = currentMouseState.Y - OldMousePos.Y;
                leftrightRot -= rotationSpeed * xDifference * time / 1000f;
                updownRot -= rotationSpeed * yDifference * time / 1000f;
                OldMousePos = new Vector2(currentMouseState.X, currentMouseState.Y);
            }

            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            LookAt = Vector3.Transform(OriginalLookAt, cameraRotation);
            //Up = Vector3.Transform(Vector3.Up, cameraRotation);
            MovingOrientation = Quaternion.CreateFromYawPitchRoll(leftrightRot, updownRot, 0);

            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();


            if (keyState.IsKeyDown(Keys.Up))
            {
                PhysicalRepresentation.MoveForward();
                moveVector += new Vector3(0, 0, 1);
            }
            if (keyState.IsKeyDown(Keys.Down))
            {
                PhysicalRepresentation.MoveBack();
                moveVector += new Vector3(0, 0, -1);
            }
            if (keyState.IsKeyDown(Keys.Right))
            {
                PhysicalRepresentation.MoveRight();
                moveVector += new Vector3(-1, 0, 0);
            }
            if (keyState.IsKeyDown(Keys.Left))
            {
                PhysicalRepresentation.MoveLeft();
                moveVector += new Vector3(1, 0, 0);
            }
            if (keyState.IsKeyDown(Keys.Q))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.Z))
                moveVector += new Vector3(0, -1, 0);

            Vector3 newTranslation = Vector3.Transform(moveVector * time * MoveSpeed, Matrix.CreateFromQuaternion(MovingOrientation));

            PhysicalRepresentation.Translate(newTranslation);
        }

        private void CalculateValuesWithBike(float time, float motionFactor)
        { 
            
        }

        public override void CalculateNewValues(float time, float motionFactor)
        {
           CalculateValuesWithKeyboardAndMouse(time, motionFactor);
        }


    }
}
