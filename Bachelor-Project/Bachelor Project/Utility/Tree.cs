﻿using Bachelor_Project.Simulation;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bachelor_Project.Utility
{
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
        /// Creates a tree structure from a droplet with oldElectrodes from before, with root as center, or as close to it, and it keeps the newElectrodes
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
                double distance = Math.Sqrt(Math.Pow(electrode.EPosX - center.EPosX, 2) + Math.Pow(electrode.EPosY - center.EPosY, 2));
                if (distance < minDistance)
                {
                    closestElectrode = electrode;
                    minDistance = distance;
                }
            }
            return closestElectrode;
        }
        public bool CheckTree()
        {
            if (Nodes.Count != d.Occupy.Count)
            {
                return false;
            }
            return true;
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
