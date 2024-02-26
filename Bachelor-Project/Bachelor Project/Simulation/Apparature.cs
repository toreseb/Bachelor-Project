using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class Apparature(int x, int y, int sizeX, int sizeY, string name = "") : TileEntity(x, y,sizeX, sizeY, name)
    {
        public List<Electrode> pointers = [];
    }
}
