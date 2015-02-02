using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CyberErgoGo
{
    abstract class PhysicalObject
    {
        protected Behaviour MovingBehaviour;
        protected IPhysicalRepresentation PhysicalRepresentation;
        protected IPhysicalRepresentation StandartPhysicalRepresentation;

        protected PhysicalObject(IPhysicalRepresentation physicalRepresentation, Behaviour movingBehaviuor)
        {
           // physicalRepresentation.SetPhysicalObject(this);
            MovingBehaviour = movingBehaviuor;
            PhysicalRepresentation = physicalRepresentation;
            StandartPhysicalRepresentation = physicalRepresentation;
            MovingBehaviour.SetPhysicalRepresentation(ref PhysicalRepresentation);
        }

        protected PhysicalObject(Behaviour movingBehaviuor):this(new NotPhysical(), movingBehaviuor)
        {
        }

        public void SetStandartPhysicalRepresentation()
        {
            PhysicalRepresentation = StandartPhysicalRepresentation;
            MovingBehaviour.SetPhysicalRepresentation(ref StandartPhysicalRepresentation);
        }

        public abstract void PhysicalUpdate();

        public void Update(float elapsedGameTime, float motionFactor)
        {
            MovingBehaviour.Update(elapsedGameTime, motionFactor);
        }

        public void SetMovingBehaviour(Behaviour movingBehaviour)
        {
            MovingBehaviour = movingBehaviour;
            MovingBehaviour.SetPhysicalRepresentation(ref PhysicalRepresentation);
        }

        public void SetPhysicalRepresentation(IPhysicalRepresentation representation)
        {
            MovingBehaviour.SetPhysicalRepresentation(ref representation);
            PhysicalRepresentation = representation;
        }
    }
}
