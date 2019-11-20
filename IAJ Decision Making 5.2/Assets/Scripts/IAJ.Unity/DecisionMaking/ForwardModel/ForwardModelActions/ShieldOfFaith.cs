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

        private int maxShieldHP;
        private int changeShieldHP;
        private int manaCost;
        
        public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
        {
            this.Character = character;
            this.maxShieldHP = 5;
            this.manaCost = 5;
            this.changeShieldHP = this.maxShieldHP - this.Character.GameManager.characterData.ShieldHP;
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change -= this.changeShieldHP;
            }

            return change;
        }

        public override bool CanExecute()
        {
            // the new shield will replace the old one but we need to double check the mana cost
            // only execute if shield hp is lower than its maximum
            if (!base.CanExecute()) return false;
            else if (this.Character.GameManager.characterData.Mana >= this.manaCost
                  && this.Character.GameManager.characterData.ShieldHP < this.maxShieldHP)
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
            int mana = (int)worldModel.GetProperty(Properties.MANA);
            int shieldhp = (int)worldModel.GetProperty(Properties.ShieldHP);

            if (!base.CanExecute(worldModel)) return false;
            else if (mana >= this.manaCost && shieldhp < this.maxShieldHP)
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

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            float goalValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);

            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, goalValue - this.changeShieldHP);

            worldModel.SetProperty(Properties.ShieldHP, this.maxShieldHP);

            int mana = (int)worldModel.GetProperty(Properties.MANA);
            worldModel.SetProperty(Properties.MANA, mana - this.manaCost);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return base.GetHValue(worldModel);
        }
    }
}
