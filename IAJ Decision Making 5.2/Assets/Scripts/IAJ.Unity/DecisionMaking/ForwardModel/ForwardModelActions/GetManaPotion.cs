using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class GetManaPotion : WalkToTargetAndExecuteAction
    {
        private int fullMana = 10;

        public GetManaPotion(AutonomousCharacter character, GameObject target) : base("GetManaPotion", character, target)
        {
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            worldModel.SetProperty(Properties.MANA, this.fullMana);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);

        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            else if (this.Character.GameManager.characterData.Mana < this.fullMana)
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
            else if (Convert.ToInt32(mana) < this.fullMana)
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
            this.Character.GameManager.GetManaPotion(this.Target);
        }

        public override float GetGoalChange(Goal goal)
        {
            return base.GetGoalChange(goal);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return base.GetHValue(worldModel);
        }
    }
}
