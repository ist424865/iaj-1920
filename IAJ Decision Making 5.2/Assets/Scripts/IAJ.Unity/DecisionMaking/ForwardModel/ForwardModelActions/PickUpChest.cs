using System.Collections.Generic;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class PickUpChest : WalkToTargetAndExecuteAction
    {
        public const int MAX_SQR_DISTANCE = 400;

        public PickUpChest(AutonomousCharacter character, GameObject target) : base("PickUpChest",character,target)
        {
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            if (goal.Name == AutonomousCharacter.GET_RICH_GOAL) change -= 5.0f;
            return change;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return true;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;
            return true;
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.PickUpChest(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.GET_RICH_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.GET_RICH_GOAL, goalValue - 5.0f);

            var money = (int)worldModel.GetProperty(Properties.MONEY);
            worldModel.SetProperty(Properties.MONEY, money + 5);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            // Chests without enemy are the best option
            var chest = this.Target;
            List<GameObject> enemies = worldModel.GetProperty(Properties.ENEMIES) as List<GameObject>;

            foreach (var enemy in enemies)
            {
                // If chest has enemy near it
                if (CheckObjectsRange(chest, enemy, MAX_SQR_DISTANCE))
                {
                    return base.GetHValue(worldModel)/7.5f;
                }
            }
            return -50.0f + base.GetHValue(worldModel)/7.5f;
        }
        
        public static bool CheckObjectsRange(GameObject obj1, GameObject obj2, float maximumSqrDistance)
        {
            return (obj1.transform.position - obj2.transform.position).sqrMagnitude <= maximumSqrDistance;
        }
    }
}
