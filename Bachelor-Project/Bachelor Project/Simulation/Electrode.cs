using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class Electrode(int x, int y, string name = "") : TileEntity(x, y ,name)
    {
        readonly int ElectrodeID;
        readonly int DriverID;

        // Contamination of tile in grid, may need changing later.
        private string contaminants { get; set; }

        protected override void GetIDs()
        {
            base.GetIDs();
            IDs = IDs.Append(ElectrodeID).ToArray();
            IDs = IDs.Append(DriverID).ToArray();
        }

        protected override void GenerateID(params int[] values)
        {
            base.GenerateID(values);
            values[1] = 2; //ElectrodeID
            values[2] = 3; //DriverID
        }

        public string GetContaminants()
        {
            return contaminants;
        }

        public int GetStatus()
        {
            return Status;
        }
    }

}
