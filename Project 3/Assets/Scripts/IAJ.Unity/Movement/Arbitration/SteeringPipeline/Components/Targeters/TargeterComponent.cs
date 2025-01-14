﻿namespace Assets.Scripts.IAJ.Unity.Movement.Arbitration.SteeringPipeline.Components.Targeters
{
    //A targeter tells the system where the character should be going
    public abstract class TargeterComponent : SteeringPipelineComponent
    {
        public abstract SteeringGoal GetGoal();
    }
}
