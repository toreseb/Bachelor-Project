using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types
{
    public class Output(int x, int y) : Accessor(x, y)
    {
      
        public int OutputID;
        override public string Type { get; set; } = "output";
        public Output():this(0,0)
        {

        }
    }
}
