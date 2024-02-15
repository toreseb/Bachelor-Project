using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class Electrode(int x, int y, string name = "") : TileEntity(x, y, 20, 20, name)
    {
        public int ElectrodeID { get; set; }
        public int DriverID { get; set; }
        public int Status { get; set; }
        // Contamination of tile in grid, may need changing later.
        private string[] Contaminants { get; set; } = [];

        public Electrode() : this(0, 0)
        {
        }

        public void Contaminate(string contaminator)
        {
            Contaminants = [.. Contaminants, contaminator];
        }

        public string[] GetContaminants()
        {
            return Contaminants;
        }

        public int GetStatus()
        {
            return Status;
        }
    }

}
