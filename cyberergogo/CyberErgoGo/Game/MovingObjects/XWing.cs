using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class XWing:SimpleModel
    {
        public XWing()
            : base(SimpleModelName.cylinder, new FlyingObject(20, Vector3.Zero, 200))
        {
        }
    }
}
