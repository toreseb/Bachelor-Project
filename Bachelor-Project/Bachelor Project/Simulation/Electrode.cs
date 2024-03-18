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

        public Electrode() : this(0, 0)
        {
        }

        public static Electrode GetClosestElectrode(List<Electrode> electrodes, Electrode goal)
        {
            Electrode? closestElectrode = null;
            double minDistance = double.MaxValue;
            foreach (Electrode electrode in electrodes)
            {
                double distance = GetDistance(electrode, goal);
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

        public List<(Electrode, Direction?)> GetExtendedNeighbors()
        {
            List<(Electrode, Direction?)> neighbors = [];
            for (int i = 0; i < 8; i++)
            {
                int xChange = 0;
                int yChange = 0;
                Direction? dir;
                switch (i)
                {
                    case 0:
                        yChange = -1;
                        dir = Direction.UP;
                        break;
                    case 1:
                        yChange = -1;
                        xChange = 1;
                        dir = null;
                        break;
                    case 2:
                        xChange = 1;
                        dir = Direction.RIGHT;
                        break;
                    case 3:
                        yChange = 1;
                        xChange = 1;
                        dir = null;
                        break;
                    case 4:
                        yChange = 1;
                        dir = Direction.DOWN;
                        break;
                    case 5:
                        yChange = 1;
                        xChange = -1;
                        dir = null;
                        break;
                    case 6:
                        xChange = -1;
                        dir = Direction.LEFT;
                        break;
                    case 7:
                        yChange = -1;
                        xChange = -1;
                        dir = null;
                        break;
                    default:
                        throw new Exception("Invalid direction");

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
                x = (Program.C.board.Information.eRow-1) - ePosX;
            }
            int y = ePosY;
            if (ePosY > (Program.C.board.Information.eRow - 1) / 2)
            {
                y = (Program.C.board.Information.eCol-1 ) - ePosY;
            }

            return Math.Min(x, y);


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
