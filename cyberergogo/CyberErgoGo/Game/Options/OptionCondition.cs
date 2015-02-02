using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public OptionCondition()
            : base(ConditionID.OptionCondition)
        {
            Parameters.Add(new Parameter(1366, ParameterIdentifier.BackBufferWidth, ID));
            Parameters.Add(new Parameter(768, ParameterIdentifier.BackBufferHeight, ID));
            Parameters.Add(new Parameter(false, ParameterIdentifier.IsStereo, ID));
            Parameters.Add(new Parameter(false, ParameterIdentifier.IsFullScreen, ID));
        }
    }
}
