using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; protected set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<Action> BestActionSequence { get; private set; }
        public int NumberOfMultiplePlayouts { get; set; }


        protected int CurrentIterations { get; set; }
        protected int CurrentIterationsInFrame { get; set; }
        protected int CurrentDepth { get; set; }

        protected CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }



        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 200;
            this.MaxIterationsProcessedPerFrame = 20;
            this.RandomGenerator = new System.Random();
            this.NumberOfMultiplePlayouts = 5;
        }


        public void InitializeMCTSearch()
        {
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<Action>();
        }

        public Action Run()
        {
            MCTSNode selectedNode;
            Reward reward = new Reward();

            var startTime = Time.realtimeSinceStartup;

            this.CurrentIterationsInFrame = 0;

            // OPTIMIZATION
            // There are some actions more important than others.
            // Level up is an instant action
            // Picking up a chest near the charact is always important.
            foreach (var child in this.InitialNode.ChildNodes)
            {

                if (child.Action.Name == "LevelUp")
                {
                    this.BestFirstChild = child;
                    this.InProgress = false;
                    return this.BestFirstChild.Action;
                }

                if (child.Action.Name.Contains("PickUpChest") && child.Action.GetHValue(this.InitialNode.State) < 0.0f)
                {
                    this.BestFirstChild = child;
                    this.InProgress = false;
                    return this.BestFirstChild.Action;
                }
            }

            while (this.CurrentIterations < this.MaxIterations && this.CurrentIterationsInFrame < this.MaxIterationsProcessedPerFrame)
            {
                selectedNode = Selection(this.InitialNode);
                float playoutSum = 0;

                for (int i = 0; i < this.NumberOfMultiplePlayouts; i++) {
                    playoutSum += Playout(selectedNode.State).Value;
                }

                reward.Value = playoutSum / this.NumberOfMultiplePlayouts;

                Backpropagate(selectedNode, reward);
                this.CurrentIterationsInFrame++;
                this.CurrentIterations++;
            }

            // Did not finish processing all possible computational budget
            if (this.CurrentIterations < this.MaxIterations)
            {
                return null;
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.InProgress = false;
            return BestFinalAction(this.InitialNode);
        }

        protected MCTSNode Selection(MCTSNode initialNode)
        {
            Action nextAction;
            MCTSNode currentNode = initialNode;
            MCTSNode bestChild;
            this.CurrentDepth = 0;

            // while s is nonterminal or reached Max Selection Depth do
            while (!currentNode.State.IsTerminal())
            {
                nextAction = currentNode.State.GetNextAction();
                if (nextAction != null)
                {
                    return Expand(currentNode, nextAction);
                }
                else
                {
                    currentNode = BestUCTChild(currentNode);
                    this.CurrentDepth++;
                }
            }

            if (this.CurrentDepth > this.MaxSelectionDepthReached) this.MaxSelectionDepthReached = this.CurrentDepth;

            bestChild = currentNode;
            return bestChild;
        }

        protected virtual Reward Playout(WorldModel initialPlayoutState)
        {
            var currentState = initialPlayoutState.GenerateChildWorldModel();
            this.CurrentDepth = 0;

            // while s is nonterminal or reached Max Playout Depth do
            while (!currentState.IsTerminal())
            {
                // chose a from Actions(s) uniformly at random                
                Action[] actions = currentState.GetExecutableActions();
                if (actions.Length == 0) break;
                Action randomAction = actions[RandomGenerator.Next(actions.Length)];

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

        protected virtual void Backpropagate(MCTSNode node, Reward reward)
        {
            while (node != null)
            {
                node.N += 1;

                node.Q += reward.Value; 
                node = node.Parent;
            }
        }

        protected MCTSNode Expand(MCTSNode parent, Action action)
        {
            // action is an untried action from parent

            // add a new child n’ to n
            // S(n’) = Result(S(n),a)
            var newstate = parent.State.GenerateChildWorldModel();
            action.ApplyActionEffects(newstate);
            newstate.CalculateNextPlayer();

            var node = new MCTSNode(newstate)
            {
                Parent = parent,
                Action = action,
            };
            parent.ChildNodes.Add(node);

            return node;
        }

        // Gets the best child of a node, using the UCT formula
        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            double bestChildUct = 0;
            MCTSNode bestChild = null;
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                float u = node.ChildNodes[i].Q / node.ChildNodes[i].N;

                int parentN = node.N;
                var uctResult = u + C * Math.Sqrt(Math.Log(parentN)/ node.ChildNodes[i].N);
                if (uctResult > bestChildUct)
                {
                    bestChildUct = uctResult;
                    bestChild = node.ChildNodes[i];
                }
            }

            return bestChild;
        }

        // Secure Child
        // This method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        // The exploration factor
        protected MCTSNode BestChild(MCTSNode node)
        {
            double bestChildUct = float.MinValue;
            MCTSNode bestChild = null;
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                float u = node.ChildNodes[i].Q / node.ChildNodes[i].N;

                var uctResult = u;
                if (uctResult > bestChildUct)
                {
                    bestChildUct = uctResult;
                    bestChild = node.ChildNodes[i];
                }
            }
            return bestChild;
        }


        protected Action BestFinalAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            //this is done for debugging proposes only
            this.BestActionSequence = new List<Action>();
            this.BestActionSequence.Add(bestChild.Action);
            node = bestChild;

            while (!node.State.IsTerminal())
            {
                bestChild = this.BestChild(node);
                if (bestChild == null) break;
                this.BestActionSequence.Add(bestChild.Action);
                node = bestChild;
            }

            return this.BestFirstChild.Action;
        }

    }
}
