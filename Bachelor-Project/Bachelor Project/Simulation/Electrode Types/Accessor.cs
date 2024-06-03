using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bachelor_Project.Simulation;

namespace Bachelor_Project.Electrode_Types
{
    /// <summary>
    /// The superclass of all <see cref="Accessor"/>s on the <see cref="Board"/>.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public abstract class Accessor(int x, int y) : Apparatus(x, y, 1, 1)
    {
        public override bool CoilInto { get; set; } = false;


    }
}
