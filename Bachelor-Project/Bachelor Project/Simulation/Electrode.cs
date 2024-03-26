using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    public class Electrode(int x, int y, string name = "") : TileEntity(x, y, 20, 20, name)
    {
        public int ElectrodeID { get; set; }
        public int DriverID { get; set; }
        public int Status { get; set; }
        public int ePosX { get; set; } // Electrode position X
        public int ePosY { get; set; } // Electrode position Y
        // Contamination of tile in grid, may need changing later.
        private List<string> Contaminants { get; set; } = [];
        public Droplet? Occupant;
        public Apparature? Apparature;

        public double? smallestGScore = null;

        public Electrode() : this(0, 0)
        {
        }

        public Electrode GetClosestElectrodeInList(List<Electrode> electrodes)
        {
            Electrode? closestElectrode = null;
            double minDistance = double.MaxValue;
            foreach (Electrode electrode in electrodes)
            {
                double distance = GetDistance(electrode, this);
                if (distance < minDistance)
                {
                    closestElectrode = electrode;
                    minDistance = distance;
                }
            }
            if (closestElectrode == null)
            {
                throw new ArgumentException("No electrodes found");
            }
            return closestElectrode;
        }

        public List<(Electrode, Direction)> GetTrueNeighbors()
        {
            List<(Electrode, Direction)> neighbors = [];
            for (int i = 0; i < 4; i++)
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
                if (Droplet_Actions.CheckEdge(ePosX + xChange, ePosY + yChange))
                {
                    neighbors.Add((Program.C.board.Electrodes[ePosX + xChange, ePosY + yChange],dir));
                }
            }
            return neighbors;
        }

        public Electrode ElectrodeStep(Direction dir)
        {
            switch (dir)
            {
                case Direction.UP:
                    return Program.C.board.Electrodes[ePosX, ePosY - 1];
                case Direction.RIGHT:
                    return Program.C.board.Electrodes[ePosX + 1, ePosY];
                case Direction.DOWN:
                    return Program.C.board.Electrodes[ePosX, ePosY + 1];
                case Direction.LEFT:
                    return Program.C.board.Electrodes[ePosX - 1, ePosY];
                default:
                    throw new Exception("Invalid direction");
            }
        }

        public List<(Electrode, Direction?)> GetExtendedNeighbors()
        {
            List<(Electrode, Direction?)> neighbors = [];
            bool upSeen = false;
            bool rightSeen = false;
            bool downSeen = false;
            bool leftSeen = false;
            for (int i = 0; i < 4; i++)
            {
                
                int xChange = 0;
                int yChange = 0;
                Direction? dir;
                switch (i)
                {
                    case 0:
                        yChange = -1;
                        upSeen = true;
                        dir = Direction.UP;
                        break;
                    case 1:
                        xChange = 1;
                        rightSeen = true;
                        dir = Direction.RIGHT;
                        break;
                    case 2:
                        yChange = 1;
                        downSeen = true;
                        dir = Direction.DOWN;
                        break;
                    case 3:
                        xChange = -1;
                        leftSeen = true;
                        dir = Direction.LEFT;
                        break;
                    default:
                        throw new Exception("Invalid direction");
                }
                if (Droplet_Actions.CheckEdge(ePosX + xChange, ePosY + yChange))
                {
                    neighbors.Add((Program.C.board.Electrodes[ePosX + xChange, ePosY + yChange], dir));
                }
            }
            for(int i = 0; i < 4; i++)
            {
                int xChange = 0;
                int yChange = 0;
                Direction? dir = null;
                if (i == 0 && (upSeen || rightSeen))
                {
                    xChange = 1;
                    yChange = -1;
                    dir = null;
                }else if (i == 1 &&(rightSeen || downSeen))
                {
                    xChange = 1;
                    yChange = 1;
                    dir = null;
                }else if (i == 2 && (downSeen || leftSeen))
                {
                    xChange = -1;
                    yChange = 1;
                    dir = null;
                }else if (i == 3 && (leftSeen || upSeen))
                {
                    xChange = -1;
                    yChange = -1;
                    dir = null;
                }
                if (Droplet_Actions.CheckEdge(ePosX + xChange, ePosY + yChange))
                {
                    neighbors.Add((Program.C.board.Electrodes[ePosX + xChange, ePosY + yChange], dir));
                }
            }
            return neighbors;
        }

        public static double GetDistance(Electrode e1, Electrode e2)
        {
            return Math.Sqrt(Math.Pow(e1.ePosX - e2.ePosX, 2) + Math.Pow(e1.ePosY - e2.ePosY, 2));
        }

        public int GetDistanceToBorder()
        {
            int x = ePosX;
            if (ePosX > (Program.C.board.Information.eRow - 1) / 2)
            {
                x = (Program.C.board.Information.eRow -1) - ePosX;
            }
            int y = ePosY;
            if (ePosY > (Program.C.board.Information.eRow - 1) / 2)
            {
                y = (Program.C.board.Information.eCol -1) - ePosY;
            }

            return Math.Min(x+1, y+1);


        }

        public void Contaminate(string contaminator)
        {
            Contaminants.Add(contaminator);
        }

        public List<string> GetContaminants()
        {
            return Contaminants;
        }

        public int GetStatus()
        {
            return Status;
        }

        public override string ToString()
        {
            return Name + " x: " + ePosX + " y: " + ePosY;
        }

    }
}
