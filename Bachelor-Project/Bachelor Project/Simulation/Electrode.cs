using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class Electrode
    {   
        public int X { get; set; }
        public int Y { get; set; }
        public Electrode(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

    }

}
