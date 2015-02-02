using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CyberErgoGo
{
    enum VWCPState
    { 
        Wheel,
        Sphere,
        Disk
    }
    class VWCP:MovingObject
    {
        VWCPState State;

        VWCPSpherePhysic SpherePhysic;
        const float SphereMass = 100;
        const float SphereSize = 2f * OverallSetting.SizeFactor;

        VWCPWheelPhysic3 WheelPhysic;
        const float WheelMass = 5000;
        const float WheelSize = 2f * OverallSetting.SizeFactor;

        VWCPDiskPhysic DiskPhysic;
        const float DiskMass = 10;
        const float DiskSize = 2f * OverallSetting.SizeFactor;

        Model SphereModel;
        float SphereModelScaleToOne;

        Model WheelModel;
        float WheelModelScaleToOne;

        Model DiskModel;
        float DiskModelScaleToOne;

        Vector3 CurrentPosition;
        Quaternion CurrentRotation;
        
        Vector3 CurrentLookLeft;
        Matrix WorldTransformOfSecondWheel = Matrix.Identity;

        KeyboardState OldKeyboardState;

        bool ShowPhysicChange = false;
        int CurrentPhysicIndex = 1;
        int SelectedPhysicIndex = 1;
        float AnimationFactor = 0;
        float AnimationSpeed = 0.2f;
        Vector3 PhysicChangeTranslation = new Vector3(-10*OverallSetting.SizeFactor,0,0);
        int StartedPhysicIndex = 1;

        public VWCP()
            : this(1)
        { 
        
        }

        public VWCP(int index)
            :this(index, GetRepresentationWithThisIndex(index))
        {
            
        }

        private VWCP(int index, IPhysicalRepresentation wheelPhysic)
            : base(wheelPhysic, new ActiveBehaviour())
        {
            SpherePhysic = new VWCPSpherePhysic(SphereSize, Vector3.Zero, SphereMass);
            WheelPhysic = new VWCPWheelPhysic3(WheelSize, WheelMass);
            DiskPhysic = new VWCPDiskPhysic(DiskSize, Vector3.Zero, DiskMass);
            if(index ==0)
                State = VWCPState.Disk;
            else
                if(index==2)
                    State = VWCPState.Sphere;
                else
                    State = VWCPState.Wheel;
        }

        static private IPhysicalRepresentation GetRepresentationWithThisIndex(int index)
        {
            IPhysicalRepresentation representation = new VWCPWheelPhysic3(WheelSize, WheelMass);
            if(index == 0)
                representation = new VWCPDiskPhysic(DiskSize, Vector3.Zero, DiskMass);
            if(index == 2)
                representation = new VWCPSpherePhysic(SphereSize, Vector3.Zero, SphereMass); ;
            return representation;
        }

        private VWCP(VWCPWheelPhysic3 wheelPhysic)
            : base(wheelPhysic, new ActiveBehaviour())
        {
            SpherePhysic = new VWCPSpherePhysic(SphereSize, Vector3.Zero, SphereMass);
            WheelPhysic = wheelPhysic;
            DiskPhysic = new VWCPDiskPhysic(DiskSize, Vector3.Zero, DiskMass);
            State = VWCPState.Wheel;
        }




        public override void StartPhysicChanging()
        {
            ShowPhysicChange = true; 
            CurrentPosition = MovingBehaviour.GetPosition();
            CurrentRotation = MovingBehaviour.GetMovingOrientation();
        }

        public override void StopPhysicChanging()
        {
            ShowPhysicChange = false;
        }

        public override void ChangePhysicTo(int indexOfPhysic)
        {
            SelectedPhysicIndex = indexOfPhysic;
        }

        void ChangeState(int indexOfPhysic)
        {


            switch (indexOfPhysic)
            {
                case 0:
                    SetPhysicalRepresentation(SpherePhysic);
                    MovingBehaviour.SetBounding(new BoundingSphere(CurrentPosition, SphereSize));
                    Console.WriteLine("to sphere");
                    State = VWCPState.Sphere;
                    break;
                case 1:

                    SetPhysicalRepresentation(WheelPhysic);
                    MovingBehaviour.SetBounding(new BoundingSphere(CurrentPosition, WheelSize));
                    State = VWCPState.Wheel;
                    Console.WriteLine("to wheel");
                    break;
                case 2:
                    SetPhysicalRepresentation(DiskPhysic);
                    MovingBehaviour.SetBounding(new BoundingSphere(CurrentPosition, DiskSize));
                    State = VWCPState.Disk;
                    Console.WriteLine("to disk");
                    break;
                default:
                    break;
            }
            MovingBehaviour.ExternalRotation(CurrentRotation);
        }

        public override void Draw(Effect effect, Microsoft.Xna.Framework.Matrix projectionMatrix, Microsoft.Xna.Framework.Matrix viewMatrix)
        {
            if (!ShowPhysicChange)
            {
                switch (State)
                {
                    case VWCPState.Sphere:
                        DrawModel(SphereModel, Matrix.CreateScale(SphereSize*SphereModelScaleToOne) * WorldMatrix, viewMatrix, projectionMatrix);
                        break;
                    case VWCPState.Wheel:
                        DrawModel(WheelModel, Matrix.CreateScale(WheelSize*1.1f*WheelModelScaleToOne) * WorldMatrix, viewMatrix, projectionMatrix);
                        //DrawModel(WheelModel, Matrix.CreateScale(scaleFactor * WheelModelScaleToOne) * WorldTransformOfSecondWheel, viewMatrix, projectionMatrix);    
                    //DrawModel(WheelModel, Matrix.CreateScale(PhysicalRepresentation.GetRadius() * WheelModelScaleToOne) * WheelPhysic.GetWorldTransformFromBackWheel(), viewMatrix, projectionMatrix);
                      //DrawModel(SphereModel, Matrix.CreateScale(1 * SphereModelScaleToOne) * Matrix.CreateTranslation(CurrentRotation.Up * 10) * Matrix.CreateTranslation(MovingBehaviour.GetPosition()), viewMatrix, projectionMatrix);
                        //DrawModel(SphereModel, Matrix.CreateScale(1 * SphereModelScaleToOne) * Matrix.CreateTranslation(CurrentRotation.Up * 10) * Matrix.CreateTranslation(MovingBehaviour.GetPosition()), viewMatrix, projectionMatrix);
                        break;
                    case VWCPState.Disk:
                        DrawModel(DiskModel, Matrix.CreateScale(DiskSize*DiskModelScaleToOne) * WorldMatrix, viewMatrix, projectionMatrix);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Matrix changedWorldMatrix = Matrix.CreateTranslation(CurrentLookLeft * 5 * AnimationFactor * OverallSetting.SizeFactor) * Matrix.CreateTranslation(0, 7 * OverallSetting.SizeFactor, 0) * Matrix.CreateTranslation(WorldMatrix.Translation);
               
                Vector3 grayColor = new Vector3(0.5f, 0.5f, 0.5f);
                if (State == VWCPState.Sphere)
                    DrawModel(SphereModel, Matrix.CreateScale(SphereSize * SphereModelScaleToOne) * Matrix.CreateTranslation(CurrentLookLeft * SphereSize * 5 * OverallSetting.SizeFactor) * Matrix.CreateTranslation(0, -7 * -AnimationFactor * OverallSetting.SizeFactor, 0) * changedWorldMatrix, viewMatrix, projectionMatrix);
                else
                    DrawModel(SphereModel, Matrix.CreateScale(SphereSize * SphereModelScaleToOne) * Matrix.CreateTranslation(CurrentLookLeft * SphereSize * 3 * OverallSetting.SizeFactor) * changedWorldMatrix, viewMatrix, projectionMatrix, grayColor);
                if (State == VWCPState.Wheel)
                    DrawModel(WheelModel, Matrix.CreateScale(WheelSize * 1.1f * WheelModelScaleToOne) * Matrix.CreateTranslation(0, -7 * (1 - Math.Abs(AnimationFactor)) * OverallSetting.SizeFactor, 0) * changedWorldMatrix, viewMatrix, projectionMatrix);
                else
                    DrawModel(WheelModel, Matrix.CreateScale(WheelSize * 1.1f * WheelModelScaleToOne) * changedWorldMatrix, viewMatrix, projectionMatrix, grayColor);
                
                if (State == VWCPState.Disk)
                    DrawModel(DiskModel, Matrix.CreateScale(DiskSize * DiskModelScaleToOne) * Matrix.CreateTranslation(0, -5 * AnimationFactor * OverallSetting.SizeFactor, 0) * Matrix.CreateTranslation(CurrentLookLeft * DiskSize * -5 * OverallSetting.SizeFactor) * changedWorldMatrix, viewMatrix, projectionMatrix);                       
                else
                    DrawModel(DiskModel, Matrix.CreateScale(DiskSize * DiskModelScaleToOne) * Matrix.CreateTranslation(CurrentLookLeft * DiskSize * -3 * OverallSetting.SizeFactor) * changedWorldMatrix, viewMatrix, projectionMatrix, grayColor);                       
                
            }
        }
        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        { 
            DrawModel( model,  world,  view,  projection, new Vector3(1f,0,0));
        }
        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection, Vector3 color)
        {
            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect meshEffect in mesh.Effects)
                {
                    meshEffect.CurrentTechnique = meshEffect.Techniques["MovingObjectShading"];
                    meshEffect.Parameters["xWorldMatrix"].SetValue(world);
                    meshEffect.Parameters["xProjectionMatrix"].SetValue(projection);
                    meshEffect.Parameters["xViewMatrix"].SetValue(view);
                    meshEffect.Parameters["xObjectColor"].SetValue(color);
                }
                mesh.Draw();
            }
        }

        public override void Load(Effect effect)
        {
            SphereModel = null;
            Util.GetInstance().LoadFile(ref SphereModel, "Models", "bowlingBall");
            Util.GetInstance().SetEffect(ref SphereModel, effect);
            SphereModelScaleToOne = Util.GetInstance().CalculateModelScaleToOneFactor(SphereModel);

            WheelModel = null;
            Util.GetInstance().LoadFile(ref WheelModel, "Models", "tire");
            Util.GetInstance().SetEffect(ref WheelModel, effect);
            WheelModelScaleToOne = Util.GetInstance().CalculateModelScaleToOneFactor(WheelModel);

            DiskModel = null;
            Util.GetInstance().LoadFile(ref DiskModel, "Models", "frisbee");
            Util.GetInstance().SetEffect(ref DiskModel, effect);
            DiskModelScaleToOne = Util.GetInstance().CalculateModelScaleToOneFactor(DiskModel);

            StartedPhysicIndex = CurrentPhysicIndex;
        }

        public override void Load()
        {

        }

        public override void Unload()
        {
            CurrentPhysicIndex = StartedPhysicIndex;
            SelectedPhysicIndex = CurrentPhysicIndex;
            ChangeState(CurrentPhysicIndex);
        }

        public override void UpdateMeshEffect(Effect effect)
        {
            Util.GetInstance().SetEffect(ref SphereModel, effect);
            Util.GetInstance().SetEffect(ref WheelModel, effect);
            Util.GetInstance().SetEffect(ref DiskModel, effect);
        }

        private void UpdateWorldMatrixFromSphere()
        {
            WorldMatrix = MovingBehaviour.GetWorldMatrix();
        }

        private void UpdateWorldMatrixFromDisk()
        {
            WorldMatrix = MovingBehaviour.GetWorldMatrix()*Matrix.CreateScale(1,1,1);
        }

        private void UpdateWorldMatrixFromWheel()
        {
            //WorldMatrix =Matrix.CreateFromYawPitchRoll(0, 0, MathHelper.ToRadians(90))* Matrix.CreateFromQuaternion(MovingBehaviour.GetRotation()) * Matrix.CreateTranslation(MovingBehaviour.GetPosition());
            WorldMatrix = MovingBehaviour.GetWorldMatrix() * Matrix.CreateScale(1, 1, 1);
            //if(typeof(VWCPWheelPhysic3) == MovingBehaviour.GetPhysicalRepresentation().GetType())
            //WorldTransformOfSecondWheel = ((VWCPWheelPhysic3)MovingBehaviour.GetPhysicalRepresentation()).GetWorldTransform2() * Matrix.CreateScale(1, 1, 1);
        }

        public override void ChangePhysic()
        {
            
        }

        public override void Update(float time)
        {
            bool readyToChangePhysic = false;
            ViewCondition vc = ((ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition));
            Vector3 currentLookAt = vc.LookAt-vc.CenterPosition;
            CurrentLookLeft = Vector3.Normalize(Vector3.Cross(currentLookAt, Vector3.Up));

            if (SelectedPhysicIndex != CurrentPhysicIndex)
            {
                if (SelectedPhysicIndex == 0)
                {
                    AnimationFactor -= AnimationSpeed;
                    if (AnimationFactor <= -1)
                    {
                        readyToChangePhysic = true;
                    }
                }
                if (SelectedPhysicIndex == 1 && CurrentPhysicIndex == 0)
                {
                    AnimationFactor += AnimationSpeed;
                    if (AnimationFactor <= AnimationSpeed && AnimationFactor >= -AnimationSpeed)
                    {
                        AnimationFactor = 0;
                        readyToChangePhysic = true;
                    }
                }

                if (SelectedPhysicIndex == 1 && CurrentPhysicIndex == 2)
                {
                    AnimationFactor -= AnimationSpeed;
                    if (AnimationFactor <= AnimationSpeed && AnimationFactor >= -AnimationSpeed)
                    {
                        AnimationFactor = 0;
                        readyToChangePhysic = true;
                    }
                }

                if (SelectedPhysicIndex == 2)
                {
                    AnimationFactor += AnimationSpeed;
                    if (AnimationFactor >= 1)
                    {
                        readyToChangePhysic = true;
                    }
                }
            }

            if (readyToChangePhysic)
            {
                CurrentPhysicIndex = SelectedPhysicIndex;
                ChangeState(SelectedPhysicIndex);
            }

            if(!ShowPhysicChange)
                base.Update(time);
            CurrentRotation = MovingBehaviour.GetMovingOrientation();
        }

        public override void PhysicalUpdate()
        {
            MyCondition.MovingOrientation = MovingBehaviour.GetMovingOrientation();
            MyCondition.Rotation = MovingBehaviour.GetRotation();
            MyCondition.Up = MovingBehaviour.GetUp();
            MyCondition.Position = MovingBehaviour.GetPosition();
            BoundingSphere bounding = new BoundingSphere(MyCondition.Position, MyCondition.ScaleFactor);
            MyCondition.Bounding = bounding;

            MyCondition.ConditionHasChanged();
            switch (State)
            {
                case VWCPState.Sphere:
                    UpdateWorldMatrixFromSphere();
                    break;
                case VWCPState.Wheel:
                    UpdateWorldMatrixFromWheel();
                    break;
                case VWCPState.Disk:
                    UpdateWorldMatrixFromDisk();
                    break;
                default:
                    break;
            }
        }
    }
}
