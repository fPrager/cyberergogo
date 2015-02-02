using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class GamePlayCondition:Condition
    {
        public Vector3 Gravity
        {
            get { return (Vector3)GetParameterValue(ParameterIdentifier.Gravity); }
            set { SetParameter(ParameterIdentifier.Gravity, value); }
        }

        public Vector3 GameColor
        {
            get { return (Vector3)GetParameterValue(ParameterIdentifier.GameColor); }
            set { SetParameter(ParameterIdentifier.GameColor, value); }
        }

        public GamePlayCondition()
            : base(ConditionID.GamePlayCondition)
        {
            Parameters.Add(new Parameter(new Vector3(0,-98.1f,0), ParameterIdentifier.Gravity, ID));
            Parameters.Add(new Parameter(new Vector3(0.3f, 0.3f, 0.3f), ParameterIdentifier.GameColor, ID));
        }
    }
}
