using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    class BikeModel:MovingObject
    {
        BikePhysic Physic;

        public BikeModel()
            : this(new BikePhysic())
        {
        }

        private BikeModel(BikePhysic physic)
            : base(physic, new ActiveBehaviour())
        {
            Physic = physic;
        }

        Model WheelModel;
        Model RackModel;
        Matrix WorldMatrix;
        List<Matrix> RackMatrixes;
        List<Matrix> WheelMatrixes;

        float RatioToWantedBounding = 1.0f;

        public override void Draw(Microsoft.Xna.Framework.Graphics.Effect effect, Microsoft.Xna.Framework.Matrix projectionMatrix, Microsoft.Xna.Framework.Matrix viewMatrix)
        {
            foreach (Matrix worldMatrix in WheelMatrixes)
            {
                Matrix[] modelTransforms = new Matrix[WheelModel.Bones.Count];
                WheelModel.CopyAbsoluteBoneTransformsTo(modelTransforms);

                foreach (ModelMesh mesh in WheelModel.Meshes)
                {
                    foreach (Effect meshEffect in mesh.Effects)
                    {
                        meshEffect.CurrentTechnique = meshEffect.Techniques["MovingObjectShading"];
                        meshEffect.Parameters["xWorldMatrix"].SetValue(worldMatrix * WorldMatrix);
                        meshEffect.Parameters["xProjectionMatrix"].SetValue(projectionMatrix);
                        meshEffect.Parameters["xViewMatrix"].SetValue(viewMatrix);
                    }
                    mesh.Draw();
                }
            }


            foreach (Matrix worldMatrix in RackMatrixes)
            {
                Matrix[] modelTransforms = new Matrix[RackModel.Bones.Count];
                RackModel.CopyAbsoluteBoneTransformsTo(modelTransforms);

                foreach (ModelMesh mesh in RackModel.Meshes)
                {
                    foreach (Effect meshEffect in mesh.Effects)
                    {
                        meshEffect.CurrentTechnique = meshEffect.Techniques["MovingObjectShading"];
                        meshEffect.Parameters["xWorldMatrix"].SetValue(worldMatrix * WorldMatrix);
                        meshEffect.Parameters["xProjectionMatrix"].SetValue(projectionMatrix);
                        meshEffect.Parameters["xViewMatrix"].SetValue(viewMatrix);
                    }
                    mesh.Draw();
                }
            }
        }

        private void UpdateWorldMatrix()
        {
            WorldMatrix = MovingBehaviour.GetWorldMatrix();
            RackMatrixes = Physic.GetRackWordlTransform();
            WheelMatrixes = Physic.GetWheelWorldTransform();

            MyCondition.MovingOrientation = MovingBehaviour.GetMovingOrientation();
            MyCondition.Rotation = MovingBehaviour.GetRotation();
            MyCondition.Position = MovingBehaviour.GetPosition();
            BoundingSphere bounding = new BoundingSphere(MyCondition.Position, MyCondition.ScaleFactor);
            MyCondition.Bounding = bounding;

            MyCondition.ConditionHasChanged();
        }

        private void LoadModelsWithThisEffect(Effect effect)
        {
            WheelModel = null;
            Util.GetInstance().LoadFile(ref WheelModel, "Models", "SimpleWheel");
            foreach (ModelMesh mesh in WheelModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();

            RackModel = null;
            Util.GetInstance().LoadFile(ref RackModel, "Models", "Cube");
            foreach (ModelMesh mesh in RackModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
        }

        public override void Load(Microsoft.Xna.Framework.Graphics.Effect effect)
        {
            LoadModelsWithThisEffect(effect);
        }

        public override void Unload()
        { }

        public override void Load()
        {
            WheelModel = null;
            Util.GetInstance().LoadFile(ref WheelModel, "Models", "Wheel");
            RackModel = null;
            Util.GetInstance().LoadFile(ref RackModel, "Models", "Rack");
        }

        public override void UpdateMeshEffect(Microsoft.Xna.Framework.Graphics.Effect effect)
        {
            foreach (ModelMesh mesh in WheelModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();

            foreach (ModelMesh mesh in RackModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
        }


        public override void PhysicalUpdate()
        {
            UpdateWorldMatrix();
        }

    }
}
