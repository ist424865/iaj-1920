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
        private int maxHP;
        private int hpChange;

        public GetHealthPotion(AutonomousCharacter character, GameObject target) : base("GetHealthPotion", character, target)
        {
            this.maxHP = this.Character.GameManager.characterData.MaxHP;
            this.hpChange = this.maxHP - this.Character.GameManager.characterData.HP;
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change -= this.hpChange;
            }

            return change;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            else if (this.Character.GameManager.characterData.HP < this.maxHP)
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
            int hp = (int)worldModel.GetProperty(Properties.HP);
            int maxHp = (int)worldModel.GetProperty(Properties.MAXHP);

            if (!base.CanExecute(worldModel)) return false;
            else if (hp < maxHp)
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

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            int hp = (int)worldModel.GetProperty(Properties.HP);
            int maxHp = (int)worldModel.GetProperty(Properties.MAXHP);
            int changeHp = maxHp - hp;

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);

            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, goalValue - changeHp);

            worldModel.SetProperty(Properties.HP, maxHp);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int maxhp = (int)worldModel.GetProperty(Properties.MAXHP);
            int currenthp = (int)worldModel.GetProperty(Properties.HP);
            int shieldhp = (int)worldModel.GetProperty(Properties.ShieldHP);

            // When character has more than 80% MaxHP this is not a good option
            if (currenthp >= 0.8f * maxhp) return 100.0f;
            // When character has less than 20% MaxHP this is the best option
            else if (currenthp <= 0.2f * maxhp) return 1.0f;
            // When character has less HP + ShieldHP than 50% MaxHP this is the best option
            else if (currenthp + shieldhp < 0.5f * maxhp) return 5.0f + base.GetHValue(worldModel);
            else return 5.0f + base.GetHValue(worldModel);
        }
    }
}
