using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberErgoGo
{
    interface IConditionObserver
    {
        void ConditionChanged(Condition condition, List<ParameterIdentifier> changedParameters);
    }
}
