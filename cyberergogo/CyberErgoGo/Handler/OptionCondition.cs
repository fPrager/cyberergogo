using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    class OptionCondition:Condition
    {
        public int BackBufferWidth
        {
            get { return (int)GetParameterValue(ParameterIdentifier.BackBufferWidth); }
            set { SetParameter(ParameterIdentifier.BackBufferWidth, value); }
        }

        public int BackBufferHeight
        {
            get { return (int)GetParameterValue(ParameterIdentifier.BackBufferHeight); }
            set { SetParameter(ParameterIdentifier.BackBufferHeight, value); }
        }

        public bool IsStereo
        {
            get { return (bool)GetParameterValue(ParameterIdentifier.IsStereo); }
            set { SetParameter(ParameterIdentifier.IsStereo, value); }
        }

        public bool IsFullScreen
        {
            get { return (bool)GetParameterValue(ParameterIdentifier.IsFullScreen); }
            set { SetParameter(ParameterIdentifier.IsFullScreen, value); }
        }

        public SpriteFont GameFont
        {
            get { return (SpriteFont)GetParameterValue(ParameterIdentifier.GameFont); }
            set { SetParameter(ParameterIdentifier.GameFont, value); }
        }

        public OptionCondition()
            : base(ConditionID.OptionCondition)
        {
            Parameters.Add(new Parameter(2800, ParameterIdentifier.BackBufferWidth, ID));
            Parameters.Add(new Parameter(1050, ParameterIdentifier.BackBufferHeight, ID));
            Parameters.Add(new Parameter(true, ParameterIdentifier.IsStereo, ID));
            Parameters.Add(new Parameter(true, ParameterIdentifier.IsFullScreen, ID));
            Parameters.Add(new Parameter(null, ParameterIdentifier.GameFont, ID));
        }
    }
}
