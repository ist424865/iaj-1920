using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.GameManager;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class Rest : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public Rest(AutonomousCharacter character) : base("Rest")
        {
            this.Character = character;
        }

        public override bool CanExecute()
        {
            int maxHP = this.Character.GameManager.characterData.MaxHP;

            if (!base.CanExecute()) return false;
            else if (this.Character.GameManager.characterData.HP < maxHP)
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
            this.Character.GameManager.Rest();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            int maxHp = (int)worldModel.GetProperty(Properties.MAXHP);
            int HP = (int)worldModel.GetProperty(Properties.HP);
            int afterRestHP = HP + AutonomousCharacter.REST_HP_RECOVERY;

            // Cannot exceed maxHP
            afterRestHP = Mathf.Min(afterRestHP, maxHp);
            worldModel.SetProperty(Properties.HP, afterRestHP);

            // Update SURVIVAL goal with HP change: afterRestHP - characterHP
            var survivalValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, survivalValue - (afterRestHP - HP));

            // Update BE QUICK goal with resting time
            var quicknessValue = worldModel.GetGoalValue(AutonomousCharacter.BE_QUICK_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.BE_QUICK_GOAL, quicknessValue + 1.5f);

            // Update world time
            var time = (float)worldModel.GetProperty(Properties.TIME);
            worldModel.SetProperty(Properties.TIME, time + AutonomousCharacter.RESTING_INTERVAL);
        }

        public override float GetGoalChange(Goal goal)
        {
            int maxHP = this.Character.GameManager.characterData.MaxHP;
            int hpChange = Mathf.Min(this.Character.GameManager.characterData.HP + AutonomousCharacter.REST_HP_RECOVERY, maxHP);

            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change -= hpChange;
            }
            else if (goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                change += AutonomousCharacter.RESTING_INTERVAL;
            }

            return change;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return base.GetHValue(worldModel);
        }
    }
}
