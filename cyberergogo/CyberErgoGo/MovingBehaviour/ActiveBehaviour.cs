using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyberErgoGo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BikeControls;

namespace CyberErgoGo
{
    class ActiveBehaviour : Behaviour
    {
        float Yaw = 0;
        float Pitch = 0;
        float Roll = 0;
        float DistanceFactor = 1f;
        Vector2 OffMousePos;
        Vector2 PreMousePos;
        int OldScrollWheelValue;

        Vector3 OriginalLookAt;
        Quaternion OriginalRotation;

        int HalfViewPortWidth;
        int HalfViewPortHeight;

        public ActiveBehaviour() : this(new Vector3(0, 0, 0)) 
        { }

        public ActiveBehaviour(Vector3 lookAt):base(lookAt)
        {
            OriginalRotation = new Quaternion(lookAt, 1);
            OriginalLookAt = lookAt;
            MouseState ms = Mouse.GetState();
            PreMousePos = new Vector2(ms.X, ms.Y);
            
            HalfViewPortWidth = Util.GetInstance().Device.Viewport.Width / 2;
            HalfViewPortHeight = Util.GetInstance().Device.Viewport.Height / 2;
        }

        private Quaternion Rotate(float xRotation, float yRotation, float zRotation)
        {
            Yaw += xRotation;
            Pitch += yRotation;
            Roll += zRotation;

            Quaternion qx = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), Yaw);
            Quaternion qy = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), Pitch);
            Quaternion qz = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), Roll);

            return(OriginalRotation * qx * qy * qz);
        }

        public override void CalculateNewValues(float time, float motionFactor)
        {
            //Console.WriteLine("motionfactor: " + motionFactor);
            // Retrieve the mousestate
            Quaternion newRotation = Quaternion.Identity;
            if (!Bike.PluggedIn)
            {
                MouseState ms = Mouse.GetState();

                // Rotate camera
                newRotation = Rotate(OffMousePos.X * 0.005f, OffMousePos.Y * 0.005f, 0);
                MovingOrientation = newRotation;

                // Save the offset between mousecoordinates, and the current mouse pos
                OffMousePos = PreMousePos - new Vector2(ms.X, ms.Y);
                PreMousePos = new Vector2(ms.X, ms.Y);
            }
            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();

            if (Bike.PluggedIn)
            {
                BikeState bikeState = Bike.GetState();
                PhysicalRepresentation.SpeedUp(bikeState.CurrentSpeed.InKilometerPerHour * motionFactor);
                PhysicalRepresentation.Steer(bikeState.CurrentSteering.InDegree * motionFactor);
                //Console.WriteLine(bikeState.CurrentSteering.AsFullRoundRadian);
                newRotation = Rotate(-(MathHelper.ToRadians(bikeState.CurrentSteering.AsFullRoundRadian)) * 0.01f * motionFactor, 0, 0);
                //MovingOrientation = newRotation;
                PhysicalRepresentation.Rotate(newRotation);
                PhysicalRepresentation.Translate(Vector3.Transform((bikeState.CurrentSpeed.InMeterPerSecond * motionFactor * new Vector3(0, 0, 1)), Matrix.CreateFromQuaternion(MovingOrientation)));
               // Console.WriteLine("rounds: " + bikeState.CurrentSpeed.Rounds);
                //PhysicalRepresentation.RollForward(180 * bikeState.CurrentSpeed.Rounds);
            }
            else
            {

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
                Vector3 newTranslation = Vector3.Transform(moveVector * time * 0.2f, Matrix.CreateFromQuaternion(newRotation));
                PhysicalRepresentation.Translate(newTranslation);
                PhysicalRepresentation.Rotate(newRotation);
                //PhysicalRepresentation.RotateAbsolute(MovingOrientation);
            }
            //Translation = Vector3.Transform(vectorToAdd, Matrix.Cre
        }

        ////Mouse-Control
        //MouseState OriginalMousState;
        //float LeftrightRot = MathHelper.PiOver2;
        //float UpdownRot = -MathHelper.Pi / 10.0f;
        //float RotationSpeed = 0.3f;
        //float MoveSpeed = 30.0f;
        //int HalfViewPortWidth;
        //int HalfViewPortHeight;

        //public ActiveBehaviour(float rotationSpeed, float moveSpeed):this()
        //{
        //    RotationSpeed = rotationSpeed;
        //    MoveSpeed = moveSpeed;
        //}

        //public ActiveBehaviour() : base()
        //{
        //    HalfViewPortWidth = Util.GetInstance().Device.Viewport.Width / 2;
        //    HalfViewPortHeight = Util.GetInstance().Device.Viewport.Height / 2;
        //    Mouse.SetPosition(HalfViewPortWidth, HalfViewPortHeight);
        //    OriginalMousState = Mouse.GetState();
        //    NewLookAt = Vector3.Zero;
        //    NewPosition = Vector3.Zero;
        //}

        //private void ProcessInput(float amount)
        //{
        //    MouseState currentMouseState = Mouse.GetState();
        //    Matrix cameraRotation;
        //    Vector3 cameraRotatedTarget;

        //    if (currentMouseState != OriginalMousState)
        //    {
        //        float xDifference = currentMouseState.X - OriginalMousState.X;
        //        float yDifference = currentMouseState.Y - OriginalMousState.Y;
        //        LeftrightRot -= RotationSpeed * xDifference * amount;
        //        UpdownRot += RotationSpeed * yDifference * amount;
        //        //TODO: Überprüfen, ob doch zu jedem "Update" die aktuellen ViewPort-Maße berechnet werden sollten
        //        Mouse.SetPosition(HalfViewPortWidth, HalfViewPortHeight);

                
        //        cameraRotation = Matrix.CreateRotationX(UpdownRot) * Matrix.CreateRotationY(LeftrightRot);
        //        cameraRotatedTarget = Vector3.Transform(OriginalLookAt, cameraRotation);
        //        NewLookAt = NewPosition + cameraRotatedTarget;
        //        NewUp = Vector3.Transform(OriginalUpVector, cameraRotation);
        //    }

        //    Vector3 moveVector = new Vector3(0, 0, 0);
        //    KeyboardState keyState = Keyboard.GetState();
        //    if (keyState.IsKeyDown(Keys.Up))
        //        moveVector -= new Vector3(0, 0, -1);
        //    if (keyState.IsKeyDown(Keys.Down))
        //        moveVector -= new Vector3(0, 0, 1);
        //    if (keyState.IsKeyDown(Keys.Right))
        //        moveVector += new Vector3(-1, 0, 0);
        //    if (keyState.IsKeyDown(Keys.Left))
        //        moveVector += new Vector3(1, 0, 0);
        //    if (keyState.IsKeyDown(Keys.Q))
        //        moveVector += new Vector3(0, 1, 0);
        //    if (keyState.IsKeyDown(Keys.Z))
        //        moveVector += new Vector3(0, -1, 0);
        //    Vector3 vectorToAdd = moveVector * amount;

        //    cameraRotation = Matrix.CreateRotationX(UpdownRot) * Matrix.CreateRotationY(LeftrightRot);
        //    Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
        //    NewPosition += MoveSpeed * rotatedVector;

        //    cameraRotatedTarget = Vector3.Transform(OriginalLookAt, cameraRotation);
        //    NewLookAt = NewPosition + cameraRotatedTarget;

        //    NewUp = Vector3.Transform(OriginalUpVector, cameraRotation);
        //}


        //public override void CalculateNewValues(Vector3 oldPosition, Vector3 oldLookAt, Vector3 oldUp, float time)
        //{
        //    //KeyboardState currentKeyboardState = Keyboard.GetState();

        //    NewLookAt = oldLookAt;
        //    NewPosition = oldPosition;
        //    NewUp = oldUp;
        //    //// Check for input to rotate the camera.
        //    //float pitch = 0;
        //    //float turn = 0;

        //    //if (currentKeyboardState.IsKeyDown(Keys.Up))
        //    //    pitch += time * 0.001f;

        //    //if (currentKeyboardState.IsKeyDown(Keys.Down))
        //    //    pitch -= time * 0.001f;

        //    //if (currentKeyboardState.IsKeyDown(Keys.Left))
        //    //    turn += time * 0.001f;

        //    //if (currentKeyboardState.IsKeyDown(Keys.Right))
        //    //    turn -= time * 0.001f;

        //    //Vector3 cameraRight = Vector3.Cross(Vector3.Up, NewLookAt);
        //    //Vector3 flatFront = Vector3.Cross(cameraRight, Vector3.Up);

        //    //Matrix pitchMatrix = Matrix.CreateFromAxisAngle(cameraRight, pitch);
        //    //Matrix turnMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, turn);

        //    //Vector3 tiltedFront = Vector3.TransformNormal(NewLookAt, pitchMatrix *
        //    //                                              turnMatrix);

        //    //// Check angle so we cant flip over
        //    //if (Vector3.Dot(tiltedFront, flatFront) > 0.001f)
        //    //{
        //    //    NewLookAt = Vector3.Normalize(tiltedFront);
        //    //}

        //    //// Check for input to move the camera around.
        //    //if (currentKeyboardState.IsKeyDown(Keys.W))
        //    //    NewPosition += NewLookAt * time * 0.1f;

        //    //if (currentKeyboardState.IsKeyDown(Keys.S))
        //    //    NewPosition -= NewLookAt * time * 0.1f;

        //    //if (currentKeyboardState.IsKeyDown(Keys.A))
        //    //    NewPosition += cameraRight * time * 0.1f;

        //    //if (currentKeyboardState.IsKeyDown(Keys.D))
        //    //    NewPosition -= cameraRight * time * 0.1f;
        //    ProcessInput(time);
        //}
       
    }
}
