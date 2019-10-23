using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicFollowPath : DynamicArrive
    {
        public override string Name
        {
            get { return "Follow Path"; }
        }

        public DynamicFollowPath()
        {
            base.DestinationTarget = new KinematicData();
            this.CurrentParam = 0;
            this.PathOffset = 0.5f;
        }

        public GlobalPath Path { get; set; }
        public float PathOffset { get; set; }
        public float CurrentParam { get; set; }

        public override MovementOutput GetMovement()
        {
            this.CurrentParam = this.Path.GetParam(this.Character.Position, this.CurrentParam);
            var targetParam = this.CurrentParam + this.Path.GetLocalPathOffset(this.CurrentParam); // TODO: what is offset

            base.DestinationTarget.Position = this.Path.GetPosition(targetParam);
            return base.GetMovement();
        }
    }
}
