using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    /// <summary>
    /// This screen shows the starting of the game
    /// </summary>
    class LoadingScreen:GameScreen
    {
        Effect LevelSelectionEffect;
        LevelContainer Container;
        GraphicsDevice Device;
        SpriteFont Font;
        SpriteBatch SpriteBatch;
        AnimatedBillboard LoadingAnimation;

        public LoadingScreen(String name)
            : base(name)
        {
        }
    }
}
