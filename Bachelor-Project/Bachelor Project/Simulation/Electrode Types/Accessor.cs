using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project.Electrode_Types
{
    internal class Accessor(int x, int y) : Electrode(x, y)
    {
        readonly int AccessorID;

        protected override void GetIDs()
        {
            base.GetIDs();
            IDs = [.. IDs, AccessorID];
        }

        protected override void GenerateID(params int[] values)
        {
            base.GenerateID(values);

            values[3] = 5; //AccessorID
        }
    }
}
