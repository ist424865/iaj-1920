using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    class ShieldOfFaith : Action
    {
        protected AutonomousCharacter Character { get; set; }

        private int hpShield;
        private int manaCost;
        public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
        {
            this.Character = character;
            this.hpShield = 5;
            this.manaCost = 5;

            // Maybe we need to take in account the character HP and check if it is already full.
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);

            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, goalValue - this.hpShield);

            worldModel.SetProperty(Properties.ShieldHP, this.hpShield);

            var mana = worldModel.GetProperty(Properties.MANA);
            worldModel.SetProperty(Properties.MANA, Convert.ToInt32(mana) - this.manaCost);

        }

        public override bool CanExecute()
        {
            // the new shield will replace the old one but we need to double check the mana cost
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
            // the new shield will replace the old one but we need to double check the mana cost
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

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.ShieldOfFaith();
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL) change -= hpShield;
            return change;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return base.GetHValue(worldModel);
        }
    }
}
