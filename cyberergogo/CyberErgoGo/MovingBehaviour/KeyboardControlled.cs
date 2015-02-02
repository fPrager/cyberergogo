using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CyberErgoGo
{
    class KeyboardControlled : Behaviour
    {
        float Yaw = 0;
        float Pitch = 0;
        float Roll = 0;
      

        const float rotationSpeed = 0.15f;
        const float MoveSpeed = 0.5f;

        float SpeedInKMH = 0;
        const float BrakeSpeed = 0.1f;
        const float AccelerationSpeed = 0.2f;

        float SteeringAngle = 0;
        const float SteeringSpeed = 2f;
        const float ToZeroAngle = 0.1f;

        Vector3 OriginalLookAt;
        Quaternion OriginalRotation;

        public KeyboardControlled()
            : this(new Vector3(0, 0, 1))
        { }

        public KeyboardControlled(Vector3 lookAt)
            : base(lookAt)
        {
            OriginalRotation = new Quaternion(lookAt, 1);
            OriginalLookAt = lookAt;
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

        private void CheckSpeed()
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.GetPressedKeys().Contains(Keys.NumPad8))
                SpeedInKMH += AccelerationSpeed;
            if (SpeedInKMH > 0) SpeedInKMH -= BrakeSpeed; 
            
        }

        private void CheckRotation()
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.GetPressedKeys().Contains(Keys.NumPad4))
                SteeringAngle -= SteeringSpeed;

            if (keyboard.GetPressedKeys().Contains(Keys.NumPad6))
                SteeringAngle += SteeringSpeed;

            if (SteeringAngle > 0) SteeringAngle -= ToZeroAngle;
            if (SteeringAngle < 0) SteeringAngle += ToZeroAngle; 
        }

        public override void CalculateNewValues(float time, float motionFactor)
        {
            CheckSpeed();

            Quaternion newRotation = Quaternion.Identity;
            PhysicalRepresentation.SpeedUp(SpeedInKMH * motionFactor);
            PhysicalRepresentation.Steer(SteeringAngle * motionFactor);
            newRotation = Rotate(-(MathHelper.ToRadians(SteeringAngle)) * 0.01f * motionFactor, 0, 0);
            PhysicalRepresentation.Rotate(newRotation);
            PhysicalRepresentation.Translate(Vector3.Transform((SpeedInKMH * motionFactor * new Vector3(0, 0, 1)), Matrix.CreateFromQuaternion(MovingOrientation)));
              
        }


    }
}
