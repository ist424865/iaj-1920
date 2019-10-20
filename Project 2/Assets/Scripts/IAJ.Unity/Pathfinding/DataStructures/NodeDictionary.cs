using System.Collections.Generic;
using System.Linq;
using RAIN.Navigation.Graph;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    //very simple (and unefficient) implementation of the open/closed sets
    public class NodeDictionary : IOpenSet, IClosedSet
    {
        private Dictionary<NavigationGraphNode, NodeRecord> NodeRecords { get; set; }

        public NodeDictionary()
        {
            this.NodeRecords = new Dictionary<NavigationGraphNode, NodeRecord>();
        }

        public void Initialize()
        {
            this.NodeRecords.Clear();
        }

        public int CountOpen()
        {
            return this.NodeRecords.Count;
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Add(nodeRecord.node, nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Remove(nodeRecord.node);
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.TryGetValue(nodeRecord.node, out NodeRecord node);
            return node;
        }

        public void AddToOpen(NodeRecord nodeRecord)
        {
            this.NodeRecords.Add(nodeRecord.node, nodeRecord);
        }

        public void RemoveFromOpen(NodeRecord nodeRecord)
        {
            this.NodeRecords.Remove(nodeRecord.node);
        }

        public NodeRecord SearchInOpen(NodeRecord nodeRecord)
        {
            this.NodeRecords.TryGetValue(nodeRecord.node, out NodeRecord node);
            return node;
        }

        public ICollection<NodeRecord> All()
        {
            return this.NodeRecords.Values;
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            //since the list is not ordered we do not need to remove the node and add the new one, just copy the different values
            //remember that if NodeRecord is a struct, for this to work we need to receive a reference
            nodeToBeReplaced.parent = nodeToReplace.parent;
            nodeToBeReplaced.fValue = nodeToReplace.fValue;
            nodeToBeReplaced.gValue = nodeToReplace.gValue;
            nodeToBeReplaced.hValue = nodeToReplace.hValue;
        }

        public NodeRecord GetBestAndRemove()
        {
            var best = this.PeekBest();
            this.NodeRecords.Remove(best.node);
            return best;
        }

        public NodeRecord PeekBest()
        {
            return this.NodeRecords.Values.Aggregate((nodeRecord1, nodeRecord2) => nodeRecord1.fValue < nodeRecord2.fValue ? nodeRecord1 : nodeRecord2);
        }
    }
}
