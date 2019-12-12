using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class DivineSmite : WalkToTargetAndExecuteAction
    {
        private int manaCost;
        private int xpChange;

        public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite", character, target)
        {
            this.manaCost = 2;
            this.xpChange = 3;
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change -= this.xpChange;
            }

            return change;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            else if (this.Character.GameManager.characterData.Mana >= this.manaCost)
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
            int mana = (int)worldModel.GetProperty(Properties.MANA);

            if (!base.CanExecute(worldModel)) return false;
            else if (mana >= this.manaCost)
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
            this.Character.GameManager.DivineSmite(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            // Update mana with cost of divine smite (2)
            int mana = (int)worldModel.GetProperty(Properties.MANA);
            worldModel.SetProperty(Properties.MANA, mana - this.manaCost);

            // There was an hit, enemy is destroyed, gain xp (3)
            // Disables the target object so that it can't be reused again
            int xp = (int)worldModel.GetProperty(Properties.XP);
            worldModel.SetProperty(this.Target.name, false);
            worldModel.SetProperty(Properties.XP, xp + this.xpChange);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int mana = (int)worldModel.GetProperty(Properties.MANA);
            if (mana >= 5.0f) return base.GetHValue(worldModel) / 5.0f;
            else if (mana < 2.0f) return 100.0f;
            else return 15.0f;
        }
    }
}
