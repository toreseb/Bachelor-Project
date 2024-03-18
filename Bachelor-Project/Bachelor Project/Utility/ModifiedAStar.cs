using Bachelor_Project.Simulation;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Utility
{
    internal class ModifiedAStar //Heavily inspired by https://en.wikipedia.org/wiki/A*_search_algorithm
    {

        public static List<(Electrode, Direction?)> ReconstructPath(Dictionary<Electrode, (Electrode, Direction)> cameFrom, Electrode current)
        {
            LinkedList<(Electrode, Direction?)> totalPath = [];
            totalPath.AddFirst((current,null));
            while (cameFrom.ContainsKey(current))
            {
                Direction oldDir = cameFrom[current].Item2;
                current = cameFrom[current].Item1;
                totalPath.AddFirst((current, oldDir));

            }
            return totalPath.ToList();
        }


        public static List<(Electrode, Direction?)> FindPath(Droplet d, Electrode goal)
        {
            Func<Electrode, Electrode, double> h = Electrode.GetDistance;
            Electrode start = Electrode.GetClosestElectrode(d.Occupy, goal);
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
                    return [];
                }
                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current);
                }
                openSet.Remove(current);

                for(int i = 0; i < 4; i++)
                {
                    int xChange = 0;
                    int yChange = 0;
                    Direction dir;
                    switch (i)
                    {
                        case 0:
                            yChange = -1;
                            dir = Direction.UP;
                            break;
                        case 1:
                            xChange = 1;
                            dir = Direction.RIGHT;
                            break;
                        case 2:
                            yChange = 1;
                            dir = Direction.DOWN;
                            break;
                        case 3:
                            xChange = -1;
                            dir = Direction.LEFT;
                            break;
                        default:
                            throw new Exception("Invalid direction");

                    }
                    if (!Droplet_Actions.CheckEdge(current.ePosX + xChange, current.ePosY + yChange))
                    {
                        continue;
                    }
                    Electrode neighbor = Program.C.board.Electrodes[current.ePosX+xChange, current.ePosY+yChange];
                    if (cameFrom.ContainsKey(current) && neighbor.Equals(cameFrom[current]))
                    {
                        continue;
                    }
                    double tentativeGScore = gScore[current] + dfunc(d, current, neighbor, dir);
                    if (!gScore.ContainsKey(neighbor))
                    {
                        gScore.Add(neighbor, double.MaxValue);
                    }
                    if (tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = (current, dir);
                        gScore[neighbor] = tentativeGScore;
                        if (!fScore.ContainsKey(neighbor))
                        {
                            fScore.Add(neighbor, gScore[neighbor] + h(neighbor, goal));
                        }
                        else
                        {
                            fScore[neighbor] = gScore[neighbor] + h(neighbor, goal);
                        }
                        
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }


            }
            throw new Exception("No path found");

        }
        private static double dfunc(Droplet d, Electrode start, Electrode end, Direction dir)
        {
            int multiple = 3 * (end.GetDistanceToBorder());

            if (!Droplet_Actions.CheckLegalMove(d, [end])) // 1: check if the move is legal
            {
                return multiple * 1000;
            }else if (end.Apparature != null) // 2: Check if the end is an apparature, and therefore important
            {
                return (multiple+5 * 5);
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

    }
}
