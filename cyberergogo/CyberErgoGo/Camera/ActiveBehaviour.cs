using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyberErgoGo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CyberErgoGo
{
    class ActiveBehaviour : Behaviour
    {

        //Mouse-Control
        MouseState OriginalMousState;
        float LeftrightRot = MathHelper.PiOver2;
        float UpdownRot = -MathHelper.Pi / 10.0f;
        const float RotationSpeed = 0.3f;
        const float MoveSpeed = 30.0f;
        int HalfViewPortWidth;
        int HalfViewPortHeight;

        public ActiveBehaviour() : base()
        {
            HalfViewPortWidth = Util.GetInstance().Device.Viewport.Width / 2;
            HalfViewPortHeight = Util.GetInstance().Device.Viewport.Height / 2;
            Mouse.SetPosition(HalfViewPortWidth, HalfViewPortHeight);
            OriginalMousState = Mouse.GetState();
            NewLookAt = Vector3.Zero;
            NewPosition = Vector3.Zero;
        }

        private void ProcessInput(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            Matrix cameraRotation;
            Vector3 cameraRotatedTarget;

            if (currentMouseState != OriginalMousState)
            {
                float xDifference = currentMouseState.X - OriginalMousState.X;
                float yDifference = currentMouseState.Y - OriginalMousState.Y;
                LeftrightRot -= RotationSpeed * xDifference * amount;
                UpdownRot += RotationSpeed * yDifference * amount;
                //TODO: Überprüfen, ob doch zu jedem "Update" die aktuellen ViewPort-Maße berechnet werden sollten
                Mouse.SetPosition(HalfViewPortWidth, HalfViewPortHeight);

                cameraRotation = Matrix.CreateRotationX(UpdownRot) * Matrix.CreateRotationY(LeftrightRot);
                cameraRotatedTarget = Vector3.Transform(OriginalLookAt, cameraRotation);
                NewLookAt = NewPosition + cameraRotatedTarget;
                NewUp = Vector3.Transform(OriginalUpVector, cameraRotation);
            }

            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up))
                moveVector -= new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down))
                moveVector -= new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Right))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Q))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.Z))
                moveVector += new Vector3(0, -1, 0);
            Vector3 vectorToAdd = moveVector * amount;

            cameraRotation = Matrix.CreateRotationX(UpdownRot) * Matrix.CreateRotationY(LeftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            NewPosition += MoveSpeed * rotatedVector;

            cameraRotatedTarget = Vector3.Transform(OriginalLookAt, cameraRotation);
            NewLookAt = NewPosition + cameraRotatedTarget;

            NewUp = Vector3.Transform(OriginalUpVector, cameraRotation);
        }


        public override void CalculateNewValues(Vector3 oldPosition, Vector3 oldLookAt, Vector3 oldUp, float time)
        {
            //KeyboardState currentKeyboardState = Keyboard.GetState();

            NewLookAt = oldLookAt;
            NewPosition = oldPosition;
            NewUp = oldUp;
            //// Check for input to rotate the camera.
            //float pitch = 0;
            //float turn = 0;

            //if (currentKeyboardState.IsKeyDown(Keys.Up))
            //    pitch += time * 0.001f;

            //if (currentKeyboardState.IsKeyDown(Keys.Down))
            //    pitch -= time * 0.001f;

            //if (currentKeyboardState.IsKeyDown(Keys.Left))
            //    turn += time * 0.001f;

            //if (currentKeyboardState.IsKeyDown(Keys.Right))
            //    turn -= time * 0.001f;

            //Vector3 cameraRight = Vector3.Cross(Vector3.Up, NewLookAt);
            //Vector3 flatFront = Vector3.Cross(cameraRight, Vector3.Up);

            //Matrix pitchMatrix = Matrix.CreateFromAxisAngle(cameraRight, pitch);
            //Matrix turnMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, turn);

            //Vector3 tiltedFront = Vector3.TransformNormal(NewLookAt, pitchMatrix *
            //                                              turnMatrix);

            //// Check angle so we cant flip over
            //if (Vector3.Dot(tiltedFront, flatFront) > 0.001f)
            //{
            //    NewLookAt = Vector3.Normalize(tiltedFront);
            //}

            //// Check for input to move the camera around.
            //if (currentKeyboardState.IsKeyDown(Keys.W))
            //    NewPosition += NewLookAt * time * 0.1f;

            //if (currentKeyboardState.IsKeyDown(Keys.S))
            //    NewPosition -= NewLookAt * time * 0.1f;

            //if (currentKeyboardState.IsKeyDown(Keys.A))
            //    NewPosition += cameraRight * time * 0.1f;

            //if (currentKeyboardState.IsKeyDown(Keys.D))
            //    NewPosition -= cameraRight * time * 0.1f;
            ProcessInput(time);
        }
       
    }
}
