using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class ViewCondition:Condition
    {
        public float EyeOffset
        {
            get { return (float)GetParameterValue(ParameterIdentifier.EyeOffset); }
            set { SetParameter(ParameterIdentifier.EyeOffset, value); }
        }

        public float FocusPlane
        {
            get { return (float)GetParameterValue(ParameterIdentifier.FocusPlane); }
            set { SetParameter(ParameterIdentifier.FocusPlane, value); }
        }

        public float Aperture
        {
            get { return (float)GetParameterValue(ParameterIdentifier.Aperture); }
            set { SetParameter(ParameterIdentifier.Aperture, value); }
        }

        public Vector3 LookAt
        {
            get { return (Vector3)GetParameterValue(ParameterIdentifier.LookAt); }
            set { SetParameter(ParameterIdentifier.LookAt, value); }
        }

        public Vector3 CenterPosition
        {
            get { return (Vector3)GetParameterValue(ParameterIdentifier.Position); }
            set { SetParameter(ParameterIdentifier.Position, value); }
        }

        public Vector3 Up
        {
            get { return (Vector3)GetParameterValue(ParameterIdentifier.Up); }
            set { SetParameter(ParameterIdentifier.Up, value); }
        }

        public Behaviour MovingBehaviour
        {
            get { return (Behaviour)GetParameterValue(ParameterIdentifier.MovingBehaviour); }
            set { SetParameter(ParameterIdentifier.MovingBehaviour, value); }
        }

        public ViewCondition()
            : base(ConditionID.ViewCondition)
        {
            Parameters.Add(new Parameter(0.3f, ParameterIdentifier.EyeOffset, ID));
            Parameters.Add(new Parameter(50f, ParameterIdentifier.FocusPlane, ID));
            Parameters.Add(new Parameter(29f, ParameterIdentifier.Aperture, ID));
            Parameters.Add(new Parameter(new Vector3(0,0,1), ParameterIdentifier.LookAt, ID));
            Parameters.Add(new Parameter(new Vector3(0, 0, 0), ParameterIdentifier.Position, ID));
            Parameters.Add(new Parameter(new Vector3(0, 1, 0), ParameterIdentifier.Up, ID));
            Parameters.Add(new Parameter(new NotMoving(new Vector3(0, 0, 0), Vector3.UnitZ), ParameterIdentifier.MovingBehaviour, ID));
        }
    }
}
