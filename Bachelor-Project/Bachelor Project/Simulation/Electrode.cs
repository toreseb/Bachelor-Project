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

        public static double GetDistance(Electrode e1, Electrode e2)
        {
            return Math.Sqrt(Math.Pow(e1.ePosX - e2.ePosX, 2) + Math.Pow(e1.ePosY - e2.ePosY, 2));
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
