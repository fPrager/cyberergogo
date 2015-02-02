using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    enum GameFigureSituation 
    { 
        TurnedBack,
        InChange,
        TurnedFront,
        IsWalking,
        IsRunning,
        IsSitting
    }

    /// <summary>
    /// This class handles the moving and reactions of the game figure, which is riding on the vehicle (e.g. the space invader).
    /// The figure is an physical object with an passiv moving (just follows the vehicle).
    /// </summary>
    class GameFigure : PhysicalObject
    {
        Vector3 Position;
        Quaternion Orientation;
        Vector3 Up;
        BillboardInvader Figure;
        Effect LevelEffect;
        Quaternion Rotation = Quaternion.Identity;
        static Vector3 PhysicalDistance = new Vector3(0, 5f, 0) * OverallSetting.SizeFactor;
        static Vector3 DrawingDistance = new Vector3(0, 0, 0);
        public GameFigureSituation Situation = GameFigureSituation.TurnedFront;

        public GameFigure(BillboardInvader figure)
            : base(new NotPhysical(), new PassivBehaviourWithRotation(ParameterIdentifier.Position, ParameterIdentifier.Up, ParameterIdentifier.MovingOrientation, ConditionID.MovingObjectCondition, PhysicalDistance, 1, 0))
        {
            Figure = figure;
        }

        public override void PhysicalUpdate()
        {
            Position = MovingBehaviour.GetPosition();
            Rotation *= Quaternion.CreateFromYawPitchRoll(0,0,0.01f);
            Up = MovingBehaviour.GetUp();
            Figure.ChangeWorldPosition(Position + DrawingDistance);
        }

        public void Update(float elapsedTimeInMSec)
        {
            Figure.Update(elapsedTimeInMSec);
        }

        public void LoadContent(Effect effect)
        {
            LevelEffect = effect;
            Figure.LoadContent();
        }

        public void SetDrawDistance(Vector3 distance)
        {
            DrawingDistance = distance;
        }

#region Animations

        public void Turn()
        {
            if (Situation == GameFigureSituation.TurnedFront)
            {
                Figure.Turn();
                Situation = GameFigureSituation.TurnedBack;
            }
            else
                if (Situation == GameFigureSituation.TurnedBack)
                {
                    Figure.TurnBack();
                    Situation = GameFigureSituation.TurnedFront;
                }
            
        }

        public void TurnToBack()
        {
            if (Situation != GameFigureSituation.TurnedBack)
            {
                Figure.Turn();
                Situation = GameFigureSituation.TurnedBack;
            }
        }

        public void TurnToFront()
        {
            Console.WriteLine("now turn to front with state " + Situation);
            if (Situation != GameFigureSituation.TurnedFront)
            {
                Figure.TurnBack();
                Situation = GameFigureSituation.TurnedFront;
            }
        }

        public void BeHappy()
        {
            Console.WriteLine("now turn to front with state " + Situation);
            Figure.TurnBackAndWave();
        }


        public void StartChange()
        {
            if(Situation == GameFigureSituation.TurnedFront)
            Figure.StartChange();
            else
                if (Situation == GameFigureSituation.TurnedBack || Situation == GameFigureSituation.IsWalking)
                Figure.StartChangeFromBehind();
        }

        public void Change()
        {
            Figure.DoAChange();
        }

        public void StopChange()
        {
            if (Situation == GameFigureSituation.TurnedFront)
                Figure.StopChange();
            else
                if (Situation == GameFigureSituation.TurnedBack || Situation == GameFigureSituation.IsWalking)
                    Figure.StopChangeAndTurn();
        }

        public void Walk()
        {
            Situation = GameFigureSituation.IsWalking;
            Figure.Walk();
        }

        public void Run()
        {
            Situation = GameFigureSituation.IsRunning;
            Figure.Run();
        }

        public void StopWalking()
        {
            Situation = GameFigureSituation.TurnedBack;
            Figure.StopWalking();
        }

        public void StandUp()
        {
            Figure.StandUp();
            Situation = GameFigureSituation.TurnedBack;
        }

        public void SitDown()
        {
            Figure.SitDown();
            Situation = GameFigureSituation.IsSitting;
        }


#endregion

        public void Draw(GraphicsDevice device, Matrix world, Matrix view, Matrix projection)
        {
            LevelEffect.CurrentTechnique = LevelEffect.Techniques["Billboarding"];
            LevelEffect.Parameters["xBillboardWidth"].SetValue(Figure.GetWidth());
            LevelEffect.Parameters["xBillboardHeight"].SetValue(Figure.GetHeight());
            LevelEffect.Parameters["xBillboardTexture"].SetValue(Figure.GetTexture());
            LevelEffect.Parameters["xAllowedRotDir"].SetValue(Up);
            LevelEffect.Parameters["xToFlip"].SetValue(Figure.GetFlip());
            LevelEffect.Parameters["xTranslation"].SetValue(Figure.GetTranslation());

            LevelEffect.Parameters["xIsAnimated"].SetValue(true);
            LevelEffect.Parameters["xFramePos"].SetValue(Figure.GetCurrentSpriteLocation());
            LevelEffect.Parameters["xFrameDim"].SetValue(Figure.GetRelativeSpriteDimension());

            LevelEffect.Parameters["xWorldMatrix"].SetValue(world);
            //LevelEffect.Parameters["xBillboardRotation"].SetValue(Figure.GetRotationMatrix());
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullNone;
            device.SamplerStates[0] = SamplerState.LinearClamp;

            LevelEffect.Parameters["xAlphaTestDirection"].SetValue(1f);


            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.Indices = Figure.GetIndexBuffer();
                device.SetVertexBuffer(Figure.GetVertexBuffer());
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Figure.GetVertexBuffer().VertexCount, 0, Figure.GetIndexBuffer().IndexCount / 3);
            }

            LevelEffect.Parameters["xAlphaTestDirection"].SetValue(-1f);

            device.BlendState = BlendState.NonPremultiplied;
            device.DepthStencilState = DepthStencilState.DepthRead;

            foreach (EffectPass pass in LevelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.Indices = Figure.GetIndexBuffer();
                device.SetVertexBuffer(Figure.GetVertexBuffer());
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Figure.GetVertexBuffer().VertexCount, 0, Figure.GetIndexBuffer().IndexCount / 3);
            }
        }
    }
}
