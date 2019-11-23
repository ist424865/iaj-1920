using System;
using System.Collections.Generic;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class LimitedBiasedMCTS : BiasedMCTS
    {
        public int MaxPlayoutIterations { get; set; }

        public LimitedBiasedMCTS(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
            this.MaxIterations = 300;
            this.MaxPlayoutIterations = 20;
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            var currentState = initialPlayoutState.GenerateChildWorldModel();
            this.CurrentDepth = 0;

            // while s is nonterminal or reached Max Playout Depth do
            while (!currentState.IsTerminal() && this.CurrentDepth < this.MaxPlayoutIterations)
            {
                // chose a from Actions(s) uniformly at random                
                Action[] actions = currentState.GetExecutableActions();
                if (actions.Length == 0) break;
                Action randomAction = BiasedActionSelection(actions, currentState);

                // s <- Result(s,a)
                randomAction.ApplyActionEffects(currentState);
                currentState.CalculateNextPlayer();
                this.CurrentDepth++;
            }

            if (this.CurrentDepth > this.MaxPlayoutDepthReached) this.MaxPlayoutDepthReached = this.CurrentDepth;

            // return reward for state s
            var reward = new Reward
            {
                Value = currentState.GetScore()
            };
            return reward;
        }
    }
}
