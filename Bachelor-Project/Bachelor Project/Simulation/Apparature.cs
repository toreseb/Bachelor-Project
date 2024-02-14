using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class Apparature : TileEntity
    {

        public Apparature(int x, int y, int sizeX, int sizeY, string name = "") : base(x, y,sizeX, sizeY, name)
        {
        }
    }
}
