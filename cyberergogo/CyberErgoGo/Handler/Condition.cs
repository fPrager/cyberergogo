using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberErgoGo
{
    enum ConditionID 
    { 
        TerrainCondition,
        KeyboardCondition,
        MovingObjectCondition,
        BikeCondition,
        LevelCondition,
        GamePlayCondition,
        ViewCondition,
        OptionCondition,
        UnKnown
    }

    /// <summary>
    /// Stores current values of the game.
    /// </summary>
    class Condition
    {
        protected ConditionID ID;
        protected List<Parameter> Parameters;

        public Condition(ConditionID id) 
        {
            ID = id;
            Parameters = new List<Parameter>();
        }

        public ConditionID GetID()
        {
            return ID;
        }

        public void ConditionHasChanged()
        {
            ConditionHandler.GetInstance().ChangeCondition(this);
        }

        public List<ParameterIdentifier> GetChangedParameters()
        {
            List<ParameterIdentifier> changedParameters = new List<ParameterIdentifier>();
            foreach (Parameter p in Parameters)
                if (p.HasChanged()) changedParameters.Add(p.GetIdentifier());
            return changedParameters;
        }

        public void SetToUnchanged()
        {
            foreach (Parameter p in Parameters)
                p.WasReaded();
        }

        public Parameter GetParameter(ParameterIdentifier id)
        { 
            Parameter parameter = Parameters.Find(
                delegate(Parameter p)
                {
                    return p.GetIdentifier() == id;
                });
            if (parameter == null) Console.WriteLine("In " + ID + " exists no Parameter with the ID: " + id);
            return parameter;
        }

        protected Object GetParameterValue(ParameterIdentifier id)
        {
            Parameter parameter = Parameters.Find(
                delegate(Parameter p)
                {
                    return p.GetIdentifier() == id;
                });

            if (parameter == null)
            {
                Console.WriteLine("In " + ID + " exists no Parameter with the ID: " + id);
                return null;
            }

            return parameter.GetValue();
        }

        protected void SetParameter(ParameterIdentifier parameterId, Object value)
        {
            Parameter parameter = Parameters.Find(
                delegate(Parameter p)
                {
                    return p.GetIdentifier() == parameterId;
                });

            if (parameter == null)
                Parameters.Add(new Parameter(value, parameterId, ID));
            else
                parameter.ChangeValue(value);
        }

    }
}
