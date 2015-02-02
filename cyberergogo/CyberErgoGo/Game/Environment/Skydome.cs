using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class Skydome
    {
        Texture2D CloudMap;
        Texture2D GradientSky;
        Model Dome;
        const float CloudMovingSpeed = 0.003f;
        float CloudSetOff;

        public Skydome()
        { 
            
        }

        public void LoadContent(Effect newModelEffect)
        {
            Util util = Util.GetInstance();
            util.LoadFile(ref Dome, "Skydome", "Dome");
            util.SetEffect(ref Dome, newModelEffect);
            util.LoadFile(ref CloudMap, "Skydome", "skymap");
            util.LoadFile(ref GradientSky, "Skydome", "gradient");
        }

        public void Update(float elapsedGameTimeInMilliseconds)
        {
            CloudSetOff += CloudMovingSpeed * elapsedGameTimeInMilliseconds/1000;
            if (CloudSetOff >= 1)
                CloudSetOff = 0;
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Matrix[] modelTransform = new Matrix[Dome.Bones.Count];
            Dome.CopyAbsoluteBoneTransformsTo(modelTransform);

            foreach (ModelMesh mesh in Dome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransform[mesh.ParentBone.Index] * world;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkydomeShading"];
                    currentEffect.Parameters["xWorldMatrix"].SetValue(worldMatrix);
                    currentEffect.Parameters["xViewMatrix"].SetValue(view);
                    currentEffect.Parameters["xProjectionMatrix"].SetValue(projection);
                    currentEffect.Parameters["xSkyboxTexture"].SetValue(CloudMap);
                    currentEffect.Parameters["xSkyboxGradientTexture"].SetValue(GradientSky);
                    currentEffect.Parameters["xCloudMoving"].SetValue(CloudSetOff);
                }
                mesh.Draw();
            }
            
        }

        public float GetCloudSetOff()
        {
            return CloudSetOff;
        }

        public Texture2D GetCloudTexture()
        {
            return CloudMap;
        }
    }
}
