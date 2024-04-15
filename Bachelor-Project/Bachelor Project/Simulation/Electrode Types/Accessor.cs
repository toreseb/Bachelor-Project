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
        public Accessor() : this(0, 0)
        {
            CoilInto = false;
        }
    }
}
