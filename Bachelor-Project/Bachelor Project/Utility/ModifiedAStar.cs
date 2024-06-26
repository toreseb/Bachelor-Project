﻿using Bachelor_Project.Simulation;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace Bachelor_Project.Utility
{
    /// <summary>
    /// Heavily inspired by https://en.wikipedia.org/wiki/A*_search_algorithm
    /// </summary>
    static class ModifiedAStar 
    {

        public static object PathLock = new object();

        /// <summary>
        /// A function which reconstructs the path the <see cref="ModifiedAStar"/> algorihm found, using <paramref name="cameFrom"/>, which is the paths its taken, and <paramref name="current"/>, which is the end.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="cameFrom"></param>
        /// <param name="current"></param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Electrode"/> and <see cref="Direction"/>s containing the <see cref="Electrode"/>s in the path, and the <see cref="Direction"/> necessary to take it.</returns>
        private static (List<(Electrode, Direction?)>, int) ReconstructPath(Droplet d, Dictionary<Electrode, (Electrode, Direction)> cameFrom, Electrode current)
        {
            LinkedList<(Electrode, Direction?)> totalPath = [];
            totalPath.AddFirst((current,null));
            int moveInsideSelf = 0;
            while (cameFrom.ContainsKey(current))
            {
                
                Direction oldDir = cameFrom[current].Item2;
                Printer.PrintLine(current.Name + " dir: " + oldDir);
                current = cameFrom[current].Item1;
                totalPath.AddFirst((current, oldDir));
                if (d.Occupy.Contains(current))
                {
                    moveInsideSelf++;
                }
            }
            return (totalPath.ToList(),moveInsideSelf-1);
        }

        /// <summary>
        /// The function which performs the <see cref="ModifiedAStar"/> algorithm. It traverses the <see cref="Board"/> as a heatmap, to find the best path for the <see cref="Droplet"/> <paramref name="d"/>.
        /// <para>The path has a <paramref name="start"/> and a <paramref name="goal"/></para>
        /// <para><paramref name="mergeDroplets"/> and <paramref name="splitDroplet"/> are given to <see cref="dfunc(Droplet, Electrode, List{string}?, string?)"/></para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="goal"></param>
        /// <param name="mergeDroplets"></param>
        /// <param name="splitDroplet"></param>
        /// <param name="start"></param>
        /// <returns>The reconstructed path that the <see cref="Droplet"/> <paramref name="d"/> would take.</returns>
        /// <exception cref="ThreadInterruptedException"></exception>
        /// <exception cref="Exception"></exception>
        public static (List<(Electrode, Direction?)>, int) FindPath(Droplet d, Electrode goal, List<string>? mergeDroplets = null, string? splitDroplet = null, Electrode? start = null)
        {
            

            Func<Electrode, Electrode, double> h = Electrode.GetDistance;
            lock (Droplet_Actions.MoveLock)
            {
                if (d.Removed)
                {
                    throw new ThreadInterruptedException();
                }
                start ??= goal.GetClosestElectrodeInList(d.Occupy);
            }
            
            lock (PathLock)
            {
                Program.C.SetPath(d, start, goal, mergeDroplets);
            }
            
            List<Electrode> openSet = [start];

            Dictionary<Electrode, (Electrode,Direction)> cameFrom = [];

            Dictionary<Electrode, double> gScore = [];
            gScore.Add(start,0);

            Dictionary<Electrode, double> fScore = [];
            fScore.Add(start, h(start, goal));

            while(openSet.Count > 0)
            {
                Electrode? current = null;
                double leastFScore = double.MaxValue;
                for (int i = 0; i < openSet.Count;i++)
                {
                    Electrode item = openSet[i];
                    if (fScore[item]<leastFScore)
                    {
                        leastFScore = fScore[item];
                        current = item;
                    }
                }
                if (current == null)
                {
                    Program.C.RemovePath(d);
                    return ([], 0);

                }
                if (current == goal)
                {
                    return ReconstructPath(d, cameFrom, current);
                }
                openSet.Remove(current);

                List<(Electrode,Direction)> neighbors = current.GetTrueNeighbors();

                foreach((Electrode,Direction) neighbor in neighbors)
                {

                    Electrode neighborT = neighbor.Item1;
                    if (cameFrom.TryGetValue(current, out (Electrode, Direction) value) && neighbor.Equals(value))
                    {
                        continue;
                    }
                    double tentativeGScore = gScore[current] + dfunc(d, neighborT, mergeDroplets, splitDroplet);
                    if (!gScore.TryGetValue(neighborT, out double value2))
                    {
                        value2 = double.MaxValue;
                        gScore.Add(neighborT, value2);
                        

                        
                        //neighborT.smallestGScore = double.MaxValue;

                    }
                    if (tentativeGScore < value2)
                    {
                        

                        //neighborT.smallestGScore = tentativeGScore;
                        cameFrom[neighborT] = (current, neighbor.Item2);
                        gScore[neighborT] = tentativeGScore;
                        if (!fScore.ContainsKey(neighborT))
                        {
                            fScore.Add(neighborT, gScore[neighborT] + h(neighborT, goal));
                        }
                        else
                        {
                            fScore[neighborT] = gScore[neighborT] + h(neighborT, goal);
                        }
                        
                        if (!openSet.Contains(neighborT))
                        {
                            openSet.Add(neighborT);
                        }
                    }
                }


            }
            throw new Exception("No path found");

        }

        /// <summary>
        /// Used to create the heatmap <see cref="ModifiedAStar"/> uses. It essentially calculate the value of the <see cref="Electrode"/> <paramref name="end"/> for the <see cref="Droplet"/> <paramref name="d"/>.
        /// <para><paramref name="mergeDroplets"/> and <paramref name="splitDroplet"/> can be specified, which allows the algorithm to move through the borders and the <paramref name="mergeDroplets"/> themselves.</para>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="end"></param>
        /// <param name="mergeDroplets"></param>
        /// <param name="splitDroplet"></param>
        /// <returns>The quality of moving onto the <see cref="Electrode"/> <paramref name="end"/> for the <see cref="Droplet"/> <paramref name="d"/></returns>
        private static double dfunc(Droplet d, Electrode end, List<string>? mergeDroplets = null, string? splitDroplet = null)
        {
            int distance = end.GetDistanceToBorder();
            int multiple = 10 * (int)Math.Pow(distance,2);

            if (!Droplet_Actions.CheckLegalMove(d, [end],mergeDroplets: mergeDroplets, source: splitDroplet).legalmove) // 1: check if the move is legal
            {
                return 100000* multiple;
            }else if (end.Apparatus != null && !end.GetContaminants().Contains(d.Substance_Name)) // 2: Check if the end is an Apparatus, and therefore important
            {
                return multiple * 100;
            } else if (end.GetContaminants().Count > 0 && !end.GetContaminants().Exists( x => d.Contamintants.Contains(x))){ // 3: Check if highway
                return 1;
            }
            return multiple;
            
              

            /*
            (bool foundEdge, int distanceToEdge) = CheckSquare(d, end, dir);
            if (foundEdge)
            {
                return (4-distanceToEdge)*3;
            }
            */

            

        }
        /*

        private static (bool, int) CheckSquare(Droplet d, Electrode end, Direction dir) // TODO: Change it to look at the entire square
        {
            if (d.SquareInfo.Item1 == null && d.SquareInfo.Item2 == null)
            {
                d.SquareInfo = FindSquare(d, end);
                if (d.SquareInfo.Item1 == null && d.SquareInfo.Item2 == null)
                {
                    return (false, 0);
                }
            }
            
            int distance = d.SquareInfo.Item1.Value;
            int jDist = 0;
            bool checkX;
            switch (dir)
            {
                case Direction.LEFT:
                    jDist = -1;
                    checkX = false;
                    break;
                case Direction.UP:
                    jDist = -1;
                    checkX = true;
                    break;
                case Direction.RIGHT:
                    jDist = 1;
                    checkX = false;
                    break;
                case Direction.DOWN:
                    jDist = 1;
                    checkX = true;
                    break;
                default:
                    throw new Exception("Invalid direction");
            }
            for (int j = 0;j < distance; j += 1)
            {
                int funcJ = j * jDist;
                for (int i = -(distance-1); i < distance; i += 1)
                {
                    if (checkX)
                    {
                        if (!Droplet_Actions.CheckLegalPosition(d, [(end.ePosX + i, end.ePosY + funcJ)]))
                        {
                            return (true, Math.Abs(j));
                        }
                    }
                    else
                    {
                        if (!Droplet_Actions.CheckLegalPosition(d, [(end.ePosX + funcJ, end.ePosY + i)]))
                        {
                            return (true, Math.Abs(j));
                        }
                    }
                }
            }
            return (false, 0);

        }

        private static (int?, Direction?) FindSquare(Droplet d, Electrode end)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 4; j++)
                {
                    int xChange = 0;
                    int yChange = 0;
                    Direction dir;
                    switch (i)
                    {
                        case 0:
                            xChange = -j;
                            dir = Direction.LEFT;
                            break;
                        case 1:
                            yChange = -j;
                            dir = Direction.UP;
                            break;
                        case 2:
                            xChange = j;
                            dir = Direction.RIGHT;
                            break;
                        case 3:
                            yChange = j;
                            dir = Direction.DOWN;
                            break;
                        default:
                            throw new Exception("Invalid direction");
                    }
                    if (!Droplet_Actions.CheckLegalPosition(d, [(end.ePosX + xChange, end.ePosY + yChange)]))
                    {
                        return (j, dir);
                    }
                }
            }
            return (null, null);

        }
        */

    }
}
