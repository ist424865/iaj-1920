using System;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class BiasedMCTS : MCTS
    {
        public BiasedMCTS(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {}

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            var currentState = initialPlayoutState.GenerateChildWorldModel();

            // while s is nonterminal do
            while (!currentState.IsTerminal())
            {
                // chose a from Actions(s) uniformly at random                
                var totalActions = currentState.GetExecutableActions();
                var randomAction = totalActions[RandomGenerator.Next(totalActions.Length)];

                // s <- Result(s,a)
                randomAction.ApplyActionEffects(currentState);
                currentState.CalculateNextPlayer();
            }

            // return reward for state s
            var reward = new Reward();
            reward.Value = currentState.GetScore();
            //reward.PlayerID = 0;
            return reward;
        }
    }
}
