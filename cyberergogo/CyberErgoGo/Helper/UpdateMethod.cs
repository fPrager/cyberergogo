using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class UpdateMethod
    {
        public delegate void Del(GameTime gameTime);
        public Del Method;
        private int UpdateEveryMilli = 0;
        private int SpanInMilli = 0;

        public UpdateMethod(Del method, int span) 
        {
            Method = method;
            UpdateEveryMilli = 0;
        }

        public UpdateMethod(Del method)
        {
            Method = method;
        }

        public void Update(GameTime gameTime) 
        {
            SpanInMilli += gameTime.ElapsedGameTime.Milliseconds;
            while (SpanInMilli >= UpdateEveryMilli) 
            {
                Method(gameTime);
                SpanInMilli -= UpdateEveryMilli;
            }
        }

    }
}
