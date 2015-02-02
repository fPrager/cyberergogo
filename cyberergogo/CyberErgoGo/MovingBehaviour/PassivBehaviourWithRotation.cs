using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CyberErgoGo
{
    class PassivBehaviourWithRotation : Behaviour,IConditionObserver
    {
        ConditionID DependedCondition;
        ParameterIdentifier FollowedPositionParameter;
        ParameterIdentifier FollowedUpParameter;
        ParameterIdentifier FollowedRotationParameter;
        Vector3 PositionToFollow = Vector3.Zero;
        Quaternion RotationToFollow = new Quaternion(0, 0, 0, 0);
        int OldScrollWheelValue = 0;
        float DistanceFactor = 0;
        Vector3 DistanceVector = new Vector3(0, 0,0);
        float LookAcross = 0;
        Vector3 CurrentUp = Vector3.Up;
        //IObjectWithBehaviour Parent;

        public PassivBehaviourWithRotation(ParameterIdentifier positionID, ParameterIdentifier upID, ParameterIdentifier rotationID, ConditionID dependedCondition, Vector3 distanceVector, float distanceScrollFactor, float lookAcross)
            : base(Vector3.Zero)
        {
            FollowedPositionParameter = positionID;
            FollowedUpParameter = upID;
            FollowedRotationParameter = rotationID;
            DistanceVector = distanceVector;
            DistanceFactor = distanceScrollFactor;
            LookAcross = lookAcross;
            UpdateToFollow(ConditionHandler.GetInstance().RegisterMe(dependedCondition, this));
        }


        private void UpdateToFollow(Condition condition)
        {
            Parameter position = condition.GetParameter(FollowedPositionParameter);
            Parameter upParameter = condition.GetParameter(FollowedUpParameter);
            Parameter rotationParameter = condition.GetParameter(FollowedRotationParameter);
            if (position != null)
            {
                if (position.GetValue().GetType() == typeof(Vector3))
                    PositionToFollow = (Vector3)condition.GetParameter(FollowedPositionParameter).GetValue();
            }
            else
            {
                Console.WriteLine("It exists no parameter with id " + FollowedPositionParameter + " in " + condition.GetID());
            }

            if (upParameter != null)
            {
                if (upParameter.GetValue().GetType() == typeof(Vector3))
                    CurrentUp = (Vector3)condition.GetParameter(FollowedUpParameter).GetValue();
            }
            else
            {
                Console.WriteLine("It exists no parameter with id " + FollowedUpParameter + " in " + condition.GetID());
            }

            if (rotationParameter != null)
            {
                if (rotationParameter.GetValue().GetType() == typeof(Quaternion))
                    RotationToFollow = (Quaternion)condition.GetParameter(FollowedRotationParameter).GetValue();
            }
            else
            {
                Console.WriteLine("It exists no parameter with id " + FollowedUpParameter + " in " + condition.GetID());
            }
            if(PhysicalRepresentation!=null)
                CalculateNewValues(0f,1f);
            //if(Parent!=null)
            //    Parent.ForceBehaviourUpdate();
        }

        public override void CalculateNewValues(float time, float motionFactor)
        {
            MouseState ms = Mouse.GetState();
            //Mouse.SetPosition(HalfViewPortWidth, HalfViewPortHeight);
            if (OldScrollWheelValue < ms.ScrollWheelValue)
            {
                DistanceFactor *= 0.5f;
            }
            else
                if (OldScrollWheelValue > ms.ScrollWheelValue)
                {
                    DistanceFactor *= 2;
                }
            OldScrollWheelValue = ms.ScrollWheelValue;

            Vector3 distance = CurrentUp*DistanceVector.Y;
            Quaternion oldRotation = PhysicalRepresentation.GetRotation();
            Quaternion newRotation = RotationToFollow;
            PhysicalRepresentation.RotateAbsolute(newRotation);
            SetUpVector(CurrentUp);
            LookAt = (PositionToFollow +  new Vector3(0,LookAcross,0)) - PhysicalRepresentation.GetPosition();
            //distance = Vector3.Transform(CurrentUp*DistanceFactor, Matrix.CreateFromQuaternion(newRotation));
            distance += PositionToFollow;

            Vector3 translation = distance - PhysicalRepresentation.GetPosition();
            PhysicalRepresentation.Translate(translation);
        }

        public void ConditionChanged(Condition condition, List<ParameterIdentifier> changedParameters)
        {
            if (changedParameters.Contains(FollowedPositionParameter) || changedParameters.Contains(FollowedUpParameter))
                UpdateToFollow(condition);
        }

        public void SetFollowedPositionParameter(ParameterIdentifier positionID, ConditionID dependedCondition)
        {
            ConditionHandler.GetInstance().UnregisterMe(this);

            FollowedPositionParameter = positionID;
            UpdateToFollow(ConditionHandler.GetInstance().RegisterMe(dependedCondition, this));
        }

        public void SetFollowedRotationParameter(ParameterIdentifier rotationID, ConditionID dependedCondition)
        {
            ConditionHandler.GetInstance().UnregisterMe(this);

            FollowedUpParameter = rotationID;
            UpdateToFollow(ConditionHandler.GetInstance().RegisterMe(dependedCondition, this));
        }
    }

}
