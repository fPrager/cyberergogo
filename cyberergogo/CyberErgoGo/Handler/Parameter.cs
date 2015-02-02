using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    enum ParameterIdentifier
    {
        //general parameter names
        Position,
        Width,
        Height,
        Rotation,
        MovingOrientation,
        MovingBehaviour,

        //terrain specific
        ColorHeightRate,
        Zooming,
        MinHeight,
        MaxHeight,
        HeightValues,
        Level0Texture,
        Level0to1,
        Level1Texture,
        Level1to2,
        Level2Texture,
        Level2to3,
        Level3Texture,
        StreetTexture,

        //view specific
        EyeOffset,
        Aperture,
        LookAt,
        BackBufferWidth,
        BackBufferHeight,
        IsStereo,
        IsFullScreen,

        //model specific
        ShapeVertices,
        ShapeIndices,
        Scale,
        PhysicalRepresentation,

        //object specific
        Bounding,
        Mass,
        LinearVelocity,

        //gamePlay specific
        Gravity,
        GameColor,

        //style specific
        GameFont,
        Up,
        FocusPlane
    }

    class PossibleTypeList 
    {
        List<Type> Types = new List<Type>();
        
        public PossibleTypeList()
        {
            Types.Add(typeof(int));
            Types.Add(typeof(float));
            Types.Add(typeof(double));
            Types.Add(typeof(String));
            Types.Add(typeof(Vector3));
            Types.Add(typeof(Matrix));
        }
    }

    class Parameter
    {
        bool Changed;
        ParameterIdentifier Identifier;
        ConditionID ParentID;

        Object Value;

        public Parameter(Object value, ParameterIdentifier identifier, ConditionID parentID)
        {
            Value = value;
            Identifier = identifier;
            ParentID = parentID;
        }

        public ParameterIdentifier GetIdentifier()
        {
            return Identifier;
        }

        public void ChangeValue(Object value)
        {
            if (value != Value)
            {
                Changed = true;
                Value = value;
            }
        }

        public ConditionID GetParentID() 
        {
            return ParentID;
        }

        public Object GetValue()
        {
           return Value;
        }

        public bool HasChanged()
        {
            return Changed;
        }

        public void WasReaded()
        {
            Changed = false;
        }
    }
}
