using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class Droplet(int x, int y,string substanceName, string color, string name = "", Board board) : TileEntity(x, y, name)
    {
        
        string SubstanceName { get; set; } = substanceName;
        public string Color = color;
        public float Temperature { get; set; }

    }
}
