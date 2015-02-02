using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace CyberErgoGo
{
    /// <summary>
    /// Stores states of the game and update the observers.
    /// </summary>
    class ConditionHandler
    {
        private static ConditionHandler Instance;

        private static List<Condition> Conditions;

        private static Dictionary<ConditionID, List<IConditionObserver>> Observers;

        private ConditionHandler() 
        {
            Conditions = new List<Condition>();
            Observers = new Dictionary<ConditionID, List<IConditionObserver>>();
        }

        public static ConditionHandler GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ConditionHandler();
            }
            return Instance;
        }

        public Condition RegisterMe(ConditionID id, IConditionObserver observer) 
        {
            if (Observers.Keys.Contains(id))
            {
                Observers[id].Add(observer);
            }
            else
            {
                List<IConditionObserver> newObserverList = new List<IConditionObserver>();
                newObserverList.Add(observer);
                Observers.Add(id, newObserverList);
                if (Conditions.Find(
                    delegate(Condition c)
                    {
                        return c.GetID() == id;
                    }) == null)
                {
                    CreateCondition(id);
                }
            }
            return GetCondition(id);
        }

        public void UnregisterMe(ConditionID id, IConditionObserver observer)
        {
            if (Observers.Keys.Contains(id))
            {
                if (Observers[id].Contains(observer))
                    Observers[id].Remove(observer);
                else
                    Console.WriteLine("This observer isn't registered to listen to " + id);
            }
            else
                Console.WriteLine("There exists no observation of " + id);
        }

        public void UnregisterMe(IConditionObserver observer)
        {
            foreach (ConditionID key in Observers.Keys)
                Observers[key].Remove(observer);
        }

        public void SetCondition(Condition condition) 
        {
            if (Conditions.Find(
                delegate(Condition c)
                {
                    return c.GetID() == condition.GetID();
                }) != null)
            {
                Console.WriteLine("There exits already a condition with the ID " + condition.GetID());
                ChangeCondition(condition);
            }
            else
            {
                Conditions.Add(condition);
                if(!Observers.Keys.Contains(condition.GetID()))
                        Observers.Add(condition.GetID(), new List<IConditionObserver>());
            }
        }

        private Condition CreateCondition(ConditionID id) 
        {
            Condition c;
            switch (id) 
            { 
                case ConditionID.TerrainCondition:
                    c = new TerrainCondition();
                    break;
                case ConditionID.BikeCondition:
                    c = new BikeCondition();
                    break;
                case ConditionID.GamePlayCondition:
                    c= new GamePlayCondition();
                    break;
                case ConditionID.KeyboardCondition:
                    c = new KeyboardCondition();
                    break;
                case ConditionID.LevelCondition:
                    c = new LevelCondition();
                    break;
                case ConditionID.MovingObjectCondition:
                    c = new MovingObjectCondition();
                    break;
                case ConditionID.ViewCondition:
                    c = new ViewCondition();
                    break;
                case ConditionID.OptionCondition:
                    c = new OptionCondition();
                    break;
                default:
                    c = new Condition(ConditionID.UnKnown);
                    break;
            }
            SetCondition(c);
            return c;
        }

        public Condition GetCondition(ConditionID id) 
        {
            foreach (Condition c in Conditions) 
            {
                if (c.GetID() == id)
                    return c;
            }
            Console.WriteLine("The ConditionHandler doesn't store the condition with the ID " + id);
            return CreateCondition(id);
        }

        public void ChangeCondition(Condition condition) 
        {
            bool conditionChanged = false;
            int i = 0;
            while (i < Conditions.Count) 
            {
                if (Conditions[i].GetID() == condition.GetID()) 
                {
                    conditionChanged = true;
                    Conditions[i] = condition;
                    List<IConditionObserver> depObservers = Observers[condition.GetID()];
                    if (depObservers == null)
                    {
                        depObservers = new List<IConditionObserver>();
                        Observers.Add(condition.GetID(), depObservers);
                    }
                    else
                    {
                        List<ParameterIdentifier> changedParameters = condition.GetChangedParameters();
                        int j = 0;
                        while (j < depObservers.Count)
                        {
                            depObservers[j].ConditionChanged(condition, changedParameters);
                            j++;
                        }
                    }
                }
                i++;
            }
            if (!conditionChanged) 
            {
                Console.WriteLine("The ConditionHandler didn't already know this condition, from know it stores it");
                Conditions.Add(condition);
            }

            condition.SetToUnchanged();
        }
    }
}
