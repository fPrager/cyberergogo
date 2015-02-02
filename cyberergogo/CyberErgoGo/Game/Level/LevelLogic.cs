using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BikeControls;

namespace CyberErgoGo
{
    /// <summary>
    /// The playing of a level should capsulated from the presentation or data of the level. 
    /// So there exists an extra class for the logic. 
    /// This offers the possibility to change the gameplay whithout effecting the presentation or data.
    /// Possible differences could be done in a freeride-mode, a wild chase or an experimanting part.
    /// </summary>
    class LevelLogic
    {
        //the level (or data) behind this logic
        Level PlayingLevel;

        //the vehicle of the game figure
        MovingObject Vehicle;
        int CurrentVehicleIndex = 0;

        //the invader as gamefigur
        const float InvaderSize = 3.5f * OverallSetting.SizeFactor;
        GameFigure Figure;

        List<GameObject> GameObjects;

        List<Billboard> Trees;

        float WaitingInMSec = 0;
        int WaitingAfterPhysicChangeInMSec = 1500;
        public bool WaitingIsOver = true;
        float TimeToPlaySound = 5;
        float InvaderSoundTime = 0;

        public List<Checkpoint> Checkpoints;

        const int CanonSize = (int)(7 * OverallSetting.SizeFactor);
        public List<BillboardCanon> Canons;

        public bool OnTheStreet = true;
        public bool LevelFinished = false;

        KeyboardState OldKeyboardState;

        public LevelLogic(Level level)
        {
            PlayingLevel = level;
            Vehicle = new XWing();
            GameObjects = new List<GameObject>();
            Checkpoints = new List<Checkpoint>();
            Canons = new List<BillboardCanon>();
        }

        public LevelLogic(Level level, MovingObject figur, List<Billboard> trees)
        {
            PlayingLevel = level;
            Vehicle = figur;
            Trees = trees;
            
            figur.SetStandartPhysicalRepresentation();
            GameObjects = new List<GameObject>();
            Checkpoints = new List<Checkpoint>();
            Canons = new List<BillboardCanon>();
        }

        

        #region Loading

        public void Load()
        {
            Vehicle.Load();
            //Änderung
            //Figure = new GameFigure(new BillboardInvader(InvaderSize, new Vector3(120, 14.5f, 100)));
            Figure = new GameFigure(new BillboardInvader(InvaderSize, new Vector3(120, 0, 100) * OverallSetting.SizeFactor));
            Figure.SetDrawDistance(new Vector3(-0.5f, 0, 0) * OverallSetting.SizeFactor);
            LoadRest();
        }

        public void Load(Effect effect)
        {
            Vehicle.Load(effect);
            Figure = new GameFigure(new BillboardInvader(InvaderSize, new Vector3(120, 0, 100) * OverallSetting.SizeFactor));
            Figure.SetDrawDistance(new Vector3(-0.5f, 0, 0) * OverallSetting.SizeFactor);
            LoadRest();
        }

        private void LoadTreesYPositions()
        {
            Terrain playingTerrain = PlayingLevel.GetPlayingTerrain();

            foreach (BillboardTree tree in Trees)
            {
                Vector3 pos = tree.GetWorldPosition();
                
                    float newYPos = playingTerrain.GetHeightAtThisPoint(new Vector2(pos.X, pos.Z));
                    bool onStreet= playingTerrain.IsThisPositionOverStreet(new Vector3(pos.X, newYPos + tree.GetHeight(), pos.Z));
                    if (!onStreet && newYPos < 100)
                    {
                        tree.ChangeWorldPosition(new Vector3(pos.X, newYPos + tree.GetHeight() / 2, pos.Z));
                        tree.ToDraw = true;
                    }
                    else
                    {
                        tree.ToDraw = false;
                    }
                }
        }

        private void LoadCheckPoints()
        {
            int index = 0;
            int dimension = PlayingLevel.GetPlayingTerrain().GetStreet().Width;
            //foreach (Vector3 position in PlayingLevel.CheckPoints)
            //{
            //    Checkpoint cp = new Checkpoint(index, dimension, 5, PlayingLevel.GetStreetOrientation(position));
            //    if (index == 0)
            //    {
            //        cp.Feature = CheckpointFeature.Start;
            //        Console.WriteLine("start reached");
            //        cp.State = CheckpointState.Reached;
            //    }
            //    if (index == 1)
            //    {
            //        cp.State = CheckpointState.Next;
                    
            //    }
            //    if (index == PlayingLevel.CheckPoints.Count - 1)
            //    {
            //        cp.Feature = CheckpointFeature.End;
            //        Console.WriteLine("end reached");
            //    }
                
            //    cp.Position = position;
            //    cp.SetUpBuffers();
            //    Checkpoints.Add(cp);
            //    cp.GenerateMinMax();
            //    //cp.Orientation = PlayingLevel.GetStreetOrientation(position);
            //    index++;
            //}
            int checkpointBreak = 4;
            index= -1;
            foreach (Vector3 position in PlayingLevel.CheckPoints)
            {
                index++;
                if ((position - PlayingLevel.Start).Length() > 50)
                {
                    if (index % checkpointBreak != 0 && index != PlayingLevel.CheckPoints.Count - 1)
                    {
                        continue;
                    }
                    //Checkpoint cp = new Checkpoint(index, dimension, 5, PlayingLevel.GetStreetOrientation(position));
                    BillboardCanon canon;
                    if (index == PlayingLevel.CheckPoints.Count - 1)
                    {
                        canon = new BillboardCanon(CanonSize * 3, position + new Vector3(0, CanonSize * 0.3f * 4, 0), PlayingLevel.GetStreetOrientation(position));
                    }
                    else
                    {
                        canon = new BillboardCanon(CanonSize, position + new Vector3(0, CanonSize * 0.3f, 0), PlayingLevel.GetStreetOrientation(position));
                    }
                    if (index == 0)
                    {
                        canon.Feature = CanonFeature.FirstCanon;
                        Console.WriteLine("start reached");
                        canon.State = CanonState.Undestroyed;
                    }
                    //if (index == 1)
                    //{
                    //    canon.State = CanonState.Next;

                    //}
                    if (index == PlayingLevel.CheckPoints.Count - 1)
                    {
                        canon.Feature = CanonFeature.BossCanon;
                        Console.WriteLine("end reached");
                    }
                    canon.LoadContent();
                    canon.GenerateMinMax();
                    Canons.Add(canon);

                    //cp.Orientation = PlayingLevel.GetStreetOrientation(position);
                }
            }
            SetUpCheckpointCollisions();
        }

        private void SetUpCheckpointCollisions()
        {
            CollisionChecker collisionChecker = Util.GetInstance().CollisionsChecker;
            foreach (BillboardCanon canon in Canons)
            {
                collisionChecker.AddCheckPointBounding(canon.Min, canon.Max);
            }
        }

        private void LoadGameObjects()
        {
            float streetDimension = PlayingLevel.GetPlayingTerrain().GetStreet().Width;
            float wallStoneWidth = streetDimension / 10;
            float wallStoneHeight = wallStoneWidth;
            float wallStoneLenght = wallStoneWidth*2;
            float wallStoneMass = wallStoneWidth;
            float width = PlayingLevel.GetPlayingTerrain().GetStreet().Width * 0.5f;

            Util.GetInstance().CollisionsChecker.RemoveAllCubes();

            Matrix scale = Matrix.CreateScale(new Vector3(wallStoneWidth, wallStoneHeight, wallStoneLenght));
            List<Vector3> checkPositions = new List<Vector3>();
            
            foreach (Vector3 position in PlayingLevel.WallPoints)
            {
                Quaternion orientation = PlayingLevel.GetStreetOrientation(position);
                bool notAllreadyIn = true;
                foreach (Vector3 oldPos in checkPositions)
                    if (oldPos == position)
                        notAllreadyIn = false;
                int index = PlayingLevel.CheckPoints.IndexOf(position);
                if (notAllreadyIn && index > 2 && index < PlayingLevel.CheckPoints.Count - 2)
                {
                    for (int i = 0; i < width/wallStoneWidth; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            Vector3 wallposition = new Vector3(i * wallStoneWidth - width / 2 + ((wallStoneWidth / 2) * (j % 2)), wallStoneHeight * j + wallStoneHeight, -10);
                            Vector3 newPosition = position + Vector3.Transform(wallposition,orientation);
                            //newPosition = Vector3.Transform(newPosition, orientation);
                            GameObjects.Add(new GameObject(Matrix.Identity, GameObjectShape.Cube));
                            Util.GetInstance().CollisionsChecker.AddPhysicalObject(GameObjectShape.Cube,newPosition, Quaternion.Identity, wallStoneWidth, wallStoneHeight, wallStoneLenght, wallStoneMass);
                        }
                    }
                    checkPositions.Add(position);
                }
            }

            foreach (Vector3 position in PlayingLevel.StonePoints)
            {
                Quaternion orientation = PlayingLevel.GetStreetOrientation(position);
                int index = PlayingLevel.CheckPoints.IndexOf(position);
                if (index > 2 && index < PlayingLevel.CheckPoints.Count - 2)
                {
                    float stoneWidth = (streetDimension / 10f) * 3 + Util.GetInstance().GetRandomNumber(-(int)(streetDimension / 5), -(int)(streetDimension / 5));
                    float stoneHeight = (streetDimension / 10f) * 3 + Util.GetInstance().GetRandomNumber(-(int)(streetDimension / 5), -(int)(streetDimension / 5));
                    float stoneLenght = (streetDimension / 10f) * 3 + Util.GetInstance().GetRandomNumber(-(int)(streetDimension / 5), -(int)(streetDimension / 5));
                    float stoneMass = stoneWidth*2;

                    Matrix scaleStone = Matrix.CreateScale(new Vector3(stoneWidth, stoneHeight, stoneLenght));

                    Vector3 stoneposition = new Vector3(Util.GetInstance().GetRandomNumber(0, (int)width) - (int)width/2, stoneHeight, Util.GetInstance().GetRandomNumber(0, (int)width / 2));
                    Vector3 newPosition = position + Vector3.Transform(stoneposition, orientation);
                   //newPosition = Vector3.Transform(newPosition, orientation);
                    GameObjects.Add(new GameObject(scale * Matrix.CreateTranslation(stoneposition) * Matrix.CreateFromQuaternion(orientation) * Matrix.CreateTranslation(position), GameObjectShape.Cube));
                    Util.GetInstance().CollisionsChecker.AddPhysicalObject(GameObjectShape.Cube,newPosition, Quaternion.Identity, stoneWidth, stoneHeight, stoneLenght, stoneMass);

                    //stoneWidth = 30 + Util.GetInstance().GetRandomNumber(-20, 10);
                    //stoneHeight = 30 + Util.GetInstance().GetRandomNumber(-20, 10);
                    //stoneLenght = 30 + Util.GetInstance().GetRandomNumber(-20, 10);
                    //stoneMass = stoneWidth;
                    //scaleStone = Matrix.CreateScale(new Vector3(stoneWidth, stoneHeight, stoneLenght));

                    //stoneposition = new Vector3(-Util.GetInstance().GetRandomNumber(0, (int)width / 2), stoneHeight, -Util.GetInstance().GetRandomNumber(0, (int)width / 2));
                    //newPosition = position + Vector3.Transform(stoneposition, orientation);
                    ////newPosition = Vector3.Transform(newPosition, orientation);
                    //GameObjects.Add(new GameObject(scale * Matrix.CreateTranslation(stoneposition) * Matrix.CreateFromQuaternion(orientation) * Matrix.CreateTranslation(position), GameObjectShape.Cube));
                    //Util.GetInstance().CollisionsChecker.AddPhysicalCube(newPosition, Quaternion.Identity, stoneWidth, stoneHeight, stoneLenght, stoneMass);
                }
            }

            //Util.GetInstance().CollisionsChecker.RemoveAllSpheres();

            float sphereRadius = streetDimension/2f;
            float sphereMass = streetDimension;

            Matrix scaleSphere = Matrix.CreateScale(sphereRadius);
            
            foreach (Vector3 position in PlayingLevel.BigSpherePoints)
            {
                Quaternion orientation = PlayingLevel.GetStreetOrientation(position);
  
                Vector3 sphereposition = new Vector3(width/2 + 20,sphereRadius*4,0);
                Vector3 newPosition = position + Vector3.Transform(sphereposition, orientation);
                GameObjects.Add(new GameObject(scale * Matrix.CreateTranslation(sphereposition) * Matrix.CreateFromQuaternion(orientation) * Matrix.CreateTranslation(position), GameObjectShape.Sphere));
                Util.GetInstance().CollisionsChecker.AddPhysicalObject(GameObjectShape.Sphere, newPosition, orientation, sphereRadius, sphereRadius, sphereRadius,sphereMass);
            }
        }

        public List<Matrix> GetGameObjectWorldTransform(GameObjectShape wantedShape)
        {
            List<Matrix> shapeMatrixes = new List<Matrix>();
            foreach (GameObject go in GameObjects)
            {
                if (go.Shape == wantedShape)
                    shapeMatrixes.Add(go.WorldTransform);
            }
            return shapeMatrixes;
        }

        private void LoadRest()
        {
            SetGameFigureAtStart();
            ChangeGameCamera();
            PlayingLevel.LoadTerrainData();
            Util.GetInstance().CollisionsChecker.LoadGround();
            LoadCheckPoints();
            LoadGameObjects();
            LoadTreesYPositions();
        }

        private void ChangeGameCamera()
        {
            //FreeCam: commend all this changes here of the moving-behaviour
            ViewCondition vc = (ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition);
            vc.MovingBehaviour = new PassivBehaviour(ParameterIdentifier.Position, ParameterIdentifier.MovingOrientation, ConditionID.MovingObjectCondition, new Vector3(0, 10, -35) * OverallSetting.SizeFactor, 2 * OverallSetting.SizeFactor, 8 * OverallSetting.SizeFactor);
            vc.ConditionHasChanged();
        }

        #endregion

        #region Restart

        public void Restart()
        {
            //GameFigure.SetPhysicalRepresentation(new RollingSphere(5f, Vector3.Zero, 100));
            SetGameFigureAtStart();
            int index = 0;
            //foreach (Checkpoint cp in Checkpoints)
            //{
            //    if (cp.Feature == CheckpointFeature.Start) cp.State = CheckpointState.Reached;
            //    else
            //        if (index == 1) cp.State = CheckpointState.Next;
            //        else
            //            cp.State = CheckpointState.Unchecked;
            //    index++;
            //}
            foreach (BillboardCanon canon in Canons)
            {
                canon.State = CanonState.Undestroyed;
                canon.Restore();
            }
            LevelFinished = false;
            ChangeGameCamera();
        }

        public void Unload()
        {
            Util.GetInstance().CollisionsChecker.RemoveAllPhysicalObjects();
            foreach (Level level in Util.GetInstance().LevelList)
            {
                if(level.GetTitle() == PlayingLevel.GetTitle())
                {
                PlayerEntry newEntry = new PlayerEntry();
                newEntry.Name = "NewBee";
                newEntry.Time = 356;
                level.PlayerEntries.Add(newEntry);
                }
            }
            Vehicle.Unload();
        }

        private void SetGameFigureAtStart()
        {
            //Bike.Brake(40);
            Vector3 StartPosition = PlayingLevel.Start;
            Quaternion Orientation = PlayingLevel.GetStreetOrientation(StartPosition);
            Vehicle.SetPosition(StartPosition + new Vector3(0, 5*OverallSetting.SizeFactor, 0));
            
            Vehicle.SetRotation(Orientation);

            Figure.TurnToBack();

            Util.GetInstance().CollisionsChecker.SetMovingObjectBounding(StartPosition + new Vector3(0, 5 * OverallSetting.SizeFactor, 0), 2 * OverallSetting.SizeFactor);            
        }

        #endregion


        #region Update
        /// <summary>
        /// The update-method handles the level changes.
        /// </summary>
        public void Update(GameTime gameTime, int roundMSec, int roundSec, int roundMin, bool withPhysicalUpdate)
        {
            //if (OldKeyboardState == null) OldKeyboardState = Keyboard.GetState();
            
            //KeyboardState newKeyBoardState = Keyboard.GetState();

            //if (!OldKeyboardState.GetPressedKeys().Contains(Keys.Enter) && newKeyBoardState.GetPressedKeys().Contains(Keys.Enter) && false)
            //    GameFigure.ChangePhysic();
            //OldKeyboardState = newKeyBoardState;

            if (!WaitingIsOver)
            {
                WaitingInMSec += gameTime.ElapsedGameTime.Milliseconds;
                if (WaitingInMSec >= WaitingAfterPhysicChangeInMSec)
                {
                    WaitingIsOver = true;
                    WaitingInMSec = 0;
                }
                Vehicle.Update(gameTime.ElapsedGameTime.Milliseconds, (float)WaitingInMSec / (float)WaitingAfterPhysicChangeInMSec);
            }

            //say objects howto move
            if (WaitingIsOver)
            {
                Vehicle.Update(gameTime.ElapsedGameTime.Milliseconds);
            }
            //check collision

                if (withPhysicalUpdate)
            {
                //FreeCamera: command the pysical update out, otherwise the vamera will always fall down!
                Util.GetInstance().CollisionsChecker.UpdateCollisions();
                Vehicle.PhysicalUpdate();
                    if(!LevelFinished)
                        Figure.PhysicalUpdate();
                if (Util.GetInstance().CollisionsChecker.CheckpointCollision)
                {
                    CheckpointReached(Util.GetInstance().CollisionsChecker.CheckpointCollisionIndex, roundMSec, roundSec, roundMin);
                }
            }
            int i = 0;
            int cubeIndex = 0;
            int sphereIndex = 0;
            while (i < GameObjects.Count)
            {
                GameObject go = GameObjects.ElementAt(i);
                Matrix worldTransform = Util.GetInstance().CollisionsChecker.GetPhysicalObjectWorldTransform(i);
                go.WorldTransform = worldTransform;
                i++;
            }
            if (!LevelFinished)
            {
                CheckIfMovingObjectIsOnTheRoad();
                CheckIfMovingObjectIsOnTheGround();
            }
            if (!Bike.PluggedIn)
            {
                if (Keyboard.GetState().GetPressedKeys().Contains(Keys.Up))
                {
                    InvaderSoundTime += gameTime.ElapsedGameTime.Milliseconds / 1000f;
                    if (TimeToPlaySound < InvaderSoundTime)
                    {
                        InvaderSoundTime = 0;
                        Util.GetInstance().SoundManager.PlayInvaderSound(0.1f);
                    }
                    if (Figure.Situation != GameFigureSituation.IsWalking && Figure.Situation != GameFigureSituation.IsSitting && Figure.Situation != GameFigureSituation.TurnedFront)

                        Figure.Walk();
                }
                else
                {
                    if (Figure.Situation == GameFigureSituation.IsWalking)
                        Figure.StopWalking();
                }
            }
            else
            {
                InvaderSoundTime += (Bike.GetState().CurrentSpeed.InKilometerPerHour/5f)*(gameTime.ElapsedGameTime.Milliseconds / 1000f);
                if (TimeToPlaySound < InvaderSoundTime)
                {
                    InvaderSoundTime = 0;
                    Util.GetInstance().SoundManager.PlayInvaderSound(0.1f);
                }


                if (Bike.GetState().CurrentSpeed.InKilometerPerHour > 1 && Bike.GetState().CurrentSpeed.InKilometerPerHour < 50)
                {
                    if (Figure.Situation != GameFigureSituation.IsWalking && Figure.Situation != GameFigureSituation.IsSitting && Figure.Situation != GameFigureSituation.TurnedFront)

                        Figure.Walk();
                }
                else
                    if (Bike.GetState().CurrentSpeed.InKilometerPerHour > 50)
                    {
                        if (Figure.Situation != GameFigureSituation.IsRunning && Figure.Situation != GameFigureSituation.IsSitting && Figure.Situation != GameFigureSituation.TurnedFront)

                            Figure.Run();
                    }
                    else
                {
                    if (Bike.GetState().CurrentSpeed.InKilometerPerHour < 1 && Figure.Situation == GameFigureSituation.IsWalking || Figure.Situation == GameFigureSituation.IsRunning)
                        Figure.StopWalking();
                }
            }
        }

        //TODO: this method should be part of the collisionchecker!
        private void CheckIfMovingObjectIsOnTheGround()
        {
            if (Vehicle.GetWorldMatrix().Translation.Y < 0) 
                Restart();
        }

        private void CheckIfMovingObjectIsOnTheRoad()
        {
            bool overStreet = PlayingLevel.IsPositionOverStreet(Vehicle.GetWorldMatrix().Translation);
            if (overStreet && !OnTheStreet)
            {
                
                Console.WriteLine("back on the road");
                OnTheStreet = true;
                //Bike.Brake(40);
            }
            else
                if (!overStreet && OnTheStreet)
                {
                    Console.WriteLine("go back!");
                    OnTheStreet = false;
                    //Bike.Brake(90);
                }
        }

        private void CheckpointReached(int index, int roundMSec, int roundSec, int roundMin)
        {
            //Checkpoint cp = Checkpoints.ElementAt(index);
            //if (cp.State != CheckpointState.Reached && cp.State==CheckpointState.Next)
            //{
            //    cp.State = CheckpointState.Reached;
            //    cp.TimeMin = roundMin;
            //    cp.TimeSec = roundSec;
            //    cp.TimeMin = roundMin;
            //    if (cp.Feature != CheckpointFeature.End)
            //    {
            //        Checkpoints.ElementAt(index + 1).State = CheckpointState.Next;
            //    }
            //    else
            //        LevelFinished = true;
            //}

            BillboardCanon canon = Canons.ElementAt(index);
            if (canon.State != CanonState.Destroyed)
            {
                canon.State = CanonState.Destroyed;
                canon.TimeMin = roundMin;
                canon.TimeSec = roundSec;
                canon.TimeMSec = roundMSec;
                if (canon == Canons.Last())
                {
                    LevelFinished = true;
                    Figure.BeHappy();
                    ViewCondition vc = (ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition);
                    vc.MovingBehaviour = new NotMoving(vc.CenterPosition, vc.LookAt);
                    vc.ConditionHasChanged();
                }
                //bool allCanonsDestroyed = true;
                //foreach (BillboardCanon c in Canons)
                //    if (c.State == CanonState.Undestroyed)
                //        allCanonsDestroyed = false;

                //if (allCanonsDestroyed)
                //{
                //    LevelFinished = true;
                //    Figure.BeHappy();
                //    ViewCondition vc = (ViewCondition)ConditionHandler.GetInstance().GetCondition(ConditionID.ViewCondition);
                //    vc.MovingBehaviour = new NotMoving(vc.CenterPosition,vc.LookAt);
                //    vc.ConditionHasChanged();
                //}
                canon.Explode();
                Util.GetInstance().SoundManager.PlayExplosion();
            }
        }

        public void ChangePhysicTo(Selection objectSelection)
        {
            Figure.Change();
            switch (objectSelection)
            {
                case Selection.LeftObject:
                    CurrentVehicleIndex = 0;
                    Vehicle.ChangePhysicTo(0);
                    Figure.SetDrawDistance(new Vector3(-0.2f, -2.2f, 0) * OverallSetting.SizeFactor);
                    break;
                case Selection.CenterObject:
                    CurrentVehicleIndex = 1;
                    Vehicle.ChangePhysicTo(1);
                    Figure.SetDrawDistance(new Vector3(-0.5f, 0, 0) * OverallSetting.SizeFactor);
                    break;
                case Selection.RightObject:
                    CurrentVehicleIndex = 2;
                    Vehicle.ChangePhysicTo(2);
                    Figure.SetDrawDistance(new Vector3(-0.2f, -3.5f, 0) * OverallSetting.SizeFactor);
                    break;
                default:
                    Console.WriteLine("the levellogic can't interprate " + objectSelection + " as an selection of an object");
                    break;
            }
        }

        public void StartPhysicChanging()
        {
            Vehicle.StartPhysicChanging();
            Figure.StartChange();
        }


        public void StopPhysicChanging()
        {
            Vehicle.StopPhysicChanging();
            WaitingIsOver = false;
            Figure.StopChange();
            if (CurrentVehicleIndex == 2) Figure.SitDown();
            else
                Figure.TurnToBack();
        }
        #endregion

        #region Getter
        /// <summary>
        /// Passes the terrain of the level.
        /// <returns>the terrain of the playing level</returns>
        /// </summary>
        public Terrain GetPlayingTerrain()
        {
            return PlayingLevel.GetPlayingTerrain();
        }

        public List<Billboard> GetTrees()
        {
            return Trees;
        }

        public GameFigure GetFigure()
        {
            return Figure;
        }

        /// <summary>
        /// Passes the MovingObject
        /// <returns>the MovingObject</returns>
        /// </summary>
        public MovingObject GetMovingObject()
        {
            return Vehicle;
        }
        #endregion

    }
}
