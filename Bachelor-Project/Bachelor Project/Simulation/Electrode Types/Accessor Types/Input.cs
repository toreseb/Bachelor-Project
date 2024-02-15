using Bachelor_Project.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types
{
    internal class Input(int x, int y) : Accessor(x, y)
    {
        readonly int InputID;
        
        public Input():this(0,0)
        {
        }
    }
}
