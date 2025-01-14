﻿using Assets.Scripts.GameManager;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public int MAX_DEPTH = 3;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public CurrentStateWorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] ActionPerLevel { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth { get; set; }

        public DepthLimitedGOAPDecisionMaking(CurrentStateWorldModel currentStateWorldModel, List<Action> actions, List<Goal> goals)
        {
            this.ActionCombinationsProcessedPerFrame = 200;
            this.Goals = goals;
            this.InitialWorldModel = currentStateWorldModel;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
            this.CurrentDepth = 0;
            this.Models = new WorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.ActionPerLevel = new Action[MAX_DEPTH];
            // Debug only: maybe save best action until calculate next action
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public Action ChooseAction()
        {
            int actionCombinationsProcessed = 0;

            float currentValue;

            var startTime = Time.realtimeSinceStartup;

            while (this.CurrentDepth >= 0 && actionCombinationsProcessed <= this.ActionCombinationsProcessedPerFrame)
            {
                /*currentValue = Models[CurrentDepth].CalculateDiscontentment(this.Goals);

                // Calculate the discontentment value for each depth: if bigger, backtrack
                if (currentValue > this.BestDiscontentmentValue)
                {
                    this.CurrentDepth--;
                    continue;
                }*/

                if (this.CurrentDepth >= MAX_DEPTH)
                {
                    currentValue = Models[CurrentDepth].CalculateDiscontentment(this.Goals);

                    if (currentValue < this.BestDiscontentmentValue)
                    {
                        this.BestDiscontentmentValue = currentValue;
                        this.BestAction = this.ActionPerLevel[0];
                        // save best actions to help debug
                        this.BestActionSequence = this.ActionPerLevel.Clone() as Action[];
                    }
                    this.CurrentDepth -= 1;
                    actionCombinationsProcessed++;
                    this.TotalActionCombinationsProcessed++;
                    continue;
                }

                var nextAction = Models[CurrentDepth].GetNextAction();

                if (nextAction != null)
                {
                    Models[CurrentDepth + 1] = Models[CurrentDepth].GenerateChildWorldModel();
                    nextAction.ApplyActionEffects(Models[CurrentDepth + 1]);
                    ActionPerLevel[CurrentDepth] = nextAction;
                    this.CurrentDepth++;
                }
                else
                {
                    this.CurrentDepth--;
                }

            }

            if (this.CurrentDepth >= 0)
            {
                // Did not finish processing all possible combinations
                return null;
            }

            if (this.CurrentDepth < 0 && this.BestAction == null)
            {
                // Could not find a sequence with MaxDepth length.
                // We have to decrease MaxDepth
                MAX_DEPTH--;
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.InProgress = false;
            return this.BestAction;
        }
    }
}
