using System;
using System.Collections.Generic;
using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class BiasedMCTS : MCTS
    {
        public BiasedMCTS(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
            this.MaxIterations = 200;
            this.MaxIterationsProcessedPerFrame = 5;
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            var currentState = initialPlayoutState.GenerateChildWorldModel();
            this.CurrentDepth = 0;

            // while s is nonterminal or reached Max Playout Depth do
            while (!currentState.IsTerminal())
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

        protected Action BiasedActionSelection(Action[] actions, WorldModel state) {
            List<float> heuristics = new List<float>();

            float sumExp = 0;

            // Calculate exp from each action heuristic
            foreach (Action action in actions)
            {
                float heuristic = action.GetHValue(state);
                float exp = Mathf.Exp(-heuristic);

                if (float.IsNegativeInfinity(exp) || float.IsPositiveInfinity(exp))
                {
                    exp = float.MaxValue;
                }

                heuristics.Add(exp);
                sumExp += exp;
            }

            // (1) Normalize heuristics
            // (2) Calculate cumulative probability
            // (3) Return biased random action
            float randomValue = (float)RandomGenerator.NextDouble();
            heuristics[0] /= sumExp;

            if (randomValue < heuristics[0])
                return actions[0];

            for (int i = 1; i < heuristics.Count; i++)
            {
                heuristics[i] /= sumExp;
                heuristics[i] += heuristics[i - 1];

                if (randomValue < heuristics[i])
                    return actions[i];
            }

            return null;
        }
    }
}
