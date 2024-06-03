using Bachelor_Project.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types
{
    /// <summary>
    /// A <see cref="Output"/> on the <see cref="Board"/> where a <see cref="Droplet"/> can be extracted.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public class Output(int x, int y) : Accessor(x, y)
    {
      
        public int OutputID;
        override public string Type { get; set; } = "output";
        public Output():this(0,0)
        {

        }
    }
}
