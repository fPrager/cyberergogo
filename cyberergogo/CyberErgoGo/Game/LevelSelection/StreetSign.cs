using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    class TracPreviewShield:SimpleModel
    {

        Color PlateColor = Color.DarkGray;
        float Size = 4;
        Vector2 RelativePostionOnTerrain;

        public TracPreviewShield(Vector3 position, Quaternion rotation, int terrainWidth, int terrainHeight, float terrainScaleFactor)
            : base(SimpleModelName.plate, new NotPhysical(), new ActiveBehaviour()) 
        {
            this.SetPosition(position);
            this.SetRotation(rotation);
            this.SetScaleFactor(Size);

            RelativePostionOnTerrain = new Vector2(position.X / ((float)terrainWidth * terrainScaleFactor), position.Z / ((float)terrainHeight * terrainScaleFactor));
        }

        public override void Load(Effect effect)
        {
            if(RelativePostionOnTerrain!=null)
                effect.Parameters["xPositionAsTextureCoord"].SetValue(RelativePostionOnTerrain);
            base.Load(effect);
        }

        public void SetStreetMap(Texture2D oldStreetMap, Texture2D newStreetMap)
        {
            foreach (ModelMesh mesh in Shape.Meshes)
            {
                foreach (Effect meshEffect in mesh.Effects)
                {
                    meshEffect.Parameters["xTerrainPreviewTextureOld"].SetValue(oldStreetMap);
                    meshEffect.Parameters["xTerrainPreviewTextureCurrent"].SetValue(newStreetMap);
                }
            }
        }

        public void SetMorphing(float morphFactor)
        {
            foreach (ModelMesh mesh in Shape.Meshes)
            {
                foreach (Effect meshEffect in mesh.Effects)
                {
                    meshEffect.Parameters["xMorphingFactor"].SetValue(morphFactor);
                }
            }
        }

        public override void Draw(Effect effect, Matrix projectionMatrix, Matrix viewMatrix)
        {
            foreach (ModelMesh mesh in Shape.Meshes)
            {
                foreach (Effect meshEffect in mesh.Effects)
                {
                    meshEffect.Parameters["xTracPreviewColor"].SetValue(PlateColor.ToVector4());
                }
            }
            base.Draw(effect, projectionMatrix, viewMatrix, "TracPreviewShading");
        }
    }
}
