using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BikeControls;

namespace CyberErgoGo
{
    class HorizontalRotation : Behaviour
    {
        Vector3 OriginalLookAt;
        Quaternion Rotation;
        float RotationSpeed = 1f;
        float BikeRotationSpeedDown = 0.1f;
        int TimeThressholdInSec = 2;
        bool ReadyToRotate = false;
        float AllTime = 0;

        public HorizontalRotation(Vector3 lookAt)
            : base(lookAt)
        {
            OriginalLookAt = lookAt;
            Rotation = Quaternion.Identity;
        }


        public override void CalculateNewValues(float time, float motionFactor)
        {
            if (!ReadyToRotate)
            {
                AllTime += time;
                if (AllTime / 1000 >= TimeThressholdInSec)
                    ReadyToRotate = true;
            }
            else
            {
                KeyboardState keyState = Keyboard.GetState();

                if (keyState.IsKeyDown(Keys.Right))
                {
                    Rotation *= Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(-RotationSpeed * time / 100), 0, 0);
                }
                if (keyState.IsKeyDown(Keys.Left))
                {
                    Rotation *= Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(RotationSpeed * time / 100), 0, 0);
                }

                if (Bike.PluggedIn)
                {
                    int SteeringAngle = Bike.GetState().CurrentSteering.InDegree;
                    float steeringRatio = 0;
                    if (SteeringAngle > 0)
                        steeringRatio = (float)SteeringAngle / (float)Bike.GetState().CurrentSteering.MaxSteering;
                    else
                        steeringRatio = (float)SteeringAngle / (float)Bike.GetState().CurrentSteering.MinSteering;

                    Rotation *= Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(-SteeringAngle * time / 100 * steeringRatio * steeringRatio * BikeRotationSpeedDown), 0, 0);
                }

                
            }
            LookAt = Vector3.Transform(OriginalLookAt, Rotation);
        }
       

    }
}
