using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    /// <summary>
    /// This class handles an animation and represents a special MenuCanvas (containing multiple textures) in context with a frame-rate.
    /// The most important addition is the Update-Method to get the animation (texture change with time).
    /// </summary>
    class AnimatedCanvas:MenuCanvas
    {

        float FrameTime;   //pictures per second
        float ElapsedTime = 0;

        /// <summary>
        ///  Declares an animated canvas.
        /// </summary>
        /// <param name="frameTime">the elapsed time in milliseconds to change to the next texture</param>
        /// <param name="position">the position on the screen</param>
        /// <param name="width">the width of the canvas</param>
        /// <param name="height">the heigth of the canvas</param>
        /// <param name="textures">the array of textures/frames</param>
        /// <param name="opacity"></param>
        public AnimatedCanvas(float frameTime, Vector3 position, float width, float height, Texture2D[] textures, float opacity)
            : base(position, width, height, textures, opacity)
        {
            FrameTime = frameTime;
        }

        /// <summary>
        ///  Declares an animated canvas.
        /// </summary>
        /// <param name="frameTime">the elapsed time in milliseconds to change to the next texture</param>
        /// <param name="position">the position on the screen</param>
        /// <param name="zOffset">the offset in depth</param>
        /// <param name="width">the width of the canvas</param>
        /// <param name="height">the heigth of the canvas</param>
        /// <param name="textures">the array of textures/frames</param>
        /// <param name="opacity"></param>
        public AnimatedCanvas(float frameTime, Vector2 position, float zOffset, float width, float height, Texture2D[] textures, float opacity)
            : base(position, zOffset, width, height, textures, opacity)
        {
            FrameTime = frameTime;
        }

        /// <summary>
        ///  Declares an animated canvas.
        /// </summary>
        /// <param name="frameTime">the elapsed time in milliseconds to change to the next texture</param>
        /// <param name="position">the position on the screen</param>
        /// <param name="zOffset">the offset in depth</param>
        /// <param name="width">the width of the canvas</param>
        /// <param name="height">the heigth of the canvas</param>
        /// <param name="textures">the array of textures/frames</param>
        /// <param name="opacity"></param>
        /// <param name="color">the backgroundcolor of the canvas</param>
        public AnimatedCanvas(float frameTime, Vector2 position, float zOffset, float width, float height, Texture2D[] textures, float opacity, Vector3 color)
            : base(position, zOffset, width, height, textures, opacity, color)
        {
            FrameTime = frameTime;
        }

        /// <summary>
        /// the method to animate the canvas depanding on the time
        /// </summary>
        /// <param name="gameTime">the time of the game</param>
        public void Animate(GameTime gameTime)
        {
            Animate(gameTime.ElapsedGameTime.Milliseconds);
        }

        /// <summary>
        /// the method to animate the canvas depanding on the elapsed time
        /// </summary>
        /// <param name="elapsedTime">the elapsed time depanding on the least animate-call in milliseconds</param>
        public void Animate(float elapsedTime)
        {
            ElapsedTime += elapsedTime;
            if (ElapsedTime > FrameTime)
            {
                NextFrame();
                ElapsedTime = ElapsedTime % FrameTime;
            }
        }

        /// <summary>
        /// change to the next frame of the canvas
        /// </summary>
        private void NextFrame()
        {
            NextTexture();
        }
    }
}
