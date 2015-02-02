using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberErgoGo
{
    class Cube:SimpleModel
    {
        public Cube() : base(SimpleModelName.Cube, new RollingSphere(20, new Microsoft.Xna.Framework.Vector3(0, 0, 0), 200)) 
        { }
    }
}
