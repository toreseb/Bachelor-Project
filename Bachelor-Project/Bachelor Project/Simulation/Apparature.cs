using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    public abstract class Apparature(int x, int y, int sizeX, int sizeY, string name = "") : TileEntity(x, y,sizeX, sizeY, name)
    {
        public List<Electrode> pointers = [];

        public abstract bool CoilInto { get; set; }


    }
}
