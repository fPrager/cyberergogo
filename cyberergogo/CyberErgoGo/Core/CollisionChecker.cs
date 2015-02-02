using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Collidables;
using BEPUphysics.MathExtensions;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using BEPUphysics.Threading;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Materials;
using BEPUphysics.Entities;
using System.Diagnostics;

namespace CyberErgoGo
{
    class CollisionChecker:IConditionObserver
    {
        List<BoundingSphere> MovingObjectBoundings;
        private Space PlayGround;
        private List<Sphere> Balls;
        private List<Model> Models;
        bool MousePressed = false;
        private ISpaceObject MovingObjectEntity;
        private BEPUphysics.Collidables.Terrain TerrainEntity;
        private List<Box> PhysicalCubes;
        private List<Sphere> PhysicalSpheres;
        private List<BoundingSphere> BoundingSpheres;
        private List<MobileMesh> PhysicalMeshes;

        private Dictionary<Entity, Matrix> PhysicalObjects;
        private BoundingSphere MovingObjectBounding;
        
        private List<BoundingBox> CheckpointBoundings;
        private bool CollisionChecked = false;
        public bool CheckpointCollision
        {
            get { return CollisionChecked; }
            set { CollisionChecked = value; }
        }
        public int CheckpointCollisionIndex = -1;

        private List<ISpaceObject> Objects;

        Dictionary<GameObjectShape, MeshData> ShapeToModel;

        struct MeshData
        {
            public int[] Indeces;
            public Vector3[] Vertices;
            public float MeshToOneFactor;

            public MeshData(int[] indeces, Vector3[] vertices, float meshToOne)
            {
                Indeces = indeces;
                Vertices = vertices;
                MeshToOneFactor = meshToOne;
            }
        }
        
        public CollisionChecker()
        {
            ConditionHandler.GetInstance().RegisterMe(ConditionID.MovingObjectCondition, this);
            ConditionHandler.GetInstance().RegisterMe(ConditionID.TerrainCondition, this);
            ConditionHandler.GetInstance().RegisterMe(ConditionID.GamePlayCondition, this);

            PlayGround = new Space();
            if (Environment.ProcessorCount > 1)
            {
                //On windows, just throw a thread at every processor.  The thread scheduler will take care of where to put them.
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    PlayGround.ThreadManager.AddThread();
                }
            }
            PhysicalCubes = new List<Box>();
            PhysicalSpheres = new List<Sphere>();
            PhysicalMeshes = new List<MobileMesh>();

            ShapeToModel = new Dictionary<GameObjectShape, MeshData>();
            ShapeToModel.Add(GameObjectShape.Cube, new MeshData(new int[1], new Vector3[1],1));
            ShapeToModel.Add(GameObjectShape.Sphere, new MeshData(new int[1], new Vector3[1],1));
            ShapeToModel.Add(GameObjectShape.Pin, new MeshData(new int[1], new Vector3[1],1));

            PhysicalObjects = new Dictionary<Entity,Matrix>();
             
            Objects = new List<ISpaceObject>();
            CheckpointBoundings = new List<BoundingBox>();
            MovingObjectBounding = new BoundingSphere();
        }

        public void Initialize()
        {
            CheckpointCollision = false;
            CheckpointCollisionIndex = -1;
            CheckpointBoundings = new List<BoundingBox>();
            MovingObjectBounding = new BoundingSphere();
        }

        public void LoadMobileMeshes()
        {
            int i = 0;
            while (i < ShapeToModel.Keys.Count)
            {
                GameObjectShape shape = ShapeToModel.Keys.ElementAt(i);
                
                Model modelOfshape = null;
                try
                {
                    Util.GetInstance().LoadFile(ref modelOfshape, "Models", shape.ToString());
                }
                catch
                {
                    Console.WriteLine("There exists no modelfile in folder 'Models' with the name " + shape.ToString() + "!");
                }
                if (modelOfshape != null)
                {
                            int[] indeces = null;
                            Vector3[] vertices = null;
                            Util.GetInstance().GetVeticesAndIndeces(ref vertices, ref indeces, modelOfshape);
                            ShapeToModel[shape] = new MeshData(indeces, vertices, Util.GetInstance().CalculateModelScaleToOneFactor(modelOfshape));
                }
                        
                i++;
        }
        }


        public void AddObject(ISpaceObject spaceObject, bool isMovingObject)
        {
            MovingObjectEntity = spaceObject;
            if(spaceObject.Space != PlayGround)
                PlayGround.Add(spaceObject);
        }

        public void AddPhysicalObject(GameObjectShape shape, Vector3 position, Quaternion rotation, float width, float height, float length, float mass)
        {
            if (!ShapeToModel.Keys.Contains(shape))
            {
                Console.WriteLine("no such shape to collid");
                return;
            }

            Entity newObject;
            if (shape == GameObjectShape.Cube)
            {
                newObject = new Box(position, width, height, length,mass);
            }
            else
                if (shape == GameObjectShape.Sphere)
                {
                    newObject = new Sphere(position, (width + height + length) / 3,mass);
                }
            else
                newObject = new MobileMesh(ShapeToModel[shape].Vertices, ShapeToModel[shape].Indeces, new AffineTransform(new Vector3(width * ShapeToModel[shape].MeshToOneFactor, height * ShapeToModel[shape].MeshToOneFactor, length * ShapeToModel[shape].MeshToOneFactor), rotation, position), MobileMeshSolidity.Counterclockwise,mass);

            //newObject.WorldTransform *= Matrix.CreateScale(width, height, length);
            PhysicalObjects.Add(newObject, Matrix.CreateScale(width * ShapeToModel[shape].MeshToOneFactor, height * ShapeToModel[shape].MeshToOneFactor, length * ShapeToModel[shape].MeshToOneFactor));
            AddObject(newObject, false);
        }

        public void AddPhysicalCube(Vector3 position, Quaternion rotation, float width, float height, float length, float mass)
        {
            Box newCube = new Box(position, width, height, length, mass);
            newCube.Orientation = rotation;
            PhysicalCubes.Add(newCube);
            AddObject(newCube, false);
        }

        public void AddPhysicalSphere(Vector3 position, Quaternion rotation, int radius, float mass)
        {
            Sphere newSphere = new Sphere(position,radius, mass);
            newSphere.Orientation = rotation;
            PhysicalSpheres.Add(newSphere);
            AddObject(newSphere, false);
        }

        public void AddPhysicalMesh(Vector3[] vertices, int[] indeces, float mass)
        {
            MobileMesh newMesh = new MobileMesh(vertices, indeces, new AffineTransform(new Vector3(0, 0, 0)), MobileMeshSolidity.Solid, mass);
            PhysicalMeshes.Add(newMesh);
            AddObject(newMesh, false);
        }

        public void RemoveAllCubes()
        {
            foreach (Box b in PhysicalCubes)
                RemoveObject(b);
            PhysicalCubes.Clear();
        }

        public void RemoveAllSpheres()
        {
            foreach (Sphere s in PhysicalSpheres)
                RemoveObject(s);
            PhysicalSpheres.Clear();
        }

        public void RemoveAllPhysicalObjects()
        {
            foreach (Entity mesh in PhysicalObjects.Keys)
            {
                RemoveObject(mesh);
            }
            PhysicalObjects.Clear();
        }

        public void AddPhysicalSphere(Vector3 position, Quaternion rotation, float radius, float mass)
        {
            Sphere newSphere = new Sphere(position, radius, mass);
            newSphere.Orientation = rotation;
            PhysicalSpheres.Add(newSphere);
            PlayGround.Add(newSphere);
        }

        public void AddBoundingSphere(BoundingSphere sphere)
        { 
            BoundingSpheres.Add(sphere);
        }

        public void UpdateMovingObjectBounding(Vector3 position)
        {
            MovingObjectBounding.Center = position;
        }

        public void SetMovingObjectBounding(Vector3 position, float radius)
        {
            MovingObjectBounding = new BoundingSphere(position, radius);
        }

        public Vector3 GetMOObjectPosition()
        {
            return MovingObjectBounding.Center;
        }

        public void AddCheckPointBounding(Vector3 min, Vector3 max)
        {
            //Vector3 newMin = min + new Vector3(-40, 0, 0);
            BoundingBox newBox = new BoundingBox(min, max);
            Vector3[] corners = newBox.GetCorners();
            CheckpointBoundings.Add(newBox);
        }

        public Matrix GetCubeWorldTransform(int index)
        { 
            
            Box cube = PhysicalCubes.ElementAt(index);
            if (cube != null)
                return Matrix.CreateScale(cube.Width,cube.Height,cube.Length)*cube.WorldTransform;
            else
                return Matrix.Identity;
        }

        public Matrix GetPhysicalObjectWorldTransform(int index)
        {
            if (PhysicalObjects.Count > index - 1)
            {
                Entity physicalObject = PhysicalObjects.Keys.ElementAt(index);
                return PhysicalObjects[physicalObject] * physicalObject.WorldTransform;
            }
            return Matrix.Identity;
        }

        public Matrix GetSphereWorldTransform(int index)
        {
            Sphere sphere = PhysicalSpheres.ElementAt(index);
            if (sphere != null)
                return Matrix.CreateScale(sphere.Radius)*sphere.WorldTransform;
            else
                return Matrix.Identity;
        }


        public void RemoveObject(ISpaceObject spaceObject)
        {
            //if(PlayGround.Entities.Contains(spaceObject))
            PlayGround.Remove(spaceObject);
        }


        public void LoadGround()
        {
            if (TerrainEntity != null)
                if (TerrainEntity.Space == PlayGround) PlayGround.Remove(TerrainEntity);

            LoadTerrainEntity();

            PlayGround.Add(TerrainEntity);

            //LoadMovingObjectEntity();

            //MovingObjectEntity = movingObject.GetBEPUEntity();
            //PlayGround.Add(MovingObjectEntity);

            PlayGround.ForceUpdater.Gravity = ((GamePlayCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.GamePlayCondition)).Gravity;
        }

        private void LoadMovingObjectEntity()
        {
            //MovingObjectCondition mc = (MovingObjectCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.MovingObjectCondition);
            //AffineTransform transform = new AffineTransform(new Vector3(mc.ScaleFactor), mc.Rotation, mc.Position);
            //if(mc.Vertices!=null)
            //if(mc.Mass == 0)
            //    MovingObjectEntity = new MobileMesh(mc.Vertices, mc.Indices, transform, MobileMeshSolidity.Solid);
            //else
            //    MovingObjectEntity = new MobileMesh(mc.Vertices, mc.Indices, transform, MobileMeshSolidity.Solid, mc.Mass);
           // MovingObjectEntity = new Sphere(mc.Bounding.Center, mc.Bounding.Radius, mc.Mass);
        }

        private void LoadTerrainEntity()
        {
            TerrainCondition tc = (TerrainCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.TerrainCondition);
            TerrainEntity = new BEPUphysics.Collidables.Terrain(tc.HeightValues, new AffineTransform(new Vector3(tc.Zooming,1,tc.Zooming), Quaternion.Identity, new Vector3(0,0,0)));
            //TerrainEntity.Material = new Material(2, 2, 0);
            TerrainEntity.Thickness = 10;
            //TerrainEntity = new StaticMesh(tc.Vertices, tc.Indices);
        }


        public List<Vector3[]> GetAllCorners()
        { 
            List<Vector3[]> corners = new List<Vector3[]>();
            foreach (BoundingBox box in CheckpointBoundings)
                corners.Add(box.GetCorners());
            return corners;
        }

        public void UpdateCollisions()
        {
            CheckpointCollision = false;
            foreach (BoundingBox box in CheckpointBoundings)
                if (MovingObjectBounding.Intersects(box))
                {
                    if (CheckpointCollisionIndex != CheckpointBoundings.IndexOf(box))
                    {
                        CheckpointCollisionIndex = CheckpointBoundings.IndexOf(box);
                        CheckpointCollision = true;
                    }
                }
           PlayGround.Update();
        }


        
        public void DrawStuff(Matrix projectionMatrix, Matrix viewMatrix)
        {
            int i = 0;
            foreach (Model model in Models)
            {
                Matrix[] modelTransforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(modelTransforms);

                Matrix WorldMatrix = Matrix.CreateScale(0.005f)*Balls[i].WorldTransform;

                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (Effect meshEffect in mesh.Effects)
                    {
                        meshEffect.CurrentTechnique = meshEffect.Techniques["MovingObjectShading"];
                        meshEffect.Parameters["xWorldMatrix"].SetValue(WorldMatrix);
                        meshEffect.Parameters["xProjectionMatrix"].SetValue(projectionMatrix);
                        meshEffect.Parameters["xViewMatrix"].SetValue(viewMatrix);
                    }
                    mesh.Draw();
                }

                i++;
            }
        }

        public void ConditionChanged(Condition condition, List<ParameterIdentifier> changedParameters)
        {
            if (condition.GetID() == ConditionID.MovingObjectCondition && (changedParameters.Contains(ParameterIdentifier.ShapeVertices) || changedParameters.Contains(ParameterIdentifier.ShapeIndices)))
                LoadMovingObjectEntity();
            if (condition.GetID() == ConditionID.MovingObjectCondition && (changedParameters.Contains(ParameterIdentifier.Scale) || changedParameters.Contains(ParameterIdentifier.Position)))
                UpdateMovingObjectBounding(((MovingObjectCondition)condition).Position);
        }
    }
}
