using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project.Electrode_Types
{

    public class Accessor(int x, int y) : Apparature(x, y, 1, 1)
    {
        public override bool CoilInto { get; set; } = false;


    }
}
