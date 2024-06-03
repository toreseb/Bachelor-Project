using Bachelor_Project.Simulation;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bachelor_Project.Utility
{
    /// <summary>
    /// <see cref="Tree"/> structure used to remove dropelts effeciently.
    /// </summary>
    public class Tree
    {
        public List<Node> Nodes;
        private List<Node> ActiveNodes;
        private List<Electrode> SeenElectrodes;
        private List<Electrode> NewElectrodes;
        private Droplet d;
        public List<Node> Leaves;
        public Electrode closestElectrode;
        /// <summary>
        /// Creates a <see cref="Tree"/> structure from a <see cref="Droplet"/> with <paramref name="oldElectrodes"/> from before, with root as <paramref name="center"/>, or as close to it, and it keeps the <paramref name="newElectrodes"/>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="oldElectrodes"></param>
        /// <param name="newElectrodes"></param>
        /// <param name="center"></param>
        public Tree(Droplet d, List<Electrode> oldElectrodes, List<Electrode> newElectrodes, Electrode center)
        {
            if (!oldElectrodes.Contains(center))
            {
                closestElectrode = FindClosestElectrode(oldElectrodes, center);
            }
            else
            {
                closestElectrode = center;
            }
            SeenElectrodes = [closestElectrode];
            NewElectrodes = newElectrodes;
            Node root = new(closestElectrode);
            Nodes = [root];
            ActiveNodes = [root];
            Leaves = [];
            this.d = d;
            BuildTree();
            
        }

        /// <summary>
        /// Creates the <see cref="Tree"/>, by traversing the <see cref="Droplet"/> <see cref="d"/>.
        /// </summary>
        /// <exception cref="ThreadInterruptedException"></exception>
        private void BuildTree()
        {
            while (ActiveNodes.Count > 0)
            {
                Node currentNode = ActiveNodes[0];
                if (currentNode.Electrode == null)
                {
                    throw new ThreadInterruptedException();
                }
                List<(Electrode, Direction)> neighbors = currentNode.Electrode.GetTrueNeighbors();

                foreach ((Electrode cEl, Direction _) in neighbors)
                {
                    CheckAddElectrode(d, currentNode, cEl);
                }

                
                ActiveNodes.Remove(currentNode);
                if (currentNode.Children.Count == 0)
                {
                    Leaves.Add(currentNode);
                }
            }
        }

        /// <summary>
        /// Removes a leaf of the <see cref="Tree"/> by moving of a leaf, that is not a part of <see cref="NewElectrodes"/>.
        /// </summary>
        /// <param name="into"></param>
        /// <returns>Which <see cref="Electrode"/> the <see cref="Droplet"/> <see cref="d"/> moved off.</returns>
        public Electrode? RemoveLeaf(bool into = false)
        {
            Electrode? returnedElectrode = null;
            if (Leaves.Count == 0)
            {
                return null;
            }
            Node cLeaf = Leaves[0];
            bool removed = false;
            while (!removed)
            {
                if (Leaves.Count == 0)
                {
                    break;
                }
                cLeaf = Leaves[0];
                if (cLeaf.Parent != null || into != false)
                {
                    if (!NewElectrodes.Contains(cLeaf.Electrode))
                    {
                        Printer.PrintLine("moving off electrode: " + cLeaf.Electrode.Name);
                        Droplet_Actions.MoveOffElectrode(d, cLeaf.Electrode);
                        returnedElectrode = cLeaf.Electrode;
                        removed = true;
                    }
                    else
                    {
                        int a = 2;
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
            return returnedElectrode;
        }

        /// <summary>
        /// Removes the entire <see cref="Tree"/>, by calling <see cref="RemoveLeaf(bool)"/> until no nodes remain.
        /// </summary>
        /// <param name="into"></param>
        public void RemoveTree(bool into = false)
        {
            while(Leaves.Count > 0)
            {
                RemoveLeaf(into);

            }
        }
        
        /// <summary>
        /// Used to create the <see cref="Tree"/>, by adding <see cref="Electrode"/>s to the tree, only if they are new, and a part of <see cref="d"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="currentNode"></param>
        /// <param name="cElectrode"></param>
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

        /// <summary>
        /// Used to create the <see cref="Tree"/>, by finding the closests <see cref="Electrode"/> to the center in a <see cref="List{T}"/>
        /// </summary>
        /// <param name="electrodes"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        private static Electrode FindClosestElectrode(List<Electrode> electrodes, Electrode center)
        {
            Electrode? closestElectrode = null;
            double minDistance = double.MaxValue;
            foreach (Electrode electrode in electrodes)
            {
                double distance = Math.Sqrt(Math.Pow(electrode.EPosX - center.EPosX, 2) + Math.Pow(electrode.EPosY - center.EPosY, 2));
                if (distance < minDistance)
                {
                    closestElectrode = electrode;
                    minDistance = distance;
                }
            }
            return closestElectrode;
        }

        /// <summary>
        /// Checks if the <see cref="Tree"/> currently has the same amount of nodes as the <see cref="Droplet"/> <see cref="d"/> has <see cref="Electrode"/>s in its <see cref="Droplet.Occupy"/>
        /// </summary>
        /// <returns> <see langword="true"/> if the amount is the same.</returns>
        public bool CheckTree()
        {
            if (Nodes.Count != d.Occupy.Count)
            {
                return false;
            }
            return true;
        }

    }

    /// <summary>
    /// The <see cref="Node"/>s the <see cref="Tree"/> is built of.
    /// </summary>
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

        /// <summary>
        /// Inserts the <see cref="Node"/> <paramref name="child"/> into this <see cref="Node"/>'s <see cref="Children"/>.
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(Node child)
        {
            Children.Add(child);
        }

        /// <summary>
        /// Removes the <see cref="Node"/> <paramref name="child"/> from this <see cref="Node"/>'s <see cref="Children"/>.
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(Node child)
        {
            Children.Remove(child);
        }


        /// <summary>
        /// ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Electrode.ToString();
        }
    }
}
