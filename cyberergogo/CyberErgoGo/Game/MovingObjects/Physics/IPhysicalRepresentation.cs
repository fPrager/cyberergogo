using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;

namespace CyberErgoGo
{
    interface IPhysicalRepresentation
    {
        void Translate(Vector3 translation);
        void TranslateAbsolute(Vector3 translation);
        void Rotate(Quaternion rotation);
        void RotateAbsolute(Quaternion rotation);
        void Push(Vector3 veolation);
        void SpeedUp(float speed);
        void Steer(float angle);
        void WeightDown(float mass);
        void SetMass(float mass);
        void MoveForward();
        void MoveBack();
        void MoveRight();
        void MoveLeft();
        void RollForward(float degree);
        void SetAbsoluteSize(BoundingSphere bounding);
        void SetUpVector(Vector3 newUp);
        float GetRadius();
        Vector3 GetPosition();
        Vector3 GetUpVector();
        Quaternion GetRotation();
        Quaternion GetMovingOrientation();
        Matrix GetWorldTransform();
        ISpaceObject GetBEPUEntity();
        //PhysicalObject GetPhysicalObject();
        //void SetPhysicalObject(PhysicalObject physicalObject);
        void AddToCollisionChecker();
        void RemoveFromCollisionChecker();
    }
}
