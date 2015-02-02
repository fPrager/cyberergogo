using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    enum GameObjectShape
    {
        Cube,
        Pin,
        Sphere
    }

    class GameObject
    {
        public Matrix WorldTransform;
        public GameObjectShape Shape;

        public GameObject(Matrix worldTransform, GameObjectShape shape)
        {
            WorldTransform = worldTransform;
            Shape = shape;
        }


    }
}
