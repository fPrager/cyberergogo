using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberErgoGo
{
    class SimpleWheel:SimpleModel
    {
        public SimpleWheel()
            : base(SimpleModelName.SimpleWheel, new RollingWheel(30,new Microsoft.Xna.Framework.Vector3(200,30,200), 20))
        {
        }
    }
}
