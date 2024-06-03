using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{

    /// <summary>
    /// The superclass of all <see cref="Apparatus"/>es on the <see cref="Board"/>.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    /// <param name="name"></param>
    public abstract class Apparatus(int x, int y, int sizeX, int sizeY, string name = "") : TileEntity(x, y,sizeX, sizeY, name)
    {
        public List<Electrode> pointers = [];

        abstract public string Type { get; set; }

        public abstract bool CoilInto { get; set; }


    }
}
