using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.DataStructures;
using BEPUphysics.Vehicle;


namespace CyberErgoGo
{
    enum SimpleModelName
    {
        XWing,
        Cube,
        Fish,
        sphere0,
        spherehigh,
        spherelow,
        ball,
        SimpleWheel,
        airplane,
        ammo,
        arrow,
        bat,
        bigship,
        car,
        car2,
        chair,
        cytovirus,
        Dwarf,
        EvilDronelow,
        EvilDrone,
        Head_Sad,
        heli,
        knot,
        LandShark,
        ring,
        room,
        scannerarm,
        seafloor,
        skullocc,
        tiger,
        tiny,
        vinestart,
        snail,
        cylinder,
        plate
    }

    class SimpleModel : MovingObject
    {
        
        SimpleModelName Name;
        protected Model Shape;

        //Vector3 Position = Vector3.Zero;
        Vector3 LookAt = new Vector3(0,0,1);
        Vector3 Up = new Vector3(0,1,0);
        float ModelScaleToOne = 1f;

        //BoundingSphere Bounding;

        float RatioToWantedBounding = 1.0f;

        public SimpleModel(SimpleModelName name, IPhysicalRepresentation representation, Behaviour movingBehaviour):base(representation, movingBehaviour)
        {
            
            //MyCondition.PhysicalRepresentation = representation;

            //MovingBehaviour.SetOriginalVectors(MyCondition.Position, LookAt, Up);

            Name = name;

        }

        public SimpleModel(SimpleModelName name, IPhysicalRepresentation representation):this(name, representation, new ActiveBehaviour())
        {
        }

        private void LoadModelWithThisEffect(Effect effect)
        {
            Shape = null;
            Util.GetInstance().LoadFile(ref Shape, "Models", Name.ToString());
            Util.GetInstance().SetEffect(ref Shape, effect);
            CalculateModelScaleToOneFactor();
            UpdateConditionValues();
        }

        private void LoadModel()
        {
            Util.GetInstance().LoadFile(ref Shape, "Models", Name.ToString());
            CalculateModelScaleToOneFactor();
            UpdateConditionValues();
        }

        private void CalculateModelScaleToOneFactor()
        {
            BoundingSphere bounding = new BoundingSphere(Vector3.Zero, 0);
            foreach (ModelMesh mesh in Shape.Meshes)
                bounding = BoundingSphere.CreateMerged(bounding, mesh.BoundingSphere);
            ModelScaleToOne = (float)1 / bounding.Radius;
        }

        private void UpdateConditionValues() 
        {
            float scaleFactor = PhysicalRepresentation.GetRadius() * ModelScaleToOne;
            WorldMatrix = Matrix.CreateScale(scaleFactor) * MovingBehaviour.GetWorldMatrix();

            MyCondition.MovingOrientation = MovingBehaviour.GetMovingOrientation();
            MyCondition.Rotation = MovingBehaviour.GetRotation();
            MyCondition.Position = MovingBehaviour.GetPosition();
            BoundingSphere bounding = new BoundingSphere(MyCondition.Position, MyCondition.ScaleFactor);
            MyCondition.Bounding = bounding;

            MyCondition.ConditionHasChanged();
        }


        public override void Draw(Effect effect, Matrix projectionMatrix, Matrix viewMatrix)
        {
            Matrix[] modelTransforms = new Matrix[Shape.Bones.Count];
            Shape.CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh mesh in Shape.Meshes)
            {
                foreach (Effect meshEffect in mesh.Effects)
                {
                    meshEffect.CurrentTechnique = meshEffect.Techniques["TerrainObjectShading"];
                    meshEffect.Parameters["xWorldMatrix"].SetValue(WorldMatrix);
                    meshEffect.Parameters["xProjectionMatrix"].SetValue(projectionMatrix);
                    meshEffect.Parameters["xViewMatrix"].SetValue(viewMatrix);
                }
                mesh.Draw();
            }
        }

        public void Draw(Effect effect, Matrix projectionMatrix, Matrix viewMatrix, String currentTechnique)
        {
            Matrix[] modelTransforms = new Matrix[Shape.Bones.Count];
            Shape.CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh mesh in Shape.Meshes)
            {
                foreach (Effect meshEffect in mesh.Effects)
                {
                    meshEffect.CurrentTechnique = meshEffect.Techniques[currentTechnique];
                    meshEffect.Parameters["xWorldMatrix"].SetValue(WorldMatrix);
                    meshEffect.Parameters["xProjectionMatrix"].SetValue(projectionMatrix);
                    meshEffect.Parameters["xViewMatrix"].SetValue(viewMatrix);
                }
                mesh.Draw();
            }
        }

        public override void Load(Effect effect)
        {
            LoadModelWithThisEffect(effect);
        }

        public override void Load()
        {
            LoadModel();
        }

        public override void Unload()
        { 
            
        }

        public override void UpdateMeshEffect(Effect effect)
        {
            Util.GetInstance().SetEffect(ref Shape, effect);
        }

        public override void PhysicalUpdate()
        {
            //SetRotation(MovingBehaviour.GetRotation());
            //SetPosition(MovingBehaviour.GetPosition());
            UpdateConditionValues();
        }

    }
}
