using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class NotMoving:Behaviour
    {
        Vector3 Position;

        public NotMoving(Vector3 position, Vector3 lookAt):base(lookAt)
        {
            Position = position;

        }

        public override void CalculateNewValues(float time, float motionFactor)
        {
            PhysicalRepresentation.TranslateAbsolute(Position);
        }
    }
}
