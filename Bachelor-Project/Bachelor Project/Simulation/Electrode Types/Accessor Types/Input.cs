using Bachelor_Project.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types
{
    public class Input(int x, int y) : Accessor(x, y)
    {
        public int InputID;
        override public string Type { get; set; } = "input";


        public Input():this(0,0)
        {
        }
    }
}
