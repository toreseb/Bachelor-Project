using Bachelor_Project.Simulation;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
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

                for(int i = 0; i < 3; i++)
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
                    double tentativeGScore = gScore[current] + dfunc(d, current, neighbor);
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

        private static double dfunc(Droplet d, Electrode start, Electrode end)
        {
            if (end.GetContaminants().Count > 0 && !end.GetContaminants().Exists( x => d.Contamintants.Contains(x))){
                return 1;
            }
            else if(end.GetContaminants().Exists(x => d.Contamintants.Contains(x))) //More or better conditions later, right now for testing.
            {
                return 100;
            }else
            {
                return 2;
            }
        }

    }
}
