using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    public class WorldModel
    {
        private Dictionary<string, object> Properties { get; set; }
        private List<Action> Actions { get; set; }
        protected IEnumerator<Action> ActionEnumerator { get; set; }

        private Dictionary<string, float> GoalValues { get; set; }

        protected WorldModel Parent { get; set; }

        private string[] enemies = { "Skeleton1", "Skeleton2", "Orc1", "Orc2", "Dragon" };
        private string[] listOfProperties = { "Mana", "HP", "ShieldHP", "MAXHP", "XP", "Time", "Money", "Level", "Position", "Enemies" };
        private string[] resources = { "Chest1", "Chest2", "Chest3", "Chest4", "Chest5", "HealthPotion1", "HealthPotion2", "ManaPotion1", "ManaPotion2" };


        public WorldModel(List<Action> actions)
        {
            this.Properties = new Dictionary<string, object>();
            this.GoalValues = new Dictionary<string, float>();
            this.Actions = actions;
            this.ActionEnumerator = actions.GetEnumerator();
        }

        public WorldModel(WorldModel parent)
        {
            this.Properties = new Dictionary<string, object>();
            this.GoalValues = new Dictionary<string, float>();
            this.Actions = parent.Actions;
            this.Parent = parent;
            this.ActionEnumerator = this.Actions.GetEnumerator();
            foreach (var name in enemies)
            {
                this.SetProperty(name, parent.GetProperty(name));
            }
            foreach (var name in listOfProperties)
            {
                this.SetProperty(name, parent.GetProperty(name));
            }
            foreach (var name in resources)
            {
                this.SetProperty(name, parent.GetProperty(name));
            }
        }

        public virtual object GetProperty(string propertyName)
        {
            return this.Properties[propertyName];
        }

        public virtual void SetProperty(string propertyName, object value)
        {
            this.Properties[propertyName] = value;
        }

        public virtual float GetGoalValue(string goalName)
        {
            //recursive implementation of WorldModel
            if (this.GoalValues.ContainsKey(goalName))
            {
                return this.GoalValues[goalName];
            }
            else if (this.Parent != null)
            {
                return this.Parent.GetGoalValue(goalName);
            }
            else
            {
                return 0;
            }
        }

        public virtual void SetGoalValue(string goalName, float value)
        {
            var limitedValue = value;
            if (value > 10.0f)
            {
                limitedValue = 10.0f;
            }

            else if (value < 0.0f)
            {
                limitedValue = 0.0f;
            }

            this.GoalValues[goalName] = limitedValue;
        }

        public virtual WorldModel GenerateChildWorldModel()
        {
            return new WorldModel(this);
        }

        public float CalculateDiscontentment(List<Goal> goals)
        {
            var discontentment = 0.0f;

            foreach (var goal in goals)
            {
                var newValue = this.GetGoalValue(goal.Name);

                discontentment += goal.GetDiscontentment(newValue);
            }

            return discontentment;
        }

        public virtual Action GetNextAction()
        {
            Action action = null;
            //returns the next action that can be executed or null if no more executable actions exist
            if (this.ActionEnumerator.MoveNext())
            {
                action = this.ActionEnumerator.Current;
            }

            while (action != null && !action.CanExecute(this))
            {
                if (this.ActionEnumerator.MoveNext())
                {
                    action = this.ActionEnumerator.Current;
                }
                else
                {
                    action = null;
                }
            }

            return action;
        }

        public virtual Action[] GetExecutableActions()
        {
            return this.Actions.Where(a => a.CanExecute(this)).ToArray();
        }

        public virtual bool IsTerminal()
        {
            return true;
        }


        public virtual float GetScore()
        {
            return 0.0f;
        }

        public virtual int GetNextPlayer()
        {
            return 0;
        }

        public virtual void CalculateNextPlayer()
        {
        }
    }
}
