using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class GetHealthPotion : WalkToTargetAndExecuteAction
    {
        private int maxHp;
        private int hpChange;

        public GetHealthPotion(AutonomousCharacter character, GameObject target) : base("GetHealthPotion", character, target)
        {
            this.maxHp = this.Character.GameManager.characterData.MaxHP;
            this.hpChange = maxHp - this.Character.GameManager.characterData.HP;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);

            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, goalValue - this.hpChange);

            worldModel.SetProperty(Properties.HP, this.maxHp);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            else if (this.Character.GameManager.characterData.HP < this.maxHp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            var hp = worldModel.GetProperty(Properties.HP);
            var maxHp = worldModel.GetProperty(Properties.MAXHP);

            if (!base.CanExecute(worldModel)) return false;
            else if (Convert.ToInt32(hp) < Convert.ToInt32(maxHp))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.GetHealthPotion(this.Target);
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL) change -= this.hpChange;
            return change;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return base.GetHValue(worldModel);
        }
    }
}
