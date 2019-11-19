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

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.DivineSmite(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            var xpValue = worldModel.GetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, xpValue - this.xpChange);

            var xp = worldModel.GetProperty(Properties.XP);
            worldModel.SetProperty(Properties.XP, Convert.ToInt32(xp) + this.xpChange);

            var mana = worldModel.GetProperty(Properties.MANA);
            worldModel.SetProperty(Properties.MANA, Convert.ToInt32(mana) - this.manaCost);

            //disables the target object so that it can't be reused again
            //skeleton enemy disappears!
            worldModel.SetProperty(this.Target.name, false);
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
            var mana = worldModel.GetProperty(Properties.MANA);

            if (!base.CanExecute(worldModel)) return false;
            else if (Convert.ToInt32(mana) >= this.manaCost)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return base.GetHValue(worldModel);
        }
    }
}
