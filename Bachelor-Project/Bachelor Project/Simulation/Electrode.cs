using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    /// <summary>
    /// The class of <see cref="Electrode"/>s which make up the <see cref="Board"/> itself.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="name"></param>
    public class Electrode(int x, int y, string name = "") : TileEntity(x, y, 20, 20, name)
    {
        public int ElectrodeID { get; set; }
        public int DriverID { get; set; }
        public int Status { get; set; }
        public int EPosX { get; set; } // Electrode position X
        public int EPosY { get; set; } // Electrode position Y
        // Contamination of tile in grid, may need changing later.
        private List<string> Contaminants { get; set; } = [];
        public Droplet? Occupant;
        public Apparatus? Apparatus;

        public double? smallestGScore = null;

        public Electrode() : this(0, 0)
        {
        }

        /// <summary>
        /// Finds the closest <see cref="Electrode"/> to this <see cref="Electrode"/> in the list. 
        /// </summary>
        /// <param name="electrodes"></param>
        /// <returns> The closests <see cref="Electrode"/></returns>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Finds the four <see cref="Electrode"/>s around this <see cref="Electrode"/> in the four <see cref="Direction"/>s which are on the <see cref="Board"/>.
        /// </summary>
        /// <returns>The <see cref="Electrode"/>s found.</returns>
        /// <exception cref="Exception"></exception>
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
                if (Droplet_Actions.CheckBoardEdge(EPosX + xChange, EPosY + yChange))
                {
                    neighbors.Add((Program.C.board.Electrodes[EPosX + xChange, EPosY + yChange],dir));
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Finds the <see cref="Electrode"/> that is the <see cref="Direction"/> <paramref name="dir"/> from this <see cref="Electrode"/>.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns>The found <see cref="Electrode"/></returns>
        /// <exception cref="Exception"></exception>
        public Electrode ElectrodeStep(Direction dir)
        {
            switch (dir)
            {
                case Direction.UP:
                    return Program.C.board.Electrodes[EPosX, EPosY - 1];
                case Direction.RIGHT:
                    return Program.C.board.Electrodes[EPosX + 1, EPosY];
                case Direction.DOWN:
                    return Program.C.board.Electrodes[EPosX, EPosY + 1];
                case Direction.LEFT:
                    return Program.C.board.Electrodes[EPosX - 1, EPosY];
                default:
                    throw new Exception("Invalid direction");
            }
        }

        /// <summary>
        /// Finds the eight <see cref="Electrodes"/> that are around this <see cref="Electrode"/>. If a <paramref name="d"/> is given, it also checks if the movement is legal for the <see cref="Droplet"/> <paramref name="d"/>.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="source"></param>
        /// <param name="splitPlacement"></param>
        /// <param name="includeApp"></param>
        /// <returns>The found extended neighbous</returns>
        /// <exception cref="Exception"></exception>
        public List<(Electrode, Direction?)> GetExtendedNeighbors(Droplet? d = null, Droplet? source = null, bool splitPlacement = false, bool includeApp = true)
        {
            List<(Electrode, Direction?)> neighbors = [];

            List<bool> cBool = [];

            for (int i = 0; i < 4; i++)
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
                if (Droplet_Actions.CheckBoardEdge(EPosX + xChange, EPosY + yChange))
                {
                    Electrode el = Program.C.board.Electrodes[EPosX + xChange, EPosY + yChange];
                    if (d != null && !Droplet_Actions.CheckLegalMove(d, [el], source: source?.Name, splitPlacement: splitPlacement).legalmove && (includeApp || el.Apparatus == null))
                    {
                        cBool.Add(false);
                        continue;
                    }
                    cBool.Add(true);
                    neighbors.Add((Program.C.board.Electrodes[EPosX + xChange, EPosY + yChange], dir));
                }
                else
                {
                    cBool.Add(false);
                }
            }
            for(int i = 0; i < 4; i++)
            {
                int xChange = 0;
                int yChange = 0;
                Direction? dir = null;
                if (i == 0 && (cBool[0] || cBool[1]))
                {
                    xChange = 1;
                    yChange = -1;
                    dir = null;
                }else if (i == 1 &&(cBool[1] || cBool[2]))
                {
                    xChange = 1;
                    yChange = 1;
                    dir = null;
                }else if (i == 2 && (cBool[2] || cBool[3]))
                {
                    xChange = -1;
                    yChange = 1;
                    dir = null;
                }else if (i == 3 && (cBool[3] || cBool[0]))
                {
                    xChange = -1;
                    yChange = -1;
                    dir = null;
                }
                if (Droplet_Actions.CheckBoardEdge(EPosX + xChange, EPosY + yChange))
                {
                    if (d != null && !Droplet_Actions.CheckLegalMove(d, [Program.C.board.Electrodes[EPosX + xChange, EPosY + yChange]]).legalmove)
                    {
                        continue;
                    }
                    neighbors.Add((Program.C.board.Electrodes[EPosX + xChange, EPosY + yChange], dir));
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Finds the four extended neighbors that are not true neighbours based of which directions are in <paramref name="foundTrueNeighbors"/>.
        /// </summary>
        /// <param name="foundTrueNeighbors"></param>
        /// <returns></returns>
        public List<Electrode> GetExtendedNeighborsFromTrue(List<Direction> foundTrueNeighbors)
        {
            List<Electrode> extendedNeighbors = [];
            for (int i = 0; i < 4; i++)
            {
                int xChange = 0;
                int yChange = 0;
                if (i == 0 && (foundTrueNeighbors.Contains(Direction.UP)|| foundTrueNeighbors.Contains(Direction.RIGHT)))
                {
                    xChange = 1;
                    yChange = -1;
                }
                else if (i == 1 && (foundTrueNeighbors.Contains(Direction.RIGHT) || foundTrueNeighbors.Contains(Direction.DOWN)))
                {
                    xChange = 1;
                    yChange = 1;
                }
                else if (i == 2 && (foundTrueNeighbors.Contains(Direction.DOWN) || foundTrueNeighbors.Contains(Direction.LEFT)))
                {
                    xChange = -1;
                    yChange = 1;
                }
                else if (i == 3 && (foundTrueNeighbors.Contains(Direction.LEFT) || foundTrueNeighbors.Contains(Direction.UP)))
                {
                    xChange = -1;
                    yChange = -1;
                }
                if (Droplet_Actions.CheckBoardEdge(EPosX + xChange, EPosY + yChange))
                {
                    extendedNeighbors.Add(Program.C.board.Electrodes[EPosX + xChange, EPosY + yChange]);
                }
            }
            return extendedNeighbors;
        }

        /// <summary>
        /// Finds the distance between the two <see cref="Electrode"/>s, <paramref name="e1"/> and <paramref name="e2"/>.
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <returns>The distance as a <see cref="float"/></returns>
        public static double GetDistance(Electrode e1, Electrode e2)
        {
            return Math.Sqrt(Math.Pow(e1.EPosX - e2.EPosX, 2) + Math.Pow(e1.EPosY - e2.EPosY, 2));
        }

        /// <summary>
        /// Finds the distance of this <see cref="Electrode"/> to the edges of the <see cref="Board"/>.
        /// </summary>
        /// <returns>The distance as an <see cref="int"/>.</returns>
        public int GetDistanceToBorder()
        {
            int x = EPosX;
            if (EPosX > (Program.C.board.Information.eRow - 1) / 2)
            {
                x = (Program.C.board.Information.eRow -1) - EPosX;
            }
            int y = EPosY;
            if (EPosY > (Program.C.board.Information.eRow - 1) / 2)
            {
                y = (Program.C.board.Information.eCol -1) - EPosY;
            }

            return Math.Min(x+1, y+1);


        }

        /// <summary>
        /// Method to contaminate this <see cref="Electrode"/> with the contamination of <paramref name="contaminator"/>
        /// </summary>
        /// <param name="contaminator"></param>
        public void Contaminate(string contaminator)
        {
            Contaminants.Add(contaminator);
        }

        /// <summary>
        /// Getter for <see cref="Contaminants"/>.
        /// </summary>
        /// <returns></returns>
        public List<string> GetContaminants()
        {
            return Contaminants;
        }

        /// <summary>
        /// Getter for <see cref="Status"/>.
        /// </summary>
        /// <returns></returns>
        public int GetStatus()
        {
            return Status;
        }

        /// <summary>
        /// ToString overrider.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + " x: " + EPosX + " y: " + EPosY;
        }

    }
}
