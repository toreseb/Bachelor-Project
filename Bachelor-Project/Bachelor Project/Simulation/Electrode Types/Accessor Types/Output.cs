using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Electrode_Types
{
    public class Output(int x, int y) : Accessor(x, y)
    {
        readonly int OutputID;
        public Output():this(0,0)
        {

        }
    }
}
