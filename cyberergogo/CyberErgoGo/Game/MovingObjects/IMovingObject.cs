using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    interface IMovingObject
    {
        void Draw(Effect effect, Matrix projectionMatrix, Matrix viewMatrix);
        void Load(Effect effect);
        void Load();
        void Update(Matrix scale, Matrix rotation, Matrix translation);
        void UpdateMeshEffect(Effect effect);
        void SetTransformation(Matrix scale, Matrix rotation, Matrix translation);
        void SetTransformation(BoundingSphere wantedBounding, Matrix rotation);
        void SetTransformation(BoundingSphere wantedBounding);
    }
}
