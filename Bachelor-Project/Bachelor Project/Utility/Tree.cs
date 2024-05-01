using Bachelor_Project.Simulation;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    public class Tree
    {
        public List<Node> Nodes;
        List<Node> ActiveNodes;
        List<Electrode> SeenElectrodes;
        List<Electrode> NewElectrodes;
        Droplet d;
        public List<Node> Leaves;
        public Electrode closestElectrode;
        public Tree(Droplet d, List<Electrode> oldElectrodes, List<Electrode> newElectrodes, Electrode center)
        {
            closestElectrode = FindClosestElectrode(oldElectrodes, center);
            SeenElectrodes = [closestElectrode];
            NewElectrodes = newElectrodes;
            Node root = new(closestElectrode);
            Nodes = [root];
            ActiveNodes = [root];
            Leaves = [];
            this.d = d;
            BuildTree();
        }

        private void BuildTree()
        {
            while (ActiveNodes.Count > 0)
            {
                Node currentNode = ActiveNodes[0];
                Electrode cElectrode;
                try
                {
                    cElectrode = Program.C.board.Electrodes[currentNode.Electrode.ePosX + 1, currentNode.Electrode.ePosY];
                    CheckAddElectrode(d, currentNode, cElectrode);
                }
                catch { }
                try
                {
                    cElectrode = Program.C.board.Electrodes[currentNode.Electrode.ePosX - 1, currentNode.Electrode.ePosY];
                    CheckAddElectrode(d, currentNode, cElectrode);
                }
                catch{ }
                try
                {
                    cElectrode = Program.C.board.Electrodes[currentNode.Electrode.ePosX, currentNode.Electrode.ePosY + 1];
                    CheckAddElectrode(d, currentNode, cElectrode);
                }
                catch{ }
                try
                {
                    cElectrode = Program.C.board.Electrodes[currentNode.Electrode.ePosX, currentNode.Electrode.ePosY - 1];
                    CheckAddElectrode(d, currentNode, cElectrode);
                }
                catch{ }
                
                
                
                ActiveNodes.Remove(currentNode);
                if (currentNode.Children.Count == 0)
                {
                    Leaves.Add(currentNode);
                }
            }
        }
        public void RemoveLeaf(bool into = false)
        {
            Node cLeaf = Leaves[0];
            if (cLeaf.Parent != null || into != null)
            {
                if (!NewElectrodes.Contains(cLeaf.Electrode))
                {
                    Printer.PrintLine("moving off electrode: "+cLeaf.Electrode.Name);
                    Droplet_Actions.MoveOffElectrode(d, cLeaf.Electrode);
                }

                if (cLeaf.Parent != null)
                {
                    cLeaf.Parent.RemoveChild(cLeaf);

                    if (cLeaf.Parent.Children.Count == 0)
                    {
                        Leaves.Add(cLeaf.Parent);
                    }
                }
                
            }
            Nodes.Remove(cLeaf);
            Leaves.Remove(cLeaf);
        }



 
        public void RemoveTree(bool into = false)
        {
            while(Leaves.Count > 0)
            {
                RemoveLeaf(into);

            }
        }
        

        private void CheckAddElectrode(Droplet d, Node currentNode, Electrode cElectrode)
        {
            if (d.Occupy.Contains(cElectrode) && !SeenElectrodes.Contains(cElectrode))
            {
                Node child = new(cElectrode);
                currentNode.AddChild(child);
                child.Parent = currentNode;
                Nodes.Add(child);
                ActiveNodes.Add(child);
                SeenElectrodes.Add(cElectrode);
            }
        }

        private static Electrode FindClosestElectrode(List<Electrode> electrodes, Electrode center)
        {
            Electrode? closestElectrode = null;
            double minDistance = double.MaxValue;
            foreach (Electrode electrode in electrodes)
            {
                double distance = Math.Sqrt(Math.Pow(electrode.ePosX - center.ePosX, 2) + Math.Pow(electrode.ePosY - center.ePosY, 2));
                if (distance < minDistance)
                {
                    closestElectrode = electrode;
                    minDistance = distance;
                }
            }
            return closestElectrode;
        }   

    }
    public class Node
    {
        public Node? Parent;
        public List<Node> Children;
        public Electrode Electrode;
        public Node(Electrode electrode)
        {
            Parent = null;
            Children = new();
            Electrode = electrode;
        }
        public void AddChild(Node child)
        {
            Children.Add(child);
        }
        public void RemoveChild(Node child)
        {
            Children.Remove(child);
        }
        public override string ToString()
        {
            return Electrode.ToString();
        }
    }
}
